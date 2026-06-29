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
        public float ShotAnticAnimation;
        public float ShotAnticWait;

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
                ModifyAttackPatternsPhase1(fsm);
                ModifyVariablesPhase1(fsm);
                ModifyAnimationTimesPhase1(fsm);
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

        private static int shotCount = 0;
        // Move this number to modifiers.
        private static int maxShotCount = 4;

        private static void ModifyAttackPatternsPhase1(PlayMakerFSM fsm)
        {
            // shoots multiple fireballs each time
            // this phase should feel tame

            // Disable the "charge" attack. Leaving the "High Spinner" attack because... idk
            var attackChoiceState = fsm.GetState("Attack Choice");

            // The first attack option is the charge so we just make that if charge is rolled we do shoot instead
            var chooseAttackAtRandom = attackChoiceState.GetFirstActionOfType<SendRandomEventV3>();
            chooseAttackAtRandom.events[0] = chooseAttackAtRandom.events[2];

            // Increase the amount of fireballs spawned when doing fireballs
            // We do this by looping the "Shot Antic" and "Shot" states

            var setShotsState = fsm.GetState("Set Shots");
            setShotsState.AddCustomAction(() =>
            {
                shotCount = 0;
            });
            var shotState = fsm.GetState("Shot");

            //shotState.InsertCustomAction(() => {
            //    var spawnObject = GameObject.Find("Dream Mage Lord/Fire Effect");
            //    var tar = spawnObject.transform;
            //    if (shotCount == 0)
            //    {
            //        tar.SetPositionX(tar.GetPositionX() - 1f);
            //        tar.SetPositionY(tar.GetPositionY() - 0.5f);
            //    } else
            //    {
            //        tar.SetPositionX(tar.GetPositionX() + 0.5f);
            //        tar.SetPositionY(tar.GetPositionY() + (0.25f * (shotCount > (maxShotCount / 2) ? -1 : 1)));
            //    }

            //    //var projectile = fsm.GetVariable<FsmGameObject>("Projectile");
            //    //// (-1, -0.5), (-0.5, -0.25), (0, 0), (+0.5, -0.25), (+1, -0.5)
            //    //var tar = projectile.Value.transform;
            //    //tar.SetPositionX(tar.GetPositionX() +  (maxShotCount - shotCount) / 5);
            //    //if (shotCount == 0 || shotCount == maxShotCount)
            //    //{
            //    //    tar.SetPositionY(tar.GetPositionY() - 0.5f);
            //    //}
            //}, 0);

            shotState.AddTransition("REPEAT SHOT", "Shot Antic");

            shotState.AddCustomAction(() =>
            {
                shotCount++;
                if (shotCount > maxShotCount)
                {
                    fsm.SendEvent("FINISHED");
                }
                else
                {
                    fsm.SendEvent("REPEAT SHOT");
                }
            });
        }

        private static void ModifyAttackPatternsPhase2(PlayMakerFSM fsm)
        {
            // No build up
            // fireballs constantly spawning
            // the double slam thing with telegraph
            // 
            throw new NotImplementedException();
        }

        private static void ModifyVariablesPhase1(PlayMakerFSM fsm)
        {
            var healthManager = _bossObject.GetComponent<HealthManager>();
            healthManager.hp = (int)(_startingHp * _modifiers.PhaseChangeHpMultiplier);
        }

        private static void ModifyVariablesPhase2(PlayMakerFSM fsm)
        {
            throw new NotImplementedException();
        }

        private static void ModifyAnimationTimesPhase1(PlayMakerFSM fsm)
        {
            Utils.SpeedupAnimation(fsm, "Shot Antic", _modifiers.ShotAnticAnimation);


            Utils.ChangeWaitFloat(fsm, "Shot Antic", 0, _modifiers.ShotAnticWait);
            Utils.ChangeWaitFloat(fsm, "Shot", 10, _modifiers.ShotAnticWait);
        }

        private static void ModifyAnimationTimesPhase2(PlayMakerFSM fsm)
        {
            throw new NotImplementedException();
        }
    }
}
