using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using Satchel;
using Satchel.Futils;
using SpellStaller.Charms;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
    internal class DummySpinner : MonoBehaviour { }

    public class FireballShooter : MonoBehaviour
    {
        private FireballSpawner spawner;

        public Vector3 SpawnPoint { get; set; }
        public Vector3 SpawnOffset { get; set; }

        public bool Spiral { get; set; }
        public bool Clockwise { get; set; }

        public float Direction { get; set; }
        public bool RemoveParticles { get; set; }
        public bool ForceLeft { get; set; }

        public FireballShooter()
        {
            spawner = new FireballSpawner();
            On.HeroController.Update += HeroController_Update;
        }

        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            RemoveParticles = true;
            ForceLeft = false;
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"1");
                Spiral = true;
                Clockwise = true;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"2");
                Spiral = true;
                Clockwise = false;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"3");
                Direction += 15f;
                Direction = Direction % 360;
                Spiral = false;
                ShootShadeSoul();
            }
        }

        public void ShootShadeSoulSpiral()
        {
            // spawner.SpawnShadeSoul(SpawnPoint + SpawnOffset, SpawnPoint, 0, ForceLeft, Spiral, Clockwise, RemoveParticles);
        }

        public void ShootShadeSoul()
        {
            spawner.SpawnShadeSoul(SpawnPoint + SpawnOffset, SpawnPoint, Direction, ForceLeft, Spiral, Clockwise, RemoveParticles);
        }
    }

    public class FireballSpawner
    {
        private static FireballSpawner _instance;

        private GameObject _fireballTopBase;
        private GameObject _fireball2TopBase;
        private static string _fireballTopBaseName = "CD_BaseFbTop";

        private GameObject _fireballBallBase;
        private GameObject _fireball2BallBase;
        private static string _fireballBallBaseName = "CD_BaseFbBall";


        internal static FireballSpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(FireballSpawner)} was never constructed");
                }
                return _instance;
            }
        }

        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new FireballSpawner();
                On.HutongGames.PlayMaker.Actions.Wait.OnEnter += ExtendFireballDuration;
            }
        }

        public GameObject FireballBaseTop
        {
            get
            {
                if (_fireballTopBase == null)
                {
                    var globalFireball = GameObject.Find("_GameManager/GlobalPool/Fireball Top(Clone)");

                    _fireballTopBase = GameObject.Instantiate(globalFireball);
                    _fireballTopBase.name = _fireballTopBaseName;
                    GameObject.DontDestroyOnLoad(_fireballTopBase);
                }
                return _fireballTopBase;
            }
        }

        public GameObject FireballBaseBall
        {
            get
            {
                if (_fireballBallBase == null)
                {
                    var globalFireball = GameObject.Find("_GameManager/GlobalPool/Fireball(Clone)");

                    _fireballBallBase = GameObject.Instantiate(globalFireball);
                    _fireballBallBase.name = _fireballBallBaseName;
                    GameObject.DontDestroyOnLoad(_fireballBallBase);
                }
                return _fireballBallBase;
            }
        }

        public GameObject Fireball2BaseTop
        {
            get
            {
                if (_fireball2TopBase == null)
                {
                    var globalFireball = GameObject.Find("_GameManager/GlobalPool/Fireball2 Top(Clone)");

                    _fireball2TopBase = GameObject.Instantiate(globalFireball);
                    _fireball2TopBase.name = _fireballTopBaseName;
                    GameObject.DontDestroyOnLoad(_fireball2TopBase);
                }
                return _fireball2TopBase;
            }
        }

        public GameObject Fireball2BaseBall
        {
            get
            {
                if (_fireball2BallBase == null)
                {
                    var globalFireball = GameObject.Find("_GameManager/GlobalPool/Fireball2 Spiral(Clone)");

                    _fireball2BallBase = GameObject.Instantiate(globalFireball);
                    _fireball2BallBase.name = _fireballBallBaseName;
                    GameObject.DontDestroyOnLoad(_fireball2BallBase);
                }
                return _fireball2BallBase;
            }
        }

        public void SpawnShadeSoul(
            Vector3 spawnPoint,
            Vector3 focalPoint,
            float direction,
            bool forceLeft,
            bool spiral,
            bool clockwise,
            bool removeParticles)
        {
            // TODO add play sound

            var tempFireball = GameObject.Instantiate(Fireball2BaseTop);
            tempFireball.name = Fireball2BaseTop.name;
            tempFireball.transform.position = spawnPoint;

            var castFSM = tempFireball.LocateMyFSM("Fireball Cast");

            castFSM.GetState("Wait").RemoveTransition("FINISHED");

            var modifyFireball = (bool left) =>
            {
                var fb = castFSM.GetVariable<FsmGameObject>("Fireball").Value;
                // Setting the name so it wont get hooked by SpellStaller.
                fb.name = _fireballBallBaseName;

                var fbControl = fb.LocateMyFSM("Fireball Control");

                // Fixing the direction on this as well.
                var init = fbControl.GetState("Init");
                var comp = init.GetFirstActionOfType<FloatCompare>();

                if (forceLeft)
                {
                    comp.equal = comp.greaterThan;
                    comp.lessThan = comp.greaterThan;
                }
                else
                {
                    comp.greaterThan = comp.equal;
                }

                // Cleaning up particles.
                // Better to remove if there are going to be many.
                if (removeParticles)
                {
                    fb.Find("white_light").SetActive(false);
                    fb.Find("Particle System").SetActive(false);
                    fb.Find("Particle System B").SetActive(false);
                    fb.Find("Dribble L").SetActive(false);
                    fb.Find("Dribble R").SetActive(false);
                }

                // Negating shaman stone effects.
                PlayerDataBoolTest modifyForShamans = (PlayerDataBoolTest)fbControl.GetState("Set Damage").Actions[4];
                modifyForShamans.isTrue = modifyForShamans.isFalse;

                // Dont recycle the ball
                fbControl.GetState("Off").RemoveTransition("FINISHED");

                if (spiral)
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Spiral");
                    fb.AddComponent<DummySpinner>().StartCoroutine(SpinFireball(
                        fb,
                        focalPoint,
                        clockwise,
                        spawnPoint,
                        true));
                } else
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Not Spiral");
                    var idle = fbControl.GetState("Idle");
                    idle.AddCustomAction(() =>
                    {
                        fb.transform.eulerAngles = Vector3.forward * direction;
                    });
                }
            };

            if (removeParticles)
            {
                tempFireball.Find("white_light").SetActive(false);
            }

            if (!spiral)
            {
                tempFireball.transform.eulerAngles = Vector3.forward * direction;
            }

            var castLeft = castFSM.GetState("Cast Left");
            castLeft.Actions[2].Enabled = false;
            castLeft.AddCustomAction(() => modifyFireball(true));
            if (!spiral)
            {
                var setAngle = (SetVelocityAsAngle)castLeft.Actions[6];
                setAngle.angle.Value = direction;
            }

            var castRight = castFSM.GetState("Cast Right");
            castRight.Actions[2].Enabled = false;
            castRight.AddCustomAction(() => modifyFireball(false));
            if (!spiral)
            {
                var setAngle = (SetVelocityAsAngle)castRight.Actions[6];
                setAngle.angle.Value = direction;
            }

            // Forcing a firing direction.
            var lr = castFSM.GetState("L or R");
            var comp = lr.GetFirstActionOfType<FloatCompare>();
            if (forceLeft)
            {
                comp.equal = comp.greaterThan;
                comp.lessThan = comp.greaterThan;
            }
            else
            {
                comp.greaterThan = comp.equal;
            }

            // Fire!
            tempFireball.SetActive(true);
        }

        public void SpawnShadeSoulAtEndOfFSM(FsmState fsm, int damageToDeal, Vector3 spawnPosition, Vector3 focalPoint, bool spiral, bool clockwise, bool forceLeft)
        {
            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;
            Satchel.FsmUtil.AddCustomAction(fsm, () =>
            {
                
            });
        }

        public void SpawnFireballAtEndOfFSM(FsmState fsm, int damageToDeal)
        {
            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;
            for (int i = 0; i < 1; ++i)
            {
            Satchel.FsmUtil.AddCustomAction(fsm, () =>
            {
                var gm = fsm.Fsm.GameObject;

                var tempFireball = GameObject.Instantiate(FireballBaseTop);
                tempFireball.name = FireballBaseTop.name;
                tempFireball.transform.position = gm.transform.position;

                var castFSM = tempFireball.LocateMyFSM("Fireball Cast");

                castFSM.GetState("Wait").RemoveTransition("FINISHED");

                var modifyFireball = (bool left) =>
                {
                    var fb = castFSM.GetVariable<FsmGameObject>("Fireball").Value;
                    fb.name = _fireballBallBaseName;
                    var fbControl = fb.LocateMyFSM("Fireball Control");
                    PlayerDataBoolTest modifyForShamans = (PlayerDataBoolTest)fbControl.GetState("Set Damage").Actions[3];
                    modifyForShamans.isTrue = modifyForShamans.isFalse;

                    fbControl.GetState("Dissipate End").RemoveTransition("FINISHED");
                    fbControl.GetState("Break").RemoveTransition("FINISHED");

                    fbControl.GetState("Idle").RemoveTransition("WALL");

                    fbControl.GetComponent<BoxCollider2D>().isTrigger = true;
                    fbControl.GetComponent<Rigidbody2D>().isKinematic = false;

                    fb.AddComponent<DummySpinner>().StartCoroutine(SpinFireball(fb, gm.transform.position, left, fb.transform.position, !left));
                };

                var castLeft = castFSM.GetState("Cast Left");
                castLeft.Actions[2].Enabled = false;
                castLeft.AddCustomAction(() => modifyFireball(true));

                var castRight = castFSM.GetState("Cast Right");
                castRight.Actions[5].Enabled = false;
                castRight.AddCustomAction(() => modifyFireball(false));

                tempFireball.SetActive(true);
            });

            }
        }

        // TODO maybe move this to the idle fsm?
        private IEnumerator SpinFireball(GameObject fb, Vector3 tar, bool isLeft, Vector3 initialPosition, bool flip=false)
        {
            var rb2 = fb.GetComponent<Rigidbody2D>();

            fb.transform.position = initialPosition;
            while (fb.activeSelf)
            {
                 fb.transform.SetScaleX(1f);
                 fb.transform.SetScaleY(1f);

                var vel = GetSpiralVelocity(fb.transform.position, tar, 3f, -0.5f, !isLeft);

                rb2.velocity = vel;

                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                fb.transform.eulerAngles = Vector3.forward * angle;
                yield return new WaitForSeconds(0.05f);
            }
        }


        public static Vector2 GetSpiralVelocity(
            Vector2 aPos,
            Vector2 bPos,
            float orbitSpeed,
            float inwardSpeed,
            bool clockwise = false)
        {
            Vector2 fromBToA = aPos - bPos;

            if (fromBToA.sqrMagnitude < 0.0001f)
                return Vector2.zero;

            Vector2 radialDir = fromBToA.normalized;

            Vector2 tangentDir = clockwise
                ? new Vector2(radialDir.y, -radialDir.x)
                : new Vector2(-radialDir.y, radialDir.x);

            Vector2 spiralVelocity =
                tangentDir * orbitSpeed -
                radialDir * inwardSpeed;

            return spiralVelocity;
        }


        public static void ExtendFireballDuration(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            try
            {
                if (self.Fsm.FsmComponent.gameObject.name.Contains(_fireballBallBaseName) &&
                    self.Fsm.Name == "Fireball Control" &&
                    self.State.Name == "Idle")
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"{self.Fsm.FsmComponent.gameObject.name} {self.Fsm.Name} {self.State.Name} {self.Fsm.FsmComponent.gameObject.tag}");
                    self.time.Value = 100f;
                }
            }
            catch (Exception err)
            {
            }
            orig(self);
        }
    }
}
