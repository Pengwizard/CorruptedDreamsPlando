using CorruptedDreamsPlando.Skills;
using RandomizerMod.RC;

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
                rb.EditItemRequest(VoidCaster.skillName, info =>
                {
                    info.getItemDef = () => new RandomizerMod.RandomizerData.ItemDef()
                    {
                        Name = VoidCaster.skillName,
                        Pool = "Skills",
                        MajorItem = true,
                        PriceCap = 1
                    };
                });

                rb.AddItemByName(VoidCaster.skillName);
        }

        private static void CloneSettings(RequestBuilder rb)
        {
            LocalSettings l = CorruptedDreamsPlandoMod.LS;
            GlobalSettings g = CorruptedDreamsPlandoMod.GS;

            l.Enabled = g.Enabled;
        }
    }
}
