using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.TimeZoneInfo;
namespace CorruptedDreamsPlando.MapAlterations
{
    public class AMoundSoulTotem
    {
        public static void Initialize()
        {
            On.GameManager.OnNextLevelReady += onSceneChange;

        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            var zombie = GameObject.Find("_Enemies/Zombie Runner");
            if (zombie == null)
            {
                zombie = GameObject.Find("Zombie Runner");
                if (zombie == null)
                {
                    return;
                }
            }

            var hpm = zombie.GetComponent<HealthManager>();
            hpm.hp = 9999;
        }

        public static void Unload()
        {
            On.GameManager.OnNextLevelReady -= onSceneChange;
        }

    }
}
