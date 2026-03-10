using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Locations;
using Satchel;
using System.Collections.Generic;

namespace CorruptedDreamsPlando.Locations
{
    public class CustomBossEssenceLocation : AutoLocation
    {
        public static string nkgEssecnceLocation = "Boss_Essence-NKG";
        public static string tlEssecnceLocation = "Boss_Essence-Traitor_Lord";

        protected override void OnLoad()
        {
            CorruptedDreamsPlandoMod.Instance.Log("CustomBossEssenceLocation load");
            On.PlayMakerFSM.OnEnable += AddNkgReward;
        }

        protected override void OnUnload()
        {
            CorruptedDreamsPlandoMod.Instance.Log("CustomBossEssenceLocation unload");
            On.PlayMakerFSM.OnEnable -= AddNkgReward;
        }

        private void AddNkgReward(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.gameObject.name == "Knight" && self.FsmName == "Dream Return")
            {
                CorruptedDreamsPlandoMod.Instance.Log("Inside dream return");
                FsmState summonState = self.GetValidState("Prostrate Grimm");
                summonState.InsertCustomAction(() => {
                    GiveInfo gi = new()
                    {
                        FlingType = FlingType.Everywhere,
                        Container = Container.Unknown,
                        MessageType = MessageType.Corner,
                        Transform = self.gameObject.transform
                    };
                    Ref.Settings.Placements[nkgEssecnceLocation].GiveAll(gi);
                }, summonState.Actions.Length);
            }
        }
    }
}
