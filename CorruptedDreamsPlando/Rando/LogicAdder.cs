using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace CorruptedDreamsPlando.Rando
{
    public static class LogicAdder
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(50, ApplyLogic);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            var name = "CorruptedDreams_VoidCaster";
            Term term = lmb.GetOrAddTerm(name, TermType.Int);
            lmb.AddItem(new SingleItem(name, new RandomizerCore.TermValue(term, 1)));
        }
    }
}
