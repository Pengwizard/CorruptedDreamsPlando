using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using RandomizerMod.RandomizerData;
using Satchel;
using SFCore;
using SFCore.Utils;
using System;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    public class CorruptedLostKin
    {
        private static readonly FsmID lostKinControl = new("Lost Kin", "IK Control");
        private static GameObject _ddarkSlam;
        private static GameObject _ddarkPillar;
        private static GameObject _ddarkMega;

        private static GameObject _lostKinObject;
        private static FsmObject _ddarkAudio = null;

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
            Events.AddFsmEdit(lostKinControl, SuperBossLostKin);
            On.HutongGames.PlayMaker.Actions.AudioPlay.OnEnter += PlayAudioEvent_OnEnter;
        }

        private static void PlayAudioEvent_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlay.orig_OnEnter orig, AudioPlay self)
        {
            if (self.Fsm.FsmComponent.gameObject.name.Contains("Knight") &&
                self.Fsm.Name.Contains("Spell Control") &&
                self.State.Name == "Q2 Land")
            {
                CorruptedDreamsPlandoMod.Instance.LogError($"Playing audio {self} {self.oneShotClip.Name}");
                _ddarkAudio = self.oneShotClip;
            }
            orig(self);
        }

        public static void Unload()
        {
            Events.RemoveFsmEdit(lostKinControl, SuperBossLostKin);
        }

        private static void PrintInsideDStab()
        {
            _lostKinObject.layer = LayerMask.NameToLayer("Enemy Attack");
        }

        private static void PrintInsideDStabDone()
        {
            _lostKinObject.layer = LayerMask.NameToLayer("Enemies");
        }

        private static FsmObject FindKnightDDarkAudio()
        {
            var knight = HeroController.instance.transform.Find("Knight").gameObject;
            var spellControl = knight.LocateMyFSM("Spell Control");
            var q2Land = Satchel.FsmUtil.GetState(spellControl, "Q2 Land");
            var audioPlay = q2Land.Actions[1];
            if (audioPlay is AudioPlay)
            {
                return (audioPlay as AudioPlay).oneShotClip;
            }

            return null;
        }

        private static void SuperBossLostKin(PlayMakerFSM fsm)
        {
            _lostKinObject = fsm.gameObject;
            var downstabLandstate = SFCore.Utils.FsmUtil.GetState(fsm, "Dstab Land");
            if (downstabLandstate == null)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Failed to find Dstab Land");
                return;
            }

            // Trying to make Lost kin "slam" spawn a DDark
            CorruptedDreamsPlandoMod.Instance.LogError("Trying to add FSMs to Dstab Land");

            downstabLandstate.AddCustomAction(PrintInsideDStab);


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

            slam.SetActive(true);
            pillar.SetActive(true);
            mega.SetActive(true);

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
                    CorruptedDreamsPlandoMod.Instance.Log($"{c.name}");

                    c.layer = LayerMask.NameToLayer("Enemy Attack");
                    c.RemoveComponent<DamageEnemies>();
                    var damageHero = c.AddComponent<DamageHero>();
                    damageHero.damageDealt = 2;
                }
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

            Satchel.FsmUtil.AddAction(downstabLandstate, activateSlam);
            Satchel.FsmUtil.AddAction(downstabLandstate, new Wait() { time = 0.75f});
            Satchel.FsmUtil.AddAction(downstabLandstate, activatePillar);
            Satchel.FsmUtil.AddAction(downstabLandstate, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(downstabLandstate, activateMega);

            downstabLandstate.AddCustomAction(PrintInsideDStabDone);

        }
    }
}
