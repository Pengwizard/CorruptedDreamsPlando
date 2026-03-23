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

namespace CorruptedDreamsPlando.Modules
{
    // TODO: Maybe add respawn hazard next to elevator?
    public class LurienElevatorOutOfService
    {
        private static readonly FsmID elevatorControlId = new("Ruins Lift", "Lift Control");


        public static void Initialize()
        {
            On.GameManager.OnNextLevelReady += onSceneChange;
        }

        public static void Unload()
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
