using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using Satchel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
    public class AbyssShriekSpawner
    {

        private static AbyssShriekSpawner _instance;

        private GameObject _srcHeads2;
        private static FsmObject _shriekAudio = null;
        private static GameObject _temporaryShriek = null;

        internal static AbyssShriekSpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(AbyssShriekSpawner)} was never constructed");
                }
                return _instance;
            }
        }


        public static void Initialize()
        {
            _instance = new AbyssShriekSpawner();
            On.GameManager.OnNextLevelReady += onSceneChange;
        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            _temporaryShriek = null;
        }

        private static void GetAbyssShriekSfx()
        {
            if (_shriekAudio == null)
            {
                var knight = GameObject.Find("Knight");
                PlayMakerFSM fsm = knight.LocateMyFSM("Spell Control");
                var state = fsm.GetState("Scream Antic2");
                _shriekAudio = state.GetFirstActionOfType<AudioPlay>().oneShotClip;
                CorruptedDreamsPlandoMod.Instance.Log(_shriekAudio);
            }
        }

        public GameObject ShriekObject
        {
            get
            {
                if (_srcHeads2 == null)
                {
                    _srcHeads2 = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Scr Heads 2").gameObject);
                    GameObject.DontDestroyOnLoad(_srcHeads2);
                }
                return _srcHeads2;
            }
        }

        public void AddShriekAtEndOfFSM(FsmState fsm, int damageToDeal, Vector3 spawnOffset)
        {
            //GameObject shriek = GameObject.Instantiate(ShriekObject);

            try
            {
                GetAbyssShriekSfx();
                if (_shriekAudio != null)
                {
                    AudioPlay playSpellAudio = new AudioPlay();
                    playSpellAudio.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault()
                    {
                        GameObject = new FsmGameObject { Value = fsm.Fsm.GameObject },
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject
                    };
                    // TODO: Audio needs to be determined by the game
                    playSpellAudio.volume = 5f;
                    playSpellAudio.oneShotClip = _shriekAudio;

                    Satchel.FsmUtil.AddAction(fsm, playSpellAudio);
                }
            }
            catch (Exception e)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Setting audio for shriek failed");
            }

            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;

            Satchel.FsmUtil.AddCustomAction(fsm, () =>
            {
                var gm = fsm.Fsm.GameObject;
                if (_temporaryShriek == null)
                {

                    CorruptedDreamsPlandoMod.Instance.Log("Creating Shriek"); 
                    _temporaryShriek = GameObject.Instantiate(ShriekObject);
                    _temporaryShriek.name = "TemporaryShriek";
                    Component.Destroy(_temporaryShriek.LocateMyFSM("Deactivate on Hit"));
                    _temporaryShriek.LocateMyFSM("FSM").FsmVariables.FindFsmBool("Unparent").Value = false;
                    _temporaryShriek.LocateMyFSM("FSM").FsmVariables.FindFsmBool("Reposition").Value = false;

                    GameObject[] gos = { _temporaryShriek };
                    foreach (var part in gos)
                    {
                        List<GameObject> children = new List<GameObject>();
                        part.FindAllChildren(children);
                        foreach (var c in children)
                        {
                            c.layer = LayerMask.NameToLayer("Enemy Attack");
                            c.RemoveComponent<DamageEnemies>();
                            var damageHero = c.AddComponent<DamageHero>();
                            damageHero.damageDealt = damageToDeal;
                        }
                    }
                }

                CorruptedDreamsPlandoMod.Instance.Log("Moving Shriek");
                _temporaryShriek.transform.position = gm.transform.position + spawnOffset;
                _temporaryShriek.SetActive(false);
                _temporaryShriek.SetActive(true);
                CorruptedDreamsPlandoMod.Instance.Log("Shriek done");
            });
        }
    }
}
