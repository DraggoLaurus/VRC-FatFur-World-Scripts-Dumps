using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BikePump : UdonSharpBehaviour
{
    [Header("Inflation / Bike Pump Script")]
    [Header("Script by Draggo Laurus")]
    [Header("Updated 3/22/2025")]
    [Header("---------------------------------------")]

    #region Variables
    [Header("Object References")]
    [Tooltip("The handle mesh to animate for visual feedback.")]
    public Transform pumpMesh; // Mesh to animate for visual feedback
    [Tooltip("UdonBehaviour to send the interact event to when pumped. This script could be for pumping a tire, or Player Inflation (using an Avatar Scaler System like JetDog's Avatar Scale Prefabs) to be triggered.")]
    public UdonBehaviour targetUdonBehaviour; // The external UdonBehavior to send the interact event to
    [Tooltip("The external UdonBehaviour for tube animation (if applicable).")]
    public UdonBehaviour tubeAnimationHandler; // The external UdonBehaviour for tube animation (if applicable)
    public AudioSource pumpSound; // AudioSource for the pump sound effect
    [Tooltip("Typically the pickup object itself, or another object that tracks the pickup's position.")]
    public Transform positionTracker; // Separate object used to track the pickup’s position

    [Header("Event Settings")]
    [Tooltip("The name of the event to trigger on the external UdonBehaviour when pumped.")]
    public string udonEventName = "_interact"; // The name of the event to trigger on the external UdonBehaviour

    [Header("Pump Settings")]
    public float interactThreshold = 0.7f; // Threshold for triggering the interact event (normalized)
    public float resetThreshold = 0.4f; // Height at which the interact can trigger again (normalized)

    [Header("Handle Pump Animation Settings")]
    public Vector3 minMeshPosition = new Vector3(0, 0, 0); // Local position of the mesh at the unpushed position
    public Vector3 maxMeshPosition = new Vector3(0, -0.3f, 0); // Local position of the mesh at the fully pushed position

    //[Header("Debugging Variables (Read-Only)")]
    //[Tooltip("Current normalized pump level (0 to 1) based on positionTracker")]
    private float pumpLevel; // Exposed for debugging the normalized pump level
    //[Tooltip("Current raw Y position of the pickup object")]
    private float pickupYPositionData; // Exposed for debugging the raw pickup Y position
    //[Tooltip("Tracks if sound has been played")]
    private bool hasPlayedSound; // Exposed for debugging sound state
    //[Tooltip("Tracks if interaction can trigger")]
    private bool canTriggerInteract; // Exposed for debugging interaction state
    private bool playerInTrigger = false; // Tracks if the player is in the trigger

    private float minHeight; // Assigned from minMeshPosition.y
    private float maxHeight; // Assigned from maxMeshPosition.y
    private float previousPumpLevel = 0f; // Tracks the pump level from the previous frame
    #endregion
    #region Code
    void Start()
    {
        // Assign min and max mesh positions for height calculations
        minHeight = minMeshPosition.y;
        maxHeight = maxMeshPosition.y;

        // Initialize the pump mesh at the unpushed position
        pumpMesh.localPosition = minMeshPosition;
    }
    
    void Update()
    {
        if (pumpMesh == null || pumpSound == null || positionTracker == null)
        {
            Debug.LogError("(BikePump) Missing required references for script. You could run into issues.");
            return;
        }

        // Get the local Y position of the position tracker
        pickupYPositionData = positionTracker.localPosition.y;

        // Properly normalize the pump level (clamped between 0 and 1)
        pumpLevel = Mathf.Clamp01((pickupYPositionData - minHeight) / (maxHeight - minHeight));

        // Update the pump mesh's position linearly based on the normalized pump level
        pumpMesh.localPosition = minMeshPosition + pumpLevel * (maxMeshPosition - minMeshPosition);

        // Trigger sound logic only when crossing the interactThreshold (using Pump Level)
        if (pumpLevel >= interactThreshold && previousPumpLevel < interactThreshold)
        {
            // Play sound when crossing into the threshold
            if (pumpSound != null)
            {
                pumpSound.Play();
            }
            hasPlayedSound = true;
        }
        else if (pumpLevel < resetThreshold && previousPumpLevel >= resetThreshold)
        {
            // Reset sound playback when leaving the resetThreshold
            hasPlayedSound = false;
        }

        // Trigger the external event
        if (canTriggerInteract && pumpLevel >= interactThreshold)
        {
            // Trigger the interact event on the external UdonBehaviour
            if (targetUdonBehaviour != null && playerInTrigger)
            {
                targetUdonBehaviour.SendCustomEvent(udonEventName);
            }
            if (tubeAnimationHandler != null)
            {
                tubeAnimationHandler.SendCustomEvent("_interact");
            }

            // Prevent further triggering until reset
            canTriggerInteract = false;
        }
        else if (!canTriggerInteract && pumpLevel < resetThreshold)
        {
            // Allow interaction event to trigger again
            canTriggerInteract = true;
        }

        // Update previous pump level for the next frame
        previousPumpLevel = pumpLevel;
    }

    public void PlayerTriggerEnter()
    {
        playerInTrigger = true;
    }

    public void PlayerTriggerExit()
    {
        playerInTrigger = false;
    }

    public override void OnDrop()
    {
        // Reset the pickup's position and rotation to the default local position (0,0,0) and local rotation (0,0,0)
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Reset the pump mesh to the unpushed position
        pumpMesh.localPosition = minMeshPosition;

        // Ensure sound resets when dropped
        hasPlayedSound = false;
    }
}
#endregion