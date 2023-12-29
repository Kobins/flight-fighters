using UnityEngine;

public class WeaponSlot : MonoBehaviour {
    public int index;
    public Camera renderCamera;
    public Weapon weapon;
    public RenderTexture renderTexture;
    public UIWeaponSlotImage image;

    private void Awake() {
        renderTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        renderCamera.targetTexture = renderTexture;
    }
    private void Start() {
        renderTexture.name = "WeaponSlot_" + index + "_RenderTexture";
    }

    public float selectedCameraSize = 18f;
    public float deselectedCameraSize = 20f;

    public void SelectWeapon(bool selected) {
        renderCamera.orthographicSize = selected ? selectedCameraSize : deselectedCameraSize;
        image.selectedSquare.gameObject.SetActive(selected);
    }
}
