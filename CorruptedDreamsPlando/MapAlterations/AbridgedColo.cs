using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKTool.Utils;
using ItemChanger;
using Satchel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class AbridgedColo : IMapAlteration
    {
        public static void InitializeTest()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
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
            if (newScene.name != SceneNames.Room_Colosseum_Bronze)
            {
                return;
            }

            var manager = newScene.FindGameObject("Colosseum Manager");
            var control = manager.LocateMyFSM("Battle Control");
            var wave5 = control.GetValidState("Wave 5");
            wave5.RemoveTransition("WAVE END");
            wave5.AddTransition("WAVE END", "Wave 28");
        }
    }
}
