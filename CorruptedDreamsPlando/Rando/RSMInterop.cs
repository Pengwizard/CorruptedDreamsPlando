using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace CorruptedDreamsPlando.Rando
{
    internal static class RSMInterop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new CorruptedDreamsSettingsProxy());
        }
    }

    internal class CorruptedDreamsSettingsProxy : RandoSettingsProxy<GlobalSettings, string>
    {
        public override string ModKey => CorruptedDreamsPlandoMod.Instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; } = new EqualityVersioningPolicy<string>(CorruptedDreamsPlandoMod.Instance.GetVersion());

        public override void ReceiveSettings(GlobalSettings settings)
        {
            settings ??= new();
            PlandoMenuPage.Instance.plandoMEF.SetMenuValues(settings);
        }

        public override bool TryProvideSettings(out GlobalSettings settings)
        {
            settings = CorruptedDreamsPlandoMod.GS;
            return settings.Enabled;
        }
    }
}