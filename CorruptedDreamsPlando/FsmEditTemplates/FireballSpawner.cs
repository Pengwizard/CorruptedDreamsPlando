using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using Satchel;
using Satchel.Futils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
    internal class DummySpinner : MonoBehaviour { }

    public class FireballSpawner
    {
        private static FireballSpawner _instance;

        private GameObject _fireballTopBase;
        private string _fireballTopBaseName = "CD_BaseVsTop";

        private GameObject _fireballBallBase;
        private string _fireballBallBaseName = "CD_BaseVSBall";

        internal static FireballSpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(FireballSpawner)} was never constructed");
                }
                return _instance;
            }
        }

        public static void Initialize()
        {
            _instance = new FireballSpawner();
        }

        public GameObject FireballBaseTop
        {
            get
            {
                if (_fireballTopBase == null)
                {
                    var globalFireball = GameObject.Find("_GameManager/GlobalPool/Fireball Top(Clone)");

                    _fireballTopBase = GameObject.Instantiate(globalFireball);
                    _fireballTopBase.name = _fireballTopBaseName;
                    GameObject.DontDestroyOnLoad(_fireballTopBase);
                }
                return _fireballTopBase;
            }
        }

        public GameObject FireballBaseBall
        {
            get
            {
                if (_fireballBallBase == null)
                {
                    var globalFireball = GameObject.Find("_GameManager/GlobalPool/Fireball(Clone)");

                    _fireballBallBase = GameObject.Instantiate(globalFireball);
                    _fireballBallBase.name = _fireballBallBaseName;
                    GameObject.DontDestroyOnLoad(_fireballBallBase);
                }
                return _fireballBallBase;
            }
        }

        public void SpawnFireballAtEndOfFSM(FsmState fsm, int damageToDeal)
        {
            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;
            for (int i = 0; i < 1; ++i)
            {
            Satchel.FsmUtil.AddCustomAction(fsm, () =>
            {
                var gm = fsm.Fsm.GameObject;

                var tempFireball = GameObject.Instantiate(FireballBaseTop);
                tempFireball.name = FireballBaseTop.name;
                tempFireball.transform.position = gm.transform.position;

                //var castFSM = tempFireball.LocateMyFSM("Fireball Cast");
                //var castLeft = castFSM.GetState("Cast Left");
                //castLeft.Actions[4].Enabled = false;

                //castLeft.InsertCustomAction(() =>
                //{
                //    var tempFireballBall = GameObject.Instantiate(FireballBaseBall);

                //    tempFireballBall.transform.parent = tempFireball.transform;
                //    tempFireballBall.transform.SetPositionAndRotation(gm.transform.position + new Vector3(-1.2f, -0.6f, -0.002f), Quaternion.identity);
                //    tempFireballBall.name = FireballBaseBall.name;
                //    CorruptedDreamsPlandoMod.Instance.Log(tempFireballBall);
                //    CorruptedDreamsPlandoMod.Instance.Log(tempFireballBall.transform.position);

                //    castFSM.GetVariable<FsmGameObject>("Fireball").Value = tempFireballBall;
                //}, 4);

                var castFSM = tempFireball.LocateMyFSM("Fireball Cast");

                Action modifyFireball = () =>
                {
                    var fb = castFSM.GetVariable<FsmGameObject>("Fireball").Value;
                    fb.name = _fireballBallBaseName;
                    var fbControl = fb.LocateMyFSM("Fireball Control");
                    PlayerDataBoolTest modifyForShamans = (PlayerDataBoolTest)fbControl.GetState("Set Damage").Actions[3];
                    modifyForShamans.isTrue = modifyForShamans.isFalse;
                    var wait = fbControl.GetState("Idle").GetFirstActionOfType<Wait>();
                    wait.time.Value = 100f;

                    fb.AddComponent<DummySpinner>().StartCoroutine(SpinFireball(fb, gm));
                };

                var castLeft = castFSM.GetState("Cast Left");
                castLeft.Actions[2].Enabled = false;
                castLeft.AddCustomAction(modifyFireball);

                var castRight = castFSM.GetState("Cast Right");
                castRight.Actions[5].Enabled = false;
                castRight.AddCustomAction(modifyFireball);

                tempFireball.SetActive(true);
            });

            }
        }

        private IEnumerator SpinFireball(GameObject fb, GameObject focalPoint)
        {
            var angle = 60f;

            while (fb.activeSelf)
            {
                var vel = GetSpiralVelocity(fb, focalPoint, 4f, -1f);

                //Vector3 direction = focalPoint - fb.transform.position;
                //direction = Quaternion.Euler(0, 0, angle) * direction;
                ////float distanceThisFrame = 4 * Time.deltaTime;

                ////float radians = Mathf.Deg2Rad * 60f;
                ////float xSpeed = Mathf.Cos(radians) * 4;
                ////float ySpeed = Mathf.Sin(radians) * 4;
                ////Vector3 shift = new Vector3(xSpeed, ySpeed, 0);

                ////fb.transform.Translate(direction.normalized * distanceThisFrame, Space.World);

                var rb2 = fb.GetComponent<Rigidbody2D>();
                rb2.velocity = vel;
                //rb2.SetRotation(Quaternion.Euler(0, 0, angle));
                //rb2.velocity = shift;
                //fb.transform.RotateAround(focalPoint, )
                yield return new WaitForSeconds(0.05f);
            }
        }


        public static Vector2 GetSpiralVelocity(
            GameObject objectA,
            GameObject objectB,
            float orbitSpeed,
            float inwardSpeed,
            bool clockwise = false)
        {
            Vector2 aPos = objectA.transform.position;
            Vector2 bPos = objectB.transform.position;

            Vector2 fromBToA = aPos - bPos;

            if (fromBToA.sqrMagnitude < 0.0001f)
                return Vector2.zero;

            Vector2 radialDir = fromBToA.normalized;

            Vector2 tangentDir = clockwise
                ? new Vector2(radialDir.y, -radialDir.x)
                : new Vector2(-radialDir.y, radialDir.x);

            Vector2 spiralVelocity =
                tangentDir * orbitSpeed -
                radialDir * inwardSpeed;

            return spiralVelocity;
        }
    }
}
