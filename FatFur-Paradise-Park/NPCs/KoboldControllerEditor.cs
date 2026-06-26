using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KoboldController))]
public class KoboldControllerEditor : Editor
{
    private void OnSceneGUI()
    {
        KoboldController kobold = (KoboldController)target;
        if (kobold.waypoints == null || kobold.waypoints.Length == 0)
            return;

        // -----------------------------
        // Draw looped waypoint path (cyan)
        // -----------------------------
        Handles.color = Color.cyan;

        for (int i = 0; i < kobold.waypoints.Length; i++)
        {
            Transform current = kobold.waypoints[i];
            Transform next = kobold.waypoints[(i + 1) % kobold.waypoints.Length];

            if (current != null && next != null)
            {
                Handles.DrawLine(current.position, next.position);
                Handles.SphereHandleCap(0, current.position, Quaternion.identity, 0.15f, EventType.Repaint);
            }
        }

        // Draw sphere for last waypoint
        Transform last = kobold.waypoints[kobold.waypoints.Length - 1];
        if (last != null)
        {
            Handles.SphereHandleCap(0, last.position, Quaternion.identity, 0.15f, EventType.Repaint);
        }

        // -----------------------------
        // Draw red line to current target waypoint
        // -----------------------------
        Handles.color = Color.red;

        int index = 0;

#if UNITY_EDITOR
        // Use the actual waypoint index from the kobold
        index = Mathf.Clamp(kobold.Editor_CurrentWaypointIndex, 0, kobold.waypoints.Length - 1);
#endif

        Transform targetWaypoint = kobold.waypoints[index];
        if (targetWaypoint != null)
        {
            // Draw red line
            Handles.DrawLine(kobold.transform.position, targetWaypoint.position);

            // Draw red circle at the end of the line
            Handles.SphereHandleCap(
                0,
                targetWaypoint.position,
                Quaternion.identity,
                0.2f,
                EventType.Repaint
            );
        }
    }
}