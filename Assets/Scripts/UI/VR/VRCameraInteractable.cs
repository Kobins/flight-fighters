namespace UI.VR {
    /// <summary>
    /// VRCameraPointer와 상호작용하는 컴포넌트가 사용하는 인터페이스
    /// </summary>
    public interface VRCameraInteractable {
        public void OnPointerEnter(VRCameraPointer pointer);
        public void OnPointerExit(VRCameraPointer pointer);
        public void OnPointerClick(VRCameraPointer pointer);
        public int GetInstanceID();
    }
}