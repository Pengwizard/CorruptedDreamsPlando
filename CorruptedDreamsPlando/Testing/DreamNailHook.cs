using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CorruptedDreamsPlando.FsmEditTemplates;
using HutongGames.PlayMaker.Actions;
using IL.InControl.NativeDeviceProfiles;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Satchel;
using SFCore.Utils;
using UnityEngine;

namespace CorruptedDreamsPlando.Testing
{
    public class DreamNailHook
    {
        private static readonly FsmID dreamNailId = new("Knight", "Dream Nail");

        public static void Initialize()
        {
            Events.AddFsmEdit(dreamNailId, HookDreamNail);
            On.GameManager.OnNextLevelReady += onSceneChange;
        }

        private static void onSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);

            var h = Utils.Hero();
            var dnail = h.LocateMyFSM("Dream Nail");
            SetupGasterBlaster(dnail);

            //var spawnPoint = new UnityEngine.Vector3(25f, 6.4f, 0.002f);

            //List<Vector3> spawnOffsets = new()
            //{
            //    new(3, 0),
            //    new(-3, 0),
            //    new(0, 3),
            //    new(0, -3),
            //};

            //GameObject fbsGo1 = new("Spawner1");

            //FireballShooter fbs1 = fbsGo1.AddComponent<FireballShooter>();
            //fbs1.SpawnPoint = spawnPoint;
            //fbs1.SpawnOffset = new UnityEngine.Vector3(0, 2);

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

            fsm.GetValidState("End").AddLastAction(new Lambda(PrintEnd));
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


        // private static GameObject blasterContainer;
        private static List<GameObject> blasterSpawners = new List<GameObject>();
        private static int blasterDirection = 0;
        private static int blasterCurrentIndex = 0;
        private static int blasterFireCount = 0;

        public static void SetupGasterBlaster(PlayMakerFSM fsm)
        {
            var BlasterCount = 16;
            var BlasterSpeed = 30f;
            var BlasterShotDelay = 0.2f;
            var BlasterTelegraph = BlasterShotDelay * 5;
            var BaseOffset = -10f;
            var BlasterShotScale = 1.2f;
            // Step 1 - figure out the amount and distance and angle we want these to be from the knight
            float angleOfFireIntervals = 360.0f / (BlasterCount);

            foreach (var bs in blasterSpawners)
            {
                GameObject.Destroy(bs);
            }

            blasterSpawners.Clear();

            for (int i = 0; i < BlasterCount; ++i)
            {
                var angle = i * angleOfFireIntervals;
                var rds = angle * Mathf.Deg2Rad;

                float xOffset = Mathf.Cos(rds) * BaseOffset;
                float yOffset = Mathf.Sin(rds) * BaseOffset;

                var go = new GameObject();

                var fireballShooter = go.AddComponent<FireballShooter>();

                //fireballShooterUp.name = "Up Shot";
                fireballShooter.SpawnOffset = new Vector3(xOffset, yOffset, 0);
                fireballShooter.BaseObject = Utils.Hero();
                fireballShooter.Direction = angle;
                fireballShooter.ForceLeft = true;
                fireballShooter.Speed = BlasterSpeed;
                fireballShooter.ShotScale = BlasterShotScale;
                fireballShooter.PreFireDelay = BlasterTelegraph;
                fireballShooter.RemoveParticles = true;
                CorruptedDreamsPlandoMod.Instance.Log("Made fireball shooter");

                blasterSpawners.Add(go);
            }

            var blasterCircleStart = fsm.AddFsmState("Blaster Circle Start");
            var blasterCircleStep = fsm.AddFsmState("Blaster Circle Step");
            var blasterCirclePause = fsm.AddFsmState("Blaster Circle Pause");
            var blasterCircleCleanup = fsm.AddFsmState("Blaster Circle Cleanup");

            blasterCircleStart.AddCustomAction(() =>
            {
                var direction = UnityEngine.Random.Range(0, 2);
                if (direction == 0)
                {
                    blasterDirection = 1;
                }
                else
                {
                    blasterDirection = -1;
                }

                blasterCurrentIndex = UnityEngine.Random.Range(0, BlasterCount);
                blasterFireCount = UnityEngine.Random.Range(BlasterCount, BlasterCount * 2);
            });

            // Satchel.FsmUtil.AddAction(shriekWaveStart, GetClipState(AbyssShriekSpawner.ShriekAudio));
            blasterCircleStart.AddFsmTransition("FINISHED", blasterCircleStep.Name);


            //Satchel.FsmUtil.AddAction(shriekWaveStep, GetClipState(AbyssShriekSpawner.ShriekAudio, 0f, 3f));

            blasterCircleStep.AddCustomAction(() =>
            {
                // This is always a valid 
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Step 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                CorruptedDreamsPlandoMod.Instance.Log($"shoot {blasterCurrentIndex}");
                if (0 <= blasterCurrentIndex && blasterCurrentIndex <= blasterSpawners.Count - 1)
                {
                    blasterSpawners[blasterCurrentIndex].GetComponent<FireballShooter>().ShootStraight();
                }
            });

            blasterCircleStep.AddFsmAction(new Wait() { time = BlasterShotDelay, finishEvent = new("FINISHED") });
            blasterCircleStep.AddFsmTransition("FINISHED", blasterCirclePause.Name);

            blasterCirclePause.AddCustomAction(() =>
            {
                // This is always a valid 
                // Note that this check is just <, not <=, because the one above includes the last step
                //CorruptedDreamsPlandoMod.Instance.Log($"Shriek Wave Pause 0 {shriekCurrentIndex} {shriekSpawners.Count - 1}");
                blasterCurrentIndex += blasterDirection;
                if (blasterCurrentIndex < 0)
                {
                    blasterCurrentIndex = blasterSpawners.Count - 1;
                }

                if (blasterCurrentIndex > blasterSpawners.Count - 1)
                {
                    blasterCurrentIndex = 0;
                }

                blasterFireCount--;

                if (blasterFireCount >= 0)
                {
                    fsm.SendEvent("BLASTER CIRCLE CONTINUE");
                }
                else
                {
                    fsm.SendEvent("BLASTER CIRCLE DONE");
                }
            });
            blasterCirclePause.AddFsmTransition("BLASTER CIRCLE CONTINUE", blasterCircleStep.Name);
            blasterCirclePause.AddFsmTransition("BLASTER CIRCLE DONE", blasterCircleCleanup.Name);

            // Cleanup step is currently doing nothing
            blasterCircleCleanup.AddFsmAction(new Wait() { time = 1f, finishEvent = new("FINISHED") });


            blasterCircleCleanup.AddFsmTransition("FINISHED", "Inactive");

            var e = fsm.GetValidState("Regain Control");
            e.RemoveFsmTransition("FINISHED");
            e.AddFsmTransition("FINISHED", blasterCircleStart.Name);

            //blasterCircleCleanup.AddFsmTransition("FINISHED", "Move Choice");


            // Now we need to insert this flow into the regular attacks flow.
            // For shriek wave, grimm won't be on screen, so no need to bring him in.

            //var moveChoice = fsm.GetValidState("Move Choice");
            //moveChoice.AddTransition("SHRIEK WAVE", shriekWaveStart.Name);

            //moveOptions.Add("SHRIEK WAVE");
        }

    }
}
