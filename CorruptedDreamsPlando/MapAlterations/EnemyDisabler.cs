using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class EnemyDisabler : IMapAlteration
    {
        public static List<string> SceneList = new()
        {
            "mines_25","mines_23", "mines_34", "mines_20", "mines_13",
            "fungus1_09", "fungus1_13", "fungus2_11",
            "deepnest_41", "deepnest_45_v02", "tutorial_01"
        };

        public void Initialize()
        {
            ModHooks.OnEnableEnemyHook += SetupDisableEnemies;
        }

        public void Unload()
        {
            ModHooks.OnEnableEnemyHook -= SetupDisableEnemies;
        }

        private static bool SetupDisableEnemies(GameObject enemy, bool isAlreadyDead)
        {
            if (SceneList.Contains(GameManager.instance.sceneName.ToLower()))
            {
                var hm = enemy.GetComponent<HealthManager>();
                hm.SetGeoLarge(0);
                hm.SetGeoMedium(0);
                hm.SetGeoSmall(0);

                hm.Die(0, AttackTypes.Nail, true);
            }

            return isAlreadyDead;
        }
    }
}
