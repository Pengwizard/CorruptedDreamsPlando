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
        public static FsmObject ShriekAudio { get
            {
                GetAbyssShriekSfx();
                return _shriekAudio;
            }}
        private static FsmObject _howlingWraithsAudio = null;
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
                var state = fsm.GetValidState("Scream Antic2");
                _shriekAudio = state.GetFirstActionOfType<AudioPlay>().oneShotClip;
            }

            if (_shriekAudio == null)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Setting audio for shriek failed");
                _shriekAudio = DescendingDarkSpawner.DDarkAudio;
            }
        }

        private static void GetHowlingWraithsSfx()
        {
            if (_howlingWraithsAudio == null)
            {
                var knight = GameObject.Find("Knight");
                PlayMakerFSM fsm = knight.LocateMyFSM("Spell Control");
                var state = fsm.GetValidState("Scream Antic 1");
                _howlingWraithsAudio = state.GetFirstActionOfType<AudioPlay>().oneShotClip;
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

        public GameObject CreateShriekSpawner(int damageToDeal, Vector3 position)
        {
            var newShriek = GameObject.Instantiate(ShriekObject);
            newShriek.name = $"CD Shriek {position.ToString()}";
            Component.Destroy(newShriek.LocateMyFSM("Deactivate on Hit"));
            var fsm = newShriek.LocateMyFSM("FSM");

            fsm.FsmVariables.FindFsmBool("Unparent").Value = false;
            fsm.FsmVariables.FindFsmBool("Reposition").Value = false;

            GameObject[] gos = { newShriek };
            foreach (var part in gos)
            {
                List<GameObject> children = new List<GameObject>();
                part.FindAllChildren(children);
                foreach (var c in children)
                {
                    c.layer = LayerMask.NameToLayer("Enemies");
                    c.RemoveComponent<DamageEnemies>();
                    var damageHero = c.AddComponent<DamageHero>();
                    damageHero.damageDealt = damageToDeal;
                }

                var spr = part.GetComponent<tk2dSprite>();
                SpriteSwapper.SetupSpellTextureSwap(spr);

                var ogScale = spr.scale;
                spr.scale = ogScale * 1.1f;
            }

            var waitState = newShriek.LocateMyFSM("FSM").GetValidState("Wait");
            waitState.GetFirstActionOfType<Tk2dPlayFrame>().frame.Value = 3;

            newShriek.transform.position = position;
            newShriek.SetActive(false);

            return newShriek;
        }

        public void AddShriekAtEndOfFSM(FsmState fsm, int damageToDeal, Vector3 spawnOffset, bool useWraithsAudio=false)
        {
            //GameObject shriek = GameObject.Instantiate(ShriekObject);

            try
            {
                if (useWraithsAudio)
                {
                    GetHowlingWraithsSfx();
                    GetAbyssShriekSfx();
                    if (_shriekAudio != null)
                    {
                        AudioPlayerOneShotSingle playSpellAudio = new AudioPlayerOneShotSingle();
                        //playSpellAudio. = new HutongGames.PlayMaker.FsmOwnerDefault()
                        //{
                        //    GameObject = new FsmGameObject { Value = fsm.Fsm.GameObject },
                        //    OwnerOption = OwnerDefaultOption.SpecifyGameObject
                        //};
                        // TODO: Audio needs to be determined by the game
                        playSpellAudio.volume = 5f;
                        playSpellAudio.audioClip = _shriekAudio;

                        Satchel.FsmUtil.AddAction(fsm, playSpellAudio);
                    }
                }
                else
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

                _temporaryShriek.transform.position = gm.transform.position + spawnOffset;
                _temporaryShriek.SetActive(false);
                _temporaryShriek.SetActive(true);
            });
        }
    }
}
