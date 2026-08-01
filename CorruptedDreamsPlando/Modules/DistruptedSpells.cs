using System;
using System.Collections;
using CorruptedDreamsPlando.CorruptedDreamBosses;
using CorruptedDreamsPlando.Skills;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using KorzUtils.Helper;
using Satchel;
using SFCore;
using SpellStaller;
using SpellStaller.Charms;
using UnityEngine;

namespace CorruptedDreamsPlando.Modules
{
    public class DistruptedSpells
    {

        public static void Initialize()
        {
            ApplyHooks();
        }

        public static void ApplyHooks()
        {
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += RoomBasedVSReaction;

            SpellStabilizer spellStall = (SpellStabilizer)SpellStaller.SpellStaller.Instance.GetCharmByName(SpellStaller.Charms.SpellStabilizer.charmName);
            SpellStabilizer.ShouldFizzle = () => {
                bool t = 
                    !spellStall.IsEquipped
                    && GameManager.instance.sceneName != SceneNames.Crossroads_ShamanTemple;
                if (t)
                {
                    GameHelper.DisplayMessage("The spell fizzles, scattering into unfocused Soul...");
                }

                return t;
            };

            SpellStabilizer.ShouldForceStall = () =>
            {
                return GameManager.instance.sceneName == SceneNames.Crossroads_ShamanTemple;
            };

        }

        public static void RoomBasedVSReaction(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            try
            {
                if (self.Fsm.FsmComponent.gameObject.name.Contains("Fireball") &&
    self.Fsm.Name == "Fireball Control" &&
    self.State.Name == "Idle")
                {
                    if (GameManager.instance.sceneName == SceneNames.Crossroads_ShamanTemple)
                    {
                        var baldur = GameObject.Find("Battle Scene/Blocker");
                        baldur.FindChild("Alert Range New").SetActive(false);
                        baldur.FindChild("Unalert Range").SetActive(false);

                        baldur.GetComponent<HealthManager>().hp = 1000;
                        baldur.LocateMyFSM("Blocker Control").GetValidState("Closed").RemoveTransition("FAR");
                        baldur.LocateMyFSM("Blocker Control").SetState("Close");
                    }

                    if (GameManager.instance.sceneName == SceneNames.White_Palace_13 && CorruptedDreamsPlandoMod.LS.CollectedVoidCaster)
                    {
                        SingleCastSoulOrbSpawner.SpawnSoul(self.Fsm.GameObject.transform);
                    }

                }
            }
            catch (Exception err)
            {
            }
            orig(self);
        }
    }
}
