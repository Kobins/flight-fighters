using System;
using UnityEngine;

namespace UI.VR {
    /// <summary>
    /// 유니티 에디터 환경에서 마우스로 카메라 회전
    /// </summary>
    public class VREditorCamera : MonoBehaviour {
#if  UNITY_EDITOR
        [SerializeField] private float _rotateSpeed = 50f;
        [SerializeField] private float _rollRotateSpeed = 100f;
        [SerializeField] private float _slerpSpeed = 30f;

        private float _currentYaw = 0f;
        private float _currentPitch = 0f;
        private float _currentRoll = 0f;

        private void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update() {
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");
            
            var dt = Time.deltaTime;
            _currentYaw += mouseX * _rotateSpeed * dt;
            _currentPitch = Mathf.Clamp(
                _currentPitch - mouseY * _rotateSpeed * dt, 
                -89.9f, 89.9f
            );

            if (Input.GetKey(KeyCode.Q)) {
                _currentRoll += _rollRotateSpeed * dt;
            }else if (Input.GetKey(KeyCode.E)) {
                _currentRoll -= _rollRotateSpeed * dt;
            }
            
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                Quaternion.Euler(_currentPitch, _currentYaw, _currentRoll),
                dt * _slerpSpeed
            );
        }

#endif
    }
}