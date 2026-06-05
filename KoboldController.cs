using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KoboldController : UdonSharpBehaviour
{
    [Header("Kobold Controller | By Draggo Laurus")]
    [Space(-8)]
    [Header("An Interactable NPC For FatFur Paradise Park.")]

    [Header("Path Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 0.5f;
    public float rotateSpeed = 1.5f;
    public float waypointThreshold = 0.2f;

    [Header("Vertical Correction")]
    public float verticalSnapSpeed = 2f;
    public float verticalThreshold = 0.05f;

    [Header("Physics Settings")]
    public Rigidbody rb;
    public float returnDelay = 3f;

    [Header("Animator")]
    public Animator anim;
    public string stateParam = "State";

    // Animator state values
    private const int STATE_IDLE = 0;
    private const int STATE_WALK = 1;
    private const int STATE_HELD = 2;
    private const int STATE_DROPPED = 3;
    private const int STATE_RUNNING = 4;

    [Header("Random Idle Settings")]
    public float idleChancePerSecond = 0.05f;
    public float idleMinTime = 1f;
    public float idleMaxTime = 3f;

    private float idleTimer = 0f;
    private bool isRandomIdle = false;

    [Header("Teleport Safety Settings")]
    public float maxDistanceFromTarget = 15f;

    // --- Synced state ---
    [UdonSynced] private int syncedWaypointIndex;
    [UdonSynced] private int syncedMode;   // 0 = Path, 1 = Physics, 2 = Returning
    [UdonSynced] private int syncedState;  // Animator state

    private int localWaypointIndex;
    private int localMode;
    private int localState;

    private float dropTimer;

#if UNITY_EDITOR
    public int Editor_CurrentWaypointIndex => localWaypointIndex;
#endif

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        localWaypointIndex = syncedWaypointIndex;
        localMode = syncedMode;
        localState = syncedState;

        if (anim != null)
            anim.SetInteger(stateParam, localState);

        if (Networking.IsOwner(gameObject))
            EnterPathMode(false);
    }

    void Update()
    {
        if (!Networking.IsOwner(gameObject)) return;

        switch (localMode)
        {
            case 0: HandlePathMode(); break;
            case 1: HandlePhysicsMode(); break;
            case 2: HandleReturningMode(); break;
        }
    }

    // ---------------------------------------------------------
    // STATE HELPER
    // ---------------------------------------------------------

    private void SetState(int newState, bool serialize = true)
    {
        if (localState == newState)
            return;

        localState = newState;

        if (anim != null)
            anim.SetInteger(stateParam, localState);

        if (serialize && Networking.IsOwner(gameObject))
        {
            syncedState = localState;
            RequestSerialization();
        }
    }

    // ---------------------------------------------------------
    // MODE SWITCHING (NO ANIMATION CHANGES FOR NON-OWNERS)
    // ---------------------------------------------------------

    private void EnterPhysicsMode(bool serialize = true)
    {
        localMode = 1;
        syncedMode = 1;

        rb.isKinematic = false;
        rb.useGravity = true;

        if (serialize)
            SetState(STATE_HELD, true);

        if (serialize && Networking.IsOwner(gameObject))
            RequestSerialization();
    }

    private void EnterPathMode(bool serialize = true)
    {
        localMode = 0;
        syncedMode = 0;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (serialize)
        {
            if (moveSpeed > 1.5f)
                SetState(STATE_RUNNING, true);
            else
                SetState(STATE_WALK, true);
        }

        if (serialize && Networking.IsOwner(gameObject))
            RequestSerialization();
    }

    private void EnterReturningMode(bool serialize = true)
    {
        localMode = 2;
        syncedMode = 2;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (serialize)
        {
            if (moveSpeed > 1.5f)
                SetState(STATE_RUNNING, true);
            else
                SetState(STATE_WALK, true);
        }

        if (serialize && Networking.IsOwner(gameObject))
            RequestSerialization();
    }

    // ---------------------------------------------------------
    // FORCE DROP + TELEPORT
    // ---------------------------------------------------------

    private void ForceDropAndTeleport()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        Transform target = waypoints[localWaypointIndex];
        if (target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }

        EnterPathMode();
    }

    // ---------------------------------------------------------
    // PHYSICS MODE
    // ---------------------------------------------------------

    private void HandlePhysicsMode()
    {
        Transform target = waypoints[localWaypointIndex];
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist > maxDistanceFromTarget)
            {
                ForceDropAndTeleport();
                return;
            }
        }

        if (dropTimer > 0f)
        {
            dropTimer -= Time.deltaTime;
            if (dropTimer <= 0f)
                EnterReturningMode();
        }
    }

    // ---------------------------------------------------------
    // PATH MODE (with random idle + vertical correction)
    // ---------------------------------------------------------

    private void HandlePathMode()
    {
        // Random idle behavior
        if (isRandomIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isRandomIdle = false;
            }
            else
            {
                SetState(STATE_IDLE);
                return;
            }
        }

        if (Random.value < idleChancePerSecond * Time.deltaTime)
        {
            isRandomIdle = true;
            idleTimer = Random.Range(idleMinTime, idleMaxTime);
            SetState(STATE_IDLE);
            return;
        }

        Transform target = waypoints[localWaypointIndex];
        Vector3 toTarget = target.position - transform.position;

        // Horizontal movement
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);

        if (flat.magnitude < waypointThreshold)
        {
            localWaypointIndex++;
            if (localWaypointIndex >= waypoints.Length)
                localWaypointIndex = 0;

            syncedWaypointIndex = localWaypointIndex;
            if (Networking.IsOwner(gameObject))
                RequestSerialization();

            SetState(STATE_IDLE);
            return;
        }

        // Vertical correction
        float targetY = target.position.y;
        float currentY = transform.position.y;
        float yDiff = targetY - currentY;

        float verticalMove = 0f;
        if (Mathf.Abs(yDiff) > verticalThreshold)
            verticalMove = Mathf.Sign(yDiff) * verticalSnapSpeed * Time.deltaTime;

        // Apply movement
        Vector3 horizontalMove = flat.normalized * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(horizontalMove.x, verticalMove, horizontalMove.z);

        // Rotation
        if (flat.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        // Animation
        if (moveSpeed > 1.5f)
            SetState(STATE_RUNNING);
        else
            SetState(STATE_WALK);
    }

    // ---------------------------------------------------------
    // RETURNING MODE (with vertical correction)
    // ---------------------------------------------------------

    private void HandleReturningMode()
    {
        Transform target = waypoints[localWaypointIndex];
        Vector3 toTarget = target.position - transform.position;

        // Horizontal movement
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);

        if (flat.magnitude < waypointThreshold)
        {
            EnterPathMode();
            return;
        }

        // Vertical correction
        float targetY = target.position.y;
        float currentY = transform.position.y;
        float yDiff = targetY - currentY;

        float verticalMove = 0f;
        if (Mathf.Abs(yDiff) > verticalThreshold)
            verticalMove = Mathf.Sign(yDiff) * verticalSnapSpeed * Time.deltaTime;

        // Apply movement
        Vector3 horizontalMove = flat.normalized * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(horizontalMove.x, verticalMove, horizontalMove.z);

        // Rotation
        if (flat.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        // Animation
        if (moveSpeed > 1.5f)
            SetState(STATE_RUNNING);
        else
            SetState(STATE_WALK);
    }

    // ---------------------------------------------------------
    // PICKUP EVENTS
    // ---------------------------------------------------------

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        EnterPhysicsMode();
    }

    public override void OnDrop()
    {
        if (!Networking.IsOwner(gameObject)) return;

        dropTimer = returnDelay;
        SetState(STATE_DROPPED);
    }

    // ---------------------------------------------------------
    // NETWORK SYNC
    // ---------------------------------------------------------

    public override void OnDeserialization()
    {
        if (Networking.IsOwner(gameObject)) return;

        localMode = syncedMode;
        localWaypointIndex = syncedWaypointIndex;
        localState = syncedState;

        // Non-owner ONLY applies synced animation state
        if (anim != null)
            anim.SetInteger(stateParam, localState);
    }
}