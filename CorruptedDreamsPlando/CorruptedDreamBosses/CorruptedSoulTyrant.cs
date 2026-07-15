using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IL;
using ItemChanger;
using ItemChanger.Internal;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using SpellStaller.Charms;
using UnityEngine;
using UnityEngine.UIElements;
using static SceneLoad;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{

    public class SingleCastSoulOrbSpawner : MonoBehaviour
    {
        public static void SpawnSoul(Transform transform)
        {
            var prefab = ObjectCache.SoulOrb;
            Destroy(prefab.Spawn());
            prefab.SetActive(true);

            // Give 34 soul per usage.
            // double if dream wielder is equipped
            // max if can dream gate.
            int soulToGive = 17;
             int masksToHeal = 0;

            FlingUtils.Config config = new()
            {
                Prefab = prefab,
                AmountMin = soulToGive,
                AmountMax = soulToGive,
                SpeedMin = 10,
                SpeedMax = 20,
                AngleMin = 0,
                AngleMax = 360,
            };
            var objs = FlingUtils.SpawnAndFling(config, transform, Vector3.zero);

            // Heal on parry.
            HeroController.instance.AddHealth(masksToHeal);

            IEnumerator Routine()
            {
                yield return new WaitForSeconds(10f);
                foreach (var o in objs)
                {
                    Destroy(o);
                }
            }

            var sos = GameManager.instance.gameObject.GetComponent<SingleCastSoulOrbSpawner>();
            if (sos == null)
            {
                sos = GameManager.instance.gameObject.AddComponent<SingleCastSoulOrbSpawner>();
            }

            sos.StartCoroutine(Routine());
            Destroy(prefab);
        }
    }

    public class NailCollisionDetector : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.name == "Clash Tink" && collision.attachedRigidbody.gameObject.name == "Knight")
            {
                var go = this.gameObject;

                var fsm = go.LocateMyFSM("Orb Control");
                if (fsm != null && go.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    SingleCastSoulOrbSpawner.SpawnSoul(go.transform);
                    fsm.SetState("Impact");
                }
            }
        }
    }

    internal class AnimationModifiersSt
    {
        public float ShotAnticAnimation;
        public float ShotAnticWait;

        // Phase 2
        public float QuakeAntic;
        public float Quake;
        public float QuakeLandWait;
        public float PostReboundDelay;

        public float SpawnFireballWait;
        public float OrbSummonWait;

        // Numbers for Phase Change bit
        public float RoarWaitTime;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate;
        public float PhaseChangeHpMultiplier;


        public static AnimationModifiersSt FromJsonFile(string path)
        {
            if (path == "")
            {
                path = "C:\\clean\\hkMods\\CorruptedDreamsPlando\\CorruptedDreamsPlando\\Resources\\bossVariables\\corruptedSoulTyrantModifiers.json";
            }

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                AnimationModifiersSt items = JsonConvert.DeserializeObject<AnimationModifiersSt>(json);
                return items;
            }
        }
    }

    public class CorruptedSoulTyrant
    {
        // TODO need a seperate setup to change phase 2
        private static readonly FsmID controlFSM = new("Dream Mage Lord", "Mage Lord");
        private static readonly FsmID controlFSMPhase2 = new("Dream Mage Lord Phase2", "Mage Lord 2");


        private static bool phaseChanged;
        private static GameObject _bossObject;
        private static GameObject _phase2BossObject;
        private static int _startingHp;
        private static int _phase1Hp;
        private static AnimationModifiersSt _modifiers;

        public static void Initialize()
        {
            Events.AddFsmEdit(controlFSM, SetupPhaseChange);
            Events.AddFsmEdit(controlFSMPhase2, SetupPhase2);
        }

        private static void HandleOrbSpawn(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (self.gameObject.name.ToLower().Contains("mage orb"))
            {
                if (self.gameObject.GetComponent<NailCollisionDetector>() == null)
                {
                    self.gameObject.GetComponent<CircleCollider2D>().isTrigger = true;
                    self.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                    self.gameObject.AddComponent<NailCollisionDetector>();

                    self.gameObject.layer = LayerMask.NameToLayer("Enemy");
                }
            }

            orig(self);
        }

        public static void Unload()
        {
            Events.RemoveFsmEdit(controlFSM, SetupPhaseChange);
            Events.RemoveFsmEdit(controlFSMPhase2, SetupPhase2);
            On.PlayMakerFSM.OnEnable += HandleOrbSpawn;
        }

        public static void UnpackModifiers()
        {
            _modifiers = AnimationModifiersSt.FromJsonFile("");
        }

        public static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Changing ST phase 1");
            phaseChanged = false;


            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            _phase2BossObject = GameObject.Find("Dream Make Lord Phase2");

            // TODO disable stagger

            // Phase change starts from Teleport In
            // Check HP gate in Damaged Idle
            // Phase transition
            // Setup speedup
            // Faster animations
            // make orbs contantly spawn
            // Setup phase 2 in preparation
            // Title drop
            // 

            // Phase 2 changes
            // Orbs contantly spawn
            // dive down, slam up
            // can also go sideways with a light glow telegraph (make generic "telegraph light" since it will be use full)

            // Adding invincibility during roar stuff.
            var rageRoarState = fsm.GetState("Roar");
            Satchel.FsmUtil.AddFirstAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

            var roarEndPhase = fsm.GetState("Roar End");
            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

            FsmState rageState = SetupRageSetupState(fsm, "Set Entry Point");
            AddPhaseChangeStep(fsm, "Idle", "TOOK DAMAGE", "Damaged Idle", rageState);
        }

        public static FsmState SetupRageSetupState(
            PlayMakerFSM fsm,
            string stateAfterRage)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Setup Rage state");
            var rageCheckState = fsm.AddState("Setup Rage");

            rageCheckState.AddTransition("FINISHED", stateAfterRage);            

            rageCheckState.AddCustomAction(() =>
            {
                var rageRoarState = fsm.GetState("Roar");
                rageRoarState.GetFirstActionOfType<Wait>().time = _modifiers.RoarWaitTime;

                CorruptedDreamsPlandoMod.Instance.Log("Raging");
                Grayscaler.TickCamera();
                SetupTitleScream(fsm);
                ModifyAttackPatternsPhase1(fsm);
                ModifyVariablesPhase1(fsm);
                ModifyAnimationTimesPhase1(fsm);
                On.PlayMakerFSM.OnEnable += HandleOrbSpawn;

                // Phase 2 things
            });

            return rageCheckState;
        }


        private static void SetupPhase2(PlayMakerFSM fsm)
        {
            ModifyAttackPatternsPhase2(fsm);
            ModifyVariablesPhase2(fsm);
        }


        public static void AddPhaseChangeStep(
            PlayMakerFSM fsm,
            string stateToHookTransitionName,
            string transitionToHook,
            string stateIfNotRageName,
            FsmState rageSetupState)
        {
            
            var healthManager = _bossObject.GetComponent<HealthManager>();
            _startingHp = healthManager.hp;
            healthManager.hp = 9999;
            _phase1Hp = 10; // (int)(_startingHp * 0.5f);
            var phaseTransitionGate = healthManager.hp - _phase1Hp;
            CorruptedDreamsPlandoMod.Instance.Log($"Setting up phase change, will trigger at {phaseTransitionGate} hp");

            string setupRageTransition = "SETUP RAGE";
            string defaultTransition = "FINISHED";

            string rageCheckStateName = "Rage?";

            var rageCheckState = fsm.AddState(rageCheckStateName);

            var stateToHookTransition = fsm.GetState(stateToHookTransitionName);
            stateToHookTransition.RemoveTransition(transitionToHook);
            stateToHookTransition.AddTransition(transitionToHook, rageCheckStateName);

            rageCheckState.AddTransition(setupRageTransition, rageSetupState.Name);
            rageCheckState.AddTransition(defaultTransition, stateIfNotRageName);

            fsm.AddVariable<FsmInt>("CD_HP");
            var hpFSM = fsm.GetVariable<FsmInt>("CD_HP");

            fsm.AddVariable<FsmInt>("CD_HPGate");
            var hpGateFSM = fsm.GetVariable<FsmInt>("CD_HPGate");
            hpGateFSM.Value = phaseTransitionGate;

            fsm.AddVariable<FsmBool>("CD_Raged");
            var ragedBool = fsm.GetVariable<FsmBool>("CD_Raged");

            ragedBool.Value = false;

            rageCheckState.AddAction(new GetHP()
            {
                target = new HutongGames.PlayMaker.FsmOwnerDefault()
                {
                    GameObject = new FsmGameObject { Value = _bossObject },
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject
                },
                storeValue = hpFSM,
            });

            rageCheckState.AddCustomAction(() =>
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Checking for rage. Currently at {hpFSM.Value} out of {hpGateFSM.Value}");
                if (ragedBool.Value)
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Already Raged");
                    fsm.SendEvent(defaultTransition);
                }
                else if (hpFSM.Value > hpGateFSM.Value)
                {
                    fsm.SendEvent(defaultTransition);
                } else
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Trying to rage");
                    ragedBool.Value = true;
                    fsm.SendEvent(setupRageTransition);
                }
            });
        }
        

        public static void SetupTitleScream(PlayMakerFSM fsm)
        {
            FsmEditTemplates.FullScreenTitleDrop.SetupTitleUp(
                fsm,
                "Abyssal Arbiter",
                "Roar",
                "Roar End",
                _modifiers.TitleFadeUpDelay,
                _modifiers.TitleFadeDownDelay,
                _modifiers.WhiteSolidUpTime,
                _modifiers.WhiteSolidUpDelay,
                UnityEngine.Color.black,
                UnityEngine.Color.white);
        }

        private static int shotCount = 0;
        // Move this number to modifiers.
        private static int maxShotCount = 4;

        private static void ModifyAttackPatternsPhase1(PlayMakerFSM fsm)
        {
            // shoots multiple fireballs each time
            // this phase should feel tame

            // Disable the "charge" attack. Leaving the "High Spinner" attack because... idk
            var attackChoiceState = fsm.GetState("Attack Choice");

            // The first attack option is the charge so we just make that if charge is rolled we do shoot instead
            var chooseAttackAtRandom = attackChoiceState.GetFirstActionOfType<SendRandomEventV3>();
            chooseAttackAtRandom.events[0] = chooseAttackAtRandom.events[2];

            // Increase the amount of fireballs spawned when doing fireballs
            // We do this by looping the "Shot Antic" and "Shot" states

            var setShotsState = fsm.GetState("Set Shots");
            setShotsState.AddCustomAction(() =>
            {
                shotCount = 0;
            });
            var shotState = fsm.GetState("Shot");

            //shotState.InsertCustomAction(() => {
            //    var spawnObject = GameObject.Find("Dream Mage Lord/Fire Effect");
            //    var tar = spawnObject.transform;
            //    if (shotCount == 0)
            //    {
            //        tar.SetPositionX(tar.GetPositionX() - 1f);
            //        tar.SetPositionY(tar.GetPositionY() - 0.5f);
            //    } else
            //    {
            //        tar.SetPositionX(tar.GetPositionX() + 0.5f);
            //        tar.SetPositionY(tar.GetPositionY() + (0.25f * (shotCount > (maxShotCount / 2) ? -1 : 1)));
            //    }

            //    //var projectile = fsm.GetVariable<FsmGameObject>("Projectile");
            //    //// (-1, -0.5), (-0.5, -0.25), (0, 0), (+0.5, -0.25), (+1, -0.5)
            //    //var tar = projectile.Value.transform;
            //    //tar.SetPositionX(tar.GetPositionX() +  (maxShotCount - shotCount) / 5);
            //    //if (shotCount == 0 || shotCount == maxShotCount)
            //    //{
            //    //    tar.SetPositionY(tar.GetPositionY() - 0.5f);
            //    //}
            //}, 0);

            shotState.AddTransition("REPEAT SHOT", "Shot Antic");

            shotState.AddCustomAction(() =>
            {
                shotCount++;
                if (shotCount > maxShotCount)
                {
                    fsm.SendEvent("FINISHED");
                }
                else
                {
                    fsm.SendEvent("REPEAT SHOT");
                }
            });
        }

        private static int counter = 0;
        private static int diveDirection = 0;
        private static int DEFAULT_DIVE_SPEED_MAGNITUTE = 175;

        private static void ModifyAttackPatternsPhase2(PlayMakerFSM fsm)
        {

            // TODO:
            // Fix shriek on dive, the wait inside the abyss shriek makes it be very slow.
            //   If the timing works, one of the attacks can just spawn a shriek on the player.                
            // Probably hijack all the way back from TeleQuake because otherwise its a bit fucked
            // Remove fake out dives
            // Increase dive counter to be between 5 to 10 pairs before shooting
            // make the dives faster, but only inside pairs. the time between pairs is great.
            // increase phase 2 hp probably, and make invuln during fireball phase
            // make fireballs during fireball phase spawn faster

            var go = fsm.gameObject;

            Utils.SpeedupAnimation(fsm, "Quake Antic", _modifiers.QuakeAntic);
            Utils.SpeedupAnimation(fsm, "Quake Down", _modifiers.Quake);

            fsm.AddVariable<FsmFloat>("Quake Speed Y");
            fsm.AddVariable<FsmFloat>("Quake Speed X");
            fsm.AddVariable<FsmVector3>("Antic Movement Vector");

            fsm.AddVariable<FsmFloat>("Arena Ground");
            fsm.AddVariable<FsmFloat>("Arena Top");
            fsm.AddVariable<FsmFloat>("Arena Left");
            fsm.AddVariable<FsmFloat>("Arena Right");

            var quakeSetup = fsm.AddState("Quake Setup");
            var quakeSelDir = fsm.AddState("Quake Select Direction");
            var quakeShriek = fsm.AddState("Quake Shriek");
            var postReboundDelay = fsm.AddState("Post Rebound Delay");
            var shriekState = fsm.AddState("Shriek State");

            var teleportQ = fsm.GetState("TeleportQ");
            var shift = fsm.GetState("Shift?");
            var quakeAntic = fsm.GetState("Quake Antic");
            var quakeDown = fsm.GetState("Quake Down");
            var teleQuake = fsm.GetState("Tele Quake");
            var teleQuake2 = fsm.GetState("Tele Quake 2");
            var quakeLand = fsm.GetState("Quake Land");
            var reactivate = fsm.GetState("Reactivate");
            var shoot = fsm.GetState("Shoot?");

            //shoot.Actions[2].Enabled = false;
            shoot.AddCustomAction(() =>
            {
                fsm.GetVariable<FsmInt>("Quakes").Value = 0;
            });

            var qsY = fsm.GetVariable<FsmFloat>("Quake Speed Y");
            var qsX = fsm.GetVariable<FsmFloat>("Quake Speed X");
            var anticMovementVector = fsm.GetVariable<FsmVector3>("Antic Movement Vector");
            var teleX = fsm.GetVariable<FsmFloat>("Tele X");
            var teleY = fsm.GetVariable<FsmFloat>("Tele Y");
            FsmFloat heroX = fsm.GetVariable<FsmFloat>("Hero X");
            FsmFloat heroY = fsm.GetVariable<FsmFloat>("Hero Y");

            fsm.GetVariable<FsmFloat>("Arena Ground").Value = 11.34f;
            fsm.GetVariable<FsmFloat>("Arena Top").Value = 23.00f;
            fsm.GetVariable<FsmFloat>("Arena Left").Value = 10.76f;
            fsm.GetVariable<FsmFloat>("Arena Right").Value = 40.18f;


            var heroObj = teleQuake.GetFirstActionOfType<GetPosition>().gameObject;

            var tweenMovement = quakeAntic.GetFirstActionOfType<iTweenMoveBy>();
            tweenMovement.vector = anticMovementVector;

            var horizontalChase = quakeAntic.GetFirstActionOfType<ChaseObjectGround>();
            var verticalChase = new ChaseObjectVertical { gameObject = horizontalChase.gameObject, target = horizontalChase.target, speedMax = 5f, acceleration = 1f };
            quakeAntic.AddAction(verticalChase);


            var getPos = quakeDown.GetFirstActionOfType<GetPosition>();
            getPos.x = fsm.GetVariable<FsmFloat>("Self X");

            var settingSpeed = quakeDown.GetFirstActionOfType<SetVelocity2d>();
            settingSpeed.x = qsX;
            settingSpeed.y = qsY;

            //var damageHero = (SetDamageHeroAmount)(quakeLand.Actions[4]);
            //damageHero.damageDealt = 1;

            // Design:
            // Identify the parameters we need to set and when to set them (spawn point, speed, rotation, end point, etc) (done)
            // Have a complete dive flow that is managed by these 4 parameters (done)
            // Have the setup change these parameters based on the direction (done)
            // Only for up direction - spawn shriek
            // After diving once increment the counter so we know if this is the first or second dive in a sequence
            // Need rebound
            // Reset rotation before fireball attack
            // Make dive faster

            //var quakeAnticState = fsm.GetState("Quake Antic");
            //var movement = quakeAnticState.GetFirstActionOfType<iTweenMoveBy>();
            //if (movement != null)
            //{
            //    movement.time.Value = movement.time.Value / _modifiers.QuakeAntic;
            //}

            // We re-make the flow, so that instead of down first, we setup some parameters, and then fire in the appropriate position.

            fsm.RemoveTransition("Shift?", "FINISHED");
            reactivate.RemoveTransition("FINISHED");
            //fsm.RemoveTransition("Tele Quake 2", "FINISHED");

            shift.AddTransition("FINISHED", quakeSelDir.Name);
            //teleQuake.AddTransition("FINISHED", quakeSelDir.Name);
            //teleQuake2.AddTransition("FINISHED", quakeSelDir.Name);

            quakeSelDir.AddTransition("DO QUAKE", quakeSetup.Name);

            // reactivate.AddTransition("FINISHED", "Tele Quake");

            fsm.RemoveTransition("Quake Land", "FINISHED");

            quakeSetup.AddTransition("FINISHED", teleportQ.Name);

            quakeLand.AddTransition("FINISHED", quakeShriek.Name);
            //quakeLand.AddTransition("DONT SHRIEK", quakeSelDir.Name);
            quakeShriek.AddTransition("FINISHED", reactivate.Name);
            quakeShriek.AddTransition("DID REBOUND", postReboundDelay.Name);
            quakeShriek.AddTransition("DID SHRIEK", shriekState.Name);
            shriekState.AddTransition("FINISHED", postReboundDelay.Name);
            postReboundDelay.AddTransition("FINISHED", shoot.Name);
            reactivate.AddTransition("FINISHED", quakeSelDir.Name);

            // TODO make json
            Utils.ChangeWaitFloat(fsm, "Quake Land", 13, _modifiers.QuakeLandWait);

            go.transform.SetScaleX(1.1f);
            go.transform.SetScaleY(1.1f);

            quakeShriek.AddCustomAction(() =>
            {
                if (diveDirection == 3)
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"shriek");
                    counter = 0;
                    fsm.SendEvent("DID SHRIEK");
                    //fsm.SendEvent("DID REBOUND");
                    go.transform.SetRotationZ(0);
                } else
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"no shriek");
                    if (counter == 2)
                    {
                        //CorruptedDreamsPlandoMod.Instance.Log($"Did Rebound");
                        counter = 0;
                        fsm.SendEvent("DID REBOUND");
                        go.transform.SetRotationZ(0);
                    }
                }
            });

            // TODO make json
            postReboundDelay.AddAction(new Wait() { time = _modifiers.PostReboundDelay, finishEvent = new("FINISHED") });

            AbyssShriekSpawner.Instance.AddShriekAtEndOfFSM(shriekState, 2, new Vector3(0, 2, 0), true);

            quakeSelDir.AddCustomAction(() =>
            {
                //CorruptedDreamsPlandoMod.Instance.Log($"Inside Quake Select Direction {diveDirection} {counter}");
                if (counter == 1)
                {
                    switch (diveDirection)
                    {
                        case 0: diveDirection = 3; break;
                        case -1: diveDirection = 3; break;
                        case 1: diveDirection = 2; break;
                        case 2: diveDirection = 1; break;
                    }
                }
                else
                {
                    diveDirection = UnityEngine.Random.Range(-1, 3);

                }
                fsm.SendEvent("DO QUAKE");
            });


            quakeSelDir.AddAction(quakeLand.GetFirstActionOfType<SetDamageHeroAmount>());
            quakeSelDir.GetFirstActionOfType<SetDamageHeroAmount>().damageDealt = 1;

            quakeSetup.AddAction(new GetPosition { gameObject = heroObj, vector = new Vector3(0, 0), x = heroX, y = heroY, z = 0, everyFrame = false, space = 0 });

            quakeSetup.AddCustomAction(() =>
            {
                //CorruptedDreamsPlandoMod.Instance.Log($"Inside Quake Setup {diveDirection}");
                var qsY = fsm.GetVariable<FsmFloat>("Quake Speed Y");
                var qsX = fsm.GetVariable<FsmFloat>("Quake Speed X");
                var anticMovementVector = fsm.GetVariable<FsmVector3>("Antic Movement Vector");
                var teleX = fsm.GetVariable<FsmFloat>("Tele X");
                var teleY = fsm.GetVariable<FsmFloat>("Tele Y");

                var heroX = fsm.GetVariable<FsmFloat>("Hero X");
                var heroY = fsm.GetVariable<FsmFloat>("Hero Y");

                var endCheck = quakeDown.GetFirstActionOfType<FloatCompare>();

                var setSpeedX = 0;
                var setSpeedY = 0;
                float rotationAngle = 0;
                FsmFloat endCheckFloat1 = null;
                FsmFloat endCheckFloat2 = null;

                FsmFloat setTeleX = null;
                FsmFloat setTeleY = null;
                Vector3 tweenMovementVector = Vector3.zero;

                switch (diveDirection)
                {
                    // Dive Down
                    case 3:
                    case -1:
                    case 0:
                        {
                            setSpeedX = 0;
                            setSpeedY = DEFAULT_DIVE_SPEED_MAGNITUTE * (-1);
                            rotationAngle = 0;
                            endCheckFloat1 = fsm.GetVariable<FsmFloat>("Self Y");
                            endCheckFloat2 = fsm.GetVariable<FsmFloat>("Arena Ground");
                            setTeleX = fsm.GetVariable<FsmFloat>("Hero X");
                            setTeleY = fsm.GetVariable<FsmFloat>("Arena Top");

                            tweenMovementVector = new Vector3(0, 2, 0);
                            horizontalChase.Enabled = true;
                            verticalChase.Enabled = false;
                            break;
                        }
                    // Right
                    case 1:
                        {
                            setSpeedX = DEFAULT_DIVE_SPEED_MAGNITUTE;
                            setSpeedY = 0;
                            rotationAngle = 90;
                            endCheckFloat1 = fsm.GetVariable<FsmFloat>("Arena Right");
                            endCheckFloat2 = fsm.GetVariable<FsmFloat>("Self X");
                            setTeleX = fsm.GetVariable<FsmFloat>("Arena Left");
                            setTeleY = fsm.GetVariable<FsmFloat>("Hero Y");

                            tweenMovementVector = new Vector3(-2, 0, 0);
                            horizontalChase.Enabled = false;
                            verticalChase.Enabled = true;
                            break;
                        }
                    // Left
                    case 2:
                        {
                            setSpeedX = DEFAULT_DIVE_SPEED_MAGNITUTE * (-1);
                            setSpeedY = 0;
                            rotationAngle = -90;
                            endCheckFloat1 = fsm.GetVariable<FsmFloat>("Self X");
                            endCheckFloat2 = fsm.GetVariable<FsmFloat>("Arena Left");
                            setTeleX = fsm.GetVariable<FsmFloat>("Arena Right");
                            setTeleY = fsm.GetVariable<FsmFloat>("Hero Y");

                            tweenMovementVector = new Vector3(2, 0, 0);
                            horizontalChase.Enabled = false;
                            verticalChase.Enabled = true;
                            break;
                        }
                }

                qsY.Value = setSpeedY;
                qsX.Value = setSpeedX;
                anticMovementVector.Value = tweenMovementVector;

                go.transform.SetRotationZ(rotationAngle);

                var teleportSetVector = teleportQ.GetFirstActionOfType<SetVector3XYZ>();
                teleportSetVector.x = setTeleX;
                teleportSetVector.y = setTeleY;

                endCheck.float1 = endCheckFloat1;
                endCheck.float2 = endCheckFloat2;

                counter++;
            });
        }

        private static void ModifyVariablesPhase1(PlayMakerFSM fsm)
        {
            var healthManager = _bossObject.GetComponent<HealthManager>();
            healthManager.hp = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);
        }

        private static void ModifyVariablesPhase2(PlayMakerFSM fsm)
        {
            Utils.ChangeWaitFloat(fsm, "Spawn Fireball", 4, _modifiers.SpawnFireballWait);
            Utils.ChangeWaitFloat(fsm, "Orb Summon", 1, _modifiers.OrbSummonWait);
            Utils.ChangeSetIntValue(fsm, "Shot Antic", 4, 10);
        }

        private static void ModifyAnimationTimesPhase1(PlayMakerFSM fsm)
        {
            Utils.SpeedupAnimation(fsm, "Shot Antic", _modifiers.ShotAnticAnimation);


            Utils.ChangeWaitFloat(fsm, "Shot Antic", 0, _modifiers.ShotAnticWait);
            Utils.ChangeWaitFloat(fsm, "Shot", 10, _modifiers.ShotAnticWait);
        }

        private static void ModifyAnimationTimesPhase2(PlayMakerFSM fsm)
        {
            throw new NotImplementedException();
        }
    }
}
