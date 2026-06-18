using ItemChanger;
using ItemChanger.Locations.SpecialLocations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Satchel;
using Satchel.Futils;
using CorruptedDreamsPlando.FsmEditTemplates;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    public class AnimationModifiersWd
    {
        // Numbers for Phase Change bit
        // Need to move all of this to a JSON
        public float RoarWaitTime = 3.5f;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate = 0.5f;
        public float PhaseChangeHpMultiplier = 3.0f;


        public static AnimationModifiersWd FromJsonFile(string path)
        {
            if (path == "")
            {
                path = "C:\\clean\\corruptedWhiteDefenderModifiers.json";
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

        public static void Log(string s)
        {
            CorruptedDreamsPlandoMod.Instance.Log(s);
        }

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
            _modifiers = new AnimationModifiersWd();
        }

        public static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            phaseChanged = false;

            // TODO Animation Modifiers

            UnpackModifiers();

            SetupTitleScream(fsm);

            FsmState phaseChange = fsm.AddState("Phase Change");

            _bossObject = fsm.gameObject;

            // No need to disable stagger

            // Setup larger HP pool to have more time to trigger rage, we won't be using this entire pool.
            var healthManager = _bossObject.GetComponent<HealthManager>();
            _startingHp = healthManager.hp;
            healthManager.hp = 9999;
            _phase1Hp = (int)(_startingHp * _modifiers.PhaseChangeHpGate);
            var phaseTransitionGate = healthManager.hp - _phase1Hp;

            // White Defender already has a phase change under "Rage" so we will take advantage of that :)
            // Step 1 - introduce the new gate. Spell Stabilizer increases damage output by an order of magnitude so we need to be careful.
            fsm.GetVariable<FsmInt>("Rage HP").Value = phaseTransitionGate;

            // Step 2 - Actually modifying what "Rage" does
            var rageRoarState = fsm.GetState("Rage Roar");
            rageRoarState.GetFirstActionOfType<Wait>().time = _modifiers.RoarWaitTime;

            // TODO: Add gray scale thingy

            Satchel.FsmUtil.AddAction(rageRoarState, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

            rageRoarState.AddCustomAction(() =>
            {
                ModifyAnimationTimes(fsm);
                ModifyAttackPatterns(fsm);
                ModifyDungBalls(fsm);
                healthManager.hp = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);
            });


            var roarEndPhase = fsm.GetState("Set Rage");

            Satchel.FsmUtil.AddAction(roarEndPhase, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });
        }


        public static void SetupTitleScream(PlayMakerFSM fsm)
        {
            FsmEditTemplates.FullScreenTitleDrop.SetupTitleUp(
                fsm,
                "Shrieking Sentinel",
                "Rage Roar",
                "Set Rage",
                2.5f,
                0.5f,
                0.5f,
                2.5f,
                UnityEngine.Color.black,
                UnityEngine.Color.white);
        }

        public static void ModifyAnimationTimes(PlayMakerFSM fsm)
        {
            Log("ModifyAnimationTimes called");
        }

        public static void ModifyAttackPatterns(PlayMakerFSM fsm)
        {
            Log("ModifyAttackPatterns called");
        }

        public static void ModifyDungBalls(PlayMakerFSM fsm)
        {
            Log("ModifyDungBalls called");
        }
    }
}
