using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using UnityEngine;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class PlacementInfo
    {
        public float x;
        public float y;
    }

    public class UpDirectionSign : IMapAlteration
    {
        public static Dictionary<string, PlacementInfo> placementInfo = new()
        {
            {
                "Deepnest_East_03", new PlacementInfo()
                {
                    x = 46.3f,
                    y = 132.3f,
                }
            },
            {
                "Mines_13", new PlacementInfo()
                {
                    x = 91.1f,
                    y = 16.6f,
                }
            },
            {
                "Mines_23", new PlacementInfo()
                {
                    x = 45f,
                    y = 24.2f,
                }
            }
        };

        public static ISprite signSprite = new EmbeddedSprite("upSign");

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
            if (!placementInfo.ContainsKey(self.sceneName))
            {
                return;
            }

            var traInfo = placementInfo[self.sceneName];
            var signObj = new GameObject();
            signObj.transform.SetPosition2D(traInfo.x, traInfo.y);
            signObj.transform.SetScaleMatching(3.5f);
            var spr = signObj.AddComponent<SpriteRenderer>();
            spr.sprite = signSprite.Value;
        }
    }
}
