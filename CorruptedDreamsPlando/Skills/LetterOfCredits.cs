using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using SFCore;
using UnityEngine;

namespace CorruptedDreamsPlando.Skills
{
    public class LetterOfCredits
    {
        public static string invKey = "CD_CREDITS_NAME";
        public static string descKey = "CD_CREDITS_DESC";
        public static string invName = "Letter of Credits";
        public static string boolName = "LetoCred";

        public static string baseName = "Letter_of_Credits";
        public static string skillName = SkillCommon.MakeItemName(baseName);
        public static string uiName = SkillCommon.CleanName(baseName);
        public static string description = "Thanks for playing\r\n Midautumn's Knightmare!\r\n\r\nThis plando was made by Nopre\r\n\r\nThanks to Staxis, cron, maginall, choombert, Jalepino Joe, aPress, and BlossomTheCellist for playtesting!\r\n\r\nAnd Purenail and TheMathGeek_314 for teaching me how to mod and letting me reference their past mods!\r\n\r\nBirdie should Hydrate";
        public static ISprite pickupSprite = new EmbeddedSprite("LetterOfCredits");


        public static void InitializeHooks()
        {
            SetupInventoryItem();
        }

        public static void SetupInventoryItem()
        {
            var sprite = new EmbeddedSprite("LetterOfCredits");

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
                return CorruptedDreamsPlandoMod.LS.CollectedLetterOfCredits;
            }

            return orig;
        }
    }

    public class LetterOfCreditsItem : AbstractItem
    {
        public LetterOfCreditsItem()
        {
            name = LetterOfCredits.skillName;

            InteropTag tag = Rando.RandoInterop.AddTag(this);

            UIDef = new MsgUIDef
            {
                name = new BoxedString(LetterOfCredits.uiName),
                shopDesc = new BoxedString(LetterOfCredits.description),
                sprite = LetterOfCredits.pickupSprite,
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            CorruptedDreamsPlandoMod.LS.CollectedLetterOfCredits = true;
        }

        public override bool Redundant()
        {
            return CorruptedDreamsPlandoMod.LS.CollectedLetterOfCredits;
        }
    }
}
