using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour {
    public UIWeaponSlot uiWeaponSlot;
    public WeaponSlot weaponSlotPrefab;
    public Text currentWeaponText;

    List<WeaponSlot> weapons;

    public void Init(ArmoredVehicle vehicle) {
        vehicle.OnSelectWeapon += Select;
        weapons = new List<WeaponSlot>(vehicle.attachedWeapons.Length);
        for (int i = 0; i < vehicle.attachedWeapons.Length; i++) {
            var slot = Instantiate(weaponSlotPrefab, transform);
            slot.index = i;
            slot.name = "Slot_" + i + "_" + vehicle.attachedWeapons[i].m_DisplayName;
            slot.transform.position = new Vector3(i * 40, 0, -20);
            weapons.Add(slot);
            vehicle.attachedWeapons[i].attachedWeaponSlot = slot;

            slot.weapon = Instantiate(vehicle.attachedWeapons[i], slot.transform);
            slot.weapon.name = slot.weapon.m_DisplayName;
            slot.weapon.transform.localPosition = slot.weapon.m_SlotViewPosition;
            slot.weapon.transform.rotation = Quaternion.Euler(slot.weapon.m_SlotViewRotation);
            slot.weapon.transform.localScale = slot.weapon.m_SlotViewScale;
            slot.weapon.slotViewing = true;

            slot.image = uiWeaponSlot.Create(slot);
        }
    }

    void Select(int index) {
        if (index < 0 || index >= weapons.Count) {
            return;
        }
        for (int i = 0; i < weapons.Count; i++) {
            weapons[i].SelectWeapon(i == index);
        }
        currentWeaponText.text = weapons[index].weapon.m_DisplayName;
    }
}
