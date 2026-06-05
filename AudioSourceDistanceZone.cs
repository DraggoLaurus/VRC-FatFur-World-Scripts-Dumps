
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AudioSourceDistanceZone : UdonSharpBehaviour
{
    [Tooltip("AudioSource to modify. If empty, the AudioSource on this GameObject will be used.")]
    public AudioSource targetAudio;

    [Tooltip("Max distance to apply when a local player enters the trigger.")]
    public float enterMaxDistance = 2f;

    [Tooltip("Max distance to apply when a local player leaves the trigger.")]
    public float leaveMaxDistance = 50f;

    void Start()
    {
        if (targetAudio == null)
        {
            targetAudio = GetComponent<AudioSource>();
        }
    }

    // Only adjust audio for the local player
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal) return;
        if (targetAudio == null) return;

        targetAudio.maxDistance = enterMaxDistance;
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal) return;
        if (targetAudio == null) return;

        targetAudio.maxDistance = leaveMaxDistance;
    }
}
