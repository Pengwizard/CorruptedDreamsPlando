using CorruptedDreamsPlando.Skills;
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
            var name = VoidCaster.skillName;
            Term term = lmb.GetOrAddTerm(name, TermType.Int);
            lmb.AddItem(new SingleItem(name, new RandomizerCore.TermValue(term, 1)));


            var name2 = Determination.skillName;
            Term term2 = lmb.GetOrAddTerm(name2, TermType.Int);
            lmb.AddItem(new SingleItem(name2, new RandomizerCore.TermValue(term, 1)));
        }
    }
}
