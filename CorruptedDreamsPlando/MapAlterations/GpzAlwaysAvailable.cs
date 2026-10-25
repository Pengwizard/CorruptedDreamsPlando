using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKTool.Utils;
using ItemChanger;
using Modding;
using Satchel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class GpzAlwaysAvailable : IMapAlteration
    {
        public static void InitializeTest()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
            On.PlayerData.GetBool += PlayerData_GetBool;
            //On.GameManager.OnNextLevelReady += onSceneChange;
        }

        public void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }

        public void Unload()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChange;
        }

        private static void OnSceneChange(Scene oldScene, Scene newScene)
        {
            if (newScene.name != SceneNames.Room_Bretta)
            {
                return;
            }

            var floorOpen = newScene.FindGameObject("Basement Open");

            foreach (var c in floorOpen.GetComponents<PlayMakerFSM>())
            {
                CorruptedDreamsPlandoMod.Instance.Log($"found component {c.name}");
                if (c.name == "FSM")
                {
                    c.enabled = false;
                }
            }

            floorOpen.SetActive(true);
        }

        private static bool PlayerData_GetBool(On.PlayerData.orig_GetBool orig, PlayerData self, string boolName)
        {
            if (GameManager.instance.sceneName != SceneNames.Room_Bretta)
            {
                return orig(self, boolName);
            }

            if (boolName == "hasDoubleJump")
            {
                return true;
            }

            return orig(self, boolName);
        }

    }
}
