using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IL;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Modules;
using Modding;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using UnityEngine;
using WavLib;
using static SceneLoad;
using Module = ItemChanger.Modules.Module;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    internal class BalloonAttackPattern
    {
        public float SpiralSpeed;
        public float SpiralInwardVel;
        public int SpiralShotCount;
        public int SpiralSwitchDirectionInterval;
        public List<float> SpiralShotDelay;
        public float ShotScale;
    }

    internal class AnimationModifiersNkg
    {
        // Staggers
        public int NkgSaysLastStagger = 5;
        public float NkgSaysStartingTelegraphTime = 0.75f;
        public float NkgSaysTelegraphSpacing = 0.05f;
        public List<int> ParryRequirements = new() { 1, 2, 3, 3, 4, 5 };
        public List<int> AttackCount = new() { 2, 2, 4, 4, 6, 6 };

        // Shriek Wave
        public int ShriekSpawnerCount = 16;
        public float ShriekSpawnerFirstX = 70f;
        public float ShriekSpawnerLastX = 100f;
        public float ShriekOffsetY = -2.2f;
        public float ShriekSpawnDelay = 0.120f;

        // Ghaster Blaster
        public float BlasterInitialDistance = 10;
        public int BlasterCount = 16;
        public float BlasterSpeed = 25;
        public float BlasterTelegraphDelayMod = 4;
        public float BlasterShotDelay = 0.25f;
        public float BlasterShotScale = 1.1f;

        // Spiral
        public List<BalloonAttackPattern> BalloonPatterns = new()
        {
            new() {
                SpiralSpeed = 10,
                SpiralInwardVel = -1.3f,
                SpiralShotCount = 16,
                SpiralShotDelay = new() {1.5f, 0.5f, 0.5f, 0.5f},
                SpiralSwitchDirectionInterval = 4,
                ShotScale = 1f
            },
            new() {
                SpiralSpeed = 12,
                SpiralInwardVel = -1.3f,
                SpiralShotCount = 16,
                SpiralShotDelay = new() {1.5f, 0.5f},
                SpiralSwitchDirectionInterval = 16,
                ShotScale = 1f
            },
            new() {
                SpiralSpeed = 9,
                SpiralInwardVel = -1.25f,
                SpiralShotCount = 15,
                SpiralShotDelay = new() {1.5f, 0.5f, 0.5f},
                SpiralSwitchDirectionInterval = 3,
                ShotScale = 1f
            },
            new() {
                SpiralSpeed = 10,
                SpiralInwardVel = -1.2f,
                SpiralShotCount = 15,
                SpiralShotDelay = new() {1.8f, 0.3f,0.3f,0.3f,0.3f},
                SpiralSwitchDirectionInterval = 5,
                ShotScale = 1f
            }
        };
        public float SpiralSpawnOffset = 3;

        // DDarks
        public float DDarkCount = 6;
        public float DDarkTelegraphBuildup = 1f;
        public float DDarkTelegraphSpacing = 0.4f;


        // Numbers for Phase Change bit
        public float RoarWaitTime = 2.0f;
        public float TitleFadeUpDelay = 2.0f;
        public float TitleFadeDownDelay = 0.5f;
        public float WhiteSolidUpTime = 0.5f;
        public float WhiteSolidUpDelay = 2.0f;
        public float PhaseChangeHpGate = 0.15f;
        public float PhaseChangeHpMultiplier = 3f;
    }

    public class CorruptedNKG : Module
    {

        private static bool hardcoreNKG = false;

        private static readonly FsmID controlFSM = new("Nightmare Grimm Boss", "Control");

        private static bool phaseChanged;
        private static bool alreadyChangedOnce = false;
        private static GameObject _bossObject;
        private static int _startingHp;
        private static int _phase1Hp;
        private static AnimationModifiersNkg _modifiers;

        private const string _hiveKnightAudioName = "Hive Knight v2-86";
        private static AudioClip _superNkgBossMusic = null;
        private static void LoadSuperAudioClip()
        {
            if (_superNkgBossMusic == null)
            {
                CorruptedDreamsPlandoMod.Instance.Log("Loading audio track");
                var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorruptedDreamsPlando.Resources.bgm.supernkgbgm.wav");
                var curMusicCue = GameManager.instance.AudioManager.CurrentMusicCue;

                WavData.Inspect(assetStream, CorruptedDreamsPlandoMod.Instance.Log);
                WavData wavData = new WavData();
                wavData.Parse(assetStream, CorruptedDreamsPlandoMod.Instance.Log);
                assetStream.Close();

                float[] wavSoundData = wavData.GetSamples();
                _superNkgBossMusic = AudioClip.Create("NightmareGrimm2", wavSoundData.Length / wavData.FormatChunk.NumChannels, wavData.FormatChunk.NumChannels, (int)wavData.FormatChunk.SampleRate, false);
                _superNkgBossMusic.SetData(wavSoundData, 0);
                GameObject.DontDestroyOnLoad(_superNkgBossMusic);
                CorruptedDreamsPlandoMod.Instance.Log("Done Loading audio track");
            }
        }

        public override void Initialize()
        {
            // TODO: Change to raw FSM editing
            Events.AddFsmEdit(controlFSM, SetupPhaseChange);
            On.AudioManager.BeginApplyMusicCue += OnApplyMusicCue;
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(controlFSM, SetupPhaseChange);
            On.AudioManager.BeginApplyMusicCue -= OnApplyMusicCue;
        }

        private static IEnumerator OnApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            bool changed = false;
            MusicCue.MusicChannelInfo[] infos = ReflectionHelper.GetField<MusicCue, MusicCue.MusicChannelInfo[]>(musicCue, "channelInfos");

            foreach (MusicCue.MusicChannelInfo info in infos)
            {
                AudioClip origAudio = ReflectionHelper.GetField<MusicCue.MusicChannelInfo, AudioClip>(info, "clip");

                if (origAudio != null)
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Checking to replace {origAudio.name}");
                    if (origAudio.name != _hiveKnightAudioName)
                    {
                        continue;
                    }

                    AudioClip possibleReplace = _superNkgBossMusic;
                    if (possibleReplace != null)
                    {
                        // Change Audio Clip
                        ReflectionHelper.SetField<MusicCue.MusicChannelInfo, AudioClip>(info, "clip", possibleReplace);
                        changed = true;
                    }
                }
            }

            if (changed) ReflectionHelper.SetField<MusicCue, MusicCue.MusicChannelInfo[]>(musicCue, "channelInfos", infos);

            yield return orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        public static void UnpackModifiers()
        {
            _modifiers = new AnimationModifiersNkg();
            try
            {
                //_modifiers = AnimationModifiersNkg.FromJsonFile();
            }
            catch (Exception e) { }
        }

        private static AudioPlayerOneShotSingle GetClipState(FsmObject audioClip, float delay = 0f, float volume = 5f)
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

        private static FsmState GetHiveKnightMusicCue(PlayMakerFSM fsm)
        {
            var hiveKnight = Preloader.Instance.HiveKnight;
            var hkControl = hiveKnight.LocateMyFSM("Control");
            var musicState = hkControl.GetValidState("Music");

            var startHiveKnightMusic = fsm.AddState("Start Hive Knight Music");

            var cue = musicState.GetFirstActionOfType<ApplyMusicCue>();
            cue.delayTime = 0f;

            startHiveKnightMusic.AddCustomAction(() =>
            {
                GameManager.instance.AudioManager.ApplyMusicCue((MusicCue)cue.musicCue.Value, 0.0f, 0f, true);
            });

            startHiveKnightMusic.AddAction(musicState.GetFirstActionOfType<TransitionToAudioSnapshot>());
            var snap = startHiveKnightMusic.GetFirstActionOfType<TransitionToAudioSnapshot>();
            snap.transitionTime = 0.5f;

            return startHiveKnightMusic;
        }

        private static FsmState GetKillMusicState(PlayMakerFSM fsm)
        {
            var deathStart = fsm.GetValidState("Death Start");

            var killMusic = fsm.AddState("Kill Music");
            killMusic.AddAction(deathStart.GetFirstActionOfType<TransitionToAudioSnapshot>());

            killMusic.GetFirstActionOfType<TransitionToAudioSnapshot>().transitionTime = 0f;

            return killMusic;

        }

        private static void SetupPhaseChangeAnimationSequence(PlayMakerFSM fsm, string stateAfterRoar)
        {
            var grimmControl = GameObject.Find("Grimm Control");
            var grimmControlFsm = grimmControl.LocateMyFSM("Control");

            var eye2 = grimmControlFsm.GetValidState("Eye 2");
            var eye2PlayOneShot = eye2.GetFirstActionOfType<AudioPlayerOneShotSingle>();
            var cameraShake = eye2.GetFirstActionOfType<SendEventByName>();

            var myRetry = fsm.CopyState("Pillar Retry", "PC Pillar Retry");
            var myPos = fsm.CopyState("Pillar Pos", "PC Pillar Pos");
            var myTeleIn = fsm.CopyState("Pillar Tele In", "PC Pillar Tele In");
            var myAntic = fsm.CopyState("Pillar Antic", "PC Pillar Antic");

            var killMusic = GetKillMusicState(fsm);

            var roarState = fsm.AddState("Roar");

            var musicState = GetHiveKnightMusicCue(fsm);

            var roarEndState = fsm.AddState("Roar End");

            myTeleIn.GetFirstActionOfType<SetPosition>().x = balloonBaseX;

            myRetry.RemoveTransition("FINISHED");
            myPos.RemoveTransition("FINISHED");
            myPos.RemoveTransition("CANCEL");
            myTeleIn.RemoveTransition("FINISHED");
            myAntic.RemoveTransition("FINISHED");

            myRetry.AddTransition("FINISHED", myPos.Name);
            myPos.AddTransition("FINISHED", myTeleIn.Name);
            myPos.AddTransition("CANCEL", myRetry.Name);
            myTeleIn.AddTransition("FINISHED", myAntic.Name);

            myAntic.AddTransition("FINISHED", killMusic.Name);

            killMusic.AddTransition("FINISHED", roarState.Name);

            roarState.AddTransition("FINISHED", musicState.Name);

            musicState.AddTransition("FINISHED", roarEndState.Name);

            roarEndState.AddTransition("FINISHED", stateAfterRoar);

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

            var appearing = GameObject.Find("Grimm Control/Appear Scene/Grimm Appear");

            roarState.AddCustomAction(() =>
            {
                appearing.transform.position = fsm.gameObject.transform.position;
                appearing.transform.SetPositionY(17.2f);
                appearing.transform.SetPositionX(balloonBaseX - 1.7f);
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

            roarEndState.AddCustomAction(() =>
            {
                Utils.Hero().LocateMyFSM("Roar Lock").SendEvent("ROAR EXIT");
                appearing.SetActive(false);
            });
        }

        private static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            LoadSuperAudioClip();

            CorruptedDreamsPlandoMod.Instance.Log("Changing NKG");
            phaseChanged = false;

            On.HeroController.NailParry -= HeroController_NailParry;

            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            SetupPhaseChangeAnimationSequence(fsm, "Tele Out");
            SetupTitleScream(fsm);

            var rageRoarState = fsm.GetValidState("Roar");
            Satchel.FsmUtil.AddFirstAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

            var roarEndPhase = fsm.GetValidState("Roar End");
            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

            FsmState rageState = SetupRageSetupState(fsm, "PC Pillar Pos");

            AddPhaseChangeStep(fsm, new() { "Balloon?", "Move Choice" }, rageState);

            // We completely disable the regular stagger, cant afford it
            var stunControl = _bossObject.LocateMyFSM("Stun");
            stunControl.GetVariable<FsmInt>("Stun Hit Max").Value = 18 * 6;
            stunControl.GetVariable<FsmInt>("Stun Combo").Value = 18 * 2;

            if (!alreadyChangedOnce)
            {
                FsmEditTemplates.Utils.DisplayDreamText("It's recommended to turn on sfx and bgm volumes (not too loud)");
            }
        }

        public static void SetupTitleScream(PlayMakerFSM fsm)
        {
            FsmEditTemplates.FullScreenTitleDrop.SetupTitleUp(
                fsm,
                "Grimm",
                "Start Hive Knight Music",
                "Roar End",
                _modifiers.TitleFadeUpDelay,
                _modifiers.TitleFadeDownDelay,
                _modifiers.WhiteSolidUpTime,
                _modifiers.WhiteSolidUpDelay,
                UnityEngine.Color.black,
                UnityEngine.Color.white,
                "FINISHED",
                null,
                "Dream Corruptor");
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
                    var healthManager = _bossObject.GetComponent<HealthManager>();
                    healthManager.hp = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);
                    CorruptedDreamsPlandoMod.Instance.Log("Raging");
                    Grayscaler.TickCamera();

                    isFirstMove = false;
                    ModifyAttackPatterns(fsm);
                    ModifyVariables(fsm);
                    ModifyAnimationTimes(fsm);

                    phaseChanged = true;
                    alreadyChangedOnce = true;
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
            _phase1Hp = (int)(_startingHp * _modifiers.PhaseChangeHpGate);
            var phaseTransitionGate = healthManager.hp - _phase1Hp;
            if (alreadyChangedOnce)
            {
                phaseTransitionGate = healthManager.hp - 1;
            }

            CorruptedDreamsPlandoMod.Instance.Log($"Setting up phase change, will trigger at {phaseTransitionGate} hp");

            string setupRageTransition = "SETUP RAGE";

            foreach (string stateToHookName in statesToAddCheck)
            {
                var stateToHookTransition = fsm.GetValidState(stateToHookName);

                stateToHookTransition.AddTransition(setupRageTransition, rageSetupState.Name);

                stateToHookTransition.InsertCustomAction(() =>
                {
                    var healthManager = _bossObject.GetComponent<HealthManager>();
                    var currHP = healthManager.hp;
                    if (currHP < phaseTransitionGate && !phaseChanged)
                    {
                        fsm.SendEvent(setupRageTransition);
                    }
                }, 0);
            }
        }

        private static void ModifyVariables(PlayMakerFSM fsm)
        {

        }

        private static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
        }

        // Balloon variables
        private static int fireballsShotBalloon = 0;

        private static float balloonBaseX = 85.77f;
        private static float balloonBaseY = 13.5f;

        private static float groundY = 9.6f;
        private static float actualGroundY = 6.4f;

        // Move options
        private static bool isFirstMove = true;
        private static List<string> moveOptions = new();

        private static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {
            SetupMoveChoice(fsm);

            SetupNewBalloonPhase(fsm);

            SetupGasterBlaster(fsm);

            SetupDDarkAttack(fsm);

            SetupAbyssShriekWave(fsm);

            ModifyEvasionBats(fsm);
        }


        private static void ModifyEvasionBats(PlayMakerFSM fsm)
        {
            var toFirebats = fsm.GetValidState("To Firebats");
            toFirebats.ChangeTransition("FINISHED", "Tele Out");
        }

        private static void CleanupGameObjectList(List<GameObject> gos)
        {
            foreach (var go in gos)
            {
                GameObject.Destroy(go);
            }

            gos.Clear();
        }

        private static int lastAttack = 0;
        private static float balloonCounterLimit = 5;
        private static float balloonCounter = balloonCounterLimit;

        public static void SetupMoveChoice(PlayMakerFSM fsm)
        {

            // Disabling the phase transition balloon, making it a regular move 
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

            moveChoice.RemoveTransition("SPIKES");
            moveChoice.RemoveTransition("PILLARS");
            moveChoice.RemoveTransition("BALLOON");

            moveOptions.Clear();

            if (hardcoreNKG)
            {
                moveChoice.RemoveTransition("SLASH");
                moveChoice.RemoveTransition("AIR DASH");
            }
            else
            {
                moveOptions.Add("SLASH");
                moveOptions.Add("SLASH");
                moveOptions.Add("AIR DASH");

                On.HeroController.NailParry += HeroController_NailParry;
            }

            moveChoice.GetFirstActionOfType<SendRandomEventV3>().Enabled = false;
            moveChoice.GetFirstActionOfType<SetBoolValue>().Enabled = false;
            moveChoice.GetFirstActionOfType<BoolTest>().Enabled = false;

            balloonCounterLimit = 5;
            balloonCounter = balloonCounterLimit;

            moveChoice.AddCustomAction(() =>
            {
                var nextMove = "";
                if (balloonCounter >= balloonCounterLimit)
                {
                    nextMove = "BALLOON SPIRAL";
                    balloonCounter = 0;
                    balloonCounterLimit += 0.5f;
                    balloonCounterLimit = Mathf.Min(balloonCounterLimit, 8);
                }
                else
                {
                    var moveIndex = UnityEngine.Random.Range(0, moveOptions.Count);
                    while (moveOptions[lastAttack] == moveOptions[moveIndex])
                    {
                        moveIndex = UnityEngine.Random.Range(0, moveOptions.Count);
                    }

                    nextMove = moveOptions[moveIndex];
                    lastAttack = moveIndex;
                    balloonCounter++;
                }

                fsm.SendEvent(nextMove);
            });
        }


        public static void SetupTelegraph(GameObject telegraphObject, Vector3 position, Vector3 finalScale, float time)
        {
            telegraphObject.transform.SetPosition3D(position.x, position.y, position.z);
            telegraphObject.transform.SetScaleX(1f);
            telegraphObject.transform.SetScaleY(1f);
            var lightPulse = telegraphObject.LocateMyFSM("Light Pulse");
            var st1 = lightPulse.GetValidState("State 1");

            var scaler = st1.GetFirstActionOfType<iTweenScaleBy>();
            scaler.loopType = iTween.LoopType.none;

            st1.AddCustomAction(() =>
            {
                scaler.time = 0.75f;
            });

            scaler.easeType = iTween.EaseType.linear;
            scaler.vector = finalScale;

            lightPulse.AddState("Deactivate");
            var de = lightPulse.GetValidState("Deactivate");
            de.AddCustomAction(() =>
            {
                telegraphObject.SetActive(false);
            });

            st1.AddTransition("FINISHED", de.Name);

            telegraphObject.SetActive(false);
        }

        public class ParryReward : MonoBehaviour
        {
            public static void SpawnSoul(Transform transform)
            {
                var prefab = ObjectCache.SoulOrb;
                Destroy(prefab.Spawn());
                prefab.SetActive(true);

                // Give 34 soul per usage.
                // double if dream wielder is equipped
                // max if can dream gate.
                int soulToGive = 17 * 2;
                int masksToHeal = 1;

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

                var sos = GameManager.instance.gameObject.GetComponent<ParryReward>();
                if (sos == null)
                {
                    sos = GameManager.instance.gameObject.AddComponent<ParryReward>();
                }

                sos.StartCoroutine(Routine());
                Destroy(prefab);
            }
        }

        private static void HeroController_NailParry(On.HeroController.orig_NailParry orig, HeroController self)
        {
            orig(self);

            ParryReward.SpawnSoul(Utils.Hero().transform);
            balloonCounterLimit -= 1f;
        }


        // Shriek Wave Variables
        private static int shriekDirectionStep = 0;
        private static int shriekCurrentIndex = 0;
        private static List<GameObject> shriekSpawners = new List<GameObject>();

        public static GameObject leftScreenTelegraph;
        public static GameObject rightScreenTelegraph;

        public static void SetupAbyssShriekWave(PlayMakerFSM fsm)
        {
            float telegraph = 0.4f;

            leftScreenTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);
            rightScreenTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);
            SetupTelegraph(leftScreenTelegraph, new Vector3(69f, 11f, 0.52f), new Vector3(5, 5, 1), telegraph);
            SetupTelegraph(rightScreenTelegraph, new Vector3(101f, 11f, 0.52f), new Vector3(5, 5, 1), telegraph);

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
                var direction = (Utils.Hero().transform.position.x >= 85) ? 1 : 0;

                if (direction == 1)
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
            shriekWaveCleanup.AddCustomAction(() =>
            {
                leftScreenTelegraph.transform.SetScaleX(1f);
                leftScreenTelegraph.transform.SetScaleY(1f);

                rightScreenTelegraph.transform.SetScaleX(1f);
                rightScreenTelegraph.transform.SetScaleY(1f);
            });

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

        public static void SetupDDarkAttack(PlayMakerFSM fsm)
        {
            // Have beam of light telegraph
            // also need to restructure the ddark thing so you get the spawner i think?

            CleanupGameObjectList(ddarkTelegraphs);
            CleanupGameObjectList(ddarkContainers);

            for (int i = 0; i < _modifiers.DDarkCount; i++)
            {
                var ddarkTelegraph = GameObject.Instantiate(Preloader.Instance.BenchLight);

                var lightPulse = ddarkTelegraph.LocateMyFSM("Light Pulse");
                var state1 = lightPulse.GetValidState("State 1");
                var tween = state1.GetFirstActionOfType<iTweenScaleBy>();
                tween.time = 1f;
                tween.vector = new Vector3(2f, 2f, 2f);

                var diveContainer = DescendingDarkSpawner.Instance.GetDDarkSpawner(
                    2,
                    Utils.Hero(),
                    actualGroundY, 0.1f, GetClipState(DescendingDarkSpawner.DDarkAudio));

                ddarkTelegraphs.Add(ddarkTelegraph);
                ddarkContainers.Add(diveContainer);
            }

            var ddarkPillarsStart = fsm.AddState("DDark Pillars Start");
            var ddarkPillarsTelegraph = fsm.AddState("DDark Pillars Telegraph");
            var ddarkPillarsBlast = fsm.AddState("DDark Pillars Blast");
            var ddarkPillarsPause = fsm.AddState("DDark Pillars Pause");
            var ddarkPillarsCleanup = fsm.AddState("DDark Pillars Cleanup");

            ddarkPillarsStart.AddCustomAction(() =>
            {
                ddarkCurrentIndex = 0;
            });

            ddarkPillarsStart.AddTransition("FINISHED", ddarkPillarsTelegraph.Name);

            ddarkPillarsTelegraph.AddCustomAction(() =>
            {
                var curIn = ddarkCurrentIndex;
                ddarkTelegraphs[curIn].transform.SetScaleX(1f);
                ddarkTelegraphs[curIn].transform.SetScaleY(1f);

                ddarkTelegraphs[curIn].transform.SetPosition2D(Utils.Hero().transform.position);
                ddarkTelegraphs[curIn].transform.SetPositionY(actualGroundY);
                ddarkTelegraphs[curIn].SetActive(true);

                ddarkContainers[curIn].transform.SetPosition2D(Utils.Hero().transform.position);
                ddarkContainers[curIn].transform.SetPositionY(actualGroundY);

                Satchel.CoroutineHelper.WaitForSecondsBeforeInvoke(_modifiers.DDarkTelegraphBuildup, () =>
                {
                    ddarkTelegraphs[curIn].SetActive(false);
                    ddarkContainers[curIn].SetActive(false);
                    ddarkContainers[curIn].SetActive(true);
                });
            });

            ddarkPillarsTelegraph.AddAction(new Wait() { time = _modifiers.DDarkTelegraphSpacing, finishEvent = new("FINISHED") });
            ddarkPillarsTelegraph.AddTransition("FINISHED", ddarkPillarsBlast.Name);

            ddarkPillarsBlast.AddCustomAction(() =>
            {
            });

            ddarkPillarsBlast.AddTransition("FINISHED", ddarkPillarsPause.Name);


            ddarkPillarsPause.AddCustomAction(() =>
            {
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
            ddarkPillarsPause.AddTransition("DDARK PILLARS CONTINUE", ddarkPillarsTelegraph.Name);
            ddarkPillarsPause.AddTransition("DDARK PILLARS DONE", ddarkPillarsCleanup.Name);

            // Cleanup step is currently doing nothing
            ddarkPillarsCleanup.AddAction(new Wait() { time = 1.5f, finishEvent = new("FINISHED") });

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

                fireballShooter.SpawnOffset = new Vector3(xOffset, yOffset, 0);
                fireballShooter.BaseObject = Utils.Hero();
                fireballShooter.Direction = angle;
                fireballShooter.Speed = BlasterSpeed;
                fireballShooter.ShotScale = BlasterShotScale;
                fireballShooter.PreFireDelay = BlasterTelegraph;
                fireballShooter.RemoveParticles = true;
                fireballShooter.DamageToDeal = 2;

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

                if (blasterDirection == -1)
                {
                    blasterCurrentIndex = 0;
                }
                else
                {
                    blasterCurrentIndex = BlasterCount / 2;
                }
                blasterFireCount = UnityEngine.Random.Range(0, BlasterCount);
                blasterFireCount += BlasterCount;
            });

            blasterCircleStart.AddTransition("FINISHED", blasterCircleStep.Name);

            blasterCircleStep.AddCustomAction(() =>
            {
                // This is always a valid 
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
            // Needs longer cooldown because this attack is async
            blasterCircleCleanup.AddAction(new Wait() { time = 2f, finishEvent = new("FINISHED") });

            blasterCircleCleanup.AddTransition("FINISHED", "Move Choice");

            var moveChoice = fsm.GetValidState("Move Choice");
            moveChoice.AddTransition("BLASTER CIRCLE", blasterCircleStart.Name);

            moveOptions.Add("BLASTER CIRCLE");
        }

        private static BalloonAttackPattern currentPattern;
        private static List<GameObject> spiralContainers = new();

        public static void SetupNewBalloonPhase(PlayMakerFSM fsm)
        {
            var inflateState = fsm.GetValidState("Inflate");

            CleanupGameObjectList(spiralContainers);

            var upCont = new GameObject();
            var fireballShooterUp = upCont.AddComponent<FireballShooter>();
            fireballShooterUp.SpawnOffset = new Vector3(0, _modifiers.SpiralSpawnOffset, 0);
            upCont.name = $"Spiral (Up)";

            var downCont = new GameObject();
            var fireballShooterDown = downCont.AddComponent<FireballShooter>();
            fireballShooterDown.SpawnOffset = new Vector3(0, -_modifiers.SpiralSpawnOffset, 0);
            downCont.name = $"Spiral (down)";

            var leftCont = new GameObject();
            var fireballShooterLeft = leftCont.AddComponent<FireballShooter>();
            fireballShooterLeft.SpawnOffset = new Vector3(_modifiers.SpiralSpawnOffset, 0, 0);
            leftCont.name = $"Spiral (left)";

            var rightCont = new GameObject();
            var fireballShooterRight = rightCont.AddComponent<FireballShooter>();
            fireballShooterRight.SpawnOffset = new Vector3(-_modifiers.SpiralSpawnOffset, 0, 0);
            rightCont.name = $"Spiral (right)";

            spiralContainers = new() { upCont, downCont, leftCont, rightCont };
            List<FireballShooter> shooterList = new() { fireballShooterUp, fireballShooterDown, fireballShooterLeft, fireballShooterRight };

            foreach (var shooter in shooterList)
            {
                // Common for all fireball shooters
                shooter.SpawnPoint = new Vector3(balloonBaseX, balloonBaseY, 0);
                shooter.RemoveParticles = true;

                shooter.SpiralShotInfo.clockwise = true;

                shooter.Speed = 0;
                shooter.SpiralShotInfo.inwardForce = 0;
                shooter.DamageToDeal = 2;
            }

            var shootSpirals = fsm.AddState("Shade Repeat");
            var shootPause = fsm.AddState("Shade Pause");

            var firePause = fsm.GetValidState("Fire Pause");
            firePause.RemoveTransition("FINISHED");
            firePause.AddTransition("FINISHED", shootSpirals.Name);

            firePause.AddCustomAction(() =>
            {
                fireballsShotBalloon = 0;
                var patternIndex = UnityEngine.Random.Range(0, _modifiers.BalloonPatterns.Count);
                currentPattern = _modifiers.BalloonPatterns[patternIndex];

                foreach (var shooter in shooterList)
                {
                    shooter.ShotScale = currentPattern.ShotScale;
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

            var endPause = fsm.GetValidState("End Pause");
            //endPause.GetFirstActionOfType<Wait>().time = 5f;

            

            var teleOut = fsm.GetValidState("Tele Out");
            var outPause = fsm.GetValidState("Out Pause");
            var moveChoice = fsm.GetValidState("Move Choice");

            var balTeleOut = fsm.AddState("Balloon Tele Out");
            var balOutPause = fsm.AddState("Balloon Out Pause");
            balTeleOut.CopyActionData(teleOut);
            balOutPause.CopyActionData(outPause);

            balTeleOut.AddTransition("FINISHED", balOutPause.Name);
            balOutPause.AddTransition("FINISHED", moveChoice.Name);

            var deflateState = fsm.GetValidState("Deflate");
            deflateState.RemoveTransition("FINISHED");
            deflateState.AddTransition("FINISHED", balTeleOut.Name);

            //deflateState.AddAction(new Wait() { time = 7f, finishEvent = new("DEFLATE FINISHED") });
            balOutPause.GetFirstActionOfType<Wait>().time = 4f;

            var balloonPos = fsm.GetValidState("Balloon Pos");
            moveChoice.AddTransition("BALLOON SPIRAL", balloonPos.Name);
        }
    }
}
