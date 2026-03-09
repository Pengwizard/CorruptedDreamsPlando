using CorruptedDreamsPlando.Skills;
using ItemChanger;
using ItemChanger.Locations;
using Modding;
using MoreDoors.Data;
using RandomizerMod.RC;
using System;

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
        }

        public override void Initialize()
        {
            Log("Initializing");

            MapAlterations.DungDoorAdder.AddDungDoor();
            Rando.RandoInterop.Hook();
            VoidCaster.InitializeHooks();

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
    }

    public class GlobalSettings
    {
        public bool Enabled = true;
    }
}
