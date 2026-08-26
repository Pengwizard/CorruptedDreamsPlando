using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using RandomizerMod.RandomizerData;
using System;
using System.Collections.Generic;
using UnityEngine;
using SFCore;

namespace CorruptedDreamsPlando.Skills
{
    public class VoidCaster
    {
        public static string invKey = "CD_VOIDCASTER_NAME";
        public static string descKey = "CD_VOIDCASTER_DESC";
        public static string invName = "Void Caster";
        public static string boolName = "VoidCaster";

        public static string baseName = "Void_Caster";
        public static string skillName = SkillCommon.MakeItemName(baseName);
        public static string uiName = SkillCommon.CleanName(baseName);
        public static string take = "You have collected";
        public static string description = "Developed by early inventors in Hallownest.<br><br>Created under orders of The Pale King, this lets the user infuse their soul projections with Void essence.";
        public static string description1 = "Developed by early inventors in Hallownest";
        public static string description2 = "Created under orders of The Pale King, this lets the user infuse their soul projections with Void essence.";
        public static ISprite pickupSprite = new EmbeddedSprite("VoidCaster");


        public static void InitializeHooks()
        {
            //On.HeroController.Update += HeroController_Update;
            On.PlayerData.GetInt += PlayerData_GetInt;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += LimitSpellLevel;

            SetupInventoryItem();
        }

        public static void SetupInventoryItem()
        {
            var sprite = new EmbeddedSprite("VoidCaster_Small");
            ItemHelper.AddNormalItem(sprite.Value, boolName, invKey, descKey);

            Events.AddLanguageEdit(new(invKey), FillName);
            Events.AddLanguageEdit(new(descKey), FillDesc);

            ModHooks.GetPlayerBoolHook += HookInventory;
        }

        private static void FillName(ref string value) => value = invName;

        private static void FillDesc(ref string value) => value = description;

        private static bool HookInventory(string name, bool orig)
        {
            if (name == boolName)
            {
                return CorruptedDreamsPlandoMod.LS.CollectedVoidCaster;
            }

            return orig;
        }

        static List<string> spellLevelInts = new List<string>{ "fireballLevel", "screamLevel", "quakeLevel" };

        static List<string> pathOfPain = new List<string> { SceneNames.White_Palace_18, SceneNames.White_Palace_17, SceneNames.White_Palace_19, SceneNames.White_Palace_20 };

        private static int PlayerData_GetInt(On.PlayerData.orig_GetInt orig, PlayerData self, string intName)
        {
            if (spellLevelInts.Contains(intName)) {
                if (!CorruptedDreamsPlandoMod.LS.CollectedVoidCaster)
                {
                    // Change all events passing to be into the "less than" because that goes into level 1.
                    return Math.Min(1, orig(self, intName));
                }



                if (pathOfPain.Contains(GameManager.instance.sceneName))
                {
                    return Math.Min(1, orig(self, intName));
                }
            }

            return orig(self, intName);
        }

        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CorruptedDreamsPlandoMod.LS.CollectedVoidCaster = !CorruptedDreamsPlandoMod.LS.CollectedVoidCaster;
            }
        }

        public static void LimitSpellLevel(
    On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig,
    HutongGames.PlayMaker.Actions.IntCompare self)
        {
                if (self.Fsm.FsmComponent.gameObject.name.Contains("Knight") &&
                        self.Fsm.Name == "Spell Control" &&
                        self.State.Name.Contains("Level Check"))
                {
                if (!CorruptedDreamsPlandoMod.LS.CollectedVoidCaster)
                {
                    // Change all events passing to be into the "less than" because that goes into level 1.
                    // self.integer2 = 3;
                } else
                {
                    // self.integer2 = 1;
                }
            }
            orig(self);
        }
    }


    public class VoidCasterItem : AbstractItem
    {
        public VoidCasterItem()
        {
            name = VoidCaster.skillName;

            InteropTag tag = Rando.RandoInterop.AddTag(this);

            // tag.Properties["PinSprite"] = VoidCaster.pickupSprite;

            UIDef = new BigUIDef
            {
                name = new BoxedString(VoidCaster.uiName),
                shopDesc = new BoxedString(VoidCaster.description),
                sprite = VoidCaster.pickupSprite,
                bigSprite = VoidCaster.pickupSprite,
                descOne = new BoxedString(VoidCaster.description1),
                descTwo = new BoxedString(VoidCaster.description2)
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            CorruptedDreamsPlandoMod.LS.CollectedVoidCaster = true;
        }

        public override bool Redundant()
        {
            return CorruptedDreamsPlandoMod.LS.CollectedVoidCaster;
        }
    }
}
