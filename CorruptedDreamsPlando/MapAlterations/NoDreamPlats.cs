using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using SFCore;
using System;
using UnityEngine;
using System;
using System.Collections;

namespace CorruptedDreamsPlando.MapAlterations
{
    public class NoDreamPlats
    {
        public static void Initialize()
        {
            On.DreamPlatform.OnEnable += DreamPlatform_OnEnable;
        }

        private static void DreamPlatform_OnEnable(On.DreamPlatform.orig_OnEnable orig, DreamPlatform self)
        {
            orig(self);
            self.gameObject.SetActive(false);
        }

        public static void Unload()
        {
            On.DreamPlatform.OnEnable -= DreamPlatform_OnEnable;
        }

    }
}
