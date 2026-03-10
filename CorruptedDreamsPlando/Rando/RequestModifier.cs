using CorruptedDreamsPlando.Skills;
using RandomizerMod.RC;
using System.Collections.Generic;

namespace CorruptedDreamsPlando.Rando
{
    public class RequestModifier
    {
        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-499, SetupItems);
            RequestBuilder.OnUpdate.Subscribe(0, CloneSettings);
        }

        private static void SetupItems(RequestBuilder rb)
        {
            List<string> customSkillNames = new List<string>{ Determination.skillName, VoidCaster.skillName };
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
    }
}
