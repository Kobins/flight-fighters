using UnityEngine;

namespace Core.VR {
    public class VRCameraController : MonoBehaviour {
        public Player player;

        public Vector3 firstPersonPosition;
        public Vector3 firstPersonRotation;

        void Update() {
            if (!player || !player.gameObject.activeSelf) {
                return;
            }
            var offset = firstPersonPosition;
            var offsetRotation = firstPersonRotation;
            var t = player.aeroplane.cameraPosition;
            transform.rotation = t.rotation * Quaternion.Euler(offsetRotation);
            var realPosition = t.position + t.TransformDirection(offset);

            transform.position = realPosition;

            if (_shakeTime > 0) {
                Vector3 shakeDelta = Random.insideUnitSphere * _shakeAmount;
                transform.localPosition += shakeDelta;
                _shakeAmount -= _shakeAmount * TimeManager.deltaTime;
                _shakeTime -= TimeManager.deltaTime;
            }
        }

        private float _shakeAmount;
        private float _shakeTime;

        public void Shake(float amount, float time) {
            _shakeAmount = amount;
            _shakeTime = time;
        }
    }
}