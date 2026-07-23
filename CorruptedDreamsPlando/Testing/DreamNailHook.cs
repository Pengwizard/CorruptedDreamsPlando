using CorruptedDreamsPlando.FsmEditTemplates;
using IL.InControl.NativeDeviceProfiles;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CorruptedDreamsPlando.Testing
{
    public class DreamNailHook
    {
        private static readonly FsmID dreamNailId = new("Knight", "Dream Nail");

        public static void Initialize()
        {
            Events.AddFsmEdit(dreamNailId, HookDreamNail);
            //On.GameManager.OnNextLevelReady += onSceneChange;
        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);

            var spawnPoint = new UnityEngine.Vector3(25f, 6.4f, 0.002f);

            List<Vector3> spawnOffsets = new()
            {
                new(3, 0),
                new(-3, 0),
                new(0, 3),
                new(0, -3),
            };

            GameObject fbsGo1 = new("Spawner1");

            FireballShooter fbs1 = fbsGo1.AddComponent<FireballShooter>();
            fbs1.SpawnPoint = spawnPoint;
            fbs1.SpawnOffset = new UnityEngine.Vector3(0, 2);

            //GameObject fbsGo2 = new("Spawner2");

            //FireballShooter fbs2 = fbsGo2.AddComponent<FireballShooter>();
            //fbs2.SpawnPoint = spawnPoint;
            //fbs2.SpawnOffset = new UnityEngine.Vector3(3, 0);

            //GameObject fbsGo3 = new("Spawner3");

            //FireballShooter fbs3 = fbsGo3.AddComponent<FireballShooter>();
            //fbs3.SpawnPoint = spawnPoint;
            //fbs3.SpawnOffset = new UnityEngine.Vector3(0, -3);

            //GameObject fbsGo4 = new("Spawner4");

            //FireballShooter fbs4 = fbsGo4.AddComponent<FireballShooter>();
            //fbs4.SpawnPoint = spawnPoint;
            //fbs4.SpawnOffset = new UnityEngine.Vector3(-3, 0);


        }

        public static void Unload()
        {
            Events.RemoveFsmEdit(dreamNailId, HookDreamNail);
        }

        public static int counter = 0;
        public static bool leftGround = false;

        public static bool ConditionFunction(GameObject go)
        {
            var tr = go.transform;
            var roundedY = (int)(tr.position.y * 10) / 10;
            if (roundedY <= 11.4f)
            {
                if (leftGround)
                {
                    leftGround = false;
                    return true;
                }

                return false;
            } else
            {
                leftGround = true;
                return false;
            }
        }

        private static void HookDreamNail(PlayMakerFSM fsm)
        {
            //var ddc = fsm.gameObject.AddComponent<DDarkComponent>();
            //ddc.StartCoroutine(DescendingDarkSpawner.Instance.ActivateDDarkForObjectOnCondition(fsm.gameObject, 2, Vector3.zero, ConditionFunction));

            fsm.GetState("End").AddLastAction(new Lambda(PrintEnd));
            //DescendingDarkSpawner.Instance.AddDDarkAtEndOfFSM(fsm.GetState("End"), 1);
            //AbyssShriekSpawner.Instance.AddShriekAtEndOfFSM(fsm.GetState("End"), 1);
            //FireballSpawner.Instance.SpawnFireballAtEndOfFSM(fsm.GetState("End"), 1);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(0, 0, 0), false, true, true, true);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(25f, 6.4f-2, 0.002f), new UnityEngine.Vector3(25f, 6.4f, 0.002f), true, true, false);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(25f, 6.4f+2, 0.002f), new UnityEngine.Vector3(25f, 6.4f, 0.002f), true, true, false);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(25f-2, 6.4f, 0.002f), new UnityEngine.Vector3(25f, 6.4f, 0.002f), true, true, false);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(25f+2, 6.4f, 0.002f), new UnityEngine.Vector3(25f, 6.4f, 0.002f), true, true, false);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(0, 0, 0), false, true, false, true);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(0, 0, 0), true, true, false, false);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(0, 0, 0), false, false, false, false);
            //FireballSpawner.Instance.SpawnShadeSoulAtEndOfFSM(fsm.GetState("End"), 1, new UnityEngine.Vector3(0, 0, 0), false, false, false, true);


            //FireballSpawner.Instance.SpawnFireballAtEndOfFSM(fsm.GetState("End"), 1);
        }

        private static void PrintEnd()
        {
            CorruptedDreamsPlandoMod.Instance.Log("Dream nail ended");
        }
    }
}
