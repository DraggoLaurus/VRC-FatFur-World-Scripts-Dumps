using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SmoothVolumeControl : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public float transitionTime = 1.0f; // Time in seconds to reach full volume or mute
    public float maxVolume = 1.0f; // Maximum volume level
    private float targetVolume = 0.0f;
    private float currentVolume = 0.0f;
    private bool isInside = false;

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("Hey! You done messed up pal! No AudioSource Component Defined in a script.");
            return;
        }
        currentVolume = audioSource.volume;
    }

    private void Update()
    {
        if (audioSource != null)
        {
            currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime / transitionTime);
            currentVolume = Mathf.Min(currentVolume, maxVolume); // Cap the volume
            audioSource.volume = currentVolume;

            if (currentVolume < 0.15f * maxVolume && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            else if (currentVolume >= 0.15f * maxVolume && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isInside = true;
            targetVolume = maxVolume; // Set to max volume
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isInside = false;
            targetVolume = 0.0f; // Mute
        }
    }

    public void ResetCurrentVolume()
    {
        currentVolume = 0.05f;
    }
}
