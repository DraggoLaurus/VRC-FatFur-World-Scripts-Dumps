
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RandomPitchOnEnable : UdonSharpBehaviour
{
    [Header("Random Pitch On Enable")]
    [Tooltip("If left empty, the AudioSource on the same GameObject will be used.")]
    public AudioSource targetAudio;

    [Tooltip("Minimum pitch (inclusive).")] 
    [Range(-3f, 3f)]
    public float minPitch = 0.9f;

    [Tooltip("Maximum pitch (inclusive).")]
    [Range(-3f, 3f)]
    public float maxPitch = 1.1f;

    [Tooltip("If true, apply random pitch when the object becomes enabled.")]
    public bool applyOnEnable = true;

    void Start()
    {
        // Ensure sensible defaults
        if (minPitch > maxPitch)
        {
            float t = minPitch;
            minPitch = maxPitch;
            maxPitch = t;
        }

        if (targetAudio == null)
        {
            targetAudio = GetComponent<AudioSource>();
        }
    }

    public void OnEnable()
    {
        if (!applyOnEnable) return;
        ApplyRandomPitch();
    }

    // Public helper so this can be called from other scripts/events
    public void ApplyRandomPitch()
    {
        if (targetAudio == null) return;
        float pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        targetAudio.pitch = pitch;
    }
}
