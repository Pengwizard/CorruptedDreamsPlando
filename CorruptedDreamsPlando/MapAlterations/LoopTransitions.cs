using IL.InControl.NativeDeviceProfiles;
using ItemChanger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class LoopTransitions
    {
        private static Dictionary<string, List<string>> roomsToModify = new Dictionary<string, List<string>>
        {
            {SceneNames.Mines_03, new List<string>{"top1"} },
            {SceneNames.Deepnest_East_02, new List<string>{ "right1" } }
        };

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
            if (!roomsToModify.ContainsKey(self.sceneName)) {
                return;
            }

            foreach (string transitionName in roomsToModify[self.sceneName])
            {
                var transitionObj = GameObject.Find($"_Transition Gates/{transitionName}");
                var transitionPoint = transitionObj.GetComponent<TransitionPoint>();
                if (transitionPoint == null)
                {
                    continue;
                }

                transitionPoint.entryPoint = transitionName;
                transitionPoint.targetScene = self.sceneName;
            }
        }

    }
}
