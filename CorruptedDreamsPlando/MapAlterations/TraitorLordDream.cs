using CorruptedDreamsPlando.Modules;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Modules;
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
        public static GameObject Door1Prefab;
        public static GameObject DreamEntry;
        public static GameObject ClothCorpsePrefab;

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

        public static void Initialize()
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Adding edit scene hook for TL");
            On.GameManager.OnNextLevelReady += onSceneChange;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += setupDreamState;
            On.GameManager.GetCurrentMapZone += GameManagerOnGetCurrentMapZone;
        }


        public static void Unload()
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Removing edit scene hook for TL");
            On.GameManager.OnNextLevelReady -= onSceneChange;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= setupDreamState;
            On.GameManager.GetCurrentMapZone -= GameManagerOnGetCurrentMapZone;
        }

        private static string GameManagerOnGetCurrentMapZone(On.GameManager.orig_GetCurrentMapZone orig, GameManager self)
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Getting map zone for {self.sceneName} {self.sceneName == destRoom}");
            if (self.sceneName == destRoom)
            {
                if (loadedDreamEnter)
                {
                    return MapZone.DREAM_WORLD.ToString();
                }
            }
            return orig(self);
        }


        private static void setupDreamState(Scene prev, Scene next)
        {
            GameObject obj3 = new("door_dreamReturn");
            var dreamReturnDoor = obj3.AddComponent<TransitionPoint>();
            dreamReturnDoor.isADoor = false;
            dreamReturnDoor.dontWalkOutOfDoor = true;
            dreamReturnDoor.gameObject.transform.position = dreamReturnPosition;

            GameObject obj4 = new("dreamReturnHazardRespawn");
            var dreamReturnHazardRespawn = obj4.AddComponent<HazardRespawnMarker>();
            dreamReturnHazardRespawn.gameObject.transform.position = door1Position;

            dreamReturnDoor.respawnMarker = dreamReturnHazardRespawn;

            if (next.name != destRoom)
            {
                return;
            }

            var sm = GameManager.instance.sm;
            // We got here via not-dreamnail.
            if (prev.name != next.name)
            {
                loadedDreamEnter = false;
                return;
            }

            if (prev.name == next.name && loadedDreamEnter)
            {
                // Died in h
                loadedDreamEnter = false;
                return;
            }

            if (sm.mapZone == GlobalEnums.MapZone.DREAM_WORLD)
            {
                // What?
                return;
            }

            loadedDreamEnter = true;

            CorruptedDreamsPlandoMod.Instance.Log($"Loaded via dream {GameManager.instance.GetCurrentMapZone()}");
            GameObject obj = new("door1");
            var door1 = obj.AddComponent<TransitionPoint>();
            door1.isADoor = false;
            door1.dontWalkOutOfDoor = true;
            door1.gameObject.transform.position = door1Position;

            GameObject obj2 = new("dreamHazardRespawn");
            var dreamHazardRespawn = obj.AddComponent<HazardRespawnMarker>();
            dreamHazardRespawn.gameObject.transform.position = door1Position;

            door1.respawnMarker = dreamHazardRespawn;

            GameObject dreamEntry = UObject.Instantiate(DreamEntry, door1Position, Quaternion.identity);
            dreamEntry.name = "Dream Entry";
            CorruptedDreamsPlandoMod.Instance.Log($"Created dream entry {dreamEntry}");
            dreamEntry.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
        }

        private static void setupOverworldState(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            var sm = GameManager.instance.sm;
            if (loadedDreamEnter)
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Loaded via dream {GameManager.instance.GetCurrentMapZone()}");
                // sm.mapZone = GlobalEnums.MapZone.DREAM_WORLD;
                // GameManager.instance.SetMapZoneToSpecific("DREAM_WORLD");
                return;
            }

            if (!HeroController.instance.playerData.killedTraitorLord)
            {
                // No need to make any changes.
                return;
            }

            if (sm.mapZone != GlobalEnums.MapZone.ROYAL_GARDENS)
            {
                return;
            }

            try
            {
                if (HeroController.instance.playerData.killedTraitorLord)
                {
                    GameObject corpseCloth = UObject.Instantiate(ClothCorpsePrefab, corpsePosition, Quaternion.identity);
                    // corpseCloth.GetComponent<DeactivateIfPlayerdataFalse>().enabled = false;
                    corpseCloth.GetComponent<DeactivateIfPlayerdataFalse>().boolName = "killedTraitorLord";
                    corpseCloth.SetActive(HeroController.instance.playerData.killedTraitorLord);
                }
            }
            catch (Exception)
            {
                CorruptedDreamsPlandoMod.Instance.Log($"Got an error with corpse");
            }

            // Shield is active, no need to add the dream enter.
            // TODO: Add dream glowy effect?
            if (CorruptedDreamShields.isShieldActive())
            {
                return;
            }


            GameObject dreamEnter = UObject.Instantiate(DreamEnterPrefab, dreamEnterPosition, Quaternion.identity);
            dreamEnter.name = "Dream Enter";
            CorruptedDreamsPlandoMod.Instance.Log($"Created dream enter {dreamEnter}");
            dreamEnter.SetActive(CorruptedDreamsPlandoMod.LS.ColletedDetermination);
            CorruptedDreamsPlandoMod.Instance.Log($"dream enter active: {CorruptedDreamsPlandoMod.LS.ColletedDetermination}");
            // We are manually settting up/removing the shield, no need for this.
            // Re-target the load scene of the dream
            var changeSceneFsm = dreamEnter.LocateMyFSM("Control");
            var changeSceneState = changeSceneFsm.GetState(targetState);
            var changeSceneAction = changeSceneState.Actions[changeSceneState.Actions.Length - 1];
            if (changeSceneAction is BeginSceneTransition)
            {
                (changeSceneAction as BeginSceneTransition).entryGateName = "door1";
                (changeSceneAction as BeginSceneTransition).sceneName = destRoom;
            }

            var impactState = changeSceneFsm.GetState("Impact");
            var dreamReturnSceneAction = impactState.Actions[2];
            if (dreamReturnSceneAction is SetPlayerDataString)
            {
                (dreamReturnSceneAction as SetPlayerDataString).value = self.sceneName;
            }
        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            CorruptedDreamsPlandoMod.Instance.Log($"Loading into {self.sceneName}");
            orig(self);
            if (self.sceneName != destRoom)
            {
                
                return;
            }

            //GameObject dreamEntrance = UObject.Instantiate(Door1Prefab, door1Position, Quaternion.identity);
            //dreamEntrance.name = "door1";
            //dreamEntrance.SetActive(true);

            if (loadedDreamEnter)
            {
            } else
            {
                setupOverworldState(orig, self);
            }

        }
    }
}
