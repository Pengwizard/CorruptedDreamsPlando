using CorruptedDreamsPlando.CorruptedDreamBosses;
using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.MapAlterations;
using CorruptedDreamsPlando.Modules;
using CorruptedDreamsPlando.ShaderThings;
using CorruptedDreamsPlando.Skills;
using Modding;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            LetterOfCredits.InitializeHooks();
            BirthplaceAccess.InitializeHooks();
        }

        public override List<(string, string)> GetPreloadNames() => Preloader.Instance.GetPreloadNames();

        public MapAlterationsManager mam;
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            // TODO: 
            // * clean the shit out of the code on the main branch
            // * Have more people playtest NKG


            Log("Initializing");

            Preloader.Instance.Initialize(preloadedObjects);
            FullScreenTitleDrop.InitializePrefabs();
            Grayscaler.Initialize();

            // Initialize Spell stuff
            DescendingDarkSpawner.Initialize();
            AbyssShriekSpawner.Initialize();
            FireballSpawner.Initialize();
            //DreamNailHook.Initialize();

            // Rando stuff
            MapAlterations.DungDoorAdder.AddDungDoor();
            Rando.RandoInterop.Hook();

            Log("Initialized");
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
        public bool CollectedLetterOfCredits = false;
        public bool CollectedBirthplaceAccess = false;
    }

    public class GlobalSettings
    {
        public bool Enabled = true;
    }
}
