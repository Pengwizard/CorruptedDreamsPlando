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

    public struct SpiralInfo
    {
        public Vector3 focalPoint;
        public bool clockwise;
        public float inwardForce;
    }

    public class FireballShooter : MonoBehaviour
    {
        private FireballSpawner spawner;

        public Vector3 SpawnPoint { get; set; }
        public GameObject BaseObject { get; set; }
        public Vector3 SpawnOffset { get; set; }

        public SpiralInfo SpiralShotInfo;

        public float Direction { get; set; }
        public float Speed { get; set; }

        public bool RemoveParticles { get; set; }

        public float ShotScale { get; set; }
        public float PreFireDelay { get; set; }
        public int DamageToDeal { get; set; }
        public FireballShooter()
        {
            spawner = new FireballSpawner();
            BaseObject = null;
            ShotScale = 1f;
        }

        public void ShootStraight()
        {
            if (BaseObject != null)
            {
                // This is a bit of a hack for NKG specifically
                SpawnPoint = BaseObject.transform.position;
            }

            var fireball = spawner.CreateStraightShadeSoul(
                SpawnPoint + SpawnOffset,
                Direction,
                Speed,
                RemoveParticles,
                ShotScale,
                DamageToDeal,
                PreFireDelay);
            // Lets make a faint telegraph?

            var telegraphObj = Utils.MakeBlankTelegraphLight();
            telegraphObj.transform.eulerAngles = Vector3.forward * Direction;
            telegraphObj.SetScale(5f, 0.5f);

            telegraphObj.transform.SetParent(fireball.transform);
            telegraphObj.transform.SetPosition2D(SpawnPoint);


            fireball.SetActive(true);
        }

        public void ShootSpiral()
        {
            if (BaseObject != null)
            {
                // This is a bit of a hack for NKG specifically
                SpawnPoint = BaseObject.transform.position;
            }

            SpiralShotInfo.focalPoint = SpawnPoint;
            var fireball = spawner.CreateSpiralShadeSoul(
                SpawnPoint + SpawnOffset,
                Direction,
                Speed,
                RemoveParticles,
                ShotScale,
                DamageToDeal,
                SpiralShotInfo);
            fireball.SetActive(true);
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

        public GameObject CreateBaseShadeSoul(
            Vector3 spawnPoint,
            float direction,
            bool removeParticles,
            float preFireDelay=0)
        {
            var visualRotation = GetVisualRotation(direction);

            var fireballTop = GameObject.Instantiate(Fireball2BaseTop);
            fireballTop.name = Fireball2BaseTop.name;
            if (spawnPoint != Vector3.zero)
            {
                fireballTop.transform.position = spawnPoint;
            }

            var castFSM = fireballTop.LocateMyFSM("Fireball Cast");

            var castWaitState = castFSM.GetValidState("Wait");

            castWaitState.RemoveTransition("FINISHED");
            castWaitState.AddCustomAction(() =>
            {
                GameObject.Destroy(fireballTop);
            });

            fireballTop.Find("white_light").SetActive(false);
            if (removeParticles)
            {
                fireballTop.Find("particles").SetActive(false);
                fireballTop.Find("particles b").SetActive(false);
            }

            // Fucking idk
            fireballTop.transform.SetScaleX(1);
            fireballTop.transform.eulerAngles = Vector3.forward * visualRotation;

            List<FsmState> castStates = new()
            {
                castFSM.GetValidState("Cast Left"),
                castFSM.GetValidState("Cast Right")
            };

            foreach (var castState in castStates)
            {
                castState.GetFirstActionOfType<PlayerDataBoolTest>().Enabled = false;
                if (removeParticles)
                {
                }
            }

            var lr = castFSM.GetValidState("L or R");

            var newLr = castFSM.AddState("CD L or R");
            newLr.CopyActionData(lr);
            newLr.AddTransition("RIGHT", "Cast Right");
            newLr.AddTransition("LEFT", "Cast Left");

            castFSM.AddVariable<FsmFloat>("New Scale X");
            var nsx = castFSM.GetVariable<FsmFloat>("New Scale X");
            var comp = newLr.GetFirstActionOfType<FloatCompare>();
            comp.float1 = nsx;
            nsx.Value = -(Mathf.Cos(direction * Mathf.Deg2Rad));

            lr.RemoveTransition("RIGHT");
            lr.RemoveTransition("LEFT");
            lr.AddTransition("FINISHED", "CD L or R");

            lr.GetAction(0).Enabled = false;
            lr.GetAction(1).Enabled = false;
            lr.GetAction(2).Enabled = false;
            lr.GetAction(3).Enabled = false;

            if (preFireDelay > 0)
            {
                var sr = fireballTop.AddComponent<SpriteRenderer>();
                sr.sprite = new EmbeddedSprite("spellAnimations.fireballFrame1").Value;
                sr.size = new Vector2(20, 20);
                sr.enabled = true;

                if (nsx.Value > 0)
                {
                    sr.flipX = true;
                }

                lr.AddFirstAction(new Wait() { time = preFireDelay, finishEvent = new("FINISHED") });
            }

            var blast = fireballTop.Find("Fireball2 Blast");
            if (nsx.Value > 0)
            {
                blast.transform.SetScaleX(blast.transform.GetScaleX() * -1);
            }

            return fireballTop;
        }

        public static bool EmptyModification(GameObject fb, PlayMakerFSM fbControl)
        {
            return true;
        }

        public static float GetVisualRotation(float direction)
        {
            return (direction >= 90 && direction < 270) ? direction - 180 : direction;
        }

        public void ModifyShadeSoulFlightBase(
            PlayMakerFSM castFSM,
            float direction,
            bool removeParticles,
            int damageToDeal,
            Func<GameObject, PlayMakerFSM, bool> extraModificaitons = null
        )
        {
            var fb = castFSM.GetVariable<FsmGameObject>("Fireball").Value;
            // Setting the name so it wont get hooked by SpellStaller.
            fb.name = _fireballBallBaseName;

            var fbControl = fb.LocateMyFSM("Fireball Control");

            // Fixing the direction on this as well.
            var init = fbControl.GetValidState("Init");

            castFSM.AddVariable<FsmFloat>("New Scale X");
            var nsx = castFSM.GetVariable<FsmFloat>("New Scale X");
            var comp = init.GetFirstActionOfType<FloatCompare>();
            comp.float1 = nsx;
            nsx.Value = -(Mathf.Cos(direction * Mathf.Deg2Rad));

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
            PlayerDataBoolTest modifyForShamans = (PlayerDataBoolTest)fbControl.GetValidState("Set Damage").Actions[4];
            modifyForShamans.isTrue = modifyForShamans.isFalse;

            // Dont recycle the ball
            var controlOffState = fbControl.GetValidState("Off");
            controlOffState.RemoveTransition("FINISHED");
            controlOffState.AddCustomAction(() =>
            {
                GameObject.Destroy(fb);
            });

            List<GameObject> children = new List<GameObject>();
            fb.layer = LayerMask.NameToLayer("Enemies");
            fb.RemoveComponent<DamageEnemies>();
            fb.LocateMyFSM("damages_enemy").enabled = false;
            var damageHero = fb.AddComponent<DamageHero>();
            damageHero.damageDealt = damageToDeal;

            if (extraModificaitons != null)
            {
                extraModificaitons(fb, fbControl);
            }
        }

        public GameObject CreateStraightShadeSoul(
            Vector3 spawnPoint,
            float direction,
            float speed,
            bool removeParticles,
            float scale,
            int damageToDeal,
            float preFireDelay = 0)
        {
            var fireballTop = CreateBaseShadeSoul(spawnPoint, direction, removeParticles, preFireDelay);

            var castFSM = fireballTop.LocateMyFSM("Fireball Cast");

            List<FsmState> castStates = new()
            {
                castFSM.GetValidState("Cast Left"),
                castFSM.GetValidState("Cast Right")
            };

            var extraModifications = (GameObject fb, PlayMakerFSM fbControl) => {

                var idle = fbControl.GetValidState("Idle");
                idle.AddCustomAction(() =>
                {
                    fb.transform.eulerAngles = Vector3.forward * GetVisualRotation(direction);
                });
                fb.AddComponent<DummySpinner>().StartCoroutine(DelayScale(fb, scale));

                fb.layer = LayerMask.NameToLayer("Enemies");

                return true;
            };


            foreach (var castState in castStates)
            {
                var setAngle = castState.GetFirstActionOfType<SetVelocityAsAngle>();
                setAngle.angle.Value = direction;
                setAngle.speed.Value = speed;

                castState.AddCustomAction(() =>
                {
                    ModifyShadeSoulFlightBase(castFSM, direction, removeParticles, damageToDeal, extraModifications);
                });
            }


            return fireballTop;
        }

        public GameObject CreateSpiralShadeSoul(
            Vector3 spawnPoint,
            float direction,
            float speed,
            bool removeParticles,
            float scale,
            int damageToDeal,
            SpiralInfo spiralInfo)
        {
            var fireballTop = CreateBaseShadeSoul(spawnPoint, direction, removeParticles);

            if (removeParticles)
            {
                fireballTop.Find("Fireball2 Blast").SetActive(false);
            }

            var castFSM = fireballTop.LocateMyFSM("Fireball Cast");

            List<FsmState> castStates = new()
            {
                castFSM.GetValidState("Cast Left"),
                castFSM.GetValidState("Cast Right")
            };


            var extraModifications = (GameObject fb, PlayMakerFSM fbControl) =>
            {
                fb.AddComponent<DummySpinner>().StartCoroutine(DelayScale(fb, scale));

                fb.GetComponent<DummySpinner>().StartCoroutine(SpinFireball2(
                    fb,
                    spawnPoint,
                    spiralInfo.focalPoint,
                    spiralInfo.clockwise,
                    speed,
                    spiralInfo.inwardForce));

                return true;
            };

            foreach (var castState in castStates)
            {
                castState.GetFirstActionOfType<AudioPlayerOneShotSingle>().Enabled = false;

                castState.AddCustomAction(() =>
                {
                    ModifyShadeSoulFlightBase(castFSM, direction, removeParticles, damageToDeal, extraModifications);
                });
            }

            return fireballTop;
        }

        private IEnumerator SpinFireball2(
            GameObject fb,
            Vector3 initialPosition,
            Vector3 target,
            bool clockwise,
            float speed,
            float inwardSpeed)
        {
            var rb2 = fb.GetComponent<Rigidbody2D>();

            fb.transform.position = initialPosition;
            while (fb.activeSelf)
            {
                var vel = GetSpiralVelocity(fb.transform.position, target, speed, inwardSpeed, !clockwise);

                rb2.velocity = vel;

                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                fb.transform.eulerAngles = Vector3.forward * angle;
                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator DelayScale(GameObject fb, float scale)
        {
            var x = Math.Abs(fb.transform.GetScaleX());
            while (x != scale)
            {
                x = Math.Abs(fb.transform.GetScaleX());
                fb.transform.SetScaleY(scale);
                if (fb.transform.GetScaleX() >= 0)
                {
                    fb.transform.SetScaleX(scale);
                }
                else
                {
                    fb.transform.SetScaleX(-scale);
                }

                yield return new WaitForEndOfFrame();
            }

            var spr = fb.GetComponent<tk2dSprite>();
            SpriteSwapper.SetupSpellTextureSwap(spr);
            var ogScale = spr.scale;
            ogScale.x *= 1f;
            ogScale.y *= 1.36f;
            ogScale.z *= 1f;
            spr.scale = ogScale;
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
                    if (self.Fsm.FsmComponent.gameObject.name.Contains("TRACKER"))
                    {
                        self.time.Value = 15f;
                    }
                    else
                    {
                        self.time.Value = 15f;
                    }
                }
            }
            catch (Exception err)
            {
            }
            orig(self);
        }
    }
}
