using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using Satchel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace CorruptedDreamsPlando.FsmEditTemplates
{

    internal class DDarkComponent : MonoBehaviour
    {}

    public class DescendingDarkSpawner
    {
        private GameObject _ddarkSlam;
        private GameObject _ddarkPillar;
        private GameObject _ddarkMega;
        private static FsmObject _ddarkAudio = null;

        private static DescendingDarkSpawner _instance;

        internal static DescendingDarkSpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(DescendingDarkSpawner)} was never constructed");
                }
                return _instance;
            }
        }

        public static void Initialize()
        {
            _instance = new DescendingDarkSpawner();
        }

        public static FsmObject DDarkAudio
        {
            get
            {
                GetDDarkShriekSfx();
                return _ddarkAudio;
            }
        }

        private static void GetDDarkShriekSfx()
        {
            if (_ddarkAudio == null)
            {
                var knight = GameObject.Find("Knight");
                PlayMakerFSM fsm = knight.LocateMyFSM("Spell Control");
                var state = fsm.GetState("Q2 Land");
                _ddarkAudio = state.GetFirstActionOfType<AudioPlay>().oneShotClip;
                CorruptedDreamsPlandoMod.Instance.Log(_ddarkAudio);
            }
        }

        public GameObject DDarkSlam
        {
            get
            {
                if (_ddarkSlam == null)
                {
                    _ddarkSlam = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Q Slam 2").gameObject);
                    GameObject.DontDestroyOnLoad(_ddarkSlam);
                }
                return _ddarkSlam;
            }
        }

        public GameObject DDarkPillar
        {
            get
            {
                if (_ddarkPillar == null)
                {
                    _ddarkPillar = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Q Pillar").gameObject);
                    GameObject.DontDestroyOnLoad(_ddarkPillar);
                }
                return _ddarkPillar;
            }
        }

        public GameObject DDarkMega
        {
            get
            {
                if (_ddarkMega == null)
                {
                    _ddarkMega = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Q Mega").gameObject);
                    GameObject.DontDestroyOnLoad(_ddarkMega);
                }
                return _ddarkMega;
            }
        }

        private static void MakeInvuln(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Enemy Attack");
        }

        private static void MakeVuln(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Enemies");
        }

        public GameObject MakeDiveContainer()
        {
            // This makes a game object with no components or children, except an FSM that fires on activation.
            var container = GameObject.Instantiate(Preloader.Instance.BenchLight);
            container.name = "DDark Container";
            container.RemoveComponent<SpriteRenderer>();
            var lightPulse = container.LocateMyFSM("Light Pulse");
            var st1 = lightPulse.GetValidState("State 1");
            var scaler = st1.GetFirstActionOfType<iTweenScaleBy>();
            scaler.Enabled = false;

            return container;
        }

        public GameObject GetDDarkSpawner(
            int damageToDeal,
            GameObject followTarget = null,
            float groundY = 0,
            float telegraphWait = 0)
        {
            GameObject diveContainer = MakeDiveContainer();

            GameObject slam = GameObject.Instantiate(DDarkSlam);
            GameObject pillar = GameObject.Instantiate(DDarkPillar);
            GameObject mega = GameObject.Instantiate(DDarkMega);

            var fsm = diveContainer.LocateMyFSM("Light Pulse");
            var targetState = fsm.GetValidState("State 1");

            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;

            slam.transform.SetParent(diveContainer.transform);
            slam.transform.position = diveContainer.transform.position + new Vector3(0, 2, 0);
            pillar.transform.SetParent(diveContainer.transform);
            pillar.transform.position = diveContainer.transform.position;
            mega.transform.SetParent(diveContainer.transform);
            mega.transform.position = diveContainer.transform.position + new Vector3(0, 6, 0);


            GameObject[] gos = { slam, pillar, mega };


            foreach (var part in gos)
            {
                List<GameObject> children = new List<GameObject>();
                part.FindAllChildren(children);
                foreach (var c in children)
                {
                    c.layer = LayerMask.NameToLayer("Enemy Attack");
                    c.RemoveComponent<DamageEnemies>();
                    var damageHero = c.AddComponent<DamageHero>();
                    damageHero.damageDealt = damageToDeal;
                }
            }

            fsm.AddState("Activate Slam");
            fsm.AddState("Activate Pillar");
            fsm.AddState("Activate Mega");

            var doSlam = fsm.GetValidState("Activate Slam");

            var doPillar = fsm.GetValidState("Activate Pillar");
            var doMega = fsm.GetValidState("Activate Mega");

            if (followTarget != null)
            {
                targetState.AddCustomAction(() =>
                {
                    diveContainer.transform.SetPositionAndRotation(followTarget.transform.position, Quaternion.identity);
                    diveContainer.transform.SetPositionY(groundY);
                });
            }

            if (telegraphWait > 0)
            {
                targetState.AddAction(new Wait() { time = telegraphWait, finishEvent = new("FINISHED") });
            }

            targetState.AddTransition("FINISHED", doSlam.Name);

            //try
            //{
            //    GetDDarkShriekSfx();
            //    if (_ddarkAudio != null)
            //    {
            //        AudioPlay playDiveAudio = new AudioPlay();
            //        playDiveAudio.gameObject = Utils.GameObjectToOwnerDefault(diveContainer);
            //        // TODO: Audio needs to be determined by the game
            //        playDiveAudio.volume = 5f;
            //        playDiveAudio.oneShotClip = _ddarkAudio;

            //        doSlam.AddAction(playDiveAudio);
            //    }
            //}
            //catch (Exception e)
            //{
            //    CorruptedDreamsPlandoMod.Instance.LogError("Setting audio for ddark failed");
            //}

            doSlam.AddAction(ActivateAndResetGameObject(slam));
            //doSlam.AddAction(new Wait() { time = 0.1f, finishEvent = new("FINISHED") });
            doSlam.AddTransition("FINISHED", doPillar.Name);

            doPillar.AddAction(ActivateAndResetGameObject(pillar));
            //doPillar.AddAction(new Wait() { time = 0.1f, finishEvent = new("FINISHED") });
            doPillar.AddTransition("FINISHED", doMega.Name);

            doMega.AddAction(ActivateAndResetGameObject(mega));

            return diveContainer;
        }

        public ActivateGameObject ActivateAndResetGameObject(GameObject go)
        {
            ActivateGameObject action = new ActivateGameObject();
            action.gameObject = Utils.GameObjectToOwnerDefault(go);
            action.activate = true;
            action.recursive = false;
            action.resetOnExit = false;
            action.everyFrame = false;

            return action;
        }
            

        public GameObject AddDDarkAtEndOfFSM(
            FsmState fsm,
            int damageToDeal,
            Vector3 spawnOffset)
        {
            GameObject diveContanier = new GameObject();
            diveContanier.name = "Dive Container";

            GameObject slam = GameObject.Instantiate(DDarkSlam);
            GameObject pillar = GameObject.Instantiate(DDarkPillar);
            GameObject mega = GameObject.Instantiate(DDarkMega);

            try
            {
                GetDDarkShriekSfx();
                if (_ddarkAudio != null)
                {
                    AudioPlay playDiveAudio = new AudioPlay();
                    playDiveAudio.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() {
                        GameObject = new FsmGameObject { Value = fsm.Fsm.GameObject },
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject };
                    // TODO: Audio needs to be determined by the game
                    playDiveAudio.volume = 5f;
                    playDiveAudio.oneShotClip = _ddarkAudio;

                    Satchel.FsmUtil.AddAction(fsm, playDiveAudio);
                }
            }
            catch (Exception e)
            {
                CorruptedDreamsPlandoMod.Instance.LogError("Setting audio for ddark failed");
            }

            var go = fsm.Fsm.GameObject;
            var goTransform = go.transform;

            diveContanier.transform.SetParent(goTransform);
            diveContanier.transform.position = goTransform.position;

            slam.transform.SetParent(diveContanier.transform);
            slam.transform.position = diveContanier.transform.position + new Vector3(0, 2, 0) + spawnOffset;
            pillar.transform.SetParent(diveContanier.transform);
            pillar.transform.position = diveContanier.transform.position + spawnOffset;
            mega.transform.SetParent(diveContanier.transform);
            mega.transform.position = diveContanier.transform.position + new Vector3(0, 6, 0) + spawnOffset;

            GameObject[] gos = { slam, pillar, mega };


            foreach (var part in gos)
            {
                List<GameObject> children = new List<GameObject>();
                part.FindAllChildren(children);
                foreach (var c in children)
                {
                    c.layer = LayerMask.NameToLayer("Enemy Attack");
                    c.RemoveComponent<DamageEnemies>();
                    var damageHero = c.AddComponent<DamageHero>();
                    damageHero.damageDealt = damageToDeal;
                }
            }

            ActivateGameObject activateSlam = new ActivateGameObject();
            activateSlam.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = slam }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            activateSlam.activate = true;
            activateSlam.recursive = false;
            activateSlam.resetOnExit = false;
            activateSlam.everyFrame = false;

            ActivateGameObject activatePillar = new ActivateGameObject();
            activatePillar.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = pillar }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            activatePillar.activate = true;
            activatePillar.recursive = false;
            activatePillar.resetOnExit = false;
            activatePillar.everyFrame = false;

            ActivateGameObject activateMega = new ActivateGameObject();
            activateMega.gameObject = new HutongGames.PlayMaker.FsmOwnerDefault() { GameObject = new FsmGameObject { Value = mega }, OwnerOption = OwnerDefaultOption.SpecifyGameObject };
            activateMega.activate = true;
            activateMega.recursive = false;
            activateMega.resetOnExit = false;
            activateMega.everyFrame = false;

            Satchel.FsmUtil.AddFirstAction(fsm, new SetInvincible() { Invincible = true, InvincibleFromDirection = 0 });
            Satchel.FsmUtil.AddAction(fsm, activateSlam);
            Satchel.FsmUtil.AddAction(fsm, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(fsm, activatePillar);
            Satchel.FsmUtil.AddAction(fsm, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(fsm, activateMega);
            //Satchel.FsmUtil.AddAction(fsm, new Wait() { time = 0.75f });
            Satchel.FsmUtil.AddAction(fsm, new SetInvincible() { Invincible = false, InvincibleFromDirection = 0 });

            fsm.AddCustomAction(() =>
            {
                CorruptedDreamsPlandoMod.Instance.Log($"ddark done {slam.name} {pillar.name} {mega.name}");
            });

            return diveContanier;
        }

        public IEnumerator ActivateDDarkForObjectOnCondition(GameObject go, int damageToDeal, Vector3 spawnOffset, Func<GameObject, bool> condition)
        {
            // TODO Add audio


            // Setting up the DDark objects
            // TODO Extract to other method and share code with FSM
            GameObject slam = GameObject.Instantiate(DDarkSlam);
            GameObject pillar = GameObject.Instantiate(DDarkPillar);
            GameObject mega = GameObject.Instantiate(DDarkMega);

            var goTransform = go.transform;

            slam.transform.SetParent(goTransform);
            slam.transform.position = goTransform.position + new Vector3(0, 2, 0) + spawnOffset;
            pillar.transform.SetParent(goTransform);
            pillar.transform.position = goTransform.position + spawnOffset;
            mega.transform.SetParent(goTransform);
            mega.transform.position = goTransform.position + new Vector3(0, 6, 0) + spawnOffset;
            var hpManager = go.GetComponent<HealthManager>();

            GameObject[] gos = { slam, pillar, mega };


            foreach (var part in gos)
            {
                List<GameObject> children = new List<GameObject>();
                part.FindAllChildren(children);
                foreach (var c in children)
                {
                    c.layer = LayerMask.NameToLayer("Enemy Attack");
                    c.RemoveComponent<DamageEnemies>();
                    var damageHero = c.AddComponent<DamageHero>();
                    damageHero.damageDealt = damageToDeal;
                }
            }

            while (true)
            {
                if (condition(go))
                {
                    if (hpManager != null)
                    {
                        hpManager.IsInvincible = true;
                        hpManager.InvincibleFromDirection = 0;
                    }

                    slam.SetActive(true);
                    yield return new WaitForSeconds(0.2f);
                    pillar.SetActive(true);
                    yield return new WaitForSeconds(0.1f);
                    mega.SetActive(true);
                    yield return new WaitForSeconds(0.1f);

                    if (hpManager != null)
                    {
                        hpManager.IsInvincible = false;
                        hpManager.InvincibleFromDirection = 0;
                    }

                }

                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
