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
    public class AnimationModifiersLK
    {
        // Float modifiers
        public float DashSpeed;
        public float DashReverse;
        public float Gravity;
        public float JumpY;
        public float JumpX;


        // Animation speed modifiers
        public float EvadeAntic;
        public float Evade;
        public float EvadeRecover;
        public float OheadAntic;
        public float OheadSlashing;
        public float OheadSlashing2;
        public float DashAntic;
        public float DashAntic2;
        public float DashAntic3;
        public float DashAttack;
        public float DashAttack2;
        public float DashAttack3;
        public float DashRecover;
        public float Fall;
        public float Jump;
        public float JumpAntic;
        public float DstabAntic;
        public float DstabFall;
        public float DstabLand;
        public float DstabRecover;
        public float Walk;

        // Wait Numbers
        public float ShakingSpawnWait;
        public float IdleCounterLower;
        public float IdleCounterUpper;
        public float OheadAnticWait;
        public float ShakingStartWait;
        public float ShakeEnd;
        public float WalkSpeedUpper;
        public float WalkSpeedLower;
        public float DashAntic2Wait;

        // Numbers for Phase Change bit
        public float RoarWaitTime;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate;
        public float PhaseChangeHpMultiplier;


        public static AnimationModifiersLK FromJsonFile()
        {
            // TODO make generic and be able to read from resource
            string path = "";
            if (path == "")
            {
                path = "C:\\clean\\hkMods\\CorruptedDreamsPlando\\CorruptedDreamsPlando\\Resources\\bossVariables\\corruptedLostKinModifiers.json";
            }

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                AnimationModifiersLK items = JsonConvert.DeserializeObject<AnimationModifiersLK>(json);
                return items;
            }
        }
    }

    public class CorruptedLostKin
    {
        private static readonly FsmID lostKinControl = new("Lost Kin", "IK Control");

        private static GameObject _bossObject;
        private static int _startingHp = 0;
        private static int _phase1Hp;
        private static FsmObject _ddarkAudio = null;
        private static bool phaseChanged = false;

        private static AnimationModifiersLK _modifiers;


        public static GameObject BossTitlePrefab;
        public static GameObject WhiteSolidPrefab;


        public static void Initialize()
        {
            Events.AddFsmEdit(lostKinControl, SetupPhaseChange);
        }

        public static void Unload()
        {
            Events.RemoveFsmEdit(lostKinControl, SetupPhaseChange);
            Events.RemoveFsmEdit(lostKinControl, SuperBossLostKin);
        }

        private static void MultiplyFsmFloat(FsmFloat f, float mul)
        {
            f.Value = f.Value * mul;
        }

        private static void CreateLoopForDDark(PlayMakerFSM fsm)
        {
            FsmState dStabRecover = SFCore.Utils.FsmUtil.GetState(fsm, "Dstab Recover");
            Utils.ChangeWaitFloat(fsm, "Dstab Recover", 0, 0.1f);

            FsmState dStabLoop = SFCore.Utils.FsmUtil.AddState(fsm, "Dstab Loop");
            FsmState dStabPause = SFCore.Utils.FsmUtil.AddState(fsm, "Dstab Pause");

            SFCore.Utils.FsmUtil.RemoveTransition(dStabRecover, "FINISHED");
            SFCore.Utils.FsmUtil.AddTransition(dStabRecover, "FINISHED", "Dstab Loop");
            FsmEvent doRepeat = new("DOREPEAT");
            FsmEvent dontRepeat = new("DONTREPEAT");

            SFCore.Utils.FsmUtil.AddTransition(dStabLoop, "DOREPEAT", "Set Dstab");
            SFCore.Utils.FsmUtil.AddTransition(dStabLoop, "DONTREPEAT", "Dstab Pause");

            SFCore.Utils.FsmUtil.AddTransition(dStabPause, "FINISHED", "Set Counter");

            fsm.AddVariable<FsmInt>("DDark Loops");
            var ddarkLoops = fsm.GetVariable<FsmInt>("DDark Loops");
            ddarkLoops = 0;
            dStabLoop.AddCustomAction(() =>
            {
                FsmInt loops = fsm.FsmVariables.GetFsmInt("DDark Loops");
                loops.Value = loops.Value + 1;
                fsm.SendEvent(loops.Value > 3 ? "DONTREPEAT" : "DOREPEAT");
            });

            //Satchel.FsmUtil.AddAction(dStabLoop, new Lambda(() => {
            //    FsmInt loops = fsm.FsmVariables.GetFsmInt("DDark Loops");
            //    loops.Value = loops.Value + 1;
            //    fsm.SendEvent(loops.Value > 3 ? "DONTREPEAT" : "DOREPEAT");
            //}));

            Satchel.FsmUtil.AddAction(dStabPause, new Wait() { time = 0.35f });

            dStabLoop.AddCustomAction(() =>
            {
                FsmInt loops = fsm.FsmVariables.GetFsmInt("DDark Loops");
                loops.Value = 0;
            });

            //Satchel.FsmUtil.AddAction(dStabPause, new Lambda(() => {
            //    FsmInt loops = fsm.FsmVariables.GetFsmInt("DDark Loops");
            //    loops.Value = 0;
            //}));
        }

        private static void SetAllNumbers(PlayMakerFSM fsm)
        {
            var am = _modifiers;

            MultiplyFsmFloat(fsm.GetVariable<FsmFloat>("Dash Speed"), am.DashSpeed);
            MultiplyFsmFloat(fsm.GetVariable<FsmFloat>("Dash Reverse"), am.DashReverse);
            MultiplyFsmFloat(fsm.GetVariable<FsmFloat>("Gravity"), am.Gravity);

            Utils.SpeedupAnimation(fsm, "Evade Antic", am.EvadeAntic);
            Utils.SpeedupAnimation(fsm, "Evade Dash", am.Evade);
            Utils.SpeedupAnimation(fsm, "Evade Recover", am.EvadeRecover);

            Utils.SpeedupAnimation(fsm, "Ohead Antic", am.OheadAntic);
            Utils.ChangeWaitFloat(fsm, "Ohead Antic", 2, am.OheadAnticWait);

            Utils.SpeedupAnimation(fsm, "Ohead Slashing", am.OheadSlashing);
            Utils.SpeedupAnimation(fsm, "Ohead Slashing", am.OheadSlashing2, 7);

            Utils.SpeedupAnimation(fsm, "Dash Antic", am.DashAntic);

            Utils.SpeedupAnimation(fsm, "Dash Antic 2", am.DashAntic2);
            Utils.ChangeWaitFloat(fsm, "Dash Antic 2", 2, am.DashAntic2Wait);
            Utils.SpeedupAnimation(fsm, "Dash Antic 3", am.DashAntic3);

            Utils.SpeedupAnimation(fsm, "Dash Attack", am.DashAttack);
            Utils.SpeedupAnimation(fsm, "Dash Attack 2", am.DashAttack2);
            Utils.SpeedupAnimation(fsm, "Dash Attack 3", am.DashAttack3);

            Utils.SpeedupAnimation(fsm, "Dash Recover", am.DashRecover);

            Utils.SpeedupAnimation(fsm, "Fall", am.Fall);

            Utils.SpeedupAnimation(fsm, "Jump 2", am.Jump);
            Utils.SpeedupAnimation(fsm, "Jump Antic 2", am.JumpAntic);
            Utils.ChangeRandomFloat(fsm, "Jump 2", 3, 60f * am.JumpY, 60f * am.JumpY);
            Utils.ChangeRandomFloat(fsm, "Aim Jump 2", 0, 25.5f, 27.5f * am.JumpX);

            Utils.SpeedupAnimation(fsm, "Jump", am.Jump);
            Utils.SpeedupAnimation(fsm, "Jump Antic", am.JumpAntic);
            Utils.ChangeRandomFloat(fsm, "Jump", 3, 60f * am.JumpY, 60f * am.JumpY);
            Utils.ChangeRandomFloat(fsm, "Jump 2", 0, 25.5f, 27.5f * am.JumpX);


            Utils.SpeedupAnimation(fsm, "Dstab Antic", am.DstabAntic);
            Utils.SpeedupAnimation(fsm, "Dstab Fall", am.DstabFall);
            Utils.ChangeSetVelocity2d(fsm, "Dstab Fall", 4, 0, -60f * am.DstabFall);
            Utils.SpeedupAnimation(fsm, "Dstab Land", am.DstabLand);
            Utils.SpeedupAnimation(fsm, "Dstab Recover", am.DstabRecover);

            Utils.SpeedupAnimation(fsm, "Idle", am.Walk);

            Utils.SpeedupAnimation(fsm, "Shaking Start", 2);

            Utils.ChangeWaitRandom(fsm, "Shaking Spawn", 1, am.ShakingSpawnWait, am.ShakingSpawnWait);
            Utils.ChangeRandomFloat(fsm, "Set Counter", 0, am.IdleCounterLower, am.IdleCounterUpper);

            Utils.ChangeRandomFloat(fsm, "Idle", 3, am.WalkSpeedLower, am.WalkSpeedUpper);

            Utils.ChangeWaitFloat(fsm, "Shaking Start", 4, am.ShakingStartWait);
            Utils.ChangeWaitFloat(fsm, "Shake End", 1, am.ShakeEnd);

        }

        private static void SuperBossLostKin(PlayMakerFSM fsm)
        {
            var downstabLandstate = SFCore.Utils.FsmUtil.GetState(fsm, "Dstab Land");
            if (downstabLandstate == null)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Failed to find Dstab Land");
                return;
            }

            SetAllNumbers(fsm);
            DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(downstabLandstate, 2, Vector3.zero);
            CreateLoopForDDark(fsm);
        }

        public static void UnpackModifiers()
        {
            _modifiers = AnimationModifiersLK.FromJsonFile();
        }

        private static void SetupTitleScream(PlayMakerFSM fsm)
        {
            FsmEditTemplates.FullScreenTitleDrop.SetupTitleUp(
                fsm,
                "Dark Descendent",
                "Roar",
                "Roar End",
                _modifiers.TitleFadeUpDelay,
                _modifiers.TitleFadeDownDelay,
                _modifiers.WhiteSolidUpTime,
                _modifiers.WhiteSolidUpDelay,
                UnityEngine.Color.black,
                UnityEngine.Color.white);
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

        public static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Changing ST phase 1");
            phaseChanged = false;

            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            // Adding invincibility during roar stuff.
            var rageRoarState = fsm.GetState("Roar");
            Satchel.FsmUtil.AddFirstAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

            var roarEndPhase = fsm.GetState("Roar End");
            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

            FsmState rageState = SetupRageSetupState(fsm, "Roar Start");
            AddPhaseChangeStep(fsm, new (){ "Idle", "Attack Choice", "Damage Response" }, rageState);
        }

        public static FsmState SetupRageSetupState(
    PlayMakerFSM fsm,
    string stateAfterRage)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Setup Rage state");
            var rageCheckState = fsm.AddState("Setup Rage");

            // Disabling staggers entirely for the fight
            var stagger = _bossObject.LocateMyFSM("Stun Control");
            stagger.enabled = false;

            rageCheckState.AddTransition("FINISHED", stateAfterRage);

            Satchel.FsmUtil.AddAction(rageCheckState, new Tk2dPlayAnimation() { clipName = "Stun" });
            Satchel.FsmUtil.AddAction(rageCheckState, new Wait() { time = 0.5f });
            Satchel.FsmUtil.AddAction(rageCheckState, new Tk2dPlayAnimation() { clipName = "Stun Recover" });

            rageCheckState.AddCustomAction(() =>
            {
                if (!phaseChanged)
                {
                    phaseChanged = true;
                    var rageRoarState = fsm.GetState("Roar");
                    rageRoarState.GetFirstActionOfType<Wait>().time = _modifiers.RoarWaitTime;

                    CorruptedDreamsPlandoMod.Instance.Log("Raging");
                    Grayscaler.TickCamera();
                    SetupTitleScream(fsm);
                    SuperBossLostKin(fsm);

                }
            });

            return rageCheckState;
        }
    }
}
