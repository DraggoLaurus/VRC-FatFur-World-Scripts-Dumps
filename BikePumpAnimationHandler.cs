using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BikePumpAnimationHandler : UdonSharpBehaviour
{
    [Header("Bike Pump Animation Handler")]
    [Header("By Draggo Laurus")]
    [Header("Updated 3/22/2025")]
    [Header("---------------------------------------")]
    
    [Header("Object References")]
    public SkinnedMeshRenderer skinnedMeshRenderer; // The mesh with the blendshapes
    public string[] blendshapeNames; // Names of the blendshapes to animate (in order)

    [Header("Flow Parameters")]
    public float pulseDuration = 0.05f; // Total time for one pulse effect
    public float pulseSpacingDelay = 0f; // Delay between consecutive pulses
    public AnimationCurve pulseCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f, 0.0f, 0.0f), 
        new Keyframe(0.125f, 0.5f, 2.6666667f, 2.6666667f), 
        new Keyframe(0.5f, 1.0f, 0.0f, 0.0f), 
        new Keyframe(0.875f, 0.5f, -2.9999857f, -2.9999857f), 
        new Keyframe(1.0f, 0.0f, 0.0f, 0.0f)
    ); // Curve defining the intensity of the pulse

    private int[] blendshapeIndices; // Resolved indices of the blendshapes
    private float[] segmentTimers; // Timers for each blendshape
    private bool[] segmentActive; // Tracks whether a pulse is active for each blendshape

    void Start()
    {
        if (skinnedMeshRenderer == null || blendshapeNames == null)
        {
            return; // Ensure all required references are assigned
        }

        Mesh mesh = skinnedMeshRenderer.sharedMesh;

        // Resolve blendshape names to indices
        blendshapeIndices = new int[blendshapeNames.Length];
        for (int i = 0; i < blendshapeNames.Length; i++)
        {
            int index = mesh.GetBlendShapeIndex(blendshapeNames[i]);
            if (index == -1)
            {
                return; // Exit if any blendshape name is not found
            }
            blendshapeIndices[i] = index;
        }

        // Initialize timers and active states for each blendshape
        segmentTimers = new float[blendshapeIndices.Length];
        segmentActive = new bool[blendshapeIndices.Length];
        for (int i = 0; i < blendshapeIndices.Length; i++)
        {
            segmentTimers[i] = -1f; // Timer inactive
            segmentActive[i] = false; // Pulse not active
        }
    }

    public override void Interact()
    {
        // Trigger the animation sequence when interacted with
        TriggerAnimation();
    }
    
    public void TriggerAnimation()
    {
        // Start the animation sequence
        if (blendshapeIndices.Length > 0)
        {
            segmentActive[0] = true;
            segmentTimers[0] = 0f; // Start the timer for the first blendshape
        }
    }

    void Update()
    {
        // Handle pulses for each blendshape
        for (int i = 0; i < blendshapeIndices.Length; i++)
        {
            if (segmentActive[i])
            {
                segmentTimers[i] += Time.deltaTime;

                // Calculate blendshape weight using the pulse curve
                float normalizedTime = Mathf.Clamp01(segmentTimers[i] / pulseDuration);
                float pulseValue = pulseCurve.Evaluate(normalizedTime) * 100f;

                skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndices[i], pulseValue);

                // End the pulse when the timer exceeds the pulse duration
                if (segmentTimers[i] >= pulseDuration)
                {
                    segmentTimers[i] = -1f; // Reset the timer
                    segmentActive[i] = false; // Deactivate the pulse

                    // Trigger the next pulse
                    if (i + 1 < blendshapeIndices.Length)
                    {
                        segmentActive[i + 1] = true;
                        segmentTimers[i + 1] = 0f;
                    }
                }
            }
        }
    }
}
