using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventsHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent<string> _onAnimationEvent;

    public void AnimationEventHandler(string eventName)
    {
        // Debug.Log($"Event triggered: {eventName}");

        _onAnimationEvent?.Invoke(eventName);
    }
}
