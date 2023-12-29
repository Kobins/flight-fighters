using UnityEngine;

public class CameraController : MonoBehaviour {

    public Player player;

    public bool isFirstPerson = false;
    public Vector3 firstPersonView;
    public Vector3 thirdPersonView;

    public Vector3 firstPersonRotation;
    public Vector3 thirdPersonRotation;

    Vector3 offset;
    Vector3 offsetRotation;

    Vector3 realPosition;
    Vector3 shakeDelta;
    void Update() {
        if (player == null || !player.gameObject.activeSelf) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            isFirstPerson = !isFirstPerson;
        }
        offset = isFirstPerson ? firstPersonView : thirdPersonView;
        offsetRotation = isFirstPerson ? firstPersonRotation : thirdPersonRotation;
        transform.rotation = player.aeroplane.rigidbody.rotation * Quaternion.Euler(offsetRotation);
        realPosition = player.aeroplane.rigidbody.position + player.aeroplane.transform.TransformDirection(offset);

        transform.position = realPosition;

        if (shakeTime > 0) {
            shakeDelta = Random.insideUnitCircle * shakeAmount;
            transform.localPosition += shakeDelta;
            shakeAmount -= shakeAmount * TimeManager.deltaTime;
            shakeTime -= TimeManager.deltaTime;
        }
    }

    float shakeAmount;
    float shakeTime;

    public void Shake(float amount, float time) {
        shakeAmount = amount;
        shakeTime = time;
    }

}
