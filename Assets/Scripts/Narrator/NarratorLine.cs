using UnityEngine;

[System.Serializable]
public class NarratorLine
{
    [Tooltip("Audio clip that will play for this narration line.")]
    public AudioClip clip;

    [TextArea]
    [Tooltip("Subtitle text shown while the audio clip is playing.")]
    public string subtitle;

    [Tooltip("Additional pause after this line finishes before the next line starts.")]
    public float delayAfter; // small pause before next line
}