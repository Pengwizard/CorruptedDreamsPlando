using CorruptedDreamsPlando.MapAlterations;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using UnityEngine;

namespace CorruptedDreamsPlando.Skills
{
    public class Determination
    {
        public static string skillName = "CorruptedDreams_Determination";
        public static string uiName = "DETERMINATION";
        public static string take = "You are now full of...";
        public static string description = "Ancient power wielded by those of the surface<br>Breaks down the hardest of shields";
        public static string description1 = "Ancient power wielded by those of the surface";
        public static string description2 = "Breaks down the hardest of shields";
        public static ISprite pickupSprite = new EmbeddedSprite("Determination");


        public static void InitializeHooks()
        {
            On.HeroController.Update += HeroController_Update;
        }

        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                CorruptedDreamsPlandoMod.LS.ColletedDetermination = true;
            }


            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                CorruptedDreamsPlandoMod.LS.ColletedDetermination = false;
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                TraitorLordDream.killedTraitorLordInDream = true;
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
