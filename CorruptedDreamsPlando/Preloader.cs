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

        // Hive Knight boss scene
        [Preload(SceneNames.Hive_05, "Battle Scene/Hive Knight")]
        public GameObject HiveKnight { get; private set; }
    }
}
