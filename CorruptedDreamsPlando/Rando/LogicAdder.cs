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
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("TWO_EXTRA_CAST", "VESSELFRAGMENTS>5 | (Spell_Twister + VESSELFRAGMENTS>2)"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FULL_CAST", "VESSELFRAGMENTS>8 + Spell_Twister"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("EXTRA_SPELL_STALL", "ANY_SPELL_STALL + EXTRA_CAST"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FULL_SPELL_STALL", "ANY_SPELL_STALL + FULL_CAST"));

            // Adding this so these become claw/wings/cdash together.
            lmb.AddLogicDef(new RawLogicDef("Bear_Claw", "Spell_Stabilizer + (SCREAM | FIREBALL)"));
            lmb.AddLogicDef(new RawLogicDef("Chicken_Wings", "Spell_Stabilizer + (SCREAM | FIREBALL) + (VESSELFRAGMENTS>2 | Spell_Twister)"));
            lmb.AddLogicDef(new RawLogicDef("Crystal_Tart", "Spell_Stabilizer + (SCREAM | FIREBALL) + VESSELFRAGMENTS>8 + Spell_Twister"));


            // Logic overrides
            lmb.AddLogicDef(new RawLogicDef("Fungus1_28[left1]", "Fungus1_28[left1] | Fungus1_28[left2] + ((NARROW_SPELL_STALL)|(FULL_SPELL_STALL + DREAMGATE))"));
            lmb.AddLogicDef(new RawLogicDef("Deepnest_43[bot1]", "(Deepnest_43[bot1] + NARROW_SPELL_STALL) | (Deepnest_43[left1] | Deepnest_43[right1]) + (ANYCLAW | WINGS | ENEMYPOGOS + DANGEROUSSKIPS + LEFTDASH)"));
            lmb.AddLogicDef(new RawLogicDef("Tutorial_01", "(Tutorial_01[right1] + TWO_EXTRA_CAST) | Tutorial_01[top1] | Tutorial_01[top2]"));
            lmb.AddLogicDef(new RawLogicDef("Cliffs_02", "(Cliffs_02[bot1] + NARROW_SPELL_STALL) | (Cliffs_02[door1] | Cliffs_02[left1]) + (ANYCLAW | WINGS | ENEMYPOGOS) | Cliffs_02[left2]"));
            lmb.AddLogicDef(new RawLogicDef("Shape_of_Unn", "Fungus1_Slug[right1] + FULLDASH + EXTRA_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Fungus1_Slug[right1]", "FULLDASH + EXTRA_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Wanderer's_Journal-Greenpath_Lower", "Fungus1_11 + NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Wanderer's_Journal-Fungal_Wastes_Thorns_Gauntlet", "Fungus2_04 + Lever-Fungal_Wastes_Thorns_Gauntlet | Fungus2_04 + NARROW_SPELL_STALL"));

        }
    }
}
