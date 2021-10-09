using Microsoft.MixedReality.Toolkit.Audio;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] List<Waypoint> waypoints;
    [SerializeField] RobotBehaviour robotBehaviour;
    public Waypoint nextWaypoint;
    [SerializeField] TextToSpeech textToSpeech;
    [SerializeField] GameObject signEvent;
    [SerializeField] GameObject dogEvent;
    [SerializeField] GameObject stairsEvent;
    [SerializeField] AudioSource continousSound;
    [SerializeField] private float soundVolume;
    private bool pathFinished;

    private int index = 0;

    void Start()
    {
        nextWaypoint = waypoints[index];
        nextWaypoint.circle.SetActive(true);
    }

    void Update()
    {
        if (!pathFinished) {
            if (nextWaypoint != null) {
                Vector3 waypointProjection = new Vector3(nextWaypoint.transform.position.x, 0, nextWaypoint.transform.position.z);
                Vector3 cameraProjection = new Vector3(cam.transform.position.x, 0, cam.transform.position.z);

                if (Vector3.Distance(waypointProjection, cameraProjection) < 1.5 && !nextWaypoint.triggered) {
                    CloseToWaypoint(nextWaypoint);
                }

                if (Vector3.Distance(waypointProjection, cameraProjection) < 0.7f) {
                    if (index < waypoints.Count - 1) {
                        nextWaypoint.circle.SetActive(false);
                        index++;
                        nextWaypoint = waypoints[index];
                        nextWaypoint.circle.SetActive(true);
                    } else {
                        nextWaypoint.circle.SetActive(false);
                        WaypointsCompleted();
                    }
                }

                soundVolume = Vector3.Dot(Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized, Vector3.ProjectOnPlane(nextWaypoint.transform.position - cam.transform.position, Vector3.up).normalized);
                continousSound.volume = Mathf.Pow(soundVolume, 100);
            } else {
                continousSound.volume = 0;
            }
        }
    }

    private void CloseToWaypoint(Waypoint waypoint) {
        if (waypoint.triggered) return;
        waypoint.triggered = true;
        switch (waypoint.waypointType) {
            case Waypoint.WaypointType.Regular:
                break;
            case Waypoint.WaypointType.Sign:
                TrainSignEvent();
                break;
            case Waypoint.WaypointType.Dog:
                DogEvent();
                break;
            case Waypoint.WaypointType.Stairs:
                StairsEvent();
                break;
            case Waypoint.WaypointType.Finish:
                break;
            default:
                break;
        }
    }

    private void TrainSignEvent() {
        textToSpeech.StartSpeaking("Next train to Itzaube departing from platform 3, in 9 minutes");
        signEvent.SetActive(true);
    }

    private void DogEvent() {
        dogEvent.SetActive(true);
        textToSpeech.StartSpeaking("Angry dog detected, don't pet, 97% confidence of bite.");
    }

    private void StairsEvent() {
        stairsEvent.SetActive(true);
        textToSpeech.StartSpeaking("Stairs ahead, be careful, don't fall.");
    }

    private void WaypointsCompleted() {
        Debug.Log("Waypoints completed");
        robotBehaviour.ChangeAppState(RobotBehaviour.State.Finish);
        pathFinished = true;
    }
}
