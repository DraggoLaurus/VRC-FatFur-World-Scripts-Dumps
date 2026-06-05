using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WeightScale : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Weight Scale by Draggo Laurus")]
    [Space(-8)]
    [Header("Last Updated: 11/17/2025 | Version 1.0")]
    
    [Header("Display")]
    [Tooltip("TextMeshPro component that will show the synced weight and tier feedback.")]
    public TextMeshPro display;

    [Header("Mass Settings")]
    [Tooltip("Reference height in meters used as a baseline for scaling.")]
    public float referenceHeight = 1.71f;

    [Tooltip("Reference mass in pounds corresponding to the reference height.")]
    public float referenceMass = 140f;

    [Header("Tier Thresholds")]
    [Tooltip("Mass above this is considered Overweight.")]
    public float overweightThreshold = 300f;

    [Tooltip("Mass above this is considered Heavyweight.")]
    public float heavyweightThreshold = 500f;

    [Tooltip("Mass above this is considered Gigantic.")]
    public float giganticThreshold = 800f;

    [Tooltip("Mass above this is considered an ERROR.")]
    public float errorThreshold = 2000f;

    [Header("Audio Feedback")]
    [Tooltip("AudioSource to play when a player steps on the scale.")]
    public AudioSource stepOnSound;

    [Tooltip("AudioSource to play when a player steps off the scale.")]
    public AudioSource stepOffSound;

    [UdonSynced(UdonSyncMode.None)]
    private float syncedMass = 0f;

    private VRCPlayerApi localPlayer;
    private bool isPlayerOnScale = false;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        UpdateDisplay();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        if (!Networking.IsOwner(localPlayer, gameObject))
            Networking.SetOwner(localPlayer, gameObject);

        isPlayerOnScale = true;
        RecalculateMass();

        if (stepOnSound != null) stepOnSound.Play();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        if (!Networking.IsOwner(localPlayer, gameObject))
            Networking.SetOwner(localPlayer, gameObject);

        isPlayerOnScale = false;
        syncedMass = 0f;
        RequestSerialization();
        UpdateDisplay();

        if (stepOffSound != null) stepOffSound.Play();
    }

    public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float previousEyeHeightAsMeters)
    {
        if (!player.isLocal || !isPlayerOnScale) return;

        RecalculateMass();
    }

    public override void OnDeserialization()
    {
        UpdateDisplay();
    }

    private void RecalculateMass()
    {
        float eyeHeight = localPlayer.GetAvatarEyeHeightAsMeters();
        float scaleFactor = eyeHeight / referenceHeight;
        syncedMass = referenceMass * Mathf.Pow(scaleFactor, 3);

        RequestSerialization();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (display == null) return;

        if (syncedMass <= 0f)
        {
            display.text = $"0.0 lbs";
            return;
        }

        string tierText = "";
        if (syncedMass > errorThreshold)
        {
            tierText = "<b><color=red><size=75%>ERROR</size></color></b>";
        }
        else if (syncedMass > giganticThreshold)
        {
            tierText = "<color=#800000><size=75%>Gigantic</size></color>";
        }
        else if (syncedMass > heavyweightThreshold)
        {
            tierText = "<color=#FF0000><size=75%>Heavyweight</size></color>";
        }
        else if (syncedMass > overweightThreshold)
        {
            tierText = "<color=#FFA500><size=75%>Overweight</size></color>";
        }

        display.text = $"{syncedMass:F1} lbs\n{tierText}";
    }
}