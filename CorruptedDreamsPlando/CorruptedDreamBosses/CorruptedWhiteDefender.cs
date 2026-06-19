using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using System.IO;
using UnityEngine;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    internal class AnimationModifiersWd
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
            _phase1Hp = (int)(_startingHp * 0.5f);
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


        private static void ModifyVariables(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("ModifyVariables called");
            Utils.ChangeSetFloatValue(fsm, "Dive In 1", 1, _modifiers.TunnelSpeed);

            Utils.ChangeWaitFloat(fsm, "Erupt Antic R", 0, _modifiers.EruptAnticTime);
            Utils.ChangeWaitFloat(fsm, "To Dolph Pause", 1, _modifiers.DolphinPauseTime);

            Utils.ChangeSetVelocity2d(fsm, "Erupt Out", 12, 0, _modifiers.EruptSpeedUp);

            Utils.ChangeRandomFloat(fsm, "Set Throws", 0, _modifiers.ThrowMax, _modifiers.ThrowMin);

            fsm.GetVariable<FsmFloat>("X Scale").Value = _modifiers.BallSpeedModifier;
            fsm.GetVariable<FsmFloat>("Dolphin Speed Crt").Value *= _modifiers.DolphinSpeed;

        }

        public static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log("ModifyAnimationTimes called");

            Utils.SpeedupAnimation(fsm, "Idle", _modifiers.DolphinIdle);
            Utils.SpeedupAnimation(fsm, "Dive In 1", _modifiers.DiveIn1);
            Utils.SpeedupAnimation(fsm, "Dive In 2", _modifiers.DiveIn2);
            Utils.SpeedupAnimation(fsm, "Erupt Out", _modifiers.EruptUp);
            Utils.SpeedupAnimation(fsm, "Erupt Fall", _modifiers.EruptDown);
            Utils.SpeedupAnimation(fsm, "Erupt Land", _modifiers.AnticR);
            Utils.SpeedupAnimation(fsm, "Throw Antic 1", _modifiers.ThrowAntic1);
            Utils.SpeedupAnimation(fsm, "Throw Antic 2", _modifiers.ThrowAntic2);
            Utils.SpeedupAnimation(fsm, "Throw Antic 3", _modifiers.ThrowAntic3);
            Utils.SpeedupAnimation(fsm, "Throw 1", _modifiers.Throw1);
            Utils.SpeedupAnimation(fsm, "Throw 2", _modifiers.Throw2);
            Utils.SpeedupAnimation(fsm, "RJ Antic", _modifiers.RjAntic);
            Utils.SpeedupAnimation(fsm, "RJ Launch", _modifiers.RjLaunch);
            Utils.SpeedupAnimation(fsm, "To Dolph Pause", _modifiers.DolphinIdle);

            Utils.SpeedupAnimation(fsm, "Dolph Antic", _modifiers.DolphinAntic);
            Utils.SpeedupAnimation(fsm, "Diving", _modifiers.DolphinDive);
            Utils.SpeedupAnimation(fsm, "Dolphin Recover", _modifiers.AnticR);

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


    }
}
