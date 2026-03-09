using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;

namespace CorruptedDreamsPlando
{
    public class PlandoMenuPage
    {
        internal MenuPage PlandoPage;
        internal MenuElementFactory<GlobalSettings> plandoMEF;
        internal VerticalItemPanel plandoVIP;

        internal SmallButton JumpToPlandoButton;

        internal static PlandoMenuPage Instance { get; private set; }

        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance.JumpToPlandoButton;
            return true;
        }

        private void SetTopLevelButtonColor()
        {
            if (JumpToPlandoButton != null)
            {
                JumpToPlandoButton.Text.color = CorruptedDreamsPlandoMod.GS.Enabled ? new UnityEngine.Color(0, 1, 0) : new UnityEngine.Color(1, 0, 0);
            }
        }

        private static void ConstructMenu(MenuPage landingPage)
        {
            Instance = new(landingPage);
        }

        private PlandoMenuPage(MenuPage landingPage)
        {
            PlandoPage = new MenuPage(Localize("PlandoPage"), landingPage);
            plandoMEF = new(PlandoPage, CorruptedDreamsPlandoMod.GS);
            plandoVIP = new(PlandoPage, new(0, 300), 75f, true, plandoMEF.Elements);
            Localize(plandoMEF);
            foreach (IValueElement e in plandoMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            JumpToPlandoButton = new(landingPage, Localize("RgbRando"));
            JumpToPlandoButton.AddHideAndShowEvent(landingPage, PlandoPage);
            SetTopLevelButtonColor();
        }
    }
}