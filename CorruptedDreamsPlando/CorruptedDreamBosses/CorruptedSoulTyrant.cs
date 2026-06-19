using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    internal class AnimationModifiersSt
    {
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


        private static bool phaseChanged;
        private static GameObject _bossObject;
        private static int _startingHp;
        private static int _phase1Hp;
        private static AnimationModifiersSt _modifiers;

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
            _modifiers = AnimationModifiersSt.FromJsonFile("");
        }

        public static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Changing ST phase 1");
            phaseChanged = false;


            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            // TODO disable stagger

            // Phase change starts from Teleport In
            // Check HP gate in Damaged Idle
            // Phase transition
            // Setup speedup
            // Faster animations
            // Modify soul orbs to give soul when destroyed (take fsm from grub jar?)
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

            //rageCheckState.AddAction(new IntCompare()
            //{
            //    integer1 = hpFSM,
            //    integer2 = hpGateFSM,
            //    greaterThan = FsmEvent.GetFsmEvent(setupRageTransition), //FsmEvent.GetFsmEvent(defaultTransition),
            //    everyFrame = false
            //});

            //rageCheckState.AddCustomAction(() =>
            //{
            //    CorruptedDreamsPlandoMod.Instance.Log($"Trying to rage");
            //});

            //rageCheckState.AddAction(new SetBoolValue()
            //{
            //    boolVariable = ragedBool,
            //    boolValue = true,
            //    everyFrame = false
            //});

            //rageCheckState.AddCustomAction(() =>
            //{
            //    CorruptedDreamsPlandoMod.Instance.Log($"Trying to fire event {FsmEventTarget.Self} {FsmEvent.GetFsmEvent(setupRageTransition)}");
            //});

            //rageCheckState.AddAction(new SendEvent()
            //{
            //    eventTarget = FsmEventTarget.Self,
            //    sendEvent = FsmEvent.GetFsmEvent(setupRageTransition),
            //});
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

        private static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {
            throw new NotImplementedException();
        }

        private static void ModifyVariables(PlayMakerFSM fsm)
        {
            throw new NotImplementedException();
        }

        private static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
            throw new NotImplementedException();
        }

    }
}
