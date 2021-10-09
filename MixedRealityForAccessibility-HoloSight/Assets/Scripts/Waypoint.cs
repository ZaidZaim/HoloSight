using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public enum WaypointType { Regular, Sign, Dog, Stairs, Finish}
    public WaypointType waypointType = WaypointType.Regular;
    public GameObject circle;
    [HideInInspector] public bool triggered;
}
