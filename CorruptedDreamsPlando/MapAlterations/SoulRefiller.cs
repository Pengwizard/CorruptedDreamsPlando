using ItemChanger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KorzUtils.Helper;
using Modding;

namespace CorruptedDreamsPlando.MapAlterations
{
    
    public class SoulRefiller
    {
        private static string WP_MESSAGE = "The presence of the king restores your energy";

        private static Dictionary<string, string> roomsToRefreshSoul = new Dictionary<string, string>
        {
            { SceneNames.Fungus1_Slug, "This proximity to The Slug fills you with power..." },
            { SceneNames.Fungus1_26, "This proximity to The Slug fills you with power..." },
            {SceneNames.White_Palace_01, WP_MESSAGE},
            {SceneNames.White_Palace_02, WP_MESSAGE},
            {SceneNames.White_Palace_03_hub, WP_MESSAGE},
            {SceneNames.White_Palace_04, WP_MESSAGE},
            {SceneNames.White_Palace_05, WP_MESSAGE},
            {SceneNames.White_Palace_06, WP_MESSAGE},
            {SceneNames.White_Palace_07, WP_MESSAGE},
            {SceneNames.White_Palace_08, WP_MESSAGE},
            {SceneNames.White_Palace_09, WP_MESSAGE},
            {SceneNames.White_Palace_11, WP_MESSAGE},
            {SceneNames.White_Palace_12, WP_MESSAGE},
            {SceneNames.White_Palace_13, WP_MESSAGE},
            {SceneNames.White_Palace_14, WP_MESSAGE},
            {SceneNames.White_Palace_15, WP_MESSAGE},
            {SceneNames.White_Palace_16, WP_MESSAGE},
            {SceneNames.White_Palace_17, WP_MESSAGE},
            {SceneNames.White_Palace_18, WP_MESSAGE},
            {SceneNames.White_Palace_19, WP_MESSAGE},
            {SceneNames.White_Palace_20, WP_MESSAGE}
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
            if (!roomsToRefreshSoul.ContainsKey(self.sceneName))
            {
                return;
            }

            // Trying to zero out the soul
            PlayerData.instance.ClearMP();
            PlayerData.instance.AddMPCharge(33 * 6);
            GameHelper.DisplayMessage(roomsToRefreshSoul[self.sceneName]);
        }
    }
}
