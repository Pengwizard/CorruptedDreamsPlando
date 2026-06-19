using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using KorzUtils.Helper;
using Satchel;
using Satchel.Futils;
using SFCore.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
    public class FullScreenTitleDrop
    {
        public static GameObject WhiteSolidPrefab;
        public static GameObject BossTitlePrefab;

        public static void InitializePrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            WhiteSolidPrefab = preloadedObjects["Dream_Final_Boss"]["Boss Control/white_solid"];
            BossTitlePrefab = preloadedObjects["Dream_Final_Boss"]["Boss Control/Boss Title"];
        }

        public static void SetupTitleUp(
            PlayMakerFSM fsm,
            string titleText,
            string fsmStateToStartFrom,
            string fsmStateAfterTitle,
            float titleFadeUpDelay,
            float titleFadeDownDelay,
            float whiteSolidUpTime,
            float whiteSolidUpDelay,
            Color bgColor,
            Color fgColor,
            string startTitleFsmEvent = "FINISHED",
            System.Func<bool> condition = null,
            string subTitleText = null)
        {
            FsmState flashUp = Satchel.FsmUtil.AddState(fsm, "Title Flash Up");
            FsmState flashDown = Satchel.FsmUtil.AddState(fsm, "Title Flash Down");


            // flashUp.AddTransition("FINISHED", "Title Flash Down");
            Satchel.FsmUtil.AddTransition(flashUp, "FINISHED", "Title Flash Down");
            if (fsmStateAfterTitle != "")
            {
                Satchel.FsmUtil.AddTransition(flashDown, "FINISHED", fsmStateAfterTitle);
            }

            GameObject whiteFader = UObject.Instantiate(WhiteSolidPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var whiteFaderSpriteRenderer = whiteFader.GetComponent<SpriteRenderer>();
            whiteFaderSpriteRenderer.color = bgColor;
            whiteFader.transform.position = new Vector3(30, 30, -1);
            float ogX = whiteFader.transform.GetScaleX();
            float ogY = whiteFader.transform.GetScaleY();
            whiteFader.transform.SetScaleX(whiteFader.transform.GetScaleX() * 0.001f);
            whiteFader.transform.SetScaleY(whiteFader.transform.GetScaleY() * 0.001f);

            GameObject bossTitle = UObject.Instantiate(BossTitlePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            bossTitle.SetActive(true);
            //bossTitle.SetActive(false);
            var bossTitleTMP = bossTitle.GetComponent<TextMeshPro>();
            foreach (var g in bossTitle.GetComponentsInChildren<TextMeshPro>())
            {
                g.color = fgColor;
                // g.text = pretitleText;
                if (subTitleText != null)
                {
                    g.SetText(subTitleText);
                }
            }

            bossTitleTMP.color = fgColor;
            bossTitleTMP.SetText(titleText);

            whiteFader.SetActive(true);
            //whiteFader.SetActive(false);
            // whiteFader.SetActive(false);
            whiteFader.GetComponent<SpriteRenderer>().enabled = true;
            // whiteFader.GetComponent<SpriteRenderer>().enabled = false;


            var titleStart = Satchel.FsmUtil.GetState(fsm, fsmStateToStartFrom);

            titleStart.RemoveTransitionsOn(startTitleFsmEvent);
            Satchel.FsmUtil.AddTransition(titleStart, startTitleFsmEvent, "Title Flash Up");
            condition = () => { return true; };

            var faderFSM = whiteFader.LocateMyFSM("FSM");
            faderFSM.GetVariable<FsmFloat>("Up Time").Value = whiteSolidUpTime;
            faderFSM.GetVariable<FsmFloat>("Up Delay").Value = whiteSolidUpDelay;

            FadeGroupUp slot2FadeGroup = new FadeGroupUp();
            slot2FadeGroup.target = new FsmOwnerDefault() { GameObject = new FsmGameObject { Value = bossTitle }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };

            FadeGroupDown slot4FadeGroup = new FadeGroupDown();
            slot4FadeGroup.target = new FsmOwnerDefault() { GameObject = new FsmGameObject { Value = bossTitle }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            slot4FadeGroup.fast = false;

            //Satchel.FsmUtil.AddCustomAction(flashUp, () => {
            //    // if you want to do something before text goes up.
            //});

            // Satchel.FsmUtil.AddAction(flashUp, slot0FlashUp);

            // Specifically using Satchel here because every 2nd utility pack has a version of FSM utils...
            Satchel.FsmUtil.AddAction(flashUp, new Wait() { time = 1f });
            Satchel.FsmUtil.AddCustomAction(flashUp, (() => {
                whiteFader.SetActive(true);
                bossTitle.SetActive(true);
                CorruptedDreamsPlandoMod.Instance.Log("Bringing up screen");
                whiteFader.transform.SetScaleX(ogX * 3f);
                whiteFader.transform.SetScaleY(ogY * 3f);
                whiteFader.LocateMyFSM("FSM").SetState("Init");
            }));

            Satchel.FsmUtil.AddAction(flashUp, slot2FadeGroup);

            Satchel.FsmUtil.AddAction(flashUp, new Wait() { time = titleFadeUpDelay });


            Satchel.FsmUtil.AddAction(flashDown, slot4FadeGroup);
            Satchel.FsmUtil.AddAction(flashDown, new Wait() { time = titleFadeDownDelay });
        }
    }
}
