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

    public enum FlightTrajectory
    {
        STRAIGHT = 0,
        SPIRAL = 1,
        TRACKER = 2
    }

    public class FireballShooter : MonoBehaviour
    {
        private FireballSpawner spawner;

        public Vector3 SpawnPoint { get; set; }
        public GameObject BaseObject { get; set; }
        public Vector3 SpawnOffset { get; set; }

        public FlightTrajectory Trajectory { get; set; }
        public bool Clockwise { get; set; }

        public float Direction { get; set; }
        public float Speed { get; set; }

        public bool RemoveParticles { get; set; }
        public bool StopShooting { get; set; }

        public float OrbitSpeed { get; set; }
        public float InwardVel { get; set; }
        public FireballShooter()
        {
            spawner = new FireballSpawner();
             On.HeroController.Update += HeroController_Update;
            BaseObject = null;
        }

        // Deprecated
        public bool ForceLeft = false;

        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            // TODO Fix fireball going left, "stabilize" fireball going right
            RemoveParticles = true;
            ForceLeft = false;
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"1");
                Trajectory = FlightTrajectory.SPIRAL;
                Clockwise = true;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"2");
                Trajectory = FlightTrajectory.SPIRAL;
                Clockwise = false;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"3");
                Direction += 15f;
                Direction = Direction % 360;
                Trajectory = FlightTrajectory.STRAIGHT;
                Speed = 20f;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Direction = 270;
                Trajectory = FlightTrajectory.STRAIGHT;
                ForceLeft = false;
                RemoveParticles = false;
                Speed = 60f;

                this.StartCoroutine(ShootShadeSoulOnInterval(0.5f, 10));
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Direction = 270;
                Trajectory = FlightTrajectory.STRAIGHT;
                ForceLeft = false;
                RemoveParticles = false;
                Speed = 20f;

                this.StartCoroutine(ShootShadeSoulOnInterval(1f));
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                StopShooting = true;
                this.StopAllCoroutines();
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                // StopShooting = true;
                ShootVengefulSpirit();
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"8");
                Trajectory = FlightTrajectory.TRACKER;
                Direction = 0f;
                ForceLeft = true;
                Speed = 20f;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"9");
                Trajectory = FlightTrajectory.TRACKER;
                Direction = 180f;
                Speed = 20f;
                ForceLeft = false;
                ShootShadeSoul();
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                CorruptedDreamsPlandoMod.Instance.Log($"0");
                Trajectory = FlightTrajectory.TRACKER;
                Direction = 180f;
                Speed = 20f;
                ForceLeft = true;
                ShootShadeSoul();
            }
        }

        public void ShootShadeSoul()
        {
            if (BaseObject != null)
            {
                SpawnPoint = BaseObject.transform.position;
                // This is a bit of a hack for NKG specifically
                ForceLeft = BaseObject.transform.GetScaleX() == 1;
            }
            spawner.SpawnShadeSoul(SpawnPoint + SpawnOffset, SpawnPoint, Direction, ForceLeft, Trajectory, Clockwise, RemoveParticles, Speed, InwardVel);
        }

        public void ShootVengefulSpirit()
        {
            spawner.SpawnVengefulSpirit(SpawnPoint + SpawnOffset, SpawnPoint, Direction, ForceLeft, Trajectory==FlightTrajectory.SPIRAL, Clockwise, RemoveParticles, Speed);
        }

        public IEnumerator ShootShadeSoulOnInterval(float interval, float limit = 10000)
        {
            StopShooting = false;
            int count = 0;
            while (!StopShooting && count < limit)
            {
                spawner.SpawnShadeSoul(SpawnPoint + SpawnOffset, SpawnPoint, Direction, ForceLeft, Trajectory, Clockwise, RemoveParticles);
                count += 1;
                CorruptedDreamsPlandoMod.Instance.Log($"{count} {limit}");
                yield return new WaitForSeconds(interval);
            }
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
            FlightTrajectory trajectory,
            bool clockwise,
            bool removeParticles,
            float speed = 45f,
            float inwardSpeed = -0.5f)
        {
            // TODO allow remove sound

            var tempFireball = GameObject.Instantiate(Fireball2BaseTop);
            tempFireball.name = Fireball2BaseTop.name;
            if (spawnPoint != Vector3.zero)
            {
                tempFireball.transform.position = spawnPoint;
            }

            var castFSM = tempFireball.LocateMyFSM("Fireball Cast");

            castFSM.GetState("Wait").RemoveTransition("FINISHED");
            castFSM.GetState("Wait").AddCustomAction(() =>
            {
                GameObject.Destroy(tempFireball);
            });

            var modifyFireball = () =>
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
                    //fb.transform.SetScaleX(-1);
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
                fbControl.GetState("Off").AddCustomAction(() =>
                {
                    GameObject.Destroy(fb);
                });

                if (trajectory == FlightTrajectory.SPIRAL)
                {
                    fb.AddComponent<DummySpinner>().StartCoroutine(SpinFireball(
                        fb,
                        focalPoint,
                        clockwise,
                        spawnPoint,
                        speed,
                        inwardSpeed,
                        true));
                } else if (trajectory == FlightTrajectory.TRACKER)
                {
                    fb.name = fb.name + " TRACKER";

                    fb.AddComponent<DummySpinner>().StartCoroutine(UpdateFireballAngle(fb));

                    var idle = fbControl.GetState("Idle");
                    //idle.AddCustomAction(() =>
                    //{
                    //    fb.transform.eulerAngles = Vector3.forward * direction;
                    //});

                    idle.AddAction(new ChaseObjectV2()
                    {
                        gameObject = new FsmOwnerDefault
                        {
                            OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                            GameObject = new FsmGameObject { Value = fb }
                        },
                        target = Utils.Hero(),
                        speedMax = speed,
                        accelerationForce = 20f,
                        offsetX = 0,
                        offsetY = 0,
                    });
                } else
                {
                    var idle = fbControl.GetState("Idle");
                    idle.AddCustomAction(() =>
                    {
                        fb.transform.eulerAngles = Vector3.forward * direction;
                    });
                }

                List<GameObject> children = new List<GameObject>();
                fb.layer = LayerMask.NameToLayer("Enemy Attack");
                fb.RemoveComponent<DamageEnemies>();
                fb.LocateMyFSM("damages_enemy").enabled = false;
                var damageHero = fb.AddComponent<DamageHero>();
                damageHero.damageDealt = 2;
            };

            tempFireball.Find("white_light").SetActive(false);
            if (removeParticles)
            {
                tempFireball.Find("particles").SetActive(false);
                tempFireball.Find("particles b").SetActive(false);
                tempFireball.Find("Fireball2 Blast").SetActive(false);
            }

            if (trajectory == FlightTrajectory.STRAIGHT)
            {
                tempFireball.transform.eulerAngles = Vector3.forward * direction;
            } else if (trajectory == FlightTrajectory.TRACKER)
            {
                tempFireball.transform.eulerAngles = Vector3.forward * direction;
                // Add something to rotate based on knight
            }

            List<FsmState> castStates = new()
            {
                castFSM.GetState("Cast Left"),
                castFSM.GetState("Cast Right")
            };

            foreach (var castState in castStates)
            {
                castState.GetFirstActionOfType<PlayerDataBoolTest>().Enabled = false;
                castState.AddCustomAction(() => modifyFireball());
                if (trajectory != FlightTrajectory.SPIRAL)
                {
                    var setAngle = castState.GetFirstActionOfType<SetVelocityAsAngle>();
                    setAngle.angle.Value = direction;
                    setAngle.speed.Value = speed;
                }
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

        public void SpawnVengefulSpirit(
            Vector3 spawnPoint,
            Vector3 focalPoint,
            float direction,
            bool forceLeft,
            bool spiral,
            bool clockwise,
            bool removeParticles,
            float speed=40f)
        {
            var tempFireball = GameObject.Instantiate(FireballBaseTop);
            tempFireball.name = FireballBaseTop.name;
            tempFireball.transform.position = spawnPoint;

            var castFSM = tempFireball.LocateMyFSM("Fireball Cast");

            castFSM.GetState("Wait").RemoveTransition("FINISHED");
            castFSM.GetState("Wait").AddCustomAction(() =>
            {
                GameObject.Destroy(tempFireball);
            });

            var modifyFireball = () =>
            {
                var fb = castFSM.GetVariable<FsmGameObject>("Fireball").Value;
                fb.name = _fireballBallBaseName;
                var fbControl = fb.LocateMyFSM("Fireball Control");
                PlayerDataBoolTest modifyForShamans = (PlayerDataBoolTest)fbControl.GetState("Set Damage").Actions[3];
                modifyForShamans.isTrue = modifyForShamans.isFalse;

                fbControl.GetState("Dissipate End").RemoveTransition("FINISHED");
                fbControl.GetState("Break").RemoveTransition("FINISHED");

                fbControl.GetState("Dissipate End").AddCustomAction(() =>
                {
                    GameObject.Destroy(fb);
                });

                fbControl.GetState("Break").AddCustomAction(() =>
                {
                    GameObject.Destroy(fb);
                });

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

                if (removeParticles)
                {
                    fb.Find("white_light").SetActive(false);
                    //fb.Find("Particle System").SetActive(false);
                    //fb.Find("Dribble L").SetActive(false);
                    //fb.Find("Dribble R").SetActive(false);
                }

                var idle = fbControl.GetState("Idle");
                    idle.AddCustomAction(() =>
                    {
                        fb.transform.eulerAngles = Vector3.forward * direction;
                    });
                //}
            };


            List<FsmState> castStates = new()
            {
                castFSM.GetState("Cast Left"),
                castFSM.GetState("Cast Right")
            };

            foreach (var castState in castStates) {
                castState.GetFirstActionOfType<PlayerDataBoolTest>().Enabled = false;
                castState.AddCustomAction(() => modifyFireball());
                //if (!spiral)
                //{
                    var setAngle = castState.GetFirstActionOfType<SetVelocityAsAngle>();
                    setAngle.angle.Value = direction;
                    setAngle.speed.Value = speed;
                //}
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

            tempFireball.Find("white_light").SetActive(false);
            if (removeParticles)
            {
                tempFireball.Find("particles").SetActive(false);
                tempFireball.Find("Fireball Blast").SetActive(false);
            }

            tempFireball.SetActive(true);
        }

        // TODO maybe move this to the idle fsm?
        private IEnumerator SpinFireball(GameObject fb, Vector3 tar, bool isLeft, Vector3 initialPosition, float speed, float inwardSpeed, bool flip = false)
        {
            var rb2 = fb.GetComponent<Rigidbody2D>();

            fb.transform.position = initialPosition;
            while (fb.activeSelf)
            {
                 fb.transform.SetScaleX(1f);
                 fb.transform.SetScaleY(1f);

                var vel = GetSpiralVelocity(fb.transform.position, tar, speed, inwardSpeed, !isLeft);

                rb2.velocity = vel;

                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                fb.transform.eulerAngles = Vector3.forward * angle;
                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator UpdateFireballAngle(GameObject fb)
        {
            var rb2 = fb.GetComponent<Rigidbody2D>();
            while (fb.activeSelf)
            {
                //fb.transform.SetScaleX(1f);
                //fb.transform.SetScaleY(1f);

                var vel = rb2.velocity;
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
                    if (self.Fsm.FsmComponent.gameObject.name.Contains("TRACKER"))
                    {
                        self.time.Value = 15f;
                    }
                    else
                    {
                        self.time.Value = 25f;
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
