using ItemChanger;
using PurenailCore.ModUtil;
using UnityEngine;

namespace CorruptedDreamsPlando
{
    internal class Preloader : PurenailCore.ModUtil.Preloader
    {
        public static Preloader Instance { get; private set; } = new();

        // For full screen title

        [Preload("Dream_Final_Boss", "Boss Control/white_solid")]
        public GameObject WhiteSolidPrefab { get; private set; }

        [Preload("Dream_Final_Boss", "Boss Control/Boss Title")]
        public GameObject BossTitlePrefab { get; private set; }

        
        // Dream shield stuff

        [Preload(SceneNames.Abyss_05, "Dusk Knight/Shield")]
        public GameObject DreamShield { get; private set; }

        [Preload(SceneNames.Grimm_Main_Tent, "Grimm_sleep_shield")]
        public GameObject GrimmShield { get; private set; }

        // Bench Light for telegraphs
        [Preload(SceneNames.Town, "RestBench/Light Small")]
        public GameObject BenchLight { get; private set; }

        // Dream Entrance and Traitor Lord

        [Preload(SceneNames.Crossroads_10_boss_defeated, "Prayer Room/FK Corpse/Dream Enter")]
        public GameObject FCDreamEnter { get; private set; }

        [Preload(SceneNames.Dream_01_False_Knight, "Dream Entry")]
        public GameObject FCDreamEntryPrefab { get; private set; }

        [Preload("Fungus3_23", "Cloth Corpse")]
        public GameObject ClothCorpse { get; private set; }

        [Preload(SceneNames.Fungus3_40, "Dream Gate")]
        public GameObject DreamFightBarrier { get; private set; }


        // Zotelings
        [Preload("GG_Mighty_Zote", "Battle Control/Tall Zotes/Zote Crew Tall")]
        public GameObject TallZote { get; private set; }

        [Preload("GG_Mighty_Zote", "Battle Control/Fat Zotes/Zote Crew Fat (1)")]
        public GameObject FatZote { get; private set; }

        [Preload("GG_Mighty_Zote", "Battle Control/Zotelings/Ordeal Zoteling")]
        public GameObject Zoteling { get; private set; }

        [Preload("GG_Mighty_Zote", "Battle Control/Zote Fluke")]
        public GameObject ZoteFluke { get; private set; }

        [Preload("GG_Mighty_Zote", "Battle Control/Zote Salubra")]
        public GameObject ZoteSalubra { get; private set; }

        [Preload("GG_Mighty_Zote", "Battle Control/Zote Thwomp")]
        public GameObject ZoteThwomp { get; private set; }
    }
}
