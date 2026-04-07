using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using Satchel;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace CorruptedDreamsPlando.SpellSpawns
{
    public class DescendingDarkSpawner
    {
        private GameObject _ddarkSlam;
        private GameObject _ddarkPillar;
        private GameObject _ddarkMega;
        private static FsmObject _ddarkAudio = null;

        public static void Initialize()
        {
            On.HeroController.Start += DoThisWhenTheKnightIsLoaded;
        }


        private static void DoThisWhenTheKnightIsLoaded(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            PlayMakerFSM fsm = self.gameObject.LocateMyFSM("Spell Control");
            var state = fsm.GetState("Q2 Land");
            _ddarkAudio = state.GetFirstActionOfType<AudioPlay>().oneShotClip;
        }

        public GameObject DDarkSlam
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

        public GameObject DDarkPillar
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

        public GameObject DDarkMega
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

        private static void MakeInvuln(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Enemy Attack");
        }

        private static void MakeVuln(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Enemies");
        }

        public void AddDDarkAtEndOfFSM(FsmState fsm, int damageToDeal)
        {
            GameObject slam = GameObject.Instantiate(DDarkSlam);
            GameObject pillar = GameObject.Instantiate(DDarkPillar);
            GameObject mega = GameObject.Instantiate(DDarkMega);

            try
            {
                if (_ddarkAudio != null)
                {
                    AudioPlay playDiveAudio = new AudioPlay();
                    playDiveAudio.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() {
                        GameObject = new FsmGameObject { Value = fsm.Fsm.GameObject },
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject };
                    // TODO: Audio needs to be determined by the game
                    playDiveAudio.volume = 5f;
                    playDiveAudio.oneShotClip = _ddarkAudio;

                    Satchel.FsmUtil.AddAction(fsm, playDiveAudio);
                }
            }
            catch (Exception e)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Setting audio for ddark failed");
            }

            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;

            slam.transform.SetParent(goTransform);
            slam.transform.position = goTransform.position + new Vector3(0, 2, 0);
            pillar.transform.SetParent(goTransform);
            pillar.transform.position = goTransform.position;
            mega.transform.SetParent(goTransform);
            mega.transform.position = goTransform.position + new Vector3(0, 6, 0);

            GameObject[] gos = { slam, pillar, mega };


            foreach (var part in gos)
            {
                List<GameObject> children = new List<GameObject>();
                part.FindAllChildren(children);
                foreach (var c in children)
                {
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

            Satchel.FsmUtil.AddFirstAction(fsm, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });
            Satchel.FsmUtil.AddAction(fsm, activateSlam);
            Satchel.FsmUtil.AddAction(fsm, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(fsm, activatePillar);
            Satchel.FsmUtil.AddAction(fsm, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(fsm, activateMega);
            Satchel.FsmUtil.AddAction(fsm, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(fsm, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });
        }
    }
}
