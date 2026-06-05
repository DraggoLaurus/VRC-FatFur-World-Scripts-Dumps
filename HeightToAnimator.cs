using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class HeightToAnimator : UdonSharpBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public string heightParameter = "HeightValue";

    [Header("Height Range (Meters)")]
    public float minAvatarHeight = 1.2f;
    public float maxAvatarHeight = 2.2f;

    [Header("Manual Override (Slider)")]
    public Slider manualSlider;

    [Header("Behavior")]
    public bool autoUpdate = true;

    [Header("Owner Display")]
    public TextMeshPro ownerNameText;   // World-space TMP text

    [UdonSynced(UdonSyncMode.None)]
    private float syncedValue = 0f;

    [UdonSynced(UdonSyncMode.None)]
    private string syncedOwnerName = "";

    private VRCPlayerApi localPlayer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;

        if (ownerNameText != null)
            ownerNameText.text = syncedOwnerName;
    }

    void Update()
    {
        if (animator == null || localPlayer == null)
            return;

        if (Networking.IsOwner(localPlayer, gameObject))
        {
            float outputValue;

            if (autoUpdate)
            {
                float eyeHeight = localPlayer.GetAvatarEyeHeightAsMeters();
                float normalized = Mathf.InverseLerp(minAvatarHeight, maxAvatarHeight, eyeHeight);
                outputValue = Mathf.Clamp01(normalized);
            }
            else
            {
                if (manualSlider != null)
                    outputValue = Mathf.Clamp01(manualSlider.value);
                else
                    outputValue = 0f;
            }

            if (Mathf.Abs(outputValue - syncedValue) > 0.0001f)
            {
                syncedValue = outputValue;
                RequestSerialization();
            }

            animator.SetFloat(heightParameter, outputValue);
        }
        else
        {
            animator.SetFloat(heightParameter, syncedValue);
        }
    }

    public override void OnDeserialization()
    {
        if (animator != null)
            animator.SetFloat(heightParameter, syncedValue);

        if (ownerNameText != null)
            ownerNameText.text = syncedOwnerName;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        if (!Networking.IsOwner(localPlayer, gameObject))
        {
            Networking.SetOwner(localPlayer, gameObject);

            // Set synced owner name
            syncedOwnerName = localPlayer.displayName;
            RequestSerialization();

            if (ownerNameText != null)
                ownerNameText.text = syncedOwnerName;
        }
    }

    public void EnableAutoUpdate()
    {
        autoUpdate = true;
    }

    public void DisableAutoUpdate()
    {
        autoUpdate = false;
    }

    public void AutoUpdateToggle()
    {
        autoUpdate = !autoUpdate;
    }
}