using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IL;
using ItemChanger;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using UnityEngine;
using static SceneLoad;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    internal class BalloonAttackPattern
    {
        public float SpiralSpeed;
        public float SpiralInwardVel;
        public int SpiralShotCount;
        public int SpiralSwitchDirectionInterval;
        public List<float> SpiralShotDelay;
    }

    internal class GhasterBlasterPattern
    {

    }

    internal class AnimationModifiersNkg
    {
        // Shriek Wave
        public int ShriekSpawnerCount;
        public float ShriekSpawnerFirstX;
        public float ShriekSpawnerLastX;
        public float ShriekOffsetY;
        public float ShriekSpawnDelay;

        // Spiral
        public List<BalloonAttackPattern> BalloonPatterns;
        public float SpiralSpawnOffset;

        // Ghaster Blaster
        public float BlasterInitialDistance;
        public int BlasterCount;
        public float BlasterSpeed;
        public float BlasterTelegraphDelay;

        // Numbers for Phase Change bit
        public float RoarWaitTime;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate;
        public float PhaseChangeHpMultiplier;


        public static AnimationModifiersNkg FromJsonFile()
        {
            var path = "C:\\clean\\hkMods\\CorruptedDreamsPlando\\CorruptedDreamsPlando\\Resources\\bossVariables\\corruptedNKGModifiers.json";

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                AnimationModifiersNkg items = JsonConvert.DeserializeObject<AnimationModifiersNkg>(json);
                return items;
            }
        }
    }

    public class CorruptedNKG
    {
        // TODO:

        private static readonly FsmID controlFSM = new("Nightmare Grimm Boss", "Control");

        private static bool phaseChanged;
        private static GameObject _bossObject;
        private static int _startingHp;
        private static int _phase1Hp;
        private static AnimationModifiersNkg _modifiers;


        public static void Initialize()
        {
            Events.AddFsmEdit(controlFSM, SetupPhaseChange);
        }

        public static void Unload()
        {
            Events.RemoveFsmEdit(controlFSM, SetupPhaseChange);
        }

        public static void UnpackModifiers()
        {
            _modifiers = AnimationModifiersNkg.FromJsonFile();
        }

        public static FsmOwnerDefault GoToOwnerDefault(GameObject go)
        {
            return new FsmOwnerDefault()
            {
                GameObject = new FsmGameObject { Value = go },
                OwnerOption = OwnerDefaultOption.SpecifyGameObject
            };
        }

        // Enter "PC Pillar Retry", exit "Roar End"

        private static AudioPlayerOneShotSingle GetClipState(FsmObject audioClip, float delay=0f, float volume=5f)
        {
            var grimmControl = GameObject.Find("Grimm Control");
            var grimmControlFsm = grimmControl.LocateMyFSM("Control");
            var eye2 = grimmControlFsm.GetValidState("Eye 2");
            var eye2PlayOneShot = eye2.GetFirstActionOfType<AudioPlayerOneShotSingle>();


            return new AudioPlayerOneShotSingle()
            {
                spawnPoint = eye2PlayOneShot.spawnPoint,
                audioPlayer = eye2PlayOneShot.audioPlayer,
                audioClip = audioClip,
                volume = 5f,
                storePlayer = eye2PlayOneShot.storePlayer,
                delay = 0,
            };
        }

        private static void SetupPhaseChangeAnimationSequence(PlayMakerFSM fsm, string stateAfterRoar)
        {

            var grimmControl = GameObject.Find("Grimm Control");
            var grimmControlFsm = grimmControl.LocateMyFSM("Control");

            var eye2 = grimmControlFsm.GetState("Eye 2");
            var eye2PlayOneShot = eye2.GetFirstActionOfType<AudioPlayerOneShotSingle>();
            var cameraShake = eye2.GetFirstActionOfType<SendEventByName>();

            var myRetry = fsm.CopyState("Pillar Retry", "PC Pillar Retry");
            var myPos = fsm.CopyState("Pillar Pos", "PC Pillar Pos");
            var myTeleIn = fsm.CopyState("Pillar Tele In", "PC Pillar Tele In");
            var myAntic = fsm.CopyState("Pillar Antic", "PC Pillar Antic");

            var roarState = fsm.AddState("Roar");
            var roarEndState = fsm.AddState("Roar End");

            myTeleIn.GetFirstActionOfType<SetPosition>().x = 75;

            myRetry.RemoveTransition("FINISHED");
            myPos.RemoveTransition("FINISHED");
            myPos.RemoveTransition("CANCEL");
            myTeleIn.RemoveTransition("FINISHED");
            myAntic.RemoveTransition("FINISHED");

            myRetry.AddTransition("FINISHED", myPos.Name);
            myPos.AddTransition("FINISHED", myTeleIn.Name);
            myPos.AddTransition("CANCEL", myRetry.Name);
            myTeleIn.AddTransition("FINISHED", myAntic.Name);
            myAntic.AddTransition("FINISHED", roarState.Name);
            roarState.AddTransition("FINISHED", roarEndState.Name);
            roarEndState.AddTransition("FINISHED", stateAfterRoar);

            //var roarWaveEmitter = GameObject.Find("Roar Wave Emitter");


            // AudioPlayerOneShotSingle
            roarState.AddAction(new AudioPlayerOneShotSingle()
            {
                spawnPoint = eye2PlayOneShot.spawnPoint,
                audioPlayer = eye2PlayOneShot.audioPlayer,
                audioClip = eye2PlayOneShot.audioClip,
                volume = 5f,
                storePlayer = eye2PlayOneShot.storePlayer,
                delay = 0,
            });

            roarState.AddAction(new SendEventByName()
            {
                eventTarget = cameraShake.eventTarget,
                sendEvent = cameraShake.sendEvent,
                everyFrame = cameraShake.everyFrame,
                delay = 0,
            });

            roarState.AddAction(new AudioPlayerOneShotSingle()
            {
                spawnPoint = eye2PlayOneShot.spawnPoint,
                audioPlayer = eye2PlayOneShot.audioPlayer,
                audioClip = eye2PlayOneShot.audioClip,
                volume = 5f,
                storePlayer = eye2PlayOneShot.storePlayer,
                delay = 1,
            });

            roarState.AddAction(new SendEventByName()
            {
                eventTarget = cameraShake.eventTarget,
                sendEvent = cameraShake.sendEvent,
                everyFrame = cameraShake.everyFrame,
                delay = 1,
            });

            roarState.AddAction(new AudioPlayerOneShotSingle()
            {
                spawnPoint = eye2PlayOneShot.spawnPoint,
                audioPlayer = eye2PlayOneShot.audioPlayer,
                audioClip = eye2PlayOneShot.audioClip,
                volume = 5f,
                storePlayer = eye2PlayOneShot.storePlayer,
                delay = 2,
            });

            roarState.AddAction(new SendEventByName()
            {
                eventTarget = cameraShake.eventTarget,
                sendEvent = cameraShake.sendEvent,
                everyFrame = cameraShake.everyFrame,
                delay = 2,
            });

            roarState.AddAction(new AudioPlayerOneShotSingle()
            {
                spawnPoint = eye2PlayOneShot.spawnPoint,
                audioPlayer = eye2PlayOneShot.audioPlayer,
                audioClip = eye2PlayOneShot.audioClip,
                volume = 5f,
                storePlayer = eye2PlayOneShot.storePlayer,
                delay = 3,
            });

            roarState.AddAction(new SendEventByName()
            {
                eventTarget = cameraShake.eventTarget,
                sendEvent = cameraShake.sendEvent,
                everyFrame = cameraShake.everyFrame,
                delay = 3,
            });

            // TODO: Remove music during phase change, and hit with new music

            var appearing = GameObject.Find("Grimm Control/Appear Scene/Grimm Appear");

            roarState.AddCustomAction(() =>
            {
                appearing.transform.position = fsm.gameObject.transform.position;
                appearing.transform.SetPositionY(17.2f);
                appearing.transform.SetPositionX(73.3f);
                appearing.SetActive(true);
            });

            roarState.AddAction(new SetFsmGameObject()
            {
                gameObject = GoToOwnerDefault(Utils.Hero()),
                fsmName = "Roar Lock",
                variableName = "Roar Object",
                setValue = fsm.gameObject
            });

            roarState.AddCustomAction(() =>
            {
                Utils.Hero().LocateMyFSM("Roar Lock").SendEvent("ROAR ENTER");
            });

            roarState.AddAction(new Wait() { time = _modifiers.RoarWaitTime, finishEvent = new("FINISHED") });

            //roarEndState.AddCustomAction(() =>
            //{
            //    var roarEmitter = fsm.GetVariable<FsmGameObject>("Roar Emitter");
            //    roarEmitter.Value.LocateMyFSM("emitter").SendEvent("END"); 
            //});

            roarEndState.AddCustomAction(() =>
            {
                Utils.Hero().LocateMyFSM("Roar Lock").SendEvent("ROAR EXIT");
                appearing.SetActive(false);
            });
        }

        private static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Changing NKG");
            phaseChanged = false;

            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            SetupPhaseChangeAnimationSequence(fsm, "Tele Out");
            SetupTitleScream(fsm);

            var rageRoarState = fsm.GetState("Roar");
            Satchel.FsmUtil.AddFirstAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

            var roarEndPhase = fsm.GetState("Roar End");
            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

            // Rage starts at "4+" state after modifications
            // Can modify this from the start i guess

            FsmState rageState = SetupRageSetupState(fsm, "PC Pillar Pos");

            AddPhaseChangeStep(fsm, new() { "Balloon?", "Move Choice" }, rageState);
        }

        public static void SetupTitleScream(PlayMakerFSM fsm)
        {
            FsmEditTemplates.FullScreenTitleDrop.SetupTitleUp(
                fsm,
                "Soul Symposer Grimm",
                "Roar",
                "Roar End",
                _modifiers.TitleFadeUpDelay,
                _modifiers.TitleFadeDownDelay,
                _modifiers.WhiteSolidUpTime,
                _modifiers.WhiteSolidUpDelay,
                UnityEngine.Color.black,
                UnityEngine.Color.white,
                "FINISHED",
                null,
                "");
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
                if (!phaseChanged)
                {
                    //var rageRoarState = fsm.GetState("Roar");
                    //rageRoarState.GetFirstActionOfType<Wait>().time = _modifiers.RoarWaitTime;

                    var healthManager = _bossObject.GetComponent<HealthManager>();
                    healthManager.hp = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);
                    CorruptedDreamsPlandoMod.Instance.Log("Raging");
                    Grayscaler.TickCamera();

                    ModifyAttackPatterns(fsm);
                    ModifyVariables(fsm);
                    ModifyAnimationTimes(fsm);

                    phaseChanged = true;
                }
            });

            return rageCheckState;
        }

        public static void AddPhaseChangeStep(
            PlayMakerFSM fsm,
            List<string> statesToAddCheck,
            FsmState rageSetupState)
        {
            var healthManager = _bossObject.GetComponent<HealthManager>();
            _startingHp = healthManager.hp;
            healthManager.hp = 9999;
            _phase1Hp = 10; // (int)(_startingHp * _modifiers.PhaseChangeHpGate);
            var phaseTransitionGate = healthManager.hp - _phase1Hp;
            CorruptedDreamsPlandoMod.Instance.Log($"Setting up phase change, will trigger at {phaseTransitionGate} hp");

            string setupRageTransition = "SETUP RAGE";

            foreach (string stateToHookName in statesToAddCheck)
            {
                var stateToHookTransition = fsm.GetState(stateToHookName);
                //stateToHookTransition.RemoveTransition(transitionToHook);
                //stateToHookTransition.AddTransition(transitionToHook, rageCheckStateName);

                stateToHookTransition.AddTransition(setupRageTransition, rageSetupState.Name);
                //rageCheckState.AddTransition(defaultTransition, stateIfNotRageName);

                stateToHookTransition.InsertCustomAction(() =>
                {
                    var healthManager = _bossObject.GetComponent<HealthManager>();
                    var currHP = healthManager.hp;
                    if (currHP < phaseTransitionGate && !phaseChanged)
                    {
                        fsm.SendEvent(setupRageTransition);
                        //phaseChanged = true;
                    }
                }, 0);
            }
        }


        private static void ModifyVariables(PlayMakerFSM fsm)
        {
            //throw new NotImplementedException();
        }

        private static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
            //throw new NotImplementedException();
        }

        // Balloon variables
        private static int fireballsShotBalloon = 0;

        // Blaster variables
        private static List<FireballShooter> blasterSpawners = new List<FireballShooter>();

        private static float groundY = 9.6f;

        // Shriek Wave Variables
        private static int shriekDirectionStep = 0;
        private static int shriekCurrentIndex = 0;
        private static List<GameObject> shriekSpawners = new List<GameObject>();


        // Move options
        private static bool isFirstMove = true;
        private static List<string> moveOptions = new();

        private static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {

            //// Add shriek to uppercut
            //// TODO: Consider removing uppercut
            //var upperCutState = fsm.GetState("Uppercut Up");
            //AbyssShriekSpawner.Instance.AddShriekAtEndOfFSM(upperCutState, 2, uppercutOffset, true);


            //// Adding dive to landing attack
            //var groundDashLand = fsm.GetState("GD Antic");
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(groundDashLand, 2, ddarkOffset);

            SetupMoveChoice(fsm);

            // Make balloon spirals
            SetupNewBalloonPhase(fsm);


            SetupAbyssShriekWave(fsm);
        }

        public static void SetupMoveChoice(PlayMakerFSM fsm)
        {
            // Disabling the phase transition balloon, making it a regular move
            // TODO: We might make it again later, but we want more steps and different patterns and stuff so...
            var rageBalloon = fsm.GetValidState("Balloon?");
            rageBalloon.RemoveTransition("BALLOON 1");
            rageBalloon.RemoveTransition("BALLOON 2");
            rageBalloon.RemoveTransition("BALLOON 3");
            foreach (var act in rageBalloon.Actions)
            {
                // Also just disabling all the actions in here.
                act.Enabled = false;
            }

            // We are keeping none of the original moves essentially, so we just remove all the move choice options, and then re-add them, except balloon
            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.RemoveTransition("FIREBATS");
            moveChoice.RemoveTransition("SLASH");
            moveChoice.RemoveTransition("AIR DASH");
            moveChoice.RemoveTransition("SPIKES");
            moveChoice.RemoveTransition("PILLARS");
            moveChoice.RemoveTransition("BALLOON");

            moveChoice.GetAction(0).Enabled = false;
            moveChoice.GetAction(1).Enabled = false;
            moveChoice.GetAction(2).Enabled = false;

            // TODO: When adding telegraph, add a move counter, and after X moves do telegraph.
            moveChoice.AddCustomAction(() =>
            {
                var nextMove = "";
                if (isFirstMove)
                {
                    nextMove = "BALLOON SPIRAL";
                    isFirstMove = false;
                }
                else
                {
                    var moveIndex = UnityEngine.Random.Range(0, moveOptions.Count);
                    //nextMove = moveOptions[moveIndex];
                    nextMove = "SHRIEK WAVE";
                }

                CorruptedDreamsPlandoMod.Instance.Log($"Move Choice {nextMove}");

                fsm.SendEvent(nextMove);
            });
        }

        public static void SetupAbyssShriekWave(PlayMakerFSM fsm)
        {
            var totalSpread = _modifiers.ShriekSpawnerLastX - _modifiers.ShriekSpawnerFirstX;
            float interval = totalSpread / (_modifiers.ShriekSpawnerCount - 1);

            shriekSpawners.Clear();
            for (int i = 0; i < _modifiers.ShriekSpawnerCount; ++i)
            {
                var xPos = _modifiers.ShriekSpawnerFirstX + (i * interval);
                var newShriek = AbyssShriekSpawner.Instance.CreateShriekSpawner(2, new Vector3(xPos, groundY + _modifiers.ShriekOffsetY, 0));
                shriekSpawners.Add(newShriek);
            }

            var shriekWaveStart = fsm.AddState("Shriek Wave Start");
            var shriekWaveStep = fsm.AddState("Shriek Wave Step");
            var shriekWavePause = fsm.AddState("Shriek Wave Pause");
            var shriekWaveCleanup = fsm.AddState("Shriek Wave Cleanup");

            shriekWaveStart.AddCustomAction(() =>
            {
                var direction = UnityEngine.Random.Range(0, 2);
                if (direction == 0)
                {
                    // Going right, index goes up
                    shriekDirectionStep = 1;
                    shriekCurrentIndex = 0;

                } else
                {
                    // Going left
                    shriekDirectionStep = -1;
                    shriekCurrentIndex = shriekSpawners.Count - 1;
                }

            });
            Satchel.FsmUtil.AddAction(shriekWaveStart, GetClipState(AbyssShriekSpawner.ShriekAudio));
            shriekWaveStart.AddTransition("FINISHED", shriekWaveStep.Name);


            Satchel.FsmUtil.AddAction(shriekWaveStep, GetClipState(AbyssShriekSpawner.ShriekAudio, 0f, 3f));

            shriekWaveStep.AddCustomAction(() =>
            {
                // This is always a valid 
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Step 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                if (0 <= shriekCurrentIndex && shriekCurrentIndex <= shriekSpawners.Count - 1)
                {
                    shriekSpawners[shriekCurrentIndex].SetActive(true);
                }
            });
            shriekWaveStep.AddAction(new Wait() { time = _modifiers.ShriekSpawnDelay, finishEvent = new("FINISHED") });
            shriekWaveStep.AddTransition("FINISHED", shriekWavePause.Name);

            shriekWavePause.AddCustomAction(() =>
            {
                // This is always a valid 
                // Note that this check is just <, not <=, because the one above includes the last step
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Pause 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                shriekCurrentIndex += shriekDirectionStep;
                if (0 <= shriekCurrentIndex && shriekCurrentIndex <= shriekSpawners.Count - 1)
                {
                    fsm.SendEvent("SHRIEK WAVE CONTINUE");
                } else
                {
                    fsm.SendEvent("SHRIEK WAVE DONE");
                }
            });
            shriekWavePause.AddTransition("SHRIEK WAVE CONTINUE", shriekWaveStep.Name);
            shriekWavePause.AddTransition("SHRIEK WAVE DONE", shriekWaveCleanup.Name);

            // Cleanup step is currently doing nothing
            shriekWaveCleanup.AddAction(new Wait() { time = 1f, finishEvent = new("FINISHED") });
            shriekWaveCleanup.AddTransition("FINISHED", "Move Choice");


            // Now we need to insert this flow into the regular attacks flow.
            // For shriek wave, grimm won't be on screen, so no need to bring him in.

            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.AddTransition("SHRIEK WAVE", shriekWaveStart.Name);

            moveOptions.Add("SHRIEK WAVE");
        }

        public static void SetupDDarkAttack(PlayMakerFSM fsm)
        {

        }

        public static void SetupGasterBlaster(PlayMakerFSM fsm)
        {
            // Step 1 - figure out the amount and distance and angle we want these to be from the knight
            float angleOfFireIntervals = 360.0f / (_modifiers.BlasterCount - 1);

            blasterSpawners.Clear();
            for (int i = 0; i < _modifiers.BlasterCount; ++i)
            {
                var angle = i * angleOfFireIntervals;

                // when i = 0, all the offset should be negative on X
                float xOffset = 0;
                float yOffset = 0;

                var fireballShooter = new FireballShooter();
                //fireballShooterUp.name = "Up Shot";
                fireballShooter.SpawnOffset = new Vector3(xOffset, yOffset, 0);
                fireballShooter.BaseObject = Utils.Hero();
                fireballShooter.Direction = angle;
                fireballShooter.ForceLeft = false;
                fireballShooter.Speed = _modifiers.BlasterSpeed;
                fireballShooter.Trajectory = FlightTrajectory.STRAIGHT;

                blasterSpawners.Add(fireballShooter);
            }

            var shriekWaveStart = fsm.AddState("Shriek Wave Start");
            var shriekWaveStep = fsm.AddState("Shriek Wave Step");
            var shriekWavePause = fsm.AddState("Shriek Wave Pause");
            var shriekWaveCleanup = fsm.AddState("Shriek Wave Cleanup");

            shriekWaveStart.AddCustomAction(() =>
            {
                var direction = UnityEngine.Random.Range(0, 2);
                if (direction == 0)
                {
                    // Going right, index goes up
                    shriekDirectionStep = 1;
                    shriekCurrentIndex = 0;

                }
                else
                {
                    // Going left
                    shriekDirectionStep = -1;
                    shriekCurrentIndex = shriekSpawners.Count - 1;
                }

            });
            Satchel.FsmUtil.AddAction(shriekWaveStart, GetClipState(AbyssShriekSpawner.ShriekAudio));
            shriekWaveStart.AddTransition("FINISHED", shriekWaveStep.Name);


            Satchel.FsmUtil.AddAction(shriekWaveStep, GetClipState(AbyssShriekSpawner.ShriekAudio, 0f, 3f));

            shriekWaveStep.AddCustomAction(() =>
            {
                // This is always a valid 
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Step 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                if (0 <= shriekCurrentIndex && shriekCurrentIndex <= shriekSpawners.Count - 1)
                {
                    shriekSpawners[shriekCurrentIndex].SetActive(true);
                }
            });
            shriekWaveStep.AddAction(new Wait() { time = _modifiers.ShriekSpawnDelay, finishEvent = new("FINISHED") });
            shriekWaveStep.AddTransition("FINISHED", shriekWavePause.Name);

            shriekWavePause.AddCustomAction(() =>
            {
                // This is always a valid 
                // Note that this check is just <, not <=, because the one above includes the last step
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Pause 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                shriekCurrentIndex += shriekDirectionStep;
                if (0 <= shriekCurrentIndex && shriekCurrentIndex <= shriekSpawners.Count - 1)
                {
                    fsm.SendEvent("SHRIEK WAVE CONTINUE");
                }
                else
                {
                    fsm.SendEvent("SHRIEK WAVE DONE");
                }
            });
            shriekWavePause.AddTransition("SHRIEK WAVE CONTINUE", shriekWaveStep.Name);
            shriekWavePause.AddTransition("SHRIEK WAVE DONE", shriekWaveCleanup.Name);

            // Cleanup step is currently doing nothing
            shriekWaveCleanup.AddAction(new Wait() { time = 1f, finishEvent = new("FINISHED") });
            shriekWaveCleanup.AddTransition("FINISHED", "Move Choice");


            // Now we need to insert this flow into the regular attacks flow.
            // For shriek wave, grimm won't be on screen, so no need to bring him in.

            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.AddTransition("SHRIEK WAVE", shriekWaveStart.Name);

            moveOptions.Add("SHRIEK WAVE");
        }

        public static void SetupSimonSays(PlayMakerFSM fsm)
        {

        }

        private static BalloonAttackPattern currentPattern;

        public static void SetupNewBalloonPhase(PlayMakerFSM fsm)
        {
            // TODO fix hp gates for fireballs.
            #region balloon
            float balloonBaseX = 85.77f;
            float balloonBaseY = 13.5f;

            var inflateState = fsm.GetState("Inflate");

            // This usually makes NKG vulnerable to spells while ballooned. Lets disable that.
            inflateState.GetFirstActionOfType<SetTag>().Enabled = false;

            var fireballShooterUp = new FireballShooter();
            //fireballShooterUp.name = "Up Shot";
            fireballShooterUp.SpawnOffset = new Vector3(0, _modifiers.SpiralSpawnOffset, 0);

            var fireballShooterDown = new FireballShooter();
            //fireballShooterDown.name = "Down Shot";
            fireballShooterDown.SpawnOffset = new Vector3(0, -_modifiers.SpiralSpawnOffset, 0);

            var fireballShooterLeft = new FireballShooter();
            //fireballShooterLeft.name = "Left Shot";
            fireballShooterLeft.SpawnOffset = new Vector3(_modifiers.SpiralSpawnOffset, 0, 0);

            var fireballShooterRight = new FireballShooter();
            //fireballShooterRight.name = "Right Shot";
            fireballShooterRight.SpawnOffset = new Vector3(-_modifiers.SpiralSpawnOffset, 0, 0);

            List<FireballShooter> shooterList = new() { fireballShooterUp, fireballShooterDown, fireballShooterLeft, fireballShooterRight };

            foreach (var shooter in shooterList)
            {
                // Common for all fireball shooters
                shooter.SpawnPoint = new Vector3(balloonBaseX, balloonBaseY, 0);
                shooter.Trajectory = FlightTrajectory.SPIRAL;
                shooter.RemoveParticles = true;

                shooter.Clockwise = true;

                shooter.Speed = 0;
                shooter.InwardVel = 0;
            }

            var shootSpirals = fsm.AddState("Shade Repeat");
            var shootPause = fsm.AddState("Shade Pause");

            var firePause = fsm.GetState("Fire Pause");
            firePause.RemoveTransition("FINISHED");
            firePause.AddTransition("FINISHED", shootSpirals.Name);

            firePause.AddCustomAction(() =>
            {
                fireballsShotBalloon = 0;
                var patternIndex = UnityEngine.Random.Range(0, _modifiers.BalloonPatterns.Count);
                currentPattern = _modifiers.BalloonPatterns[patternIndex];

                foreach (var shooter in shooterList)
                {
                    shooter.Speed = currentPattern.SpiralSpeed;
                    shooter.InwardVel = currentPattern.SpiralInwardVel;
                }
            });


            fsm.AddVariable<FsmFloat>("ShootWait");
            var shootWait = fsm.GetVariable<FsmFloat>("ShootWait");

            shootPause.AddTransition("FINISHED", shootSpirals.Name);
            shootPause.AddCustomAction(() =>
            {
                shootWait.Value = currentPattern.SpiralShotDelay[fireballsShotBalloon % currentPattern.SpiralShotDelay.Count];
                if (fireballsShotBalloon % currentPattern.SpiralSwitchDirectionInterval == 0)
                {
                    foreach (var shooter in shooterList)
                    {
                        shooter.Clockwise = !shooter.Clockwise;

                    }
                }
            });
            shootPause.AddAction(new Wait() { time = shootWait, finishEvent = new("FINISHED") });

            shootSpirals.AddTransition("CONTINUE", shootPause.Name);
            shootSpirals.AddTransition("END", "End Pause");

            shootSpirals.AddCustomAction(() =>
            {
                if (fireballsShotBalloon >= currentPattern.SpiralShotCount)
                {
                    fsm.SendEvent("END");
                }
                else
                {
                    fireballsShotBalloon++;

                    foreach (var shooter in shooterList)
                    {
                        shooter.ShootShadeSoul();
                    }

                    fsm.SendEvent("CONTINUE");
                }


            });

            var balloonPos = fsm.GetValidState("Balloon Pos");
            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.AddTransition("BALLOON SPIRAL", balloonPos.Name);
            moveOptions.Add("BALLOON SPIRAL");

            #endregion
        }
    }
}
