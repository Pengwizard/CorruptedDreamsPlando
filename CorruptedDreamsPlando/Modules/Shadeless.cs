using Modding;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;

namespace CorruptedDreamsPlando.Modules
{
    // This is copied from the Knight of Nights plando by Purenail.
    internal class Shadeless : Module
    {
        private static readonly FsmID deathAnimId = new("Hero Death", "Hero Death Anim");

        public static bool IsShadeless;

        public override void Initialize()
        {
            Events.AddFsmEdit(deathAnimId, ModifyDeathAnim);
            ModHooks.GetPlayerBoolHook += HookIsShadeless;
            IsShadeless = true;
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(deathAnimId, ModifyDeathAnim);
            ModHooks.GetPlayerBoolHook -= HookIsShadeless;
            IsShadeless = false;
        }

        private static void ModifyDeathAnim(PlayMakerFSM fsm)
        {
            fsm.GetState("Remove Geo").ClearActions();
            fsm.GetState("Limit Soul").ClearActions();

            var setShadeState = fsm.GetState("Set Shade");
            setShadeState.AddTransition("SKIP", "Save");
            setShadeState.AddFirstAction(new Lambda(() => {
                fsm.FsmVariables.GetFsmGameObject("Self").Value = fsm.gameObject;
                fsm.SendEvent("SKIP");
            }));

            fsm.GetState("End").RemoveFirstActionOfType<SendMessage>();
        }

        private static bool HookIsShadeless(string name, bool orig) => name == nameof(IsShadeless) ? IsShadeless : orig;
    }
}
