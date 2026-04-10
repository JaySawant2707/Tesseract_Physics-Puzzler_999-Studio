using UnityEngine;
using System.Collections.Generic;

public class NarratorTrigger : MonoBehaviour
{
    [Header("Narration")]
    [Tooltip("Unique ID used to prevent this narration from replaying during the current game session.")]
    public string narrationID;

    [Tooltip("The ordered list of narration lines to play when the player enters this trigger.")]
    public List<NarratorLine> conversation;

    [Tooltip("Priority of this narration. Higher priority narration can interrupt lower priority narration.")]
    public int priority = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        NarratorManager.Instance.SpeakSequence(
            conversation,
            priority,
            narrationID
        );
    }
}