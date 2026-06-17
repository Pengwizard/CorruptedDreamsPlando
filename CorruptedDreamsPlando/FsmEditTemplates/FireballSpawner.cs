using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using Satchel;
using Satchel.Futils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
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
                    PlayerDataBoolTest modifyForShamans = (PlayerDataBoolTest)fb.LocateMyFSM("Fireball Control").GetState("Set Damage").Actions[3];
                    modifyForShamans.isTrue = modifyForShamans.isFalse;
                };

                var castLeft = castFSM.GetState("Cast Left");
                castLeft.Actions[2].Enabled = false;
                castLeft.AddCustomAction(modifyFireball);

                var castRight = castFSM.GetState("Cast Right");
                castRight.Actions[5].Enabled = false;
                castLeft.AddCustomAction(modifyFireball);

                tempFireball.SetActive(true);
            });

            }
        }
    }
}
