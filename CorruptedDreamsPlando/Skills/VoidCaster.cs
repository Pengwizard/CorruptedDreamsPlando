using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using System.Collections.Generic;
using UnityEngine;

namespace CorruptedDreamsPlando.Skills
{
    public class VoidCaster
    {
        public static string baseName = "Void_Caster";
        public static string skillName = SkillCommon.MakeItemName(baseName);
        public static string uiName = SkillCommon.CleanName(baseName);
        public static string take = "You have collected";
        public static string description = "Developed by early inventors in Hallownest. Infuses one's soul projections with Void";
        public static string description1 = "Developed by early inventors in Hallownest";
        public static string description2 = "Infuses one's soul projections with Void";
        public static ISprite pickupSprite = new EmbeddedSprite("VoidCaster");


        public static void InitializeHooks()
        {
            On.HeroController.Update += HeroController_Update;
            On.PlayerData.GetInt += PlayerData_GetInt;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += LimitSpellLevel;
        }

        static List<string> spellLevelInts = new List<string>{ "fireballLevel", "screamLevel", "quakeLevel" };

        private static int PlayerData_GetInt(On.PlayerData.orig_GetInt orig, PlayerData self, string intName)
        {
            if (spellLevelInts.Contains(intName)) {
                if (!CorruptedDreamsPlandoMod.LS.CollectedVoidCaster)
                {
                    // Change all events passing to be into the "less than" because that goes into level 1.
                    return 1;
                }
            }

            return orig(self, intName);
        }

        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                CorruptedDreamsPlandoMod.LS.CollectedVoidCaster = true;
            }


            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                CorruptedDreamsPlandoMod.LS.CollectedVoidCaster = false;
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
