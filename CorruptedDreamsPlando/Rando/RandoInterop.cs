using CorruptedDreamsPlando.Locations;
using CorruptedDreamsPlando.MapAlterations;
using CorruptedDreamsPlando.Skills;
using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Tags;
using Modding;
using RandomizerMod.RC;
using UnityEngine;

namespace CorruptedDreamsPlando.Rando
{
    internal static class RandoInterop
    {

        public static void Hook()
        {
            RequestModifier.Hook();
            LogicAdder.Hook();

            RandoController.OnExportCompleted += AddRgbModule;

            DefineItems();
            AddCustomLocations();

            if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            {
                RSMInterop.Hook();
            }
        }

        private static void AddRgbModule(RandoController controller)
        {
            if (!CorruptedDreamsPlandoMod.GS.Enabled)
            {
                return;
            }
            // Place for modules in the future
            // ItemChangerMod.Modules.GetOrAdd<RgbModule>();
        }

        public static void DefineItems()
        {
            Finder.DefineCustomItem(new VoidCasterItem());
            Finder.DefineCustomItem(new DeterminationItem());
            Finder.DefineCustomItem(new BirthplaceAccessItem());
        }

        public static InteropTag AddTag(TaggableObject obj)
        {
            InteropTag tag = obj.GetOrAddTag<InteropTag>();
            tag.Message = "RandoSupplementalMetadata";
            tag.Properties["ModSource"] = CorruptedDreamsPlandoMod.Instance.GetName();

            return tag;
        }

        public static void AddCustomLocations()
        {
            CustomBossEssenceLocation scareLoc = new() { name = CustomBossEssenceLocation.nkgEssecnceLocation };
            CustomBossEssenceLocation scare2Loc = new() { name = CustomBossEssenceLocation.tlEssecnceLocation };

            DefineLoc(scare2Loc, SceneNames.Fungus3_23, "EssenceIcon", 2, 2);
            DefineLoc(scareLoc, SceneNames.Town, "EssenceIcon", TraitorLordDream.dreamEnterPosition.x, TraitorLordDream.dreamEnterPosition.y);
        }

        public static void DefineLoc(AbstractLocation loc, string scene, string sprite, float x, float y)
        {
            InteropTag tag = AddTag(loc);
            tag.Properties["PinSprite"] = new EmbeddedSprite(sprite);
            tag.Properties["WorldMapLocation"] = (scene, x, y);
            Finder.DefineCustomLocation(loc);
        }
    }
}
