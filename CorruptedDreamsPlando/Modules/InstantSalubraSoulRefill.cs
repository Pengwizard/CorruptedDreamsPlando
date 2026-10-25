using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Modules;

namespace CorruptedDreamsPlando.Modules
{
    public class InstantSalubraSoulRefill : Module
    {
        private static readonly FsmID salubraBlessingId = new("Blessing Ghost", "Blessing Control");


        public override void Initialize()
        {
            Events.AddFsmEdit(salubraBlessingId, HurryUpSalubra);
        }


        public override void Unload()
        {
            Events.RemoveFsmEdit(salubraBlessingId, HurryUpSalubra);
        }

        private static void HurryUpSalubra(PlayMakerFSM fsm)
        {
            // Please hurry up we don't have all day
            var pauseStep = fsm.GetState("Blessing Pause");
            if (pauseStep != null)
            {
                var waitStep = pauseStep.GetActionsOfType<Wait>()[0];
                waitStep.time = 1f;
            }

            var fillStep = fsm.GetState("Fill Soul");
            if (fillStep != null)
            {
                var callStep = fillStep.GetActionsOfType<CallMethodProper>()[0];
                callStep.parameters[0].intValue = 15;
            }
        }

    }
}
