using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance;

    [Header("Audio")]
    [Tooltip("The audio source used to play narration clips.")]
    public AudioSource audioSource;

    [Header("Subtitles (Optional)")]
    [Tooltip("Optional TextMeshProUGUI object used to display narration subtitles on screen.")]
    public TextMeshProUGUI subtitleText;

    private Coroutine narrationRoutine;
    private int currentPriority;

    // Session-only memory of narration IDs that have already played.
    private HashSet<string> playedNarrations = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasPlayed(string narrationID)
    {
        return !string.IsNullOrEmpty(narrationID) &&
               playedNarrations.Contains(narrationID);
    }

    public void SpeakSequence(
        List<NarratorLine> lines,
        int priority,
        string narrationID
    )
    {
        if (lines == null || lines.Count == 0)
            return;

        // Do not replay narrations with the same session ID.
        if (HasPlayed(narrationID))
            return;

        // Do not interrupt a higher-priority narration.
        if (narrationRoutine != null && priority < currentPriority)
            return;

        if (narrationRoutine != null)
            StopCoroutine(narrationRoutine);

        narrationRoutine = StartCoroutine(
            PlaySequence(lines, priority, narrationID)
        );
    }

    IEnumerator PlaySequence(
        List<NarratorLine> lines,
        int priority,
        string narrationID
    )
    {
        currentPriority = priority;

        foreach (var line in lines)
        {
            if (line.clip == null)
                continue;

            audioSource.Stop();
            audioSource.clip = line.clip;
            audioSource.Play();

            if (subtitleText != null)
                subtitleText.text = line.subtitle;

            // Wait for this clip plus any extra delay before the next line.
            yield return new WaitForSeconds(line.clip.length + line.delayAfter);
        }

        if (subtitleText != null)
            subtitleText.text = "";

        // Record that this narration has played so it won't replay this session.
        if (!string.IsNullOrEmpty(narrationID))
            playedNarrations.Add(narrationID);

        currentPriority = 0;
        narrationRoutine = null;
    }
}