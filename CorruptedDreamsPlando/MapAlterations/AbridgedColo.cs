using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using Satchel;
using UnityEngine;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class AbridgedColo : IMapAlteration
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
            if (self.sceneName != SceneNames.Room_Colosseum_Bronze)
            {
                return;
            }

            var manager = GameObject.Find("Colosseum Manager");
            var control = manager.LocateMyFSM("Battle Control");
            var wave5 = control.GetValidState("Wave 5");
            wave5.RemoveTransition("WAVE END");
            wave5.AddTransition("WAVE END", "Wave 28");
        }
    }
}
