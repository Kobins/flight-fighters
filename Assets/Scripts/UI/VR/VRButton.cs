using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.VR {
    /// <summary>
    /// VR 환경 버튼
    /// </summary>
    public class VRButton : MonoBehaviour, VRCameraInteractable {
        public float stayTimeToPress = 3f;

        private bool _isClicked = false;
        private VRCameraPointer _isPointing;
        private float _currentTimeToPress = 0f;

        [ContextMenu("Set Box Collider")]
        private void AddBoxCollider() {
            var boxCollider = gameObject.GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.isTrigger = true;
            //
            // var button = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            // button.targetGraphic.rectTransform.rect
        }
        
        private void Update() {
            if (_isPointing) {
                _currentTimeToPress += Time.deltaTime;
                if (!_isClicked && _currentTimeToPress >= stayTimeToPress) {
                    _isClicked = true;
                    OnClick();
                }
                else {
                    _isPointing.Reticle?.UpdateProgress(_currentTimeToPress / stayTimeToPress);
                }
            }
        }

        public void OnPointerClick(VRCameraPointer pointer) {
            OnClick();
        }

        private void OnClick() {
            PointerEventData data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerClickHandler);
            _isPointing.Reticle?.ResetProgress();
        }

        public void OnPointerEnter(VRCameraPointer pointer) {
            _currentTimeToPress = 0f;
            _isPointing = pointer;
            _isClicked = false;
            PointerEventData data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerEnterHandler);
            pointer.Reticle?.ResetProgress();
        }

        public void OnPointerExit(VRCameraPointer pointer) {
            _currentTimeToPress = 0f;
            _isPointing = null;
            _isClicked = false;
            PointerEventData data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerExitHandler);
            pointer.Reticle?.ResetProgress();
        }
    }
}