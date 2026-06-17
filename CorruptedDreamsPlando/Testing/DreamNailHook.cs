using CorruptedDreamsPlando.FsmEditTemplates;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;

namespace CorruptedDreamsPlando.Testing
{
    public class DreamNailHook
    {
        private static readonly FsmID dreamNailId = new("Knight", "Dream Nail");

        public static void Initialize()
        {
            Events.AddFsmEdit(dreamNailId, HookDreamNail);
        }


        public static void Unload()
        {
            Events.RemoveFsmEdit(dreamNailId, HookDreamNail);
        }

        private static void HookDreamNail(PlayMakerFSM fsm)
        {
            fsm.GetState("End").AddLastAction(new Lambda(PrintEnd));
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(fsm.GetState("End"), 1);
            //AbyssShriekSpawner.Instance.AddShriekAtEndOfFSM(fsm.GetState("End"), 1);
            FireballSpawner.Instance.SpawnFireballAtEndOfFSM(fsm.GetState("End"), 1);
        }

        private static void PrintEnd()
        {
            CorruptedDreamsPlandoMod.Instance.Log("Dream nail ended");
        }
    }
}
