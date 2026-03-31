using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using UnityEngine;

namespace CorruptedDreamsPlando;

internal class AnimationAccelerator : MonoBehaviour
{
    private float accel = 1;
    private string? clip;
    private tk2dSpriteAnimator? animator;

    private void Awake() => animator = GetComponent<tk2dSpriteAnimator>();

    internal void AccelerateClip(string name, float a)
    {
        clip = name;
        accel = a;
    }

    private void Update()
    {
        if (animator == null) return;

        if (animator.CurrentClip.name != clip)
        {
            clip = null;
            accel = 1;
        }
        else if (accel > 1) animator.UpdateAnimation(Time.deltaTime * (accel - 1));
    }
}

internal static class AnimationAcceleratorExtensions
{
    internal static void AccelerateAnimation(this FsmState self, AnimationAccelerator accelerator, float accel, int index=-1)
    {
        string clip = "";
        if (index >= 0)
        {
            var action = self.Actions[index];
            if (action is Tk2dPlayAnimationWithEvents)
            {
                clip = (action as Tk2dPlayAnimationWithEvents).clipName.Value;
            }

            if (action is Tk2dPlayAnimation)
            {
                clip = (action as Tk2dPlayAnimation).clipName.Value;
            }
        } else
        {
            clip = self.GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()?.clipName.Value ?? self.GetFirstActionOfType<Tk2dPlayAnimation>().clipName.Value;
        }

        self.AddLastAction(new Lambda(() => accelerator.AccelerateClip(clip, accel)));
    }
}