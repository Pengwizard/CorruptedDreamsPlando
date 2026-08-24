using CorruptedDreamsPlando.MapAlterations;
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

        protected override void OnLoad()
        {
            On.PlayMakerFSM.OnEnable += AddEssenceRewardsTriggers;
        }

        protected override void OnUnload()
        {
            On.PlayMakerFSM.OnEnable -= AddEssenceRewardsTriggers;
        }

        private void AddEssenceRewardsTriggers(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.gameObject.name == "Knight" && self.FsmName == "Dream Return")
            {
                FsmState summonState = self.GetValidState("Ready");
                summonState.InsertCustomAction(() => {
                    if (PlayerData.instance.killedNightmareGrimm)
                    {
                        GiveInfo gi = new()
                        {
                            FlingType = FlingType.Everywhere,
                            Container = Container.Unknown,
                            MessageType = MessageType.Corner,
                            Transform = self.gameObject.transform
                        };
                        Ref.Settings.Placements[nkgEssecnceLocation].GiveAll(gi);
                    }

                }, summonState.Actions.Length);
            }
        }
    }
}
