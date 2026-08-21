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
        public float BlasterTelegraphDelayMod;
        public float BlasterShotDelay;
        public float BlasterShotScale;

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
                gameObject = Utils.GameObjectToOwnerDefault(Utils.Hero()),
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

        private static float groundY = 9.6f;
        private static float actualGroundY = 6.4f;

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

            SetupNewBalloonPhase(fsm);

            SetupGasterBlaster(fsm);

            SetupDDarkAttack(fsm);

            SetupAbyssShriekWave(fsm);
        }
        
        private static void CleanupGameObjectList(List<GameObject> gos)
        {
            foreach (var go in gos)
            {
                GameObject.Destroy(go);
            }

            gos.Clear();
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
                    nextMove = moveOptions[moveIndex];
                    //nextMove = "DDARK PILLARS";
                }

                CorruptedDreamsPlandoMod.Instance.Log($"Move Choice {nextMove}");

                fsm.SendEvent(nextMove);
            });
        }


        // Shriek Wave Variables
        private static int shriekDirectionStep = 0;
        private static int shriekCurrentIndex = 0;
        private static List<GameObject> shriekSpawners = new List<GameObject>();

        public static GameObject leftScreenTelegraph;
        public static GameObject rightScreenTelegraph;

        public static void SetupTelegraph(GameObject telegraphObject, Vector3 position, Vector3 finalScale, float time)
        {
            //leftScreenTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);
            telegraphObject.transform.SetPosition3D(position.x, position.y, position.z);
            telegraphObject.transform.SetScaleX(1f);
            telegraphObject.transform.SetScaleY(1f);
            var lightPulse = telegraphObject.LocateMyFSM("Light Pulse");
            var st1 = lightPulse.GetValidState("State 1");
            var scaler = st1.GetFirstActionOfType<iTweenScaleBy>();
            scaler.loopType = iTween.LoopType.none;
            scaler.time = time;
            scaler.easeType = iTween.EaseType.linear;
            scaler.vector = finalScale;

            lightPulse.AddState("Deactivate");
            var de = lightPulse.GetValidState("Deactivate");
            de.AddCustomAction(() =>
            {
                telegraphObject.transform.SetScaleX(1f);
                telegraphObject.transform.SetScaleY(1f);

                telegraphObject.SetActive(false);
            });

            st1.AddTransition("FINISHED", de.Name);

            telegraphObject.SetActive(false);
        }

        public static void SetupAbyssShriekWave(PlayMakerFSM fsm)
        {
            // TODO move to modifiers
            float telegraph = 0.4f;

            leftScreenTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);
            rightScreenTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);
            SetupTelegraph(leftScreenTelegraph, new Vector3(69f, 11f, 0.52f), new Vector3(5, 5, 1), telegraph);
            SetupTelegraph(rightScreenTelegraph, new Vector3(101f, 11f, 0.52f), new Vector3(5, 5, 1), telegraph);
            //leftScreenTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);


            var totalSpread = _modifiers.ShriekSpawnerLastX - _modifiers.ShriekSpawnerFirstX;
            float interval = totalSpread / (_modifiers.ShriekSpawnerCount - 1);

            CleanupGameObjectList(shriekSpawners);

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
                    leftScreenTelegraph.SetActive(true);
                }
                else
                {
                    // Going left
                    shriekDirectionStep = -1;
                    shriekCurrentIndex = shriekSpawners.Count - 1;
                    rightScreenTelegraph.SetActive(true);
                }

            });

            shriekWaveStart.AddAction(GetClipState(AbyssShriekSpawner.ShriekAudio));
            shriekWaveStart.AddAction(new Wait() { time = telegraph, finishEvent = new("FINISHED") });
            shriekWaveStart.AddTransition("FINISHED", shriekWaveStep.Name);


            shriekWaveStep.AddAction(GetClipState(AbyssShriekSpawner.ShriekAudio));

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

        // DDark Attack Variables
        private static List<GameObject> ddarkTelegraphs = new List<GameObject>();
        private static List<GameObject> ddarkContainers = new List<GameObject>();
        private static int ddarkCurrentIndex = 0;

        // TODO move to modifiers
        private static float DDarkTelegraph = 0.4f;
        private static int DDarkCount = 4;
        private static Vector3 DDarkFinalScale = new Vector3(1f, 5, 1);

        public static void SetupDDarkAttack(PlayMakerFSM fsm)
        {
            // Have beam of light telegraph
            // also need to restructure the ddark thing so you get the spawner i think?

            CleanupGameObjectList(ddarkTelegraphs);
            CleanupGameObjectList(ddarkContainers);

            for (int i = 0; i < DDarkCount; i++)
            {
                var ddarkTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);
                SetupTelegraph(ddarkTelegraph, Vector3.zero, DDarkFinalScale, DDarkTelegraph);

                var lightPulse = ddarkTelegraph.LocateMyFSM("Light Pulse");
                var de = lightPulse.GetValidState("Deactivate");

                var diveContainer = DescendingDarkSpawner.Instance.GetDDarkSpawner(
                    2,
                    Utils.Hero(),
                    actualGroundY, DDarkTelegraph, GetClipState(DescendingDarkSpawner.DDarkAudio));

                //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(de, 2, Vector3.zero);

                //de.AddAction(GetClipState(DescendingDarkSpawner.DDarkAudio));

                ddarkTelegraphs.Add(ddarkTelegraph);
                ddarkContainers.Add(diveContainer);
            }

            var ddarkPillarsStart = fsm.AddState("DDark Pillars Start");
            var ddarkPillarsStep = fsm.AddState("DDark Pillars Step");
            var ddarkPillarsPause = fsm.AddState("DDark Pillars Pause");
            var ddarkPillarsCleanup = fsm.AddState("DDark Pillars Cleanup");

            ddarkPillarsStart.AddCustomAction(() =>
            {
                ddarkCurrentIndex = 0;
            });

            ddarkPillarsStart.AddTransition("FINISHED", ddarkPillarsStep.Name);


            ddarkPillarsStep.AddCustomAction(() =>
            {
                // This is always a valid 
                CorruptedDreamsPlandoMod.Instance.Log($"ddark {ddarkCurrentIndex}");
                if (ddarkCurrentIndex < ddarkTelegraphs.Count)
                {
                    ddarkTelegraphs[ddarkCurrentIndex].transform.SetPositionAndRotation(Utils.Hero().transform.position, Quaternion.identity);
                    ddarkTelegraphs[ddarkCurrentIndex].transform.SetPositionY(actualGroundY);

                    ddarkTelegraphs[ddarkCurrentIndex].SetActive(true);

                    ddarkContainers[ddarkCurrentIndex].SetActive(true);
                }
            });

            //ddarkPillarsStep.AddAction(GetClipState(DescendingDarkSpawner.DDarkAudio, DDarkTelegraph));
            ddarkPillarsStep.AddAction(new Wait() { time = DDarkTelegraph * 3, finishEvent = new("FINISHED") });
            ddarkPillarsStep.AddTransition("FINISHED", ddarkPillarsPause.Name);

            ddarkPillarsPause.AddCustomAction(() =>
            {
                // This is always a valid 
                // Note that this check is just <, not <=, because the one above includes the last step
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Pause 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                ddarkContainers[ddarkCurrentIndex].SetActive(false);
                ddarkCurrentIndex++;

                if (ddarkCurrentIndex < ddarkTelegraphs.Count)
                {
                    fsm.SendEvent("DDARK PILLARS CONTINUE");
                }
                else
                {
                    fsm.SendEvent("DDARK PILLARS DONE");
                }
            });
            ddarkPillarsPause.AddTransition("DDARK PILLARS CONTINUE", ddarkPillarsStep.Name);
            ddarkPillarsPause.AddTransition("DDARK PILLARS DONE", ddarkPillarsCleanup.Name);

            // Cleanup step is currently doing nothing
            ddarkPillarsCleanup.AddAction(new Wait() { time = 1f, finishEvent = new("FINISHED") });

            ddarkPillarsCleanup.AddTransition("FINISHED", "Move Choice");

            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.AddTransition("DDARK PILLARS", ddarkPillarsStart.Name);

            moveOptions.Add("DDARK PILLARS");
        }

        private static List<GameObject> blasterSpawners = new List<GameObject>();
        private static int blasterDirection = 0;
        private static int blasterCurrentIndex = 0;
        private static int blasterFireCount = 0;

        public static void SetupGasterBlaster(PlayMakerFSM fsm)
        {
            var BlasterCount = _modifiers.BlasterCount;
            var BlasterSpeed = _modifiers.BlasterSpeed;
            var BlasterShotDelay = _modifiers.BlasterShotDelay;
            var BlasterTelegraph = BlasterShotDelay * _modifiers.BlasterTelegraphDelayMod;
            var BlasterBaseOffset = -_modifiers.BlasterInitialDistance;
            var BlasterShotScale = _modifiers.BlasterShotScale;

            float angleOfFireIntervals = 360.0f / (BlasterCount);

            CleanupGameObjectList(blasterSpawners);

            for (int i = 0; i < BlasterCount; ++i)
            {
                var angle = i * angleOfFireIntervals;
                var rds = angle * Mathf.Deg2Rad;

                float xOffset = Mathf.Cos(rds) * BlasterBaseOffset;
                float yOffset = Mathf.Sin(rds) * BlasterBaseOffset;

                var go = new GameObject();

                var fireballShooter = go.AddComponent<FireballShooter>();

                go.name = $"Blaster Circle ({angle})";

                //fireballShooterUp.name = "Up Shot";
                fireballShooter.SpawnOffset = new Vector3(xOffset, yOffset, 0);
                fireballShooter.BaseObject = Utils.Hero();
                fireballShooter.Direction = angle;
                fireballShooter.ForceLeft = true;
                fireballShooter.Speed = BlasterSpeed;
                fireballShooter.ShotScale = BlasterShotScale;
                fireballShooter.PreFireDelay = BlasterTelegraph;
                fireballShooter.RemoveParticles = true;
                CorruptedDreamsPlandoMod.Instance.Log("Made fireball shooter");

                blasterSpawners.Add(go);
            }

            var blasterCircleStart = fsm.AddState("Blaster Circle Start");
            var blasterCircleStep = fsm.AddState("Blaster Circle Step");
            var blasterCirclePause = fsm.AddState("Blaster Circle Pause");
            var blasterCircleCleanup = fsm.AddState("Blaster Circle Cleanup");

            blasterCircleStart.AddCustomAction(() =>
            {
                var direction = UnityEngine.Random.Range(0, 2);
                if (direction == 0)
                {
                    blasterDirection = 1;
                }
                else
                {
                    blasterDirection = -1;
                }

                blasterCurrentIndex = UnityEngine.Random.Range(0, BlasterCount / 2);
                blasterFireCount = UnityEngine.Random.Range(BlasterCount, BlasterCount * 2);
            });

            // Satchel.FsmUtil.AddAction(shriekWaveStart, GetClipState(AbyssShriekSpawner.ShriekAudio));
            blasterCircleStart.AddTransition("FINISHED", blasterCircleStep.Name);


            //Satchel.FsmUtil.AddAction(shriekWaveStep, GetClipState(AbyssShriekSpawner.ShriekAudio, 0f, 3f));

            blasterCircleStep.AddCustomAction(() =>
            {
                // This is always a valid 
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Step 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                CorruptedDreamsPlandoMod.Instance.Log($"shoot {blasterCurrentIndex}");
                if (0 <= blasterCurrentIndex && blasterCurrentIndex <= blasterSpawners.Count - 1)
                {
                    blasterSpawners[blasterCurrentIndex].GetComponent<FireballShooter>().ShootStraight();
                }
            });

            blasterCircleStep.AddAction(new Wait() { time = BlasterShotDelay, finishEvent = new("FINISHED") });
            blasterCircleStep.AddTransition("FINISHED", blasterCirclePause.Name);

            blasterCirclePause.AddCustomAction(() =>
            {
                // This is always a valid 
                // Note that this check is just <, not <=, because the one above includes the last step
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Pause 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                blasterCurrentIndex += blasterDirection;
                if (blasterCurrentIndex < 0)
                {
                    blasterCurrentIndex = blasterSpawners.Count - 1;
                }

                if (blasterCurrentIndex > blasterSpawners.Count - 1)
                {
                    blasterCurrentIndex = 0;
                }

                blasterFireCount--;

                if (blasterFireCount >= 0)
                {
                    fsm.SendEvent("BLASTER CIRCLE CONTINUE");
                }
                else
                {
                    fsm.SendEvent("BLASTER CIRCLE DONE");
                }
            });
            blasterCirclePause.AddTransition("BLASTER CIRCLE CONTINUE", blasterCircleStep.Name);
            blasterCirclePause.AddTransition("BLASTER CIRCLE DONE", blasterCircleCleanup.Name);

            // Cleanup step is currently doing nothing
            blasterCircleCleanup.AddAction(new Wait() { time = 1f, finishEvent = new("FINISHED") });



            blasterCircleCleanup.AddTransition("FINISHED", "Move Choice");

            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.AddTransition("BLASTER CIRCLE", blasterCircleStart.Name);

            moveOptions.Add("BLASTER CIRCLE");
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
                shooter.RemoveParticles = true;

                shooter.SpiralShotInfo.clockwise = true;

                shooter.Speed = 0;
                shooter.SpiralShotInfo.inwardForce = 0;
                shooter.DamageToDeal = 1;
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
                    shooter.SpiralShotInfo.inwardForce = currentPattern.SpiralInwardVel;
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
                        shooter.SpiralShotInfo.clockwise = !shooter.SpiralShotInfo.clockwise;

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
                        shooter.ShootSpiral();
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
