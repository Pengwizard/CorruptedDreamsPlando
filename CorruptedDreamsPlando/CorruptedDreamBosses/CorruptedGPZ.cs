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
using UnityEngine;
using static SceneLoad;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    // TODO: make a "base" modifier class with the file reading thingy.
    // TODO: Add flow into rage state from "attack choice"
    internal class AnimationModifiersGpz
    {
        // Phase change variables
        public float LandwaveSpeed;
        public float LandwaveHeight;
        public float ShockwaveY;
        public float RunSpeed;
        public float StandTimeMin;
        public float StandTimeMax;
        public float RunTimeMin;
        public float RunTimeMax;
        public float StunTime;
        public int StunComboCount;
        public int StunHitMax;
        public int SpitsMin;
        public int SpitsMax;
        public int ActiveZotelinsMax;

        // Numbers for Phase Change bit
        public float RoarWaitTime;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate;
        public float PhaseChangeHpMultiplier;


        public static AnimationModifiersGpz FromJsonFile()
        {
            var path = "C:\\clean\\hkMods\\CorruptedDreamsPlando\\CorruptedDreamsPlando\\Resources\\bossVariables\\corruptedGPZModifiers.json";

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                AnimationModifiersGpz items = JsonConvert.DeserializeObject<AnimationModifiersGpz>(json);
                return items;
            }
        }
    }

    public class CorruptedGPZ
    {
        private static readonly FsmID controlFSM = new("Grey Prince", "Control");


        private static bool phaseChanged;
        private static GameObject _bossObject;
        private static int _startingHp;
        private static int _phase1Hp;
        private static AnimationModifiersGpz _modifiers;

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
            _modifiers = AnimationModifiersGpz.FromJsonFile();
        }

        private static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Changing ST phase 1");
            phaseChanged = false;

            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            var rageRoarState = fsm.GetState("Roar");
            Satchel.FsmUtil.AddFirstAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

            var roarEndPhase = fsm.GetState("Roar End");
            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

            // Rage starts at "4+" state after modifications
            // Can modify this from the start i guess

            FsmState rageState = SetupRageSetupState(fsm, "4+");

            var preMoveChoice = fsm.GetState("Move Level");
            var thingyState = fsm.GetState("4+");
            thingyState.RemoveTransition("FINISHED");
            thingyState.AddTransition("FINISHED", "Enter 1");

            AddPhaseChangeStep(fsm, "Move Level", "1", "Move Choice 1", rageState);
        }

        public static FsmState SetupRageSetupState(
    PlayMakerFSM fsm,
    string stateAfterRage)
        {
            CorruptedDreamsPlandoMod.Instance.Log("GPZ Setup Rage state");
            var rageCheckState = fsm.AddState("Setup Rage");

            rageCheckState.AddTransition("FINISHED", stateAfterRage);

            rageCheckState.AddCustomAction(() =>
            {
                if (!phaseChanged)
                {
                    var roarState = fsm.GetState("Roar");
                    roarState.AddCustomAction(() =>
                    {
                        fsm.SendEvent("END ROAR");
                    });
                    var rageRoarState = fsm.GetState("Enter 2");
                    // We disable the "short" roar check
                    rageRoarState.Actions[0].Enabled = false;
                    rageRoarState.GetFirstActionOfType<Wait>().time = _modifiers.RoarWaitTime;

                    Grayscaler.TickCamera();
                    SetupTitleScream(fsm);
                    ModifyAttackPatterns(fsm);
                    ModifyVariables(fsm);
                    ModifyAnimationTimes(fsm);



                    var dormant = fsm.GetState("Enter 1");
                    Satchel.FsmUtil.AddAction(dormant, new Wait() { time = 5f, finishEvent = new("FINISHED") });
                    dormant.AddCustomAction(() =>
                    {
                        //fsm.gameObject.transform.SetPosition2D(30f, 8.6f);
                        fsm.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    });

                    phaseChanged = true;
                }
            });

            return rageCheckState;
        }

        public static void AddPhaseChangeStep(
            PlayMakerFSM fsm,
            string stateToHookTransitionName,
            string transitionToHook,
            string stateIfNotRageName,
            FsmState rageSetupState)
        {

            var healthManager = _bossObject.GetComponent<HealthManager>();
            var firstLevel = fsm.GetState("Level 1").GetFirstActionOfType<SetHP>();
            _startingHp = firstLevel.hp.Value;
            firstLevel.hp.Value = 9999;
            _phase1Hp = 10; // (int)(_startingHp * _modifiers.PhaseChangeHpGate);
            var phaseTransitionGate = firstLevel.hp.Value - _phase1Hp;
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
                }
                else
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
                "Desolate Despot\r\nZote",
                "Roar",
                "Music",
                _modifiers.TitleFadeUpDelay,
                _modifiers.TitleFadeDownDelay,
                _modifiers.WhiteSolidUpTime,
                _modifiers.WhiteSolidUpDelay,
                UnityEngine.Color.black,
                UnityEngine.Color.white,
                "END ROAR",
                null,
                "Tynannical\r\nDreadful\r\nTerrible\r\nAuthoritarian\r\nHorrific\r\nGrim\r\nAtrocious\r\nDifficult\r\nFragile\r\nToxic\r\nAwful\r\nAntagonistic\r\nCondecending\r\nGreedy\r\nPompous\r\nVain\r\nGrumpy\r\nFussy\r\nUnforgiving");

            var go = GameObject.Find("Boss Title(Clone)/Boss Title (1)");
            go.transform.SetPositionY(6.8f);
        }

        private static void ModifyVariables(PlayMakerFSM fsm)
        {
            // I would like to personally thank TC for consolidating a bunch of variables in one spot
            var lastLevelState = fsm.GetState("4+");

            // Level is always 4, so damage for everything is set to 2.
            fsm.GetVariable<FsmInt>("Level").Value = 4;

            lastLevelState.GetAction<SetHP>(0).hp.Value = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);

            Utils.ChangeSetFloatValue(fsm, "4+", 3, _modifiers.LandwaveSpeed);
            Utils.ChangeSetFloatValue(fsm, "4+", 4, _modifiers.LandwaveHeight);
            Utils.ChangeSetFloatValue(fsm, "4+", 5, _modifiers.ShockwaveY);
            Utils.ChangeSetFloatValue(fsm, "4+", 6, _modifiers.RunSpeed);
            Utils.ChangeSetFloatValue(fsm, "4+", 7, _modifiers.StandTimeMin);
            Utils.ChangeSetFloatValue(fsm, "4+", 8, _modifiers.StandTimeMax);
            Utils.ChangeSetFloatValue(fsm, "4+", 9, _modifiers.RunTimeMin);
            Utils.ChangeSetFloatValue(fsm, "4+", 10, _modifiers.RunTimeMax);
            Utils.ChangeSetFloatValue(fsm, "4+", 11, _modifiers.StunTime);
            Utils.ChangeSetIntValue(fsm, "4+", 12, _modifiers.StunComboCount);
            Utils.ChangeSetIntValue(fsm, "4+", 13, _modifiers.StunHitMax);
            Utils.ChangeSetIntValue(fsm, "4+", 14, _modifiers.SpitsMin);
            Utils.ChangeSetIntValue(fsm, "4+", 15, _modifiers.SpitsMax);
            Utils.ChangeSetIntValue(fsm, "4+", 16, _modifiers.ActiveZotelinsMax);
        }

        private static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
        }

        private static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {
            // 1. Create Audio Source
            // 2. Make something that sets up DDark outside of the regular spawner
            // 3. Add a check for Y value of owning object

            //var landNormal = fsm.GetState("Land Normal");
            //var ftThrough = fsm.GetState("FT Throught");
            //var chargeSlam = fsm.GetState("Charge Slam");
            //var stomp = fsm.GetState("Stomp");
            //var stun = fsm.GetState("Stun");

            var vec = new Vector3(0, -2, 0);

            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(landNormal, 2, vec);
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(ftThrough, 2, vec);
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(chargeSlam, 2, vec);
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(stomp, 2, vec);
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(stun, 2, vec);

            var ddc = fsm.gameObject.AddComponent<DDarkComponent>();
            ddc.StartCoroutine(DescendingDarkSpawner.Instance.ActivateDDarkForObjectOnCondition(fsm.gameObject, 2, vec, ConditionFunction));
        }

        public static bool leftGround = false;
        public static bool ConditionFunction(GameObject go)
        {
            var tr = go.transform;
            var roundedY = (int)(tr.position.y * 10) / 10;
            if (roundedY <= 8.7f)
            {
                if (leftGround)
                {
                    leftGround = false;
                    return true;
                }

                return false;
            }
            else
            {
                leftGround = true;
                return false;
            }
        }
    }
}
