using System.Collections.Generic;
using UnityEngine;

public delegate void CollisionEvent(Collision collision);
public delegate void DamageEvent(ArmoredVehicle attacker, ArmoredVehicle victim, float amount);
public delegate void HitEvent(Weapon weapon, ArmoredVehicle victim, float damage);
public delegate void MissEvent(Weapon weapon);
public delegate void DeathEvent(ArmoredVehicle attacker, ArmoredVehicle victim);
public delegate void SelectWeaponEvent(int index);

public abstract class ArmoredVehicle : MonoBehaviour {
    public static List<ArmoredVehicle> Vehicles { get; private set; } = new List<ArmoredVehicle>();

    [HideInInspector] public float hp;

    [Header("Spec")]
    public float maxHp = 100f;

    [Header("Effects")]
    public CustomEffect deathEffect;

    [Header("Weapons")]
    public Weapon[] attachedWeapons;
    public Weapon currentWeapon;

    protected virtual void Awake() {
        hp = maxHp;
        Vehicles.Add(this);
        for (int i = 0; i < attachedWeapons.Length; i++) {
            if (attachedWeapons[i] == null) {
                continue;
            }
            attachedWeapons[i].attached = this;
        }
    }

    protected virtual void Start() {
        SelectWeapon(0);
    }

    private void OnDestroy() {
        Vehicles.Remove(this);
    }
    public SelectWeaponEvent OnSelectWeapon { get; set; }

    public void SelectWeapon(KeyCode key) {
        if (key < KeyCode.Alpha0 || key > KeyCode.Alpha9) {
            return;
        }
        if (key == KeyCode.Alpha0) {
            SelectWeapon(9);
            return;
        }
        SelectWeapon(key - KeyCode.Alpha1); //0~8
    }
    public void SelectWeapon(int index) {
        if (index < 0 || index >= attachedWeapons.Length) {
            return;
        }
        currentWeapon = attachedWeapons[index];
        if (attachedWeapons[index] == null) return;
        for (int i = 0; i < attachedWeapons.Length; i++) {
            attachedWeapons[i].holding = i == index;
        }
        OnSelectWeapon?.Invoke(index);

    }

    public void TriggerWeapon() {
        if (currentWeapon == null) return;
        currentWeapon.Trigger();
    }

    public DamageEvent OnDamage { get; set; }
    public HitEvent OnWeaponHit { get; set; }
    public MissEvent OnWeaponMiss { get; set; }
    public DeathEvent OnDeath { get; set; }
    public void Damage(float amount) {
        Damage(null, this, amount);
    }
    public void Damage(ArmoredVehicle attacker, ArmoredVehicle victim, float amount) {
        if (!gameObject.activeInHierarchy) return;
        hp -= amount;
        if (hp <= 0) {
            gameObject.SetActive(false);
            Death(attacker, victim);
            return;
        }
        OnDamage?.Invoke(attacker, victim, amount);
    }
    public virtual void Death(ArmoredVehicle attacker, ArmoredVehicle victim) {
        OnDeath?.Invoke(attacker, victim);
        if (deathEffect) {
            deathEffect.GetEffect(transform.position);
        }
    }
}