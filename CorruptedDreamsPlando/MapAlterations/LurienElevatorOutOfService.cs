using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using SFCore;
using System;
using UnityEngine;
using System;
using System.Collections;

namespace CorruptedDreamsPlando.MapAlterations
{
    // TODO: Maybe add respawn hazard next to elevator?
    public class LurienElevatorOutOfService : IMapAlteration
    {
        public void Initialize()
        {
            On.GameManager.OnNextLevelReady += onSceneChange;
        }

        public void Unload()
        {
            On.GameManager.OnNextLevelReady -= onSceneChange;
        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (GameManager.instance.sceneName == SceneNames.Ruins2_Watcher_Room)
            {
                var liftRaw = GameObject.Find("Ruins Lift");
                if (liftRaw != null)
                {
                    RuinsLift lift = liftRaw.GetComponent<RuinsLift>();
                    lift.stopPositions = new float[] { 21f, 6.9f };
                }
            }
        }
    }
}
