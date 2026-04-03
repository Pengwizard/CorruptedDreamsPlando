using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Newtonsoft.Json;
using Satchel;
using SFCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    public class AnimationModifiers
    {
        // float modifiers
        public float DashSpeed;
        public float DashReverse;
        public float Gravity;
        public float JumpY;
        public float JumpX;


        // animation speed modifiers
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
        public float RoadWaitTime;
        public float WhiteSolidUpTime;
        public float WhiteSolidUpDelay;
        public float TitleFadeUpDelay;
        public float TitleFadeDownDelay;
        public float PhaseChangeHpGate;
        public float PhaseChangeHpMultiplier;


        public static AnimationModifiers FromJsonFile(string path)
        {
            if (path == "")
            {
                path = "C:\\clean\\corruptedLostKinModifiers.json";
            }

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                AnimationModifiers items = JsonConvert.DeserializeObject<AnimationModifiers>(json);
                return items;
            }
        }

        public override string ToString()
        {
            return $"DashSpeed = {DashSpeed}, " +
                   $"DashReverse = {DashReverse}, " +
                   $"Gravity = {Gravity}, " +
                   $"JumpY = {JumpY}, " +
                   $"JumpX = {JumpX}, " +
                   $"EvadeAntic = {EvadeAntic}, " +
                   $"Evade = {Evade}, " +
                   $"EvadeRecover = {EvadeRecover}, " +
                   $"OheadAntic = {OheadAntic}, " +
                   $"OheadSlashing = {OheadSlashing}, " +
                   $"DashAntic = {DashAntic}, " +
                   $"DashAntic2 = {DashAntic2}, " +
                   $"DashAntic3 = {DashAntic3}, " +
                   $"DashAttack = {DashAttack}, " +
                   $"DashAttack2 = {DashAttack2}, " +
                   $"DashAttack3 = {DashAttack3}, " +
                   $"DashRecover = {DashRecover}, " +
                   $"Fall = {Fall}, " +
                   $"Jump = {Jump}, " +
                   $"JumpAntic = {JumpAntic}, " +
                   $"DstabAntic = {DstabAntic}, " +
                   $"DstabFall = {DstabFall}, " +
                   $"DstabLand = {DstabLand}, " +
                   $"DstabRecover = {DstabRecover}, " +
                   $"Walk = {Walk}";
        }
    }

    public class CorruptedLostKin
    {
        private static readonly FsmID lostKinControl = new("Lost Kin", "IK Control");
        private static GameObject _ddarkSlam;
        private static GameObject _ddarkPillar;
        private static GameObject _ddarkMega;

        private static GameObject _lostKinObject;
        private static int _startingHp = 0;
        private static FsmObject _ddarkAudio = null;
        private static bool phaseChanged = false;



        public static GameObject BossTitlePrefab;
        public static GameObject WhiteSolidPrefab;


        public static GameObject DDarkSlam
        {
            get
            {
                if (_ddarkSlam == null)
                {
                    _ddarkSlam = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Q Slam 2").gameObject);
                    GameObject.DontDestroyOnLoad(_ddarkSlam);
                }
                return _ddarkSlam;
            }
        }

        public static GameObject DDarkPillar
        {
            get
            {
                if (_ddarkPillar == null)
                {
                    _ddarkPillar = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Q Pillar").gameObject);
                    GameObject.DontDestroyOnLoad(_ddarkPillar);
                }
                return _ddarkPillar;
            }
        }

        public static GameObject DDarkMega
        {
            get
            {
                if (_ddarkMega == null)
                {
                    _ddarkMega = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Q Mega").gameObject);
                    GameObject.DontDestroyOnLoad(_ddarkMega);
                }
                return _ddarkMega;
            }
        }

        public static void Initialize()
        {
            Events.AddFsmEdit(lostKinControl, SetupPhaseChange);
            // Events.AddFsmEdit(lostKinControl, SuperBossLostKin);

            On.HutongGames.PlayMaker.Actions.AudioPlay.OnEnter += PlayAudioEvent_OnEnter;
        }

        private static void PlayAudioEvent_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlay.orig_OnEnter orig, AudioPlay self)
        {
            if (self.Fsm.FsmComponent.gameObject.name.Contains("Knight") &&
                self.Fsm.Name.Contains("Spell Control") &&
                self.State.Name == "Q2 Land")
            {
                _ddarkAudio = self.oneShotClip;
            }
            orig(self);
        }

        public static void Unload()
        {
            Events.AddFsmEdit(lostKinControl, SetupPhaseChange);
            Events.RemoveFsmEdit(lostKinControl, SuperBossLostKin);
        }

        private static void MakeInvuln()
        {
            // CorruptedDreamsPlandoMod.Instance.Log($"Setting invuln {DateTime.Now}");
            _lostKinObject.layer = LayerMask.NameToLayer("Enemy Attack");
        }

        private static void MakeVuln()
        {
            // CorruptedDreamsPlandoMod.Instance.Log($"Setting to enemy {DateTime.Now}");
            _lostKinObject.layer = LayerMask.NameToLayer("Enemies");
        }

        private static void MultiplyFsmFloat(FsmFloat f, float mul)
        {
            // CorruptedDreamsPlandoMod.Instance.Log($"Setting {f.Name} from {f.Value} to {f.Value * mul}");
            f.Value = f.Value * mul;
        }

        private static void SpeedupAnimation(PlayMakerFSM fsm, string animationName, AnimationAccelerator accel, float animationSpeed, int index=-1)
        {
            if (index >= 0)
            {
                // CorruptedDreamsPlandoMod.Instance.Log($"Setting {animationName} from {fsm} to be faster as index is {index}");
            } else
            {
                // CorruptedDreamsPlandoMod.Instance.Log($"Setting {animationName} from {fsm} to be faster");
            }
            SFCore.Utils.FsmUtil.GetState(fsm, animationName).AccelerateAnimation(accel, animationSpeed, index);
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

        private static void ChangeWaitRandom(PlayMakerFSM fsm, string stateName, int actionIndex, float upperBound, float lowerBound)
        {
            var setCounterState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var rFloat = setCounterState.Actions[actionIndex];
            if (rFloat is WaitRandom)
            {
                (rFloat as WaitRandom).timeMin = upperBound;
                (rFloat as WaitRandom).timeMax = lowerBound;
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

        private static void ChangeWaitFloat(PlayMakerFSM fsm, string stateName, int actionIndex, float newWaitTime)
        {
            var waitState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var waitAction = waitState.Actions[actionIndex];
            if (waitAction is Wait)
            {
                (waitAction as Wait).time = newWaitTime;
            }
        }

        private static void CreateLoopForDDark(PlayMakerFSM fsm)
        {
            // This first bit is to make it so even if the player isn't in "dstab range" we still ddark
            //FsmState inAirState = SFCore.Utils.FsmUtil.GetState(fsm, "In Air");
            //FsmStateAction checkAirState = inAirState.Actions[7];
            //if (checkAirState is BoolAllTrue)
            //{
            //    var real = checkAirState as BoolAllTrue;
            //    real.boolVariables[2] = real.boolVariables[0];
            //}

            FsmState dStabRecover = SFCore.Utils.FsmUtil.GetState(fsm, "Dstab Recover");
            ChangeWaitFloat(fsm, "Dstab Recover", 0, 0.1f);

            FsmState dStabLoop = SFCore.Utils.FsmUtil.AddState(fsm, "Dstab Loop");
            FsmState dStabPause = SFCore.Utils.FsmUtil.AddState(fsm, "Dstab Pause");

            SFCore.Utils.FsmUtil.RemoveTransition(dStabRecover, "FINISHED");
            SFCore.Utils.FsmUtil.AddTransition(dStabRecover, "FINISHED", "Dstab Loop");
            FsmEvent doRepeat = new("DOREPEAT");
            FsmEvent dontRepeat = new("DONTREPEAT");

            SFCore.Utils.FsmUtil.AddTransition(dStabLoop, "DOREPEAT", "Set Dstab");
            SFCore.Utils.FsmUtil.AddTransition(dStabLoop, "DONTREPEAT", "Dstab Pause");

            SFCore.Utils.FsmUtil.AddTransition(dStabPause, "FINISHED", "Set Counter");

            var ddarkLoops = fsm.AddFsmInt("DDark Loops", 0);
            Satchel.FsmUtil.AddAction(dStabLoop, new Lambda(() => {
                FsmInt loops = fsm.FsmVariables.GetFsmInt("DDark Loops");
                loops.Value = loops.Value + 1;
                fsm.SendEvent(loops.Value > 3 ? "DONTREPEAT" : "DOREPEAT");
            }));

            Satchel.FsmUtil.AddAction(dStabPause, new Wait() {time=0.35f});
            Satchel.FsmUtil.AddAction(dStabPause, new Lambda(() => {
                FsmInt loops = fsm.FsmVariables.GetFsmInt("DDark Loops");
                loops.Value = 0;
            }));
        }

        private static void SetAllNumbers(PlayMakerFSM fsm)
        {
            AnimationModifiers am = AnimationModifiers.FromJsonFile("");
            // CorruptedDreamsPlandoMod.Instance.Log($"{am}");

            CreateLoopForDDark(fsm);

            var accel = fsm.gameObject.GetOrAddComponent<AnimationAccelerator>();

            List<string> fsmFloatNames = new List<string>(){"WalkSpeed", "Evade Speed Crt"};


            MultiplyFsmFloat(fsm.GetFloatVariable("Dash Speed"), am.DashSpeed);
            MultiplyFsmFloat(fsm.GetFloatVariable("Dash Reverse"), am.DashReverse);
            MultiplyFsmFloat(fsm.GetFloatVariable("Gravity"), am.Gravity);
            // MultiplyFsmFloat(fsm.GetFloatVariable("Jump X"), am.JumpX);
            // MultiplyFsmFloat(fsm.GetFloatVariable("Jump Y"), am.JumpY);

            SpeedupAnimation(fsm, "Evade Antic", accel, am.EvadeAntic);
            SpeedupAnimation(fsm, "Evade Dash", accel, am.Evade);
            SpeedupAnimation(fsm, "Evade Recover", accel, am.EvadeRecover);

            SpeedupAnimation(fsm, "Ohead Antic", accel, am.OheadAntic);
            ChangeWaitFloat(fsm, "Ohead Antic", 2, am.OheadAnticWait);

            SpeedupAnimation(fsm, "Ohead Slashing", accel, am.OheadSlashing);
            SpeedupAnimation(fsm, "Ohead Slashing", accel, am.OheadSlashing2, 7);

            SpeedupAnimation(fsm, "Dash Antic", accel, am.DashAntic);

            SpeedupAnimation(fsm, "Dash Antic 2", accel, am.DashAntic2);
            ChangeWaitFloat(fsm, "Dash Antic 2", 2, am.DashAntic2Wait);
            SpeedupAnimation(fsm, "Dash Antic 3", accel, am.DashAntic3);

            SpeedupAnimation(fsm, "Dash Attack", accel, am.DashAttack);
            SpeedupAnimation(fsm, "Dash Attack 2", accel, am.DashAttack2);
            SpeedupAnimation(fsm, "Dash Attack 3", accel, am.DashAttack3);

            SpeedupAnimation(fsm, "Dash Recover", accel, am.DashRecover);

            SpeedupAnimation(fsm, "Fall", accel, am.Fall);

            SpeedupAnimation(fsm, "Jump 2", accel, am.Jump);
            SpeedupAnimation(fsm, "Jump Antic 2", accel, am.JumpAntic);
            ChangeRandomFloat(fsm, "Jump 2", 3, 60f * am.JumpY, 60f * am.JumpY);
            ChangeRandomFloat(fsm, "Aim Jump 2", 0, 25.5f, 27.5f * am.JumpX);

            SpeedupAnimation(fsm, "Jump", accel, am.Jump);
            SpeedupAnimation(fsm, "Jump Antic", accel, am.JumpAntic);
            ChangeRandomFloat(fsm, "Jump", 3, 60f * am.JumpY, 60f * am.JumpY);
            ChangeRandomFloat(fsm, "Jump 2", 0, 25.5f, 27.5f * am.JumpX);


            SpeedupAnimation(fsm, "Dstab Antic", accel, am.DstabAntic);
            SpeedupAnimation(fsm, "Dstab Fall", accel, am.DstabFall);
            ChangeSetVelocity2d(fsm, "Dstab Fall", 4, 0, -60f * am.DstabFall);
            SpeedupAnimation(fsm, "Dstab Land", accel, am.DstabLand);
            SpeedupAnimation(fsm, "Dstab Recover", accel, am.DstabRecover);

            SpeedupAnimation(fsm, "Idle", accel, am.Walk);

            SpeedupAnimation(fsm, "Shaking Start", accel, 2);


            ChangeWaitRandom(fsm, "Shaking Spawn", 1, am.ShakingSpawnWait, am.ShakingSpawnWait);
            ChangeRandomFloat(fsm, "Set Counter", 0, am.IdleCounterLower, am.IdleCounterUpper);


            ChangeRandomFloat(fsm, "Idle", 3, am.WalkSpeedLower, am.WalkSpeedUpper);

            ChangeWaitFloat(fsm, "Shaking Start", 4, am.ShakingStartWait);
            ChangeWaitFloat(fsm, "Shake End", 1, am.ShakeEnd);

        }

        private static void SuperBossLostKin(PlayMakerFSM fsm)
        {
            var downstabLandstate = SFCore.Utils.FsmUtil.GetState(fsm, "Dstab Land");
            if (downstabLandstate == null)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Failed to find Dstab Land");
                return;
            }

            // Trying to make Lost kin "slam" spawn a DDark
            // downstabLandstate.AddCustomAction(MakeInvuln);

            SetAllNumbers(fsm);

            GameObject slam = GameObject.Instantiate(DDarkSlam);
            GameObject pillar = GameObject.Instantiate(DDarkPillar);
            GameObject mega = GameObject.Instantiate(DDarkMega);

            var lostKin = fsm.gameObject;
            var lkTransform = lostKin.transform;

            slam.transform.SetParent(lkTransform);
            slam.transform.position = lkTransform.position + new Vector3(0, 2, 0);
            pillar.transform.SetParent(lkTransform);
            pillar.transform.position = lkTransform.position;
            mega.transform.SetParent(lkTransform);
            mega.transform.position = lkTransform.position + new Vector3(0, 6, 0);

            //slam.SetActive(true);
            //pillar.SetActive(true);
            //mega.SetActive(true);

            GameObject[] go = { slam, pillar, mega };

            try
            {
                if (_ddarkAudio != null)
                {
                    AudioPlay playDiveAudio = new AudioPlay();
                    playDiveAudio.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = fsm.gameObject }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
                    // TODO: Audio needs to be determined by the game
                    playDiveAudio.volume = 5f;
                    playDiveAudio.oneShotClip = _ddarkAudio;

                    Satchel.FsmUtil.AddAction(downstabLandstate, playDiveAudio);
                }
            }
            catch (Exception e)
            {

            }

            foreach (var part in go) {
                List<GameObject> children = new List<GameObject>();
                part.FindAllChildren(children);
                foreach (var c in children)
                {
                    // CorruptedDreamsPlandoMod.Instance.Log($"{c.name}");
                    c.layer = LayerMask.NameToLayer("Enemy Attack");
                    c.RemoveComponent<DamageEnemies>();
                    var damageHero = c.AddComponent<DamageHero>();
                    damageHero.damageDealt = 2;
                }
            }

            foreach (var damageHero in _lostKinObject.GetComponentsInChildren<DamageHero>(true))
            {
                damageHero.damageDealt = 2;
            }

            ActivateGameObject activateSlam = new ActivateGameObject();
            activateSlam.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = slam }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            activateSlam.activate = true;
            activateSlam.recursive = false;
            activateSlam.resetOnExit = false;
            activateSlam.everyFrame = false;

            ActivateGameObject activatePillar = new ActivateGameObject();
            activatePillar.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = pillar }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            activatePillar.activate = true;
            activatePillar.recursive = false;
            activatePillar.resetOnExit = false;
            activatePillar.everyFrame = false;

            ActivateGameObject activateMega = new ActivateGameObject();
            activateMega.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = mega }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            activateMega.activate = true;
            activateMega.recursive = false;
            activateMega.resetOnExit = false;
            activateMega.everyFrame = false;

            Satchel.FsmUtil.AddCustomAction(downstabLandstate, MakeInvuln);
            Satchel.FsmUtil.AddAction(downstabLandstate, activateSlam);
            Satchel.FsmUtil.AddAction(downstabLandstate, new Wait() { time = 0.75f});
            Satchel.FsmUtil.AddAction(downstabLandstate, activatePillar);
            Satchel.FsmUtil.AddAction(downstabLandstate, new Wait() { time = 0.75f});
            Satchel.FsmUtil.AddAction(downstabLandstate, activateMega);
            Satchel.FsmUtil.AddAction(downstabLandstate, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddCustomAction(downstabLandstate, MakeVuln);
        }

        private static void SetupTitleUp(PlayMakerFSM fsm, AnimationModifiers am)
        {
            FsmState flashUp = SFCore.Utils.FsmUtil.AddState(fsm, "Flash Up");
            FsmState flashDown = SFCore.Utils.FsmUtil.AddState(fsm, "Flash Down");

            SFCore.Utils.FsmUtil.AddTransition(flashUp, "FINISHED", "Flash Down");
            SFCore.Utils.FsmUtil.AddTransition(flashDown, "FINISHED", "Roar End");


            GameObject whiteFader = UObject.Instantiate(WhiteSolidPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var whiteFaderSpriteRenderer = whiteFader.GetComponent<SpriteRenderer>();
            whiteFaderSpriteRenderer.color = Color.black;
            whiteFader.transform.position = new Vector3(30, 30, 0);

            GameObject bossTitle = UObject.Instantiate(BossTitlePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            bossTitle.SetActive(true);
            var bossTitleTMP = bossTitle.GetComponent<TextMeshPro>();
            bossTitleTMP.color = Color.white;
            bossTitleTMP.SetText("Dark Descendent");
            foreach (var g in bossTitle.GetComponentsInChildren<TextMeshPro>())
            {
                g.color = Color.white;
            }

            whiteFader.SetActive(true);
            // whiteFader.SetActive(false);
            whiteFader.GetComponent<SpriteRenderer>().enabled = true;
            // whiteFader.GetComponent<SpriteRenderer>().enabled = false;

            var faderFSM = whiteFader.LocateMyFSM("FSM");
            faderFSM.GetFloatVariable("Up Time").Value = am.WhiteSolidUpTime;
            faderFSM.GetFloatVariable("Up Delay").Value = am.WhiteSolidUpDelay;

            FadeGroupUp slot2FadeGroup = new FadeGroupUp();
            slot2FadeGroup.target = new FsmOwnerDefault() { GameObject = new FsmGameObject { Value = bossTitle }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };

            FadeGroupDown slot4FadeGroup = new FadeGroupDown();
            slot4FadeGroup.target = new FsmOwnerDefault() { GameObject = new FsmGameObject { Value = bossTitle }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            slot4FadeGroup.fast = false;

            Satchel.FsmUtil.AddAction(flashUp, new Lambda(() => {
                CorruptedDreamsPlandoMod.Instance.Log($"Start flash up");
            }));

            // Satchel.FsmUtil.AddAction(flashUp, slot0FlashUp);
            Satchel.FsmUtil.AddAction(flashUp, new Wait() { time = 1f});
            Satchel.FsmUtil.AddAction(flashUp, new Lambda(() => {
                whiteFader.LocateMyFSM("FSM").SetState("Init");
            }));

            Satchel.FsmUtil.AddAction(flashUp, slot2FadeGroup);
            
            Satchel.FsmUtil.AddAction(flashUp, new Wait() { time = am.TitleFadeUpDelay });


            Satchel.FsmUtil.AddAction(flashDown, slot4FadeGroup);
            Satchel.FsmUtil.AddAction(flashDown, new Wait() { time = am.TitleFadeDownDelay });
        }

        private static void DoPhaseChange(PlayMakerFSM fsm, AnimationModifiers am)
        {
            //var alreadyPhaseChanged = fsm.GetBoolVariable("Phase Change Triggered");
            //alreadyPhaseChanged = true;
            CorruptedDreamsPlandoMod.Instance.Log($"Changing phase for the first time");
            var healthManager = _lostKinObject.GetComponent<HealthManager>();
            healthManager.hp = (int)(_startingHp * am.PhaseChangeHpMultiplier);
        }

        private static void SetupPhaseChange(PlayMakerFSM fsm)
        {
            phaseChanged = false;
            AnimationModifiers am = AnimationModifiers.FromJsonFile("");
            SetupTitleUp(fsm, am);

            FsmState phaseChange = SFCore.Utils.FsmUtil.AddState(fsm, "Phase Change");

            _lostKinObject = fsm.gameObject;
            var stagger = _lostKinObject.LocateMyFSM("Stun Control");
            stagger.enabled = false;

            var damageResponseState = SFCore.Utils.FsmUtil.GetState(fsm, "Damage Response");
            var healthManager = _lostKinObject.GetComponent<HealthManager>();
            _startingHp = healthManager.hp;

            // Giving us a larger detection buffer.
            healthManager.hp = _startingHp + (int)(_startingHp * am.PhaseChangeHpGate);


            fsm.AddFsmBool("Phase Change Triggered", false);
            SFCore.Utils.FsmUtil.AddTransition(damageResponseState, "TRIGGER_PHASE_CHANGE", "Phase Change");
            FsmEvent phaseChangeDone = new("PHASE_CHANGE_DONE");
            SFCore.Utils.FsmUtil.AddTransition(phaseChange, "FINISHED", "Roar Start");


            Satchel.FsmUtil.AddFirstAction(damageResponseState, new Lambda(() => {
                if (phaseChanged == true)
                {
                    // Nothing to do here...
                    return;
                }

                var healthManager = _lostKinObject.GetComponent<HealthManager>();
                CorruptedDreamsPlandoMod.Instance.Log($"current hp is {healthManager.hp}, and starting hp is {_startingHp}");
                if (healthManager.hp < _startingHp)
                {
                    fsm.SendEvent("TRIGGER_PHASE_CHANGE");
                }
            }));

            Satchel.FsmUtil.AddAction(phaseChange, new Lambda(() => {
                _lostKinObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                DoPhaseChange(fsm, am);

                var roarStart = SFCore.Utils.FsmUtil.GetState(fsm, "Roar Start");
                Satchel.FsmUtil.AddAction(roarStart, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });

                var roarState = SFCore.Utils.FsmUtil.GetState(fsm, "Roar");
                var roarWait = roarState.Actions[5] as Wait;
                // TODO Add to json
                roarWait.time = am.RoadWaitTime;

                SFCore.Utils.FsmUtil.RemoveTransition(roarState, "FINISHED");
                SFCore.Utils.FsmUtil.AddTransition(roarState, "FINISHED", "Flash Up");

            }));

            Satchel.FsmUtil.AddAction(phaseChange, new Tk2dPlayAnimation() { clipName = "Stun" });
            Satchel.FsmUtil.AddAction(phaseChange, new Wait() { time = 3f});
            Satchel.FsmUtil.AddAction(phaseChange, new Tk2dPlayAnimation() { clipName = "Stun Recover" });

            Satchel.FsmUtil.AddAction(phaseChange, new Lambda(() => {
                fsm.SetState("Roar Start");
                SuperBossLostKin(fsm);
                stagger.enabled = true;
                phaseChanged = true;
            }));
        }
    }
}
