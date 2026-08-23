using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Modules;
using Satchel;
using Satchel.Futils;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class TraitorLordDream
    {
        public static GameObject DreamEnterPrefab;
        public static GameObject DreamEntry;
        public static GameObject ClothCorpsePrefab;
        public static GameObject DreamArenaBorder;

        // BeginSceneTransition sceneName
        public static string sourceRoom = SceneNames.Crossroads_10_boss_defeated;
        public static string sourceObject = "Prayer Room/FK Corpse/Dream Enter";
        public static string targetFsm = "Control";
        public static string targetState = "Change Scene";
        public static string targetAction = "Change Scene";

        public static string destRoom = SceneNames.Fungus3_23;
        public static string corpseObject = "Cloth Corpse";
        public static Vector3 corpsePosition = new Vector3(34.0316f, 32.5225f, 0.1f);
        public static Vector3 dreamEnterPosition = new Vector3(34f, 29f, 0.0f);
        public static Vector3 dreamReturnPosition = new Vector3(40f, 29f, 0.0f);
        public static Vector3 door1Position = new Vector3(93f, 5.4f, 0.0f);

        public static bool loadedDreamEnter = false;
        // We need the "late" dream enter because some thing need to be overriden only after we enter the dream.
        public static bool loadedLateDreamEnter = false;
        public static bool killedTraitorLordInDream = false;
        public static bool loadingInWithDream = false;

        public static bool IsTargetScene(string sceneName)
        {
            return sceneName == "Fungus3_23_boss" || sceneName == "Fungus3_23";
        }

        public static void Initialize()
        {
            On.GameManager.OnNextLevelReady += setupOverworldState;
            On.GameManager.OnNextLevelReady += lateSetupDreamState;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += setupDreamState;
            On.GameManager.GetCurrentMapZone += GameManagerOnGetCurrentMapZone;
            On.PlayerData.GetBool += OverrideTraitorLordFight;
            On.PlayerData.SetBool += OverrideTraitorKill;
        }

        public static void Unload()
        {
            On.GameManager.OnNextLevelReady -= setupOverworldState;
            On.GameManager.OnNextLevelReady -= lateSetupDreamState;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= setupDreamState;
            // Cleanup just to be sure
            On.GameManager.GetCurrentMapZone -= GameManagerOnGetCurrentMapZone;
            On.PlayerData.GetBool -= OverrideTraitorLordFight;
            On.PlayerData.SetBool -= OverrideTraitorKill;
        }


        // Need this to lie to the game that we are in a dream state.
        // Important for dying in the fight.
        private static string GameManagerOnGetCurrentMapZone(On.GameManager.orig_GetCurrentMapZone orig, GameManager self)
        {
            if (self.sceneName.Contains(destRoom))
            {
                if (loadedLateDreamEnter)
                {
                    return MapZone.DREAM_WORLD.ToString();
                }
            }
            return orig(self);
        }

        private static void setupDreamState(Scene prev, Scene next)
        {
            if (next.name != destRoom)
            {
                return;
            }

            setupDreamReturnDoor();
            setupDoor1();

            if (!loadingInWithDream)
            {
                // Loaded in not via dream, erase late dream load state.
                loadedLateDreamEnter = false;
                return;
            }

            loadedLateDreamEnter = true;

            setupDreamEntry();
        }

        private static void OverrideTraitorKill(On.PlayerData.orig_SetBool orig, PlayerData self, string boolName, bool value)
        {
            if (GameManager.instance.sceneName.Contains(destRoom) && boolName == "killedTraitorLord")
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Trying to set TL kill in scene {GameManager.instance.sceneName}");
                if (loadingInWithDream || GameManager.instance.GetCurrentMapZone() == MapZone.DREAM_WORLD.ToString())
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Trying to set TL kill in a dream to {value}");
                    // We are in the dream state, and in our special room.
                    killedTraitorLordInDream = value;
                } else
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Setting TL kill but not in dream");
                }
            }

            orig(self, boolName, value);
        }

        private static bool OverrideTraitorLordFight(On.PlayerData.orig_GetBool orig, PlayerData self, string boolName)
        {
            if (GameManager.instance.sceneName.Contains(destRoom) && boolName == "killedTraitorLord")
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Trying to get TL kill in scene {GameManager.instance.sceneName}");
                if (loadingInWithDream || GameManager.instance.GetCurrentMapZone() == MapZone.DREAM_WORLD.ToString())
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Trying to get TL kill in a dream which is {killedTraitorLordInDream}");
                    // We are in the dream state, and in our special room.
                    return killedTraitorLordInDream;
                } else
                {
                    CorruptedDreamsPlandoMod.Instance.Log($"Getting TL kill but not in dream");
                    return false;
                }
            }

            return orig(self, boolName);
        }

        // Need this to have something "catch" the knight entering via dream.
        private static void setupDreamEntry()
        {
            GameObject dreamEntry = UObject.Instantiate(DreamEntry, door1Position, Quaternion.identity);
            dreamEntry.name = "Dream Entry";
            dreamEntry.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
        }

        // Door 1 is used for dream entrance.
        private static void setupDoor1()
        {
            GameObject doorObj = new("door1");
            var door1 = doorObj.AddComponent<TransitionPoint>();
            door1.isADoor = false;
            door1.dontWalkOutOfDoor = true;
            door1.gameObject.transform.position = door1Position;

            var dreamHazardRespawn = doorObj.AddComponent<HazardRespawnMarker>();
            dreamHazardRespawn.gameObject.transform.position = door1Position;
            door1.respawnMarker = dreamHazardRespawn;
        }

        private static void setupDreamReturnDoor()
        {
            GameObject door_dreamReturn = new("door_dreamReturn");
            var dreamReturnDoor = door_dreamReturn.AddComponent<TransitionPoint>();
            dreamReturnDoor.isADoor = false;
            dreamReturnDoor.dontWalkOutOfDoor = true;
            dreamReturnDoor.gameObject.transform.position = dreamReturnPosition;

            var dreamReturnHazardRespawn = door_dreamReturn.AddComponent<HazardRespawnMarker>();
            dreamReturnHazardRespawn.gameObject.transform.position = dreamReturnPosition;
            dreamReturnDoor.respawnMarker = dreamReturnHazardRespawn;
        }

        private static void lateSetupDreamState(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (self.sceneName != destRoom)
            {
                // Not our room to change.
                return;
            }

            if (!loadedLateDreamEnter)
            {
                // We loaded in with a dream so we can skip everything.
                return;
            }

            var transitionRight = GameObject.Find("right1");
            var right1Body = transitionRight.GetComponent<BoxCollider2D>();
            right1Body.isTrigger = false;
            transitionRight.layer = LayerMask.NameToLayer("Terrain");

            var transitionLeft = GameObject.Find("left1");
            var left1Body = transitionLeft.GetComponent<BoxCollider2D>();
            left1Body.isTrigger = false;
            transitionLeft.layer = LayerMask.NameToLayer("Terrain");

            
            GameObject dg1 = UObject.Instantiate(DreamArenaBorder, transitionRight.transform.position + new Vector3(0, 0, 0.2f), Quaternion.identity);
            dg1.SetActive(true);
            GameObject dg2 = UObject.Instantiate(DreamArenaBorder, transitionLeft.transform.position + new Vector3(0, 0, 0.2f), Quaternion.identity);
            dg2.SetActive(true);
        }

        private static void setupOverworldState(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            if (self.sceneName != destRoom)
            {
                // Not our room to change.
                return;
            }

            loadingInWithDream = false;
            if (loadedLateDreamEnter)
            {
                // We loaded in with a dream so we can skip everything.
                return;
            }

            if (!HeroController.instance.playerData.killedTraitorLord)
            {
                // No need to make any changes.
                return;
            }

            try
            {
                GameObject corpseCloth = UObject.Instantiate(ClothCorpsePrefab, corpsePosition, Quaternion.identity);
                Component.Destroy(corpseCloth.GetComponent<DeactivateIfPlayerdataFalse>());
                corpseCloth.SetActive(true);
            }
            catch (Exception)
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Got an error with corpse");
            }

            // Shield is active, no need to add the dream enter.
            // TODO: Add dream glowy effect?
            // TODO: Actually place shield
            if (CorruptedDreamShields.isShieldActive())
            {
                CorruptedDreamShields.PlaceShieldIfNeeded(dreamEnterPosition);
            }


            // Creating the dream enter object - this is what actually yeets you into the dream.
            GameObject dreamEnter = UObject.Instantiate(DreamEnterPrefab, dreamEnterPosition, Quaternion.identity);
            dreamEnter.name = "Dream Enter";
            dreamEnter.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            // Re-target the load scene of the dream
            var changeSceneFsm = dreamEnter.LocateMyFSM("Control");
            var changeSceneState = ItemChanger.Extensions.PlayMakerExtensions.GetState(changeSceneFsm, targetState);
            var changeSceneAction = changeSceneState.Actions[changeSceneState.Actions.Length - 1];
            if (changeSceneAction is BeginSceneTransition)
            {
                (changeSceneAction as BeginSceneTransition).entryGateName = "door1";
                (changeSceneAction as BeginSceneTransition).sceneName = destRoom;
            }

            // Setting up where to return to
            var impactState = ItemChanger.Extensions.PlayMakerExtensions.GetState(changeSceneFsm, "Impact");
            var dreamReturnSceneAction = impactState.Actions[2];
            if (dreamReturnSceneAction is SetPlayerDataString)
            {
                (dreamReturnSceneAction as SetPlayerDataString).value = self.sceneName;
            }

            // Setting up something to let us know very early in room load that we are in a dream.
            CustomFsmAction setMyBool = new CustomFsmAction();
            setMyBool.method = () =>
            {
                loadingInWithDream = true;
            };
            Satchel.FsmUtil.InsertAction(changeSceneState, setMyBool, 0);
        }

    }
}
