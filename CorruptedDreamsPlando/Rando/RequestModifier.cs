using CorruptedDreamsPlando.Locations;
using CorruptedDreamsPlando.Skills;
using ItemChanger;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.IO;

namespace CorruptedDreamsPlando.Rando
{
    public class RequestModifier
    {
        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-499, SetupItems);
            RequestBuilder.OnUpdate.Subscribe(-100, ApplyNkgRewardDef);
            RequestBuilder.OnUpdate.Subscribe(0, CloneSettings);
        }

        private static void SetupItems(RequestBuilder rb)
        {
            List<string> customSkillNames = new List<string>{ Determination.skillName, VoidCaster.skillName, BirthplaceAccess.skillName };
            foreach (string name in customSkillNames) {
                rb.EditItemRequest(name, info =>
                {
                    info.getItemDef = () => new RandomizerMod.RandomizerData.ItemDef()
                    {
                        Name = name,
                        Pool = "Skills",
                        MajorItem = true,
                        PriceCap = 1
                    };
                });

                rb.AddItemByName(name);
            }
        }

        private static void CloneSettings(RequestBuilder rb)
        {
            LocalSettings l = CorruptedDreamsPlandoMod.LS;
            GlobalSettings g = CorruptedDreamsPlandoMod.GS;

            l.Enabled = g.Enabled;
        }


        public static void ApplyNkgRewardDef(RequestBuilder rb)
        {
            AddAndEditLocation(rb, CustomBossEssenceLocation.nkgEssecnceLocation, FlingType.Everywhere, false, SceneNames.Town);
            AddAndEditLocation(rb, CustomBossEssenceLocation.tlEssecnceLocation, FlingType.Everywhere, false, SceneNames.Fungus3_23);
        }

        private static void AddAndEditLocation(RequestBuilder rb, string name, FlingType flingType, bool progressionPenalty, string scene)
        {
            rb.AddLocationByName(name);
            rb.EditLocationRequest(name, info => {
                info.customPlacementFetch = (factory, placement) => {
                    if (factory.TryFetchPlacement(name, out AbstractPlacement ap1))
                        return ap1;
                    AbstractLocation absLoc = Finder.GetLocation(name);
                    absLoc.flingType = flingType;
                    AbstractPlacement ap = absLoc.Wrap();
                    factory.AddPlacement(ap);
                    return ap;
                };
                info.getLocationDef = () => new()
                {
                    Name = name,
                    FlexibleCount = false,
                    AdditionalProgressionPenalty = progressionPenalty,
                    SceneName = scene
                };
            });
        }
    }
}
