using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FadeInAudioSource : UdonSharpBehaviour
{
    [Header("Fade In Audio Source")]
    [Header("Script By Draggo Laurus")]
    [Header("Last Updated: 3/20/2025")]
    [Header("-----------------------------------")]
    
    [Header("Object References")]
    public AudioSource audioSource; // The audio source to control

    [Header("Audio Source Settings")]
    public AnimationCurve fadeCurve; // Curve defining the fade-in effect
    public float maxVolume = 1.0f; // Max volume of the audio source
    public float fadeDuration = 2.0f; // Duration of the fade-in effect

    private bool wasLoopingOnStart; // Tracks if looping was enabled initially
    private bool isFadingIn = false; // Tracks if fading in is active
    private float fadeTimer = 0.0f; // Tracks the elapsed time for fading

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("You done mess up partner. AudioSource is not assigned!");
            return;
        }

        // Cache the initial looping state of the audio source
        wasLoopingOnStart = audioSource.loop;

        // Ensure the audio starts at volume 0
        audioSource.volume = 0.0f;
        audioSource.loop = false; // Loop will be enabled/disabled based on player interaction
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return; // Only act on the local player

        // Start the fade-in process
        isFadingIn = true;
        fadeTimer = 0.0f;

        // Play the audio if it's not already playing
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        // Enable looping if it was originally enabled
        if (wasLoopingOnStart)
        {
            audioSource.loop = true;
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.isLocal) return; // Only act on the local player

        // Stop the audio and disable looping
        audioSource.Stop();
        audioSource.loop = false;

        // Reset fading state
        isFadingIn = false;
        fadeTimer = 0.0f;
    }

    private void Update()
    {
        if (isFadingIn)
        {
            FadeIn();
        }
    }

    private void FadeIn()
    {
        // Increment the timer
        fadeTimer += Time.deltaTime;

        // Calculate the volume based on the fade curve
        float normalizedTime = Mathf.Clamp01(fadeTimer / fadeDuration);
        audioSource.volume = fadeCurve.Evaluate(normalizedTime) * maxVolume;

        // If the fade-in is complete, stop the fading process
        if (fadeTimer >= fadeDuration)
        {
            isFadingIn = false;
        }
    }
}
