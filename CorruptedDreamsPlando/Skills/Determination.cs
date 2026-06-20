using CorruptedDreamsPlando.MapAlterations;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using RandomizerMod.RandomizerData;
using SFCore;
using UnityEngine;

namespace CorruptedDreamsPlando.Skills
{
    public class Determination
    {
        public static string invKey = "CD_DETERMINATION_NAME";
        public static string descKey = "CD_DETERMINATION_DESC";
        public static string invName = "Determination";
        public static string boolName = "HasDetermination";

        public static string skillName = "CorruptedDreams_Determination";
        public static string uiName = "DETERMINATION";
        public static string take = "You are now full of...";
        public static string description = "Ancient power wielded by those of the surface.<br><br>Powers up the user's dream nail to enter corrupted dreams, even through the hardest shields.";
        public static string description1 = "Ancient power wielded by those of the surface";
        public static string description2 = "Breaks down the hardest of shields";
        public static ISprite pickupSprite = new EmbeddedSprite("Determination");


        public static void InitializeHooks()
        {
            On.HeroController.Update += HeroController_Update;

            SetupInventoryItem();
        }
        public static void SetupInventoryItem()
        {
            CorruptedDreamsPlandoMod.Instance.Log($"setting up inventory thingy");

            ItemHelper.AddNormalItem(pickupSprite.Value, boolName, invKey, descKey);

            Events.AddLanguageEdit(new(invKey), FillName);
            Events.AddLanguageEdit(new(descKey), FillDesc);

            ModHooks.GetPlayerBoolHook += HookInventory;
            CorruptedDreamsPlandoMod.Instance.Log($"done setup");
        }

        private static void FillName(ref string value) => value = invName;

        private static void FillDesc(ref string value) => value = description;

        private static bool HookInventory(string name, bool orig)
        {
            if (name == boolName)
            {
                CorruptedDreamsPlandoMod.Instance.Log($"determination hook");
                return CorruptedDreamsPlandoMod.LS.ColletedDetermination;
            }

            return orig;
        }

        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                CorruptedDreamsPlandoMod.LS.ColletedDetermination = !CorruptedDreamsPlandoMod.LS.ColletedDetermination;
            }
        }
    }

    public class DeterminationItem : AbstractItem
    {
        public DeterminationItem()
        {
            name = Determination.skillName;

            InteropTag tag = Rando.RandoInterop.AddTag(this);

            UIDef = new BigUIDef
            {
                name = new BoxedString(Determination.uiName),
                shopDesc = new BoxedString(Determination.description),
                sprite = Determination.pickupSprite,
                bigSprite = Determination.pickupSprite,
                descOne = new BoxedString(Determination.description1),
                descTwo = new BoxedString(Determination.description2),
                take = new BoxedString(Determination.take)
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            CorruptedDreamsPlandoMod.LS.ColletedDetermination = true;
        }

        public override bool Redundant()
        {
            return CorruptedDreamsPlandoMod.LS.ColletedDetermination;
        }
    }
}
