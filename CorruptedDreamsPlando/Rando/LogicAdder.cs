using CorruptedDreamsPlando.Skills;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using SpellStaller.Charms;
using System;
using System.Collections.Generic;
using System.IO;

namespace CorruptedDreamsPlando.Rando
{
    public static class LogicAdder
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(50, ApplyLogic);
            RCData.RuntimeLogicOverride.Subscribe(51, ApplySpellStallLogic);

            RequestBuilder.OnUpdate.Subscribe(0.3f, AddLocationAndItemToRandomization);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            var skillNames = new List<string> { VoidCaster.skillName, Determination.skillName, BirthplaceAccess.skillName };
            foreach (string sk in skillNames)
            {
                Term term = lmb.GetOrAddTerm(sk, TermType.Int);
                lmb.AddItem(new SingleItem(sk, new RandomizerCore.TermValue(term, 1)));
            }

            JsonLogicFormat fmt = new();
            using Stream s = typeof(LogicAdder).Assembly.GetManifestResourceStream("CorruptedDreamsPlando.Resources.logic.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);
        }

        private static void AddLocationAndItemToRandomization(RequestBuilder rb)
        {
            // Adding this to add skill logic.
            rb.AddToVanilla(new("Mantis_Claw", "Bear_Claw"));
            rb.AddToVanilla(new("Monarch_Wings", "Chicken_Wings"));
            rb.AddToVanilla(new("Crystal_Heart", "Crystal_Tart"));
        }

        private static void ApplySpellStallLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("ANY_SPELL_STALL", "Spell_Stabilizer + (SCREAM | FIREBALL)"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("NARROW_SPELL_STALL", "Spell_Stabilizer + (SCREAM | FIREBALL>1)"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("EXTRA_CAST", "VESSELFRAGMENTS>2 | Spell_Twister"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FULL_CAST", "VESSELFRAGMENTS>8 + Spell_Twister"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("EXTRA_SPELL_STALL", "ANY_SPELL_STALL + EXTRA_CAST"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FULL_SPELL_STALL", "ANY_SPELL_STALL + FULL_CAST"));

            // Adding this so these become claw/wings/cdash together.
            lmb.AddLogicDef(new RawLogicDef("Bear_Claw", "Spell_Stabilizer + (SCREAM | FIREBALL)"));
            lmb.AddLogicDef(new RawLogicDef("Chicken_Wings", "Spell_Stabilizer + (SCREAM | FIREBALL) + (VESSELFRAGMENTS>2 | Spell_Twister)"));
            lmb.AddLogicDef(new RawLogicDef("Crystal_Tart", "Spell_Stabilizer + (SCREAM | FIREBALL) + VESSELFRAGMENTS>8 + Spell_Twister"));


            // Logic overrides

        }
    }
}
