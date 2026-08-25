using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using Modding;
using Satchel;
using UnityEngine;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class UnkillableMyla : IMapAlteration
    {
        public void Initialize()
        {
            On.PlayerData.GetBool += PlayerData_GetBool;
            ModHooks.LanguageGetHook += LanguageGetHook;
        }

        private string LanguageGetHook(string key, string sheetTitle, string orig)
        {
            return key switch
            {
                "MINER_MEET_1" => "Wow! I'm not really sure how you broke down this door... Nice to meet you!",
                "MINER_MEET_1_B" => "Yeah I'm just *mining* my own business here, get it?",
                "MINER_MEET_REPEAT" => "I have a note here, saying 'Thanks for playing the plando. Also if this is Vale, get rekt'. Huh, weird. Are you Vale?",
                _ => orig,
            };
        }

        public void Unload()
        {
            On.PlayerData.GetBool -= PlayerData_GetBool;
            ModHooks.LanguageGetHook += LanguageGetHook;
        }
        private static bool PlayerData_GetBool(On.PlayerData.orig_GetBool orig, PlayerData self, string boolName)
        {
            if (GameManager.instance.sceneName != SceneNames.Crossroads_45 && GameManager.instance.sceneName != SceneNames.Crossroads_14 && GameManager.instance.sceneName != SceneNames.Mines_01)
            {
                return orig(self, boolName);
            }

            if (boolName == "hasSuperDash" || boolName == "mageLordDefeated")
            {
                return false;
            }

            return orig(self, boolName);
        }
    }
}
