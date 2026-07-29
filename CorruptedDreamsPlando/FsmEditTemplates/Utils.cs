using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace CorruptedDreamsPlando.FsmEditTemplates
{
    public static class Utils
    {

        public static AnimationAccelerator GetOrAddAccel(GameObject obj)
        {
            AnimationAccelerator accel = obj.GetComponent<AnimationAccelerator>();
            if (accel == null)
            {
                accel = obj.AddComponent<AnimationAccelerator>();
            }

            return accel;
        }

        public static void SpeedupAnimation(PlayMakerFSM fsm, string animationName, float animationSpeed, int index = -1)
        {
            try
            {
                var accel = GetOrAddAccel(fsm.gameObject);
                CorruptedDreamsPlandoMod.Instance.Log($"Changing {animationName}");
                SFCore.Utils.FsmUtil.GetState(fsm, animationName).AccelerateAnimation(accel, animationSpeed, index);
            }
            catch (Exception e)
            {
                CorruptedDreamsPlandoMod.Instance.Log("Error");
            }
        }
        public static void ChangeSetFloatValue(PlayMakerFSM fsm, string stateName, int actionIndex, float newValue)
        {
            var setState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setAction = setState.Actions[actionIndex];
            if (setAction is SetFloatValue)
            {
                (setAction as SetFloatValue).floatValue = newValue;
            }
        }

        public static void ChangeSetIntValue(PlayMakerFSM fsm, string stateName, int actionIndex, int newValue)
        {
            var setState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setAction = setState.Actions[actionIndex];
            if (setAction is SetIntValue)
            {
                (setAction as SetIntValue).intValue = newValue;
            }
        }

        public static void ChangeWaitFloat(PlayMakerFSM fsm, string stateName, int actionIndex, float newWaitTime)
        {
            var waitState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var waitAction = waitState.Actions[actionIndex];
            if (waitAction is Wait)
            {
                (waitAction as Wait).time = newWaitTime;
            }
        }
        public static void ChangeSetVelocity2d(PlayMakerFSM fsm, string stateName, int actionIndex, float x, float y)
        {
            var setVeloState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setVeloAction = setVeloState.Actions[actionIndex];
            if (setVeloAction is SetVelocity2d)
            {
                (setVeloAction as SetVelocity2d).x = x;
                (setVeloAction as SetVelocity2d).y = y;
            }
        }

        public static void MultiplySetVelocity2d(PlayMakerFSM fsm, string stateName, int actionIndex, float mod)
        {
            var setVeloState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var setVeloAction = setVeloState.Actions[actionIndex];
            if (setVeloAction is SetVelocity2d)
            {
                (setVeloAction as SetVelocity2d).x.Value *= mod;
                (setVeloAction as SetVelocity2d).y.Value *= mod;
            }
        }

        public static void ChangeRandomFloat(PlayMakerFSM fsm, string stateName, int actionIndex, float upperBound, float lowerBound)
        {
            var setCounterState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var rFloat = setCounterState.Actions[actionIndex];
            if (rFloat is RandomFloat)
            {
                (rFloat as RandomFloat).min = upperBound;
                (rFloat as RandomFloat).max = lowerBound;
            }
        }

        public static void ChangeWaitRandom(PlayMakerFSM fsm, string stateName, int actionIndex, float upperBound, float lowerBound)
        {
            var setCounterState = SFCore.Utils.FsmUtil.GetState(fsm, stateName);
            var rFloat = setCounterState.Actions[actionIndex];
            if (rFloat is WaitRandom)
            {
                (rFloat as WaitRandom).timeMin = upperBound;
                (rFloat as WaitRandom).timeMax = lowerBound;
            }
        }
    }
}
