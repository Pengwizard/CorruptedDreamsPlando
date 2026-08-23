using ItemChanger;
using Satchel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.TimeZoneInfo;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class FuckRightSideShrogo : IMapAlteration
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
            if (self.sceneName != SceneNames.White_Palace_15)
            {
                return;
            }

            var sawblade = GameObject.Find("wp_saw");
            if (sawblade == null)
            {
                return;
            }

            GameObject sawbladeBlock = UObject.Instantiate(sawblade, new Vector3(17, 51, 0), Quaternion.identity);
            sawbladeBlock.SetActive(true);
            sawbladeBlock.SetScale(1.36f, 1.36f);
        }
    }
}
