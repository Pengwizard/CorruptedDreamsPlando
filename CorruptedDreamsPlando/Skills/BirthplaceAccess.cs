using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using UnityEngine;

namespace CorruptedDreamsPlando.Skills
{
    public class BirthplaceAccess
    {
        public static string baseName = "Birthplace_Access";
        public static string skillName = SkillCommon.MakeItemName(baseName);
        public static string uiName = SkillCommon.CleanName(baseName);
        public static string description = "Opens the Birthplace floor without needing Kingsoul";
        public static ISprite pickupSprite = new EmbeddedSprite("BirthplaceAccess");


        public static void InitializeHooks()
        {
            On.HeroController.Update += HeroController_Update;
            On.GameManager.OnNextLevelReady += onSceneChange;
        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (self.sceneName != SceneNames.Abyss_06_Core)
            {
                return;
            }

            var floorOpen = GameObject.Find("Floor Opening Animation");
            if (floorOpen == null)
            {
                return;
            }

            floorOpen.LocateMyFSM("FSM").enabled = false;
            floorOpen.SetActive(CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess);
        }

        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess = true;
            }
        }
    }


    public class BirthplaceAccessItem : AbstractItem
    {
        public BirthplaceAccessItem()
        {
            name = BirthplaceAccess.skillName;

            InteropTag tag = Rando.RandoInterop.AddTag(this);

            UIDef = new BigUIDef
            {
                name = new BoxedString(BirthplaceAccess.uiName),
                shopDesc = new BoxedString(BirthplaceAccess.description),
                sprite = BirthplaceAccess.pickupSprite,
                bigSprite = BirthplaceAccess.pickupSprite,
                descOne = new BoxedString(BirthplaceAccess.description),
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess = true;
        }

        public override bool Redundant()
        {
            return CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess;
        }
    }
}
