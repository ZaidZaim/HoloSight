using Microsoft.MixedReality.Toolkit.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBehaviour : MonoBehaviour
{
    public enum Target { User, Waypoint}
    [SerializeField] Target target = Target.Waypoint;
    [SerializeField] WaypointNavigator waypointNavigator;
    [SerializeField] Camera cam;
    private Transform destination;
    [SerializeField] AudioSource soundEmitter;
    [SerializeField] TextToSpeech textToSpeech;
    [SerializeField] Material eyesMaterial;
    [SerializeField] Material headMaterial;

    public enum State { ScanQR, Directing, Dog, Stairs, Finish}
    public State appState = State.ScanQR;

    private float startingTime;

    private Coroutine eyesLightCoroutine;

    private void Start() {
        ChangeAppState(State.ScanQR);
        startingTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == Target.User) {
            soundEmitter.Stop();
            Vector3 target = cam.transform.position + cam.transform.forward * 1 + cam.transform.up * Mathf.Sin(Time.time) * 0.1f;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 3);
            transform.LookAt(cam.transform.position);
        } else if (target == Target.Waypoint && waypointNavigator.nextWaypoint != null){
            destination = waypointNavigator.nextWaypoint.transform;
            Vector3 target = destination.transform.position + destination.transform.up * 1.5f + destination.transform.up * Mathf.Sin(Time.time) * 0.1f;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime);
        }
        transform.LookAt(cam.transform.position);

        //Instructions to scan QR code every 5 seconds
        if(appState == State.ScanQR) {
            if(Time.time - startingTime > 5) {
                textToSpeech.StartSpeaking("Look for a QR code in the floor");
                startingTime = Time.time;
                EyesLight();
            }
        }

        //Robot sound every 2 seconds to direct user.
        if (appState == State.Directing) {
            if (Time.time - startingTime > 2) {
                soundEmitter.Play();
                EyesLight();
                startingTime = Time.time;
            }
        }
    }

    public void SwitchTargetType(Target targetType) {
        target = targetType;
    }

    private void EyesLight() {
        if(eyesLightCoroutine != null) {
            StopCoroutine(eyesLightCoroutine);
        }
        eyesLightCoroutine = StartCoroutine(Co_EyesLight());
    }

    IEnumerator Co_EyesLight() {
        float duration = 1f;
        for (float i = 0; i < duration; i += Time.deltaTime) {
            eyesMaterial.color = Color.Lerp(Color.white, Color.grey, i / duration);
            headMaterial.color = Color.Lerp(Color.white, Color.black, i / duration);
            yield return null;
        }
    }

    public void ChangeAppState(State newState) {
        Debug.Log("App state set to: " + newState.ToString());
        appState = newState;
        switch (newState) {
            case State.ScanQR:
                SwitchTargetType(Target.User);
                break;
            case State.Directing:
                SwitchTargetType(Target.Waypoint);
                startingTime = Time.time;
                break;
            case State.Dog:
                break;
            case State.Stairs:
                break;
            case State.Finish:
                SwitchTargetType(Target.User);
                break;
            default:
                break;
        }
    }
}
