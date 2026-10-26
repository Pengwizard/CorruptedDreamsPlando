using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using CorruptedDreamsPlando.FsmEditTemplates;
using CorruptedDreamsPlando.ShaderThings;
using HKTool.Utils;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using IL;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Modules;
using Modding;
using Newtonsoft.Json;
using Satchel;
using Satchel.Futils;
using UnityEngine;
using UnityEngine.SceneManagement;
using WavLib;
using static CutsceneHelper;
using static SceneLoad;
using Module = ItemChanger.Modules.Module;

namespace CorruptedDreamsPlando.CorruptedDreamBosses
{
    public class CorruptedRadiance : Module
    {
        //private static readonly FsmID beamFSM = new("Boss Control/Radiance/Eye Beam Glow/Ascend Beam", "Control");
        private static readonly FsmID radianceFSM = new("Radiance", "Control");
        private static readonly FsmID attackControl = new("Radiance", "Attack Commands");

        public override void Initialize()
        {
            Events.AddFsmEdit(attackControl, SetupInstantBeams);
            Events.AddFsmEdit(radianceFSM, RadianceDisableAbyss);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }

        public override void Unload()
        {
            Events.RemoveFsmEdit(attackControl, SetupInstantBeams);
            Events.RemoveFsmEdit(radianceFSM, RadianceDisableAbyss);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChange;
        }

        private static void RadianceDisableAbyss(PlayMakerFSM fsm)
        {
            var ascendTele = fsm.GetValidState("Ascend Tele");
            ascendTele.AddCustomAction(() =>
            {
                var bossControl = GameObject.Find("Boss Control");
                var abyssPit = GameObject.Find("Boss Control/Abyss Pit");
                abyssPit.LocateMyFSM("Ascend").enabled = false;
                //bossControl.Find("Ascend Respawns").SetActive(false);
            });

            var platSetup = fsm.GetValidState("Plat Setup");
            platSetup.Actions[4].Enabled = false;


            var bossControl = GameObject.Find("Boss Control");
            var platSets = GameObject.Find("Plat Sets");
            CorruptedDreamsPlandoMod.Instance.Log($"{bossControl}");
            CorruptedDreamsPlandoMod.Instance.Log($"{platSets}");
            CorruptedDreamsPlandoMod.Instance.Log($"{platSets.Find("P2 SetA")}");
            CorruptedDreamsPlandoMod.Instance.Log($"{platSets.Find("Ascend Set")}");
            CorruptedDreamsPlandoMod.Instance.Log($"{bossControl.Find("Ascend Respawns")}");

            platSets.Find("P2 SetA").SetActive(false);
            //platSets.Find("Hazard Plat/Radiant Plat Small (1)").SetActive(false);
            platSets.Find("Ascend Set").SetActive(false);
            bossControl.Find("Ascend Respawns").SetActive(false);

            GameObject.Find("Boss Control/Plat Sets/Hazard Plat/Radiant Plat Small (1)").SetActive(false);
        }

        private static void SetupInstantBeams(PlayMakerFSM fsm)
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Setting up beams to be faster");
            var abStart = fsm.GetValidState("AB Start");
            FsmEditTemplates.Utils.ChangeWaitFloat(fsm, "AB Start", 1, 0.1f);
            FsmEditTemplates.Utils.ChangeWaitFloat(fsm, "Aim", 11, 1.2f);
        }


        private static void OnSceneChange(Scene oldScene, Scene newScene)
        {
            if (newScene.name != SceneNames.Dream_Final_Boss)
            {
                return;
            }
        }

    }
}
