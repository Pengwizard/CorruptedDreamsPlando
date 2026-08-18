using System;
using System.Collections;
using System.Collections.Generic;
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
using UnityEngine;
using static SceneLoad;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    internal class AnimationModifiersNkg
    {
        // Phase change variables


        // Spiral
        public float SpiralSpeed;
        public float SpiralInwardVel;
        public int SpiralShotCount;
        public int SpiralSwitchDirectionInterval;
        public List<float> SpiralShotDelay;
        public float SpawnOffset { get; set; }

        // Tracking shot
        public float TrackingShotXOffset;
        public float TrackingShotYOffsetStep;
        public float TrackingFireSpeed;
        public float TrackingShotDelay;
        public int TrackingShotLimit;

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

        private static Vector3 uppercutOffset = new Vector3(0, 0, 0);
        private static Vector3 ddarkOffset = new Vector3(0, 0, 0);

        private static int fireballsShotBalloon = 0;
        private static int fireballsShotTracker = 0;

        private static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {

            // Add shriek to uppercut
            // TODO: Consider removing uppercut
            var upperCutState = fsm.GetState("Uppercut Up");
            AbyssShriekSpawner.Instance.AddShriekAtEndOfFSM(upperCutState, 2, uppercutOffset, true);


            // Adding dive to landing attack
            var groundDashLand = fsm.GetState("GD Antic");
            DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(groundDashLand, 2, ddarkOffset);


            // Make balloon spirals
            SetupNewBalloonPhase(fsm);


            // Replace bats with fireballs:
            //  Detach the entire regular bats FSM flow
            //  Add 4 fireball spawners at 4 heights
            // 

            SetupNewBatsAttack(fsm);
        }

        public static void SetupAbyssShriekWave(PlayMakerFSM fsm)
        {

        }

        public static void SetupDDarkAttack(PlayMakerFSM fsm)
        {

        }

        public static void SetupGasterBlaster(PlayMakerFSM fsm)
        {

        }

        public static void SetupSimonSays(PlayMakerFSM fsm)
        {

        }

        public static void SetupNewBatsAttack(PlayMakerFSM fsm)
        {

            var fireballShooterHigh = _bossObject.AddComponent<FireballShooter>();
            //fireballShooterUp.name = "Up Shot";
            fireballShooterHigh.SpawnOffset = new Vector3(_modifiers.TrackingShotXOffset, _modifiers.TrackingShotYOffsetStep * 2, 0);

            var fireballShooterMiddleUp = _bossObject.AddComponent<FireballShooter>();
            //fireballShooterDown.name = "Down Shot";
            fireballShooterMiddleUp.SpawnOffset = new Vector3(_modifiers.TrackingShotXOffset, _modifiers.TrackingShotYOffsetStep, 0);

            var fireballShooterMiddleLow = _bossObject.AddComponent<FireballShooter>();
            //fireballShooterLeft.name = "Left Shot";
            fireballShooterMiddleLow.SpawnOffset = new Vector3(_modifiers.TrackingShotXOffset, -_modifiers.TrackingShotYOffsetStep, 0);

            var fireballShooterLow = _bossObject.AddComponent<FireballShooter>();
            //fireballShooterRight.name = "Right Shot";
            fireballShooterLow.SpawnOffset = new Vector3(_modifiers.TrackingShotXOffset, -(_modifiers.TrackingShotYOffsetStep * 2), 0);

            List<FireballShooter> shooterList = new() {
                fireballShooterHigh,
                fireballShooterMiddleUp,
                fireballShooterMiddleLow,
                fireballShooterLow
            };

            foreach (var shooter in shooterList)
            {
                // shooter.transform.SetParent(_bossObject.transform);
                shooter.SpawnPoint = Vector3.zero;

                // Common for all fireball shooters
                shooter.Trajectory = FlightTrajectory.TRACKER;
                shooter.RemoveParticles = true;

                shooter.Speed = _modifiers.TrackingFireSpeed;
                shooter.BaseObject = _bossObject;
            }

            // We are going to remove the existing 5 shots and re-work the flow.
            var fbCast = fsm.GetState("FB Cast");
            var fbBehind = fsm.GetState("FB Behind");
            var fbCastEnd = fsm.GetState("FB Cast End");

            var fireTrackingShot = fsm.AddState("CD Fireball Tracking Shot");
            var fireTrackingShotShort = fsm.AddState("CD Fireball Tracking Shot Short");
            var fireTrackingShotDelay = fsm.AddState("CD Fireball Tracking Shot Delay");

            fbCast.RemoveTransition("REPEAT");
            fbCast.RemoveTransition("FINISHED");

            fbCast.AddTransition("REPEAT", fireTrackingShotShort.Name);
            fbCast.AddTransition("FINISHED", fireTrackingShot.Name);


            fireTrackingShot.AddTransition("TRACKER END", fbCastEnd.Name);
            fireTrackingShot.AddTransition("TRACKER CONTINUE", fireTrackingShotDelay.Name);
            fireTrackingShot.AddTransition("BEHIND", fbBehind.Name);
            fireTrackingShotDelay.AddTransition("FINISHED", fireTrackingShot.Name);
            fireTrackingShotShort.AddTransition("FINISHED", fireTrackingShot.Name);

            fireTrackingShotDelay.AddAction(new Wait() { time = _modifiers.TrackingShotDelay, finishEvent = new("FINISHED") });

            List<Tuple<int, int>> firePatterns = new()
            {
                new(0, 2),
                new(0, 3),
                new(1, 2),
                new(1, 3)
            };

            fbCast.AddCustomAction(() =>
            {
                fireballsShotTracker = 0;
            });

            fireTrackingShotShort.AddCustomAction(() =>
            {
                fireballsShotTracker = _modifiers.TrackingShotLimit - 2;
            });

            fireTrackingShot.AddCustomAction(() =>
            {
                if (fireballsShotTracker >= _modifiers.TrackingShotLimit)
                {
                    fsm.SendEvent("TRACKER END");
                    return;
                }

                var fireballPatternIndex = UnityEngine.Random.Range(0, 4);
                var pat = firePatterns[fireballPatternIndex];
                shooterList[pat.Item1].ShootShadeSoul();
                shooterList[pat.Item2].ShootShadeSoul();
                fireballsShotTracker++;
                fsm.SendEvent("TRACKER CONTINUE");
            });
        }

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
            fireballShooterUp.SpawnOffset = new Vector3(0, _modifiers.SpawnOffset, 0);

            var fireballShooterDown = new FireballShooter();
            //fireballShooterDown.name = "Down Shot";
            fireballShooterDown.SpawnOffset = new Vector3(0, -_modifiers.SpawnOffset, 0);

            var fireballShooterLeft = new FireballShooter();
            //fireballShooterLeft.name = "Left Shot";
            fireballShooterLeft.SpawnOffset = new Vector3(_modifiers.SpawnOffset, 0, 0);

            var fireballShooterRight = new FireballShooter();
            //fireballShooterRight.name = "Right Shot";
            fireballShooterRight.SpawnOffset = new Vector3(-_modifiers.SpawnOffset, 0, 0);

            List<FireballShooter> shooterList = new() { fireballShooterUp, fireballShooterDown, fireballShooterLeft, fireballShooterRight };

            foreach (var shooter in shooterList)
            {
                // Common for all fireball shooters
                shooter.SpawnPoint = new Vector3(balloonBaseX, balloonBaseY, 0);
                shooter.Trajectory = FlightTrajectory.SPIRAL;
                shooter.RemoveParticles = true;

                shooter.Clockwise = true;

                shooter.Speed = _modifiers.SpiralSpeed;
                shooter.InwardVel = _modifiers.SpiralInwardVel;
            }

            var shootSpirals = fsm.AddState("Shade Repeat");
            var shootPause = fsm.AddState("Shade Pause");

            var firePause = fsm.GetState("Fire Pause");
            firePause.RemoveTransition("FINISHED");
            firePause.AddTransition("FINISHED", shootSpirals.Name);

            firePause.AddCustomAction(() =>
            {
                fireballsShotBalloon = 0;
            });


            fsm.AddVariable<FsmFloat>("ShootWait");
            var shootWait = fsm.GetVariable<FsmFloat>("ShootWait");

            shootPause.AddTransition("FINISHED", shootSpirals.Name);
            shootPause.AddCustomAction(() =>
            {
                shootWait.Value = _modifiers.SpiralShotDelay[fireballsShotBalloon % _modifiers.SpiralShotDelay.Count];
                if (fireballsShotBalloon % _modifiers.SpiralSwitchDirectionInterval == 0)
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
                if (fireballsShotBalloon >= _modifiers.SpiralShotCount)
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

            #endregion
        }
    }
}
