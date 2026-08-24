using System;
using System.Collections.Generic;
using System.Linq;
using FStats;
using FStats.StatControllers;
using FStats.Util;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using RandomizerCore.Extensions;
using RandoPlus;
using Steamworks;

namespace CorruptedDreamsPlando.Rando
{
    public static class FStatsInterop
    {

        public static void Hook()
        {
            API.OnGenerateFile += GenerateStats;
        }

        private static void GenerateStats(Action<StatController> generateStats)
        {
            generateStats(new MAKStats());
        }
    }

    public class MAKStats : StatController
    {
        public override void Initialize()
        {
            On.HutongGames.PlayMaker.Actions.SetVelocityAsAngle.OnEnter += DetectFireball;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += DetectDreamnail;

        }
        public override void Unload()
        {
            On.HutongGames.PlayMaker.Actions.SetVelocityAsAngle.OnEnter += DetectFireball;
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += DetectDreamnail;
        }


        public int FireballsCast = 0;
        public int AmountDreamNailed = 0;

        private void DetectDreamnail(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ActivateGameObject self)
        {
            orig(self);

            try
            {
                if (self.Fsm.FsmComponent.gameObject.name.Contains("Knight") &&
                    self.Fsm.Name == "Dream Nail" &&
                    self.State.Name == "Slash")
                {
                    AmountDreamNailed++;
                }
            }
            catch (Exception err)
            {
            }
        }

        public void DetectFireball(On.HutongGames.PlayMaker.Actions.SetVelocityAsAngle.orig_OnEnter orig, SetVelocityAsAngle self)
        {
            try
            {
                if (self.Fsm.FsmComponent.gameObject.name.Contains("Fireball") &&
                    self.Fsm.FsmComponent.gameObject.name.Contains("Top") &&
                    self.Fsm.Name == "Fireball Cast")
                {
                    FireballsCast++;
                }
            }
            catch (Exception err)
            {
            }
            orig(self);
        }

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> colums = new() { $"Fireballs cast: {FireballsCast}", $"Dreamnail Used: {AmountDreamNailed}" };

            yield return new()
            {
                Title = "Midautumn's Knightmare",
                MainStat = "Usage Stages",
                StatColumns = colums,
                Priority = BuiltinScreenPriorityValues.ExtensionStats
            };
        }
    }
}