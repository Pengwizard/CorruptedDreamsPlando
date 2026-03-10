using CorruptedDreamsPlando.Locations;
using CorruptedDreamsPlando.Skills;
using ItemChanger;
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
        }

        public static string Clean(string name)
        {
            return name.Replace("-", " - ");
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
            CustomBossEssenceLocation scareLoc = new() { name = CustomBossEssenceLocation.nkgEssecnceLocation, sceneName = SceneNames.Grimm_Main_Tent };
            DefineLoc(scareLoc, SceneNames.Grimm_Main_Tent, "DungDoor", 0, 0);
        }

        public static void DefineLoc(AbstractLocation loc, string sprite, string scene, float x, float y)
        {
            CorruptedDreamsPlandoMod.Instance.Log("Defining the location");
            InteropTag tag = AddTag(loc);
            tag.Properties["PinSprite"] = new EmbeddedSprite(sprite);
            tag.Properties["WorldMapLocation"] = (scene, x, y);
            Finder.DefineCustomLocation(loc);
        }
    }
}
