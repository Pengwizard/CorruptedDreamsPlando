using System;
using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using Satchel;
using SFCore;
using UnityEngine;


namespace CorruptedDreamsPlando.Skills
{
    // Make this more serious in some way
    public class BirthplaceAccess
    {
        public static string invKey = "CD_BIRTHKEY_NAME";
        public static string descKey = "CD_BIRTHKEY_DESC";
        public static string invName = "Caretaker's Key";
        public static string boolName = "Birthkey";

        public static string baseName = "Birthplace_Access";
        public static string skillName = SkillCommon.MakeItemName(baseName);
        public static string uiName = SkillCommon.CleanName(baseName);
        public static string description = "Was made for caretakers to get access to the Birthplace without a King's Soul.";
        public static ISprite pickupSprite = new EmbeddedSprite("BirthplaceAccess");


        public static void InitializeHooks()
        {
            On.HeroController.Update += HeroController_Update;

            On.PlayerData.GetBool += PlayerData_GetBool;
            //On.GameManager.OnNextLevelReady += onSceneChange;
            //AddFsmEdits();

            SetupInventoryItem();
        }

        public static void SetupInventoryItem()
        {
            var sprite = new EmbeddedSprite("BirthplaceAccess");

            ItemHelper.AddNormalItem(sprite.Value, boolName, invKey, descKey);

            Events.AddLanguageEdit(new(invKey), FillName);
            Events.AddLanguageEdit(new(descKey), FillDesc);

            ModHooks.GetPlayerBoolHook += HookInventory;
        }

        private static void FillName(ref string value) => value = invName;

        private static void FillDesc(ref string value) => value = description;

        private static bool HookInventory(string name, bool orig)
        {
            if (name == boolName)
            {
                return CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess;
            }

            return orig;
        }

        private static bool PlayerData_GetBool(On.PlayerData.orig_GetBool orig, PlayerData self, string boolName)
        {
            if (GameManager.instance.sceneName != SceneNames.Abyss_06_Core && GameManager.instance.sceneName != SceneNames.Abyss_04)
            {
                return orig(self, boolName);
            }

            if (boolName == "equippedCharm_36")
            {
                return CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess;
            }

            return orig(self, boolName);
        }


        //private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        //{
        //    orig(self);

        //    if (self.sceneName != SceneNames.Room_Bretta)
        //    {
        //        return;
        //    }

        //    var floorOpen = GameObject.Find("Basement Open");
        //    CorruptedDreamsPlandoMod.Instance.Log($"couldnt find animation");

        //    foreach (var c in floorOpen.GetComponents<PlayMakerFSM>())
        //    {
        //        CorruptedDreamsPlandoMod.Instance.Log($"found component {c.name}");
        //        if (c.name == "FSM")
        //        {
        //            c.enabled = false;
        //        }
        //    }

        //    floorOpen.SetActive(true);

        //    CorruptedDreamsPlandoMod.Instance.Log("inside birthplace access check");
        //    if (self.sceneName != SceneNames.Abyss_06_Core)
        //    {
        //        CorruptedDreamsPlandoMod.Instance.Log("bad scene name");
        //        return;
        //    }

        //    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        //    {
        //        var fsmInParent = go.GetComponentInParent<PlayMakerFSM>();
        //        if (fsmInParent == null)
        //        {
        //            continue;
        //        }

        //        CorruptedDreamsPlandoMod.Instance.Log($"found component {go.name} {fsmInParent.gameObject.name}");
        //    }

        //    var floorOpen = GameObject.Find("Floor Opening Animation");
        //    if (floorOpen == null)
        //    {
        //        CorruptedDreamsPlandoMod.Instance.Log("couldnt find animation");
        //        return;
        //    }

        //    foreach (var c in floorOpen.GetComponents<PlayMakerFSM>())
        //    {
        //        CorruptedDreamsPlandoMod.Instance.Log($"found component {c.name}");
        //        if (c.name == "FSM")
        //        {
        //            c.enabled = false;
        //        }
        //    }

        //    CorruptedDreamsPlandoMod.Instance.Log($"setting active {CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess}");
        //    floorOpen.SetActive(CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess);
        //}

        protected static void AddFsmEdits()
        {
            Events.AddFsmEdit(SceneNames.Abyss_06_Core, new("floor_closed", "Disappear"), FromTheDepths);
            Events.AddFsmEdit(SceneNames.Abyss_06_Core, new("floor_full_open", "FSM"), OfTheAbyss);
            Events.AddFsmEdit(SceneNames.Abyss_06_Core, new("Floor Opening Animation", "FSM"), OfTheAbyss);
        }

        protected static void RemoveFsmEdits()
        {
            Events.RemoveFsmEdit(SceneNames.Abyss_06_Core, new("floor_closed", "Disappear"), FromTheDepths);
            Events.RemoveFsmEdit(SceneNames.Abyss_06_Core, new("floor_full_open", "FSM"), OfTheAbyss);
            Events.RemoveFsmEdit(SceneNames.Abyss_06_Core, new("Floor Opening Animation", "FSM"), OfTheAbyss);
        }

        private static void FromTheDepths(PlayMakerFSM fsm)
        {
            if (CorruptedDreamsPlandoMod.LS.Enabled || !CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess)
                return;

            if (PlayerData.instance.openedBlackEggPath)
                fsm.gameObject.SetActive(false);

            FsmState state1 = fsm.GetValidState("State 1");
            state1.ClearActions();
            state1.RemoveTransitionsOn("BLACK FLOOR OPEN");
        }

        private static void OfTheAbyss(PlayMakerFSM fsm)
        {
            if (CorruptedDreamsPlandoMod.LS.Enabled || !CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess)
                return;

            fsm.enabled = false;
        }

        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess = !CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess;
            }
        }
    }


    public class BirthplaceAccessItem : AbstractItem
    {
        public BirthplaceAccessItem()
        {
            name = BirthplaceAccess.skillName;

            InteropTag tag = Rando.RandoInterop.AddTag(this);

            UIDef = new BigUIDef
            {
                name = new BoxedString(BirthplaceAccess.uiName),
                shopDesc = new BoxedString(BirthplaceAccess.description),
                sprite = BirthplaceAccess.pickupSprite,
                bigSprite = BirthplaceAccess.pickupSprite,
                descOne = new BoxedString(BirthplaceAccess.description),
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess = true;
        }

        public override bool Redundant()
        {
            return CorruptedDreamsPlandoMod.LS.CollectedBirthplaceAccess;
        }
    }
}