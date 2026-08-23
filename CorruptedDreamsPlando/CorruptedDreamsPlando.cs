using CorruptedDreamsPlando.CorruptedDreamBosses;
using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.MapAlterations;
using CorruptedDreamsPlando.Modules;
using CorruptedDreamsPlando.ShaderThings;
using CorruptedDreamsPlando.Skills;
using CorruptedDreamsPlando.Testing;
using ItemChanger;
using ItemChanger.Locations;
using MapChanger;
using Modding;
using MoreDoors.Data;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace CorruptedDreamsPlando
{
    public class CorruptedDreamsPlandoMod : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>
    {
        private static CorruptedDreamsPlandoMod _instance;

        internal static CorruptedDreamsPlandoMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(CorruptedDreamsPlandoMod)} was never constructed");
                }
                return _instance;
            }
        }

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public CorruptedDreamsPlandoMod() : base("CorruptedDreamsPlando")
        {
            _instance = this;

            VoidCaster.InitializeHooks();
            Determination.InitializeHooks();
            // BirthplaceAccess.InitializeHooks();
        }

        public override List<(string, string)> GetPreloadNames() => Preloader.Instance.GetPreloadNames();

        private static void OnSetGameMap(GameObject goMap)
        {
            try
            {
                VmmPinManager.MakePins(goMap);
            }
            catch (Exception e)
            {
                Instance.LogError(e);
            }
        }

        private static void OnEnterGame()
        {
            MapChanger.Events.OnSetGameMap += OnSetGameMap;
        }

        public MapAlterationsManager mam;
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Log($"{Preloader.Instance.GetPreloadNames()}");

            Preloader.Instance.Initialize(preloadedObjects);
            FullScreenTitleDrop.InitializePrefabs();
            Grayscaler.Initialize();

            // Initialize Spell stuff
            DescendingDarkSpawner.Initialize();
            AbyssShriekSpawner.Initialize();
            FireballSpawner.Initialize();
            DreamNailHook.Initialize();

            // Rando stuff
            MapAlterations.DungDoorAdder.AddDungDoor();
            Rando.RandoInterop.Hook();

            //TraitorLordDream.DreamEnterPrefab = preloadedObjects[SceneNames.Crossroads_10_boss_defeated]["Prayer Room/FK Corpse/Dream Enter"];
            //TraitorLordDream.DreamEntry = preloadedObjects[SceneNames.Dream_01_False_Knight]["Dream Entry"];
            //TraitorLordDream.ClothCorpsePrefab = preloadedObjects[SceneNames.Fungus3_23]["Cloth Corpse"];
            //TraitorLordDream.DreamArenaBorder = preloadedObjects[SceneNames.Fungus3_40]["Dream Gate"];

            //CorruptedLostKin.WhiteSolidPrefab = preloadedObjects[SceneNames.Dream_Final_Boss]["Boss Control/white_solid"];
            //CorruptedLostKin.BossTitlePrefab = preloadedObjects[SceneNames.Dream_Final_Boss]["Boss Control/Boss Title"];


            // Map Alterations
            // TODO make "map alterations" module

            //FuckRightSideShrogo.Initialize();
            //LoopTransitions.Initialize();
            //LurienElevatorOutOfService.Initialize();
            //EnemyDisabler.Initialize();
            //VerticalClimbs.Initialize();
            //UpDirectionSign.Initialize();
            //AMoundSoulTotem.Initialize();
            //NoDreamPlats.Initialize();

            RandoController.OnExportCompleted += AddModules;
            //TraitorLordDream.Initialize();
            //SoulRefiller.Initialize();
            // todo make "extra gimmicks" module
            InstantSalubraSoulRefill.Initialize();
            Shadeless.Initialize();
            DistruptedSpells.Initialize();

            // Super boss time!
            // TODO make "super bosses" into a module.

            // TODO: Re add these bosses later
            //CorruptedLostKin.Initialize();
            //CorruptedWhiteDefender.Initialize();
            //CorruptedSoulTyrant.Initialize();
            //CorruptedGPZ.Initialize();

            CorruptedNKG.Initialize();

            MapChanger.Events.OnEnterGame += OnEnterGame;


            // For testing
            mam = new();
            mam.Initialize();

            Log("Initialized");
        }

        public void AddModules(RandoController rc)
        {
            //ItemChangerMod.Modules.Add<CorruptedDreamShields>();
        }

        public static LocalSettings LS { get; set; } = new();
        public void OnLoadLocal(LocalSettings s) => LS = s;
        public LocalSettings OnSaveLocal() => LS;

        public static GlobalSettings GS { get; set; } = new();
        public void OnLoadGlobal(GlobalSettings s) => GS = s;
        public GlobalSettings OnSaveGlobal() => GS;

    }

    public class LocalSettings
    {
        public bool Enabled = true;
        public bool CollectedVoidCaster = false;
        public bool ColletedDetermination = false;
        public bool CollectedBirthplaceAccess = false;
        public bool CollectedDreamerMap = false;
    }

    public class GlobalSettings
    {
        public bool Enabled = true;
    }
}
