using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using Modding;
using SFCore.Utils;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.Modules
{
    //  FSM deactivate_ifnot_playerdatabool Floor Opening Animation Abyss_06_Core
    // Grimm_sleep_shield Grimm_sleep_shield Grimm_Main_Tent

    public class CorruptedDreamShields : ItemChanger.Modules.Module
    {
        public static GameObject ShieldPrefab;
        public static Dictionary<string, string> VanillaDreamBossCorpseRoomsToObject = new Dictionary<string, string>
        {
            { SceneNames.Crossroads_10_boss_defeated, "Prayer Room/FK Corpse/Dream Enter" },
            { SceneNames.Crossroads_10, "Prayer Room/FK Corpse/Dream Enter" },
            { SceneNames.Abyss_19, "IK Remains/Dream Enter" },
            { SceneNames.Ruins1_24, "Mage Lord Remains/Dream Enter" },
            { SceneNames.Ruins1_24_boss_defeated, "Mage Lord Remains/Dream Enter" },
        };

        public override void Initialize()
        {
            On.GameManager.OnNextLevelReady += onSceneChange;
        }

        public override void Unload()
        {
            On.GameManager.OnNextLevelReady -= onSceneChange;
        }


        public static bool isShieldActive()
        {
            return !CorruptedDreamsPlandoMod.LS.ColletedDetermination;
        }

        private void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (VanillaDreamBossCorpseRoomsToObject.Keys.Contains(self.sceneName))
            {
                var dreamEnter = GameObject.Find(VanillaDreamBossCorpseRoomsToObject[self.sceneName]);
                if (dreamEnter != null)
                {
                    dreamEnter.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
                }

                PlaceShieldIfNeeded(dreamEnter.transform.position);
                //GameObject shield = UObject.Instantiate(ShieldPrefab, dreamEnter.transform.position, Quaternion.identity);
                //shield.name = "Corrupted Dream Shield";
                //shield.SetActive(!CorruptedDreamsPlandoMod.LS.ColletedDetermination);
                //var shieldDeactivateFsm = shield.LocateMyFSM("deactivate");
                //// We are manually settting up/removing the shield, no need for this.
                //shieldDeactivateFsm.enabled = false;
            }
        }

        public static void PlaceShieldIfNeeded(Vector3 position)
        {
            GameObject shield = UObject.Instantiate(ShieldPrefab, position, Quaternion.identity);
            shield.name = "Corrupted Dream Shield";
            shield.SetActive(!CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            var shieldDeactivateFsm = shield.LocateMyFSM("deactivate");
            // We are manually settting up/removing the shield, no need for this.
            shieldDeactivateFsm.enabled = false;
        }
    }
}
