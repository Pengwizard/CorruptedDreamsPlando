using CorruptedDreamsPlando.Skills;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
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
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("SHIELD_BREAKER", "CorruptedDreams_Determination"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("VOID_SPELLS", "CorruptedDreams_Void_Caster"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("BOSS", "MINIBOSS"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("AERIALMINIBOSS", "MINIBOSS"));

            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("ANY_SPELL_STALL", "Spell_Stabilizer + (SCREAM | FIREBALL)"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("NARROW_SPELL_STALL", "Spell_Stabilizer + (SCREAM | FIREBALL>1)"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("EXTRA_CAST", "VESSELFRAGMENTS>2 | Spell_Twister"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FIVE_CASTS", "VESSELFRAGMENTS>5 | (Spell_Twister + VESSELFRAGMENTS>2)"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FULL_CAST", "VESSELFRAGMENTS>8 + Spell_Twister"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("EXTRA_SPELL_STALL", "ANY_SPELL_STALL + EXTRA_CAST"));
            lmb.DoMacroEdit(new System.Collections.Generic.KeyValuePair<string, string>("FULL_SPELL_STALL", "ANY_SPELL_STALL + FULL_CAST"));

            // Adding this so these become claw/wings/cdash together.
            lmb.AddLogicDef(new RawLogicDef("Bear_Claw", "Spell_Stabilizer + (SCREAM | FIREBALL)"));
            lmb.AddLogicDef(new RawLogicDef("Chicken_Wings", "Spell_Stabilizer + Dream_Sifter + (SCREAM | FIREBALL) + (VESSELFRAGMENTS>2 | Spell_Twister)"));
            lmb.AddLogicDef(new RawLogicDef("Crystal_Tart", "Spell_Stabilizer + Dream_Sifter + (SCREAM | FIREBALL) + VESSELFRAGMENTS>8 + Spell_Twister"));

            // Transition overrides:
            lmb.AddLogicDef(new RawLogicDef("Crossroads_35[bot1]", "Crossroads_35[bot1] | (Crossroads_35[right1] + ANY_SPELL_STALL + FULLDASH)"));
            lmb.AddLogicDef(new RawLogicDef("Crossroads_04[door_charmshop]",
                "Crossroads_04[door_charmshop] | Crossroads_04[right1] | (Crossroads_04[door1] | Crossroads_04[door_Mender_House] | (Crossroads_04[left1] | Crossroads_04[top1]) + Defeated_Gruz_Mother) + (FIREBALL | RIGHTCLAW | RIGHTDASH | WINGS | RIGHTSUPERDASH | SPELLAIRSTALL + $CASTSPELL[3,before:AREASOUL] | $SHADESKIP | INFECTED) | Bench-Salubra"));

            lmb.AddLogicDef(new RawLogicDef("Fungus1_28[left1]", "Fungus1_28[left1] | Fungus1_28[left2] + (NARROW_SPELL_STALL)"));
            lmb.AddLogicDef(new RawLogicDef("Deepnest_43[bot1]", "NONE"));
            lmb.AddLogicDef(new RawLogicDef("Deepnest_42[top1]", "NONE"));
            lmb.AddLogicDef(new RawLogicDef("Tutorial_01", "(Tutorial_01[right1] + FIVE_CASTS + ANY_SPELL_STALL) | Tutorial_01[top1] | Tutorial_01[top2]"));
            lmb.AddLogicDef(new RawLogicDef("Cliffs_02", "(Cliffs_02[bot1] + NARROW_SPELL_STALL) | (Cliffs_02[door1] | Cliffs_02[left1]) + (ANYCLAW | WINGS | ENEMYPOGOS) | Cliffs_02[left2]"));
            lmb.AddLogicDef(new RawLogicDef("Cliffs_02[left2]", "Cliffs_02[left2] | Cliffs_02 + FIVE_CASTS + ANY_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Fungus1_Slug[right1]", "FULLDASH + EXTRA_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Deepnest_41[left1]",
                "NARROW_SPELL_STALL + (Deepnest_41[left1] | Deepnest_41[right1] + (LANTERN | (DARKROOMS | NOLANTERN)) + (LEFTCLAW | WINGS + (RIGHTCLAW | ENEMYPOGOS) | RIGHTCLAW + ENEMYPOGOS) | Deepnest_41[left2] + (LANTERN | (DARKROOMS | NOLANTERN)) + (FULLCLAW | WINGS + (LEFTCLAW + ENEMYPOGOS | RIGHTCLAW)))"));

            // End of white palace CorruptedDreams_Void_Caster
            lmb.AddLogicDef(new RawLogicDef("White_Palace_13[left1]", "VOID_SPELLS"));


            // Crystal Peak disconnecting top from bottom
            lmb.AddLogicDef(new RawLogicDef("Mines_05", "Mines_05[right1] | Mines_05[top1] | Mines_05[left1] | Mines_05[left2]"));
            lmb.AddLogicDef(new RawLogicDef("Mines_03", "Mines_03[right1]"));

            // Fixing Kedge Logic
            lmb.AddLogicDef(new RawLogicDef("Deepnest_East_03[top2]", "Deepnest_East_03[left1] + ((Dream_Sifter + EXTRA_SPELL_STALL) | (FIVE_CASTS + ANY_SPELL_STALL))"));
            lmb.AddLogicDef(new RawLogicDef("Deepnest_East_03[left1]", "NONE"));
            lmb.AddLogicDef(new RawLogicDef("Deepnest_East_03[left2]", "NONE"));

            lmb.AddLogicDef(new RawLogicDef("Fungus3_44[bot1]", "Fungus3_44[bot1] | Fungus3_44 + (LEFTSHADOWDASH | Lever-Below_Overgrown_Mound | (GREATSLASH | DASHSLASH))"));

            // Baldur killing override
            lmb.AddLogicDef(new RawLogicDef("Defeated_Crossroads_Baldur", "Crossroads_11_alt[right1] + FIREBALL + Spell_Stabilizer"));

            // Check overrides:
            lmb.AddLogicDef(new RawLogicDef("Whispering_Root-Crossroads", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Whispering_Root-Ancestral_Mound", "Crossroads_ShamanTemple[left1] + (SCREAM | FIREBALL) + DREAMNAIL"));
            lmb.AddLogicDef(new RawLogicDef("Soul_Catcher", "Crossroads_ShamanTemple[left1] + (SCREAM | FIREBALL)"));
            // lmb.AddLogicDef(new RawLogicDef("Whispering_Root-Ancestral_Mound", "Crossroads_ShamanTemple + (SCREAM | FIREBALL)"));
            lmb.AddLogicDef(new RawLogicDef("Glowing_Womb", "ANY_SPELL_STALL + FULLDASH"));
            lmb.AddLogicDef(new RawLogicDef("Soul_Totem-Crossroads_Goam_Journal", "Crossroads_18 + ANY_SPELL_STALL"));

            lmb.AddLogicDef(new RawLogicDef("Charm_Notch-Fog_Canyon", "Fungus3_28[right1] + (EXTRA_SPELL_STALL | (ANY_SPELL_STALL + Dream_Sifter))"));
            lmb.AddLogicDef(new RawLogicDef("Monomon", "Uumuu_Arena + Defeated_Uumuu + DREAMNAIL + EXTRA_SPELL_STALL + Dream_Sifter"));
            lmb.AddLogicDef(new RawLogicDef("Rancid_Egg-Blue_Lake", "Crossroads_50[left1] + (EXTRA_SPELL_STALL | (ANY_SPELL_STALL + Dream_Sifter))"));

            lmb.AddLogicDef(new RawLogicDef("Wanderer's_Journal-Greenpath_Lower", "Fungus1_11 + NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Wanderer's_Journal-Fungal_Wastes_Thorns_Gauntlet", "Fungus2_04 + NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Shape_of_Unn", "Fungus1_Slug[right1] + FULLDASH + EXTRA_SPELL_STALL + Dream_Sifter"));
            lmb.AddLogicDef(new RawLogicDef("Bretta_Key", "FIVE_CASTS + Dream_Sifter"));
            lmb.AddLogicDef(new RawLogicDef("Mask_Shard-Queen's_Station", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Hallownest_Seal-Queen's_Station", "ANY_SPELL_STALL"));

            lmb.AddLogicDef(new RawLogicDef("Grub-Queen's_Gardens_Marmu", "(Fungus3_48[bot1] | Fungus3_48[right2]) + NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Grub-Isma's_Grove", "FULL_SPELL_STALL + TRAM"));
            lmb.AddLogicDef(new RawLogicDef("Isma's_Tear", "FULL_SPELL_STALL + TRAM"));

            lmb.AddLogicDef(new RawLogicDef("Lurien", "FULL_SPELL_STALL + Quick_Slash + Shaman_Stone + Dream_Sifter"));
            lmb.AddLogicDef(new RawLogicDef("Lore_Tablet-Watcher's_Spire", "FULL_SPELL_STALL + Quick_Slash + Shaman_Stone + Dream_Sifter"));

            lmb.AddLogicDef(new RawLogicDef("Vessel_Fragment-Basin", "(Abyss_04 + (ANYCLAW | WINGS + ENEMYPOGOS) | Abyss_04[top1]) + Can_Visit_Lemm"));

            lmb.AddLogicDef(new RawLogicDef("Lifeblood_Cocoon-Galien", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Pale_Ore-Nosk", "FULLDASH + EXTRA_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Grub-Beast's_Den", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Soul_Totem-Beast's_Den", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Rancid_Egg-Beast's_Den", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Herrah", "NARROW_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Geo_Chest-Weavers_Den", "Deepnest_45_v02[left1] + ANY_SPELL_STALL + Dream_Sifter"));


            lmb.AddLogicDef(new RawLogicDef("Grub-Waterways_Requires_Tram",
                "Waterways_14[bot1] + RIGHTSUPERDASH"));

            // Dream Bosses
            lmb.AddLogicDef(new RawLogicDef("Boss_Essence-Failed_Champion", "SHIELD_BREAKER + Defeated_Failed_Champion"));
            lmb.AddLogicDef(new RawLogicDef("Boss_Essence-Lost_Kin", "SHIELD_BREAKER + Defeated_Lost_Kin"));
            lmb.AddLogicDef(new RawLogicDef("Boss_Essence-Soul_Tyrant", "SHIELD_BREAKER + Defeated_Soul_Tyrant"));
            lmb.AddLogicDef(new RawLogicDef("Boss_Essence-White_Defender", "SHIELD_BREAKER + Defeated_White_Defender"));
            lmb.AddLogicDef(new RawLogicDef("Boss_Essence-Grey_Prince_Zote", "SHIELD_BREAKER + Defeated_Grey_Prince_Zote"));
            //lmb.AddLogicDef(new RawLogicDef("Boss_Essence-NKG", "SHIELD_BREAKER"));


            // Boss Logic
            lmb.AddLogicDef(new RawLogicDef("Uumuu_Arena", "Fungus3_archive_02[top1] + EXTRA_SPELL_STALL"));
            lmb.AddLogicDef(new RawLogicDef("Defeated_Dung_Defender", "Waterways_05_Proxy[bot1] + (ANYCLAW | WINGS) + COMBAT[Dung_Defender] + MOREDOORS_DUNG_DOOR_KEY"));
            lmb.AddLogicDef(new RawLogicDef("Defeated_Enraged_Guardian", "Defeated_Crystal_Guardian"));
        }
    }
}
