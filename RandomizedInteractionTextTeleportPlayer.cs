using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RandomizedInteractionTextTeleportPlayer : UdonSharpBehaviour
{
    [Header("Teleport Target")]
    public Transform teleportTarget; // The target Transform to teleport to

    [Header("Interaction Texts")]
    public string[] interactionTexts; // Array of possible interaction strings

    private void Start()
    {
        // Set a random interaction text at game start
        SetRandomInteractionText();
    }

    public override void Interact()
    {
        // Teleport the player
        Teleport();

        // Pick a new random interaction text after each interaction
        SetRandomInteractionText();
    }

    private void SetRandomInteractionText()
    {
        if (interactionTexts != null && interactionTexts.Length > 0)
        {
            int randomIndex = Random.Range(0, interactionTexts.Length);
            InteractionText = interactionTexts[randomIndex]; // ✅ correct property
        }
    }

    private void Teleport()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        // Teleport the local player to the target position
        localPlayer.TeleportTo(teleportTarget.position, teleportTarget.rotation);
    }
}