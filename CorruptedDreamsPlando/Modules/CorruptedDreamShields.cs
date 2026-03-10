using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using Modding;
using SFCore.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.Modules
{
    public class CorruptedDreamShields : ItemChanger.Modules.Module
    {
        public static GameObject ShieldPrefab;

        public override void Initialize()
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Adding edit scene hook");
            On.GameManager.OnNextLevelReady += onSceneChange;
            // Events.AddSceneChangeEdit(SceneNames.Crossroads_10_boss_defeated, SetupCorruptedDreamShield);
        }

        public override void Unload()
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Removing edit scene hook");
            On.GameManager.OnNextLevelReady -= onSceneChange;
            // Events.RemoveSceneChangeEdit(SceneNames.Crossroads_10_boss_defeated, SetupCorruptedDreamShield);
        }

        //private string LanguageGetHook(string key, string sheetTitle, string orig) => key switch
        //{
        //    $"{BossCoordinator.TITLE}_SUPER" => "",
        //    $"{BossCoordinator.TITLE}_MAIN" => "Brettor & Brettek",
        //    $"{BossCoordinator.TITLE}_SUB" => "",
        //    _ => orig
        //};

        private void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (self.sceneName == SceneNames.Crossroads_10_boss_defeated || self.sceneName == SceneNames.Crossroads_10)
            {
                GameObject shield = UObject.Instantiate(ShieldPrefab, new Vector3(33f, 48.4f), Quaternion.identity);
                CorruptedDreamsPlandoMod.Instance.Log($"Found shield prefab: {shield}");
                shield.name = "Corrupted Dream Shield";
                shield.SetActive(!CorruptedDreamsPlandoMod.LS.ColletedDetermination);
                var shieldDeactivateFsm = shield.LocateMyFSM("deactivate");
                // We are manually settting up/removing the shield, no need for this.
                shieldDeactivateFsm.enabled = false;
                // var boolTestState = shield.LocateMyFSM("deactivate").GetFsmState("Check");
                // var boolTestAction = boolTestState.Actions[0] as PlayerDataBoolTest;
                // boolTestAction.
                CorruptedDreamsPlandoMod.Instance.Log($"Setting shield to {!CorruptedDreamsPlandoMod.LS.ColletedDetermination} {shield.transform.position}");
                var dreamEnter = GameObject.Find("Prayer Room/FK Corpse/Dream Enter");
                CorruptedDreamsPlandoMod.Instance.Log($"Found dream enter prefab: {dreamEnter} {dreamEnter.transform.position}");
                if (dreamEnter != null)
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Setting dream enter to {CorruptedDreamsPlandoMod.LS.ColletedDetermination}");
                    dreamEnter.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
                }
            }
        }
    }
}
