using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimEventsExtension
{
    public static void AddAnimationEvent(this AnimationClip clip, float time, string functionName)
    {
        float clipDuration = clip.length;

        if (time < 0f)
        {
            Debug.LogError("Event time must be greater >= than 0.0f second");
            return;
        }
        else if (time > clipDuration)
        {
            Debug.LogError($"Event time must be less <= than clip's duration {clipDuration} seconds");
            return;
        }

        AnimationEvent animationEvent = new AnimationEvent
        {
            time = time,
            functionName = functionName
        };

        clip.AddEvent(animationEvent);
    }
}
