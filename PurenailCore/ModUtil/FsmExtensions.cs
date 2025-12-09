using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using System;
using System.Reflection;
using UnityEngine;

namespace PurenailCore.ModUtil;

internal class DelayedLambda(float delay, Action action) : FsmStateAction
{
    private float remaining;

    public override void OnEnter()
    {
        base.OnEnter();
        remaining = delay;
        if (remaining <= 0) action();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (remaining <= 0) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0) action();
    }
}
internal class AnimationAccelerator : MonoBehaviour
{
    private float accel = 1;
    private string? clipName;
    private tk2dSpriteAnimator? animator;

    private void Awake() => animator = gameObject.GetComponent<tk2dSpriteAnimator>();

    internal void AccelerateClip(string clipName, float accel)
    {
        this.clipName = clipName;
        this.accel = accel;
    }

    private void Update()
    {
        if (animator == null) return;

        if (animator.CurrentClip.name != clipName)
        {
            clipName = null;
            accel = 1;
        }
        else if (accel > 1) animator.UpdateAnimation(Time.deltaTime * (accel - 1));
    }
}

internal class AccelerateAnimationAction : FsmStateAction
{
    internal AnimationAccelerator? accelerator;
    internal string clip = "";
    internal float accel = 1;

    public override void OnEnter() => accelerator?.AccelerateClip(clip, accel);
}

internal static class FsmExtensions
{
    public static void AccelerateAnimation(this FsmState self, AnimationAccelerator accelerator, float accel)
    {
        var clip = (self.GetFirstActionOfType<Tk2dPlayAnimationWithEvents>()?.clipName ?? self.GetFirstActionOfType<Tk2dPlayAnimation>()?.clipName)?.Value;
        if (clip == null) return;

        var action = self.GetFirstActionOfType<AccelerateAnimationAction>();
        if (action == null)
        {
            action = new();
            self.AddLastAction(action);
        }

        action.accelerator = accelerator;
        action.clip = clip;
        action.accel = accel;
    }

    private static MethodInfo doPlayRandomClipSingle = typeof(AudioPlayerOneShotSingle).GetMethod("DoPlayRandomClip", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void DoPlayRandomClipImpl(this AudioPlayerOneShotSingle self, float? volume = null)
    {
        if (volume.HasValue)
        {
            var prev = self.volume;
            self.volume = volume.Value;
            doPlayRandomClipSingle.Invoke(self, []);
            self.volume = prev;
        }
        else doPlayRandomClipSingle.Invoke(self, []);
    }

    public static void DoPlayRandomClip(this AudioPlayerOneShotSingle self, float? volume = null)
    {
        if (self.delay.Value <= 0) self.DoPlayRandomClipImpl(volume);
        else self.Fsm.FsmComponent.gameObject.DoAfter(() => self.DoPlayRandomClipImpl(volume), self.delay.Value);
    }

    private static MethodInfo doPlayRandomClip = typeof(AudioPlayerOneShot).GetMethod("DoPlayRandomClip", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void DoPlayRandomClipImpl(this AudioPlayerOneShot self, float? volume = null)
    {
        if (volume.HasValue)
        {
            var prev = self.volume;
            self.volume = volume.Value;
            doPlayRandomClip.Invoke(self, []);
            self.volume = prev;
        }
        else doPlayRandomClip.Invoke(self, []);
    }

    public static void DoPlayRandomClip(this AudioPlayerOneShot self, float? volume = null)
    {
        if (self.delay.Value <= 0) self.DoPlayRandomClipImpl(volume);
        else self.Fsm.FsmComponent.gameObject.DoAfter(() => self.DoPlayRandomClipImpl(volume), self.delay.Value);
    }

    public static void PlayOneShot(this AudioPlaySimple self)
    {
        var target = self.Fsm.GetOwnerDefaultTarget(self.gameObject);
        if (target == null) return;

        var audio = target.GetComponent<AudioSource>();
        if (audio == null) return;

        var clip = self.oneShotClip.Value as AudioClip;
        if (clip == null) return;

        if (self.volume.IsNone) audio.PlayOneShot(clip);
        else audio.PlayOneShot(clip, self.volume.Value);
    }
}
