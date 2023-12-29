using UnityEngine;

public class UIWeaponSlot : MonoBehaviour {
    public UIWeaponSlotImage slotPrefab;

    public UIWeaponSlotImage Create(WeaponSlot slot) {
        UIWeaponSlotImage prefab = slot.weapon.slotPrefab != null ? slot.weapon.slotPrefab : slotPrefab;
        var slotImage = Instantiate(prefab, transform);
        slotImage.name = "WeaponSlot_" + slot.index + "_RawImage";
        slotImage.image.texture = slot.renderTexture;
        return slotImage;
    }
}
