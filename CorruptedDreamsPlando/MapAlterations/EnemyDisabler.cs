using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class EnemyDisabler
    {
        public static List<string> SceneList = new()
        {
            "mines_25","mines_23", "mines_34", "mines_20", "mines_13", "fungus1_09"
        };

        public static void Initialize()
        {
            ModHooks.OnEnableEnemyHook += SetupDisableEnemies;
        }

        public static void Unload()
        {
            ModHooks.OnEnableEnemyHook -= SetupDisableEnemies;
        }

        private static bool SetupDisableEnemies(GameObject enemy, bool isAlreadyDead)
        {
            if (SceneList.Contains(GameManager.instance.sceneName.ToLower()))
            {
                enemy.SetActive(false);
            }

            return isAlreadyDead;
        }
    }
}
