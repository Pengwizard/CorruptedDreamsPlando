using System;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using KorzUtils.Helper;
using Satchel;
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

        private static List<string> unfizzleRooms = new()
        {
            SceneNames.Crossroads_ShamanTemple,
            SceneNames.Crossroads_10,
            SceneNames.Crossroads_06
        };

        public static void ApplyHooks()
        {
            On.GameManager.OnNextLevelReady += onSceneChange;
            On.HutongGames.PlayMaker.Actions.Wait.OnEnter += RoomBasedVSReaction;

            SpellStabilizer spellStall = (SpellStabilizer)SpellStaller.SpellStaller.Instance.GetCharmByName(SpellStaller.Charms.SpellStabilizer.charmName);
            SpellStabilizer.ShouldFizzle = () => {
                bool t = 
                    !spellStall.IsEquipped
                    && !unfizzleRooms.Contains(GameManager.instance.sceneName);
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

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (GameManager.instance.sceneName == SceneNames.Crossroads_ShamanTemple)
            {
                var baldur = GameObject.Find("Battle Scene/Blocker");
                if (baldur != null)
                {
                    baldur.LocateMyFSM("Blocker Control").GetValidState("Idle").RemoveTransition("ATTACK");


                    var newX = 20f;
                    var newY = 37f;
                    var newBaldur = GameObject.Instantiate(baldur);
                    newBaldur.transform.SetPosition2D(newX, newY);

                    var nbc = newBaldur.LocateMyFSM("Blocker Control").GetValidState("Close");
                    var setPot = nbc.GetFirstActionOfType<SetPosition>();

                    setPot.x.Value = newX;
                    setPot.y.Value = newY;

                    baldur.transform.SetScaleX(-1);
                    baldur.transform.SetScaleX(1);

                }
            }
        }

        public static void RoomBasedVSReaction(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
        {
            try
            {
                if (self.Fsm.FsmComponent.gameObject.name.Contains("Fireball") &&
    self.Fsm.Name == "Fireball Control" &&
    self.State.Name == "Idle")
                {
                    if (GameManager.instance.sceneName == SceneNames.White_Palace_13 && CorruptedDreamsPlandoMod.LS.CollectedVoidCaster)
                    {
                        CorruptedDreamBosses.CorruptedNKG.ParryReward.SpawnSoul(self.Fsm.GameObject.transform);
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
