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
    // TODO add shield in GPZ room

    public class CorruptedDreamShields // : ItemChanger.Modules.Module
    {
        public static Dictionary<string, string> StandardDreamEnters = new Dictionary<string, string>
        {
            { SceneNames.Crossroads_10_boss_defeated, "Prayer Room/FK Corpse/Dream Enter" },
            { SceneNames.Crossroads_10, "Prayer Room/FK Corpse/Dream Enter" },
            { SceneNames.Abyss_19, "IK Remains/Dream Enter" },
            { SceneNames.Ruins1_24, "Mage Lord Remains/Dream Enter" },
            { SceneNames.Ruins1_24_boss_defeated, "Mage Lord Remains/Dream Enter" },
            { SceneNames.Waterways_15, "Dung Defender_Sleep/Dream Enter" },
            { SceneNames.Room_Bretta_Basement, "Dream Enter" },
        };

        public void Initialize()
        {
            On.GameManager.OnNextLevelReady += onSceneChange;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += handleSpecialConditions;
        }

        public void Unload()
        {
            On.GameManager.OnNextLevelReady -= onSceneChange;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= handleSpecialConditions;
        }


        public static bool isShieldActive()
        {
            return !CorruptedDreamsPlandoMod.LS.ColletedDetermination;
        }

        private void handleSpecialConditions(Scene prev, Scene next)
        {
            if (next.name == SceneNames.Waterways_05)
            {
                // We want dung defender to always be there, either with a shield or entry, but that guy is napping.
                // Yes he will be there even if we didn't beat him but idc this is a plando :)
                HeroController.instance.playerData.dungDefenderSleeping = true;
            }
        }

        private void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (StandardDreamEnters.Keys.Contains(self.sceneName))
            {
                var dreamEnter = GameObject.Find(StandardDreamEnters[self.sceneName]);
                if (dreamEnter != null)
                {
                    dreamEnter.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
                    PlaceShieldIfNeeded(dreamEnter.transform.position);
                }
            }

            lateLoadSpecialCondition(self);
        }

        private void lateLoadSpecialCondition(GameManager self)
        {
            if (self.sceneName == SceneNames.Waterways_15)
            {
                var dungSleep = GameObject.Find("Dung Defender_Sleep");
                //dungSleep.GetComponent<DeactivateIfPlayerdataFalse>().enabled = false;
                //dungSleep.GetComponent<DeactivateIfPlayerdataTrue>().enabled = false;
                dungSleep.SetActive(true);
            }

            //if (self.sceneName == SceneNames.Grimm_Main_Tent)
            //{
            //    // This entire tent is a special case...
            //    if (!PlayerData.instance.gotGrimmNotch)
            //    {
            //        // player didn't get notch, nothing special here...
            //        return;
            //    }

            //    var backroomMask = GameObject.Find("Backroom Mask");
            //    // No masks, no problems.
            //    backroomMask.SetActive(false);

            //    var dreamEnter = GameObject.Find("Dream Enter Grimm");
            //    // var sleepShield = GameObject.Find("Grimm_sleep_shield");

            //    dreamEnter.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            //    GameObject shield = UObject.Instantiate(Preloader.Instance.GrimmShield, new Vector3(167.89f, 12.7f, -0.008f), Quaternion.identity);
            //    // sleepShield.SetActive(!CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            //    shield.GetComponent<DeactivateIfPlayerdataTrue>().enabled = false;
            //    shield.SetActive(!CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            //}
        }

        public static void PlaceShieldIfNeeded(Vector3 position)
        {
            GameObject shield = UObject.Instantiate(Preloader.Instance.DreamShield, position, Quaternion.identity);
            shield.name = "Corrupted Dream Shield";
            shield.SetActive(!CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            var shieldDeactivateFsm = shield.LocateMyFSM("deactivate");
            // We are manually settting up/removing the shield, no need for this.
            shieldDeactivateFsm.enabled = false;
        }
    }
}
