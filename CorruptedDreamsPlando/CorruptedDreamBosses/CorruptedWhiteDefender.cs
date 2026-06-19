using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Locations.SpecialLocations;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    public class AnimationModifiersWd
    {
        // FsmVariables
        public float DiveInSpeed;
        public float TunnelSpeed;
        public float EruptAnticTime;
        public float DolphinSpeed;

        // Throw Variables
        public float ThrowMin;
        public float ThrowMax;
        public float BallSpeedModifier;

        // Erupt up/down speed
        public float EruptSpeedUp;
        public float EruptSpeedDown;

        // Dolphin Pause
        public float DolphinPauseTime;

        // Animations speed up
        public float DiveIn1;
        public float DiveIn2;
        public float EruptUp;
        public float EruptDown;
        public float AnticR;
        public float ThrowAntic1;
        public float ThrowAntic2;
        public float ThrowAntic3;
        public float Throw1;
        public float Throw2;
        public float RjAntic;
        public float RjLaunch;
        public float DolphinIdle;
        public float DolphinAntic;
        public float DolphinDive;

        // Numbers for Phase Change bit
        public float RoarWaitTime;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate;
        public float PhaseChangeHpMultiplier;


        public static AnimationModifiersWd FromJsonFile(string path)
        {
            if (path == "")
            {
                path = "C:\\clean\\hkMods\\CorruptedDreamsPlando\\CorruptedDreamsPlando\\Resources\\bossVariables\\corruptedWhiteDefenderModifiers.json";
            }

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                AnimationModifiersWd items = JsonConvert.DeserializeObject<AnimationModifiersWd>(json);
                return items;
            }
        }
    }

    public class CorruptedWhiteDefender
    {
        private static readonly FsmID controlFSM = new("White Defender", "Dung Defender");

        private static bool phaseChanged;
        private static GameObject _bossObject;
        private static int _startingHp;
        private static int _phase1Hp;
        private static AnimationModifiersWd _modifiers;

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
            _modifiers = AnimationModifiersWd.FromJsonFile("");
        }

        public static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Changing WD");
            phaseChanged = false;


            CorruptedDreamsPlandoMod.Instance.Log("Unpacking Modifiers");
            UnpackModifiers();

            _bossObject = fsm.gameObject;

            // No need to disable stagger

            // Setup larger HP pool to have more time to trigger rage, we won't be using this entire pool.
            var healthManager = _bossObject.GetComponent<HealthManager>();
            _startingHp = healthManager.hp;
            healthManager.hp = 9999;
            _phase1Hp = 10; // (int)(_startingHp * 0.5f);
            var phaseTransitionGate = healthManager.hp - _phase1Hp;
            CorruptedDreamsPlandoMod.Instance.Log($"Will phase change after {_phase1Hp} damage and will have {_startingHp} * {_modifiers.PhaseChangeHpMultiplier} hp");

            // White Defender already has a phase change under "Rage" so we will take advantage of that :)
            // Step 1 - introduce the new gate. Spell Stabilizer increases damage output by an order of magnitude so we need to be careful.
            fsm.GetVariable<FsmInt>("Rage HP").Value = phaseTransitionGate;

            // Step 2 - Actually modifying what "Rage" does
            var rageState = fsm.GetState("Rage?");
            rageState.AddCustomAction(() =>
            {
                CorruptedDreamsPlandoMod.Instance.Log(_phase1Hp.ToString());
                CorruptedDreamsPlandoMod.Instance.Log(fsm.GetVariable<FsmInt>("Rage HP").Value.ToString());
            });

            var rageRoarState = fsm.GetState("Rage Roar");
            rageRoarState.GetFirstActionOfType<Wait>().time = _modifiers.RoarWaitTime;

            // TODO: Add gray scale thingy

            Satchel.FsmUtil.InsertAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 }, 0);

            rageRoarState.InsertCustomAction(() =>
            {
                Grayscaler.TickCamera();
                CorruptedDreamsPlandoMod.Instance.Log("Raging");
                ModifyAnimationTimes(fsm);
                ModifyVariables(fsm);
                ModifyAttackPatterns(fsm);
                ModifyDungBalls(fsm);
                ModifyRollJump(fsm);
                healthManager.hp = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);
            }, 0);


            var roarEndPhase = fsm.GetState("Set Rage");

            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });
            CorruptedDreamsPlandoMod.Instance.Log("Setting up title");
            SetupTitleScream(fsm);
        }

        public static void SetupTitleScream(PlayMakerFSM fsm)
        {
            FsmEditTemplates.FullScreenTitleDrop.SetupTitleUp(
                fsm,
                "Shrieking Sentinel",
                "Rage Roar",
                "Set Rage",
                _modifiers.TitleFadeUpDelay,
                _modifiers.TitleFadeDownDelay,
                _modifiers.WhiteSolidUpTime,
                _modifiers.WhiteSolidUpDelay,
                UnityEngine.Color.black,
                UnityEngine.Color.white);
        }

        private static void SpeedupAnimation(PlayMakerFSM fsm, string animationName, AnimationAccelerator accel, float animationSpeed, int index = -1)
        {
            try
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Changing {animationName}");
                SFCore.Utils.FsmUtil.GetState(fsm, animationName).AccelerateAnimation(accel, animationSpeed, index);
            } catch (Exception e)
            {
                CorruptedDreamsPlandoMod.Instance.Log("Error");
            }
        }

        private static void ModifyVariables(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("ModifyVariables called");
            ChangeSetFloatValue(fsm, "Dive In 1", 1, _modifiers.TunnelSpeed);

            ChangeWaitFloat(fsm, "Erupt Antic R", 0, _modifiers.EruptAnticTime);
            ChangeWaitFloat(fsm, "To Dolph Pause", 1, _modifiers.DolphinPauseTime);

            ChangeSetVelocity2d(fsm, "Erupt Out", 12, 0, _modifiers.EruptSpeedUp);

            ChangeRandomFloat(fsm, "Set Throws", 0, _modifiers.ThrowMax, _modifiers.ThrowMin);

            fsm.GetVariable<FsmFloat>("X Scale").Value = _modifiers.BallSpeedModifier;
            fsm.GetVariable<FsmFloat>("Dolphin Speed Crt").Value *= _modifiers.DolphinSpeed;

        }

        public static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("ModifyAnimationTimes called");
            AnimationAccelerator accel = fsm.gameObject.GetComponent<AnimationAccelerator>();
            if (accel == null)
            {
                accel = fsm.gameObject.AddComponent<AnimationAccelerator>();
            }

            SpeedupAnimation(fsm, "Idle", accel, _modifiers.DolphinIdle);
            SpeedupAnimation(fsm, "Dive In 1", accel, _modifiers.DiveIn1);
            SpeedupAnimation(fsm, "Dive In 2", accel, _modifiers.DiveIn2);
            SpeedupAnimation(fsm, "Erupt Out", accel, _modifiers.EruptUp);
            SpeedupAnimation(fsm, "Erupt Fall", accel, _modifiers.EruptDown);
            SpeedupAnimation(fsm, "Erupt Land", accel, _modifiers.AnticR);
            SpeedupAnimation(fsm, "Throw Antic 1", accel, _modifiers.ThrowAntic1);
            SpeedupAnimation(fsm, "Throw Antic 2", accel, _modifiers.ThrowAntic2);
            SpeedupAnimation(fsm, "Throw Antic 3", accel, _modifiers.ThrowAntic3);
            SpeedupAnimation(fsm, "Throw 1", accel, _modifiers.Throw1);
            SpeedupAnimation(fsm, "Throw 2", accel, _modifiers.Throw2);
            SpeedupAnimation(fsm, "RJ Antic", accel, _modifiers.RjAntic);
            SpeedupAnimation(fsm, "RJ Launch", accel, _modifiers.RjLaunch);
            SpeedupAnimation(fsm, "To Dolph Pause", accel, _modifiers.DolphinIdle);
            //
            SpeedupAnimation(fsm, "Dolph Antic", accel, _modifiers.DolphinAntic);
            SpeedupAnimation(fsm, "Diving", accel, _modifiers.DolphinDive);
            SpeedupAnimation(fsm, "Dolphin Recover", accel, _modifiers.AnticR);

        }

        public static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("ModifyAttackPatterns called");
            // Erupt causes a shriek
            var eruptOut = fsm.GetState("Erupt Out");
            AbyssShriekSpawner.Instance.AddShriekAtEndOfFSM(eruptOut, 2, new Vector3(0, 6, 0));
        }

        public static void ModifyRollJump(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("ModifyRollJump called");
            // No need to handle the dive in from roll flow bc that already handles invincibility
            var invulnStartState = fsm.GetState("RJ Launch");
            var invulnStopState = fsm.GetState("RJ Unroll");


            Satchel.FsmUtil.AddAction(invulnStartState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });
            Satchel.FsmUtil.AddAction(invulnStopState, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

        }

        public static void ModifyDungBalls(PlayMakerFSM fsm)
        {
            // Might need to make this not be destroyed by projectiles
            CorruptedDreamsPlandoMod.Instance.Log("ModifyDungBalls called");
            var eruptOut = fsm.GetState("Erupt Out");
            var speedSets = eruptOut.GetActions<SetVelocity2d>();
            foreach (var set in speedSets)
            {
                // Mr dung defender is the only thing that doesnt have x speed, so this just changes his balls
               //set.x.Value *= _modifiers.BallSpeedModifier;
            }

            // Balls deal double damage to hero.
            // TODO (2nd Phase): Make them actually restrict soul usage.
            var damageSets = eruptOut.GetActions<SetDamageHeroAmount>();
            foreach (var set in damageSets)
            {
                // Mr dung defender is the only thing that doesnt have x speed, so this just changes his balls
                set.damageDealt.Value = 2;
            }
        }


        private static void ChangeSetFloatValue(PlayMakerFSM fsm, string stateName, int actionIndex, float newValue)
        {
            var setState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setAction = setState.Actions[actionIndex];
            if (setAction is SetFloatValue)
            {
                (setAction as SetFloatValue).floatValue = newValue;
            }
        }

        private static void ChangeWaitFloat(PlayMakerFSM fsm, string stateName, int actionIndex, float newWaitTime)
        {
            var waitState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var waitAction = waitState.Actions[actionIndex];
            if (waitAction is Wait)
            {
                (waitAction as Wait).time = newWaitTime;
            }
        }
        private static void ChangeSetVelocity2d(PlayMakerFSM fsm, string stateName, int actionIndex, float x, float y)
        {
            var setVeloState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setVeloAction = setVeloState.Actions[actionIndex];
            if (setVeloAction is SetVelocity2d)
            {
                (setVeloAction as SetVelocity2d).x = x;
                (setVeloAction as SetVelocity2d).y = y;
            }
        }

        private static void MultiplySetVelocity2d(PlayMakerFSM fsm, string stateName, int actionIndex, float mod)
        {
            var setVeloState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setVeloAction = setVeloState.Actions[actionIndex];
            if (setVeloAction is SetVelocity2d)
            {
                (setVeloAction as SetVelocity2d).x.Value *= mod;
                (setVeloAction as SetVelocity2d).y.Value *= mod;
            }
        }

        private static void ChangeRandomFloat(PlayMakerFSM fsm, string stateName, int actionIndex, float upperBound, float lowerBound)
        {
            var setCounterState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var rFloat = setCounterState.Actions[actionIndex];
            if (rFloat is RandomFloat)
            {
                (rFloat as RandomFloat).min = upperBound;
                (rFloat as RandomFloat).max = lowerBound;
            }
        }
    }
}
