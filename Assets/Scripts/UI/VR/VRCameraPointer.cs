using System;
using Google.XR.Cardboard;
using UnityEngine;

namespace UI.VR {
    /// <summary>
    /// CameraPointer의 확장, VRCameraInteractable과 상호작용
    /// </summary>
    public class VRCameraPointer : MonoBehaviour {
        public bool disableRecenter;
        [SerializeField] private float _range = 10f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _recenterTime = 3f;

        public VRReticle Reticle { get; private set; }

        private void Awake() {
            Reticle = GetComponentInChildren<VRReticle>();
        }

        private void Update() {
            if (!Reticle || !Reticle.gameObject.activeInHierarchy) {
                return;
            }
            if(!disableRecenter)
                UpdateRecenter();
            UpdateInteractable();
        }

        private bool _isRecenter = false;
        private float _currentRecenterTime = 0f;
        private void UpdateRecenter() {
            if (_currentRecenterTime < 0f) {
                _isRecenter = false;
                Reticle.ResetProgress();
                _currentRecenterTime += Time.deltaTime;
                return;
            }
            if (VRUtil.IsTriggerPressing) {
                _currentRecenterTime += Time.deltaTime;
                _isRecenter = true;
                Reticle.UpdateProgressByRecenter(_currentRecenterTime / _recenterTime);

                if (_currentRecenterTime >= _recenterTime) {
                    Reticle.ResetProgress();
                    _currentRecenterTime = -1f;
                    #if !UNITY_EDITOR
                        Api.Recenter();
                    #endif
                    _isRecenter = false;
                }
            }
            else if(_currentRecenterTime > 0f) {
                
                _currentRecenterTime = 0f;
                Reticle.ResetProgress();
                _isRecenter = false;
            }
        }

        private VRCameraInteractable _currentObject;
        private void UpdateInteractable() {
            // raycast해서 닿은 VRCameraInteractable
            VRCameraInteractable newObject = null;
            if (!_isRecenter && Physics.Raycast(transform.position, transform.forward, out var hit, _range, _layerMask.value)) {
                newObject = hit.transform.GetComponent<VRCameraInteractable>();
            }

            // 현재 보고 있는 오브젝트와 다르면 exit 및 enter 호출
            if (_currentObject?.GetInstanceID() != newObject?.GetInstanceID()) {
                _currentObject?.OnPointerExit(this);
                _currentObject = newObject;
                _currentObject?.OnPointerEnter(this);
            }

            // 화면 터치 처리
            if (VRUtil.IsTriggerPressed) {
                _currentObject?.OnPointerClick(this);
            }
        }

        private void OnDrawGizmosSelected() {
            var t = transform;
            Gizmos.DrawRay(t.position, t.forward * _range);
        }
    }
}