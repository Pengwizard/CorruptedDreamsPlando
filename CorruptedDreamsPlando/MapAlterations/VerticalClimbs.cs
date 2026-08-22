using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.TimeZoneInfo;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class TransitionInfo
    {
        public string targetScene;
        public string entryPoint;
        public string transitionObject;
    }

    public class VerticalClimbs
    {
        public static Dictionary<string, TransitionInfo> transitionInfo = new()
        {
            {
                "Deepnest_East_03", new TransitionInfo()
                {
                    targetScene = "Deepnest_East_07",
                    entryPoint = "bot2",
                    transitionObject = "top2",
                }
            },
            {
                "Mines_13", new TransitionInfo()
                {
                    targetScene = "Mines_20",
                    entryPoint = "bot1",
                    transitionObject = "top1",
                }
            },
            {
                "Mines_23", new TransitionInfo()
                {
                    targetScene = "Mines_25",
                    entryPoint = "left1",
                    transitionObject = "top1",
                }
            }
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
            if (self.sceneName == "Deepnest_East_07")
            {
                var platform = GameObject.Find("plat_float_07 (2)");
                platform.transform.SetPosition2D(32.7f, 3.3f);

                // 32.6201 4.94 0.07

                var corpse = GameObject.Find("Col_outside_corpse_collapse_020003");
                corpse.transform.SetPosition2D(32.6201f, 4.94f);

                var platform2 = GameObject.Instantiate(platform);
                platform.transform.SetPosition2D(40.7f, 3.3f);
                var corpse2 = GameObject.Instantiate(corpse);
                corpse2.transform.SetScaleX(-1);
                corpse.transform.SetPosition2D(40.5f, 4.94f);

            }

            orig(self);
            if (!transitionInfo.ContainsKey(self.sceneName))
            {
                return;
            }

            var traInfo = transitionInfo[self.sceneName];
            var transitionObj = GameObject.Find(traInfo.transitionObject);
            var bc = transitionObj.GetComponent<BoxCollider2D>();
            bc.enabled = true;

            var transitionPoint = transitionObj.GetComponent<TransitionPoint>();
            if (transitionPoint == null)
            {
                return;
            }

            transitionPoint.entryPoint = traInfo.entryPoint;
            transitionPoint.targetScene = traInfo.targetScene;

            if (self.sceneName == "Deepnest_East_07") {
                transitionPoint.alwaysEnterLeft = true;
            }
        }
    }
}
