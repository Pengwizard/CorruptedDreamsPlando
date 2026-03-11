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
            {SceneNames.Mines_03, new List<string>{ "_Transition Gates/top1" } },
            {SceneNames.Deepnest_East_02, new List<string>{ "_Transition Gates/right1" } },
            {SceneNames.Deepnest_10, new List<string>{ "door2" } }
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
                var transitionObj = GameObject.Find(transitionName);                  
                var transitionPoint = transitionObj.GetComponent<TransitionPoint>();
                if (transitionPoint == null)
                {
                    continue;
                }

                if (transitionPoint.isADoor)
                {
                    // for some reason looping doesnt work for me on doors?
                    transitionObj.SetActive(false);
                    continue;
                }

                transitionPoint.entryPoint = transitionObj.name;
                transitionPoint.targetScene = self.sceneName;
            }
        }

    }
}
