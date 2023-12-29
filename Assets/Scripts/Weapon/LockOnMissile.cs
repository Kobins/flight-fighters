using System.Collections.Generic;
using UnityEngine;

public class LockOnMissile : MonoBehaviour {
    public static List<LockOnMissile> missiles = new List<LockOnMissile>();

    [HideInInspector] public LockOnMissileLauncher launcher;
    [HideInInspector] public ArmoredVehicle shooted;
    [HideInInspector] public ArmoredVehicle target;

    [Header("Effects")]
    public CustomEffect explodeEffect;
    public CustomEffect trailEffect;
    [Header("Performance")]
    public float m_InitialSpeed = 50f;
    public float m_AcceleratePerSeconds = 100f;
    public float m_AccelerateDuration = 3f;
    public float m_RotateSpeed = 30f;
    public float m_Damage = 50f;
    public float m_MissingTargetDistance = 50f;
    public float m_MaxRange = 10000f;

    CustomEffect trail;

    float time;
    float speed;
    float moved;

    List<Collider> colliders;

    private void Awake() {
        colliders = new List<Collider>();
        foreach (var component in GetComponents<Collider>()) {
            colliders.Add(component);
        }
        foreach (var component in GetComponentsInChildren<Collider>()) {
            colliders.Add(component);
        }
        missiles.Add(this);
    }
    void Start() {
        time = 0;
        speed = m_InitialSpeed;
        moved = 0f;
        colliders.ForEach(c => c.isTrigger = true);
        if (trailEffect) {
            trail = trailEffect.GetEffect();
            trail.Particle.Play();
            trail.transform.position = transform.position;
        }
    }

    void FixedUpdate() {
        if (target != null && target.gameObject.activeInHierarchy) {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(target.transform.position - transform.position),
                m_RotateSpeed * Time.fixedDeltaTime
            );
        }
        if (time <= m_AccelerateDuration) {
            speed += m_AcceleratePerSeconds * Time.fixedDeltaTime;
            time += Time.fixedDeltaTime;
        }
        float velocity = speed * Time.fixedDeltaTime;
        transform.Translate(Vector3.forward * velocity);
        moved += velocity;
        if (moved >= m_MaxRange) {
            launcher.attached.OnWeaponMiss?.Invoke(launcher);
            gameObject.SetActive(false);
            return;
        }
        if (trail) {
            trail.transform.position = transform.position;
        }
        if (target == null || !target.gameObject.activeInHierarchy) {
            return;
        }
        var local = transform.InverseTransformPoint(target.transform.position);
        if (local.z < -m_MissingTargetDistance) {
            launcher.attached.OnWeaponMiss?.Invoke(launcher);
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Terrain")) {
            launcher.attached.OnWeaponMiss?.Invoke(launcher);
            gameObject.SetActive(false);
            return;
        }
        var vehicle = other.transform.GetComponent<ArmoredVehicle>();
        if (vehicle) {
            DamageVehicle(vehicle);
            return;
        }
        if (other.transform.parent != null) {
            vehicle = other.transform.parent.GetComponent<ArmoredVehicle>();
            if (vehicle) {
                DamageVehicle(vehicle);
                return;
            }
        }
    }

    void DamageVehicle(ArmoredVehicle vehicle) {
        if (vehicle.gameObject.Equals(shooted.gameObject)) {
            return;
        }
        vehicle.Damage(shooted, vehicle, m_Damage);
        launcher.attached.OnWeaponHit?.Invoke(launcher, vehicle, m_Damage);
        gameObject.SetActive(false);
    }

    private void OnDisable() {
        missiles.Remove(this);
        if (trail) {
            trail.Particle.Stop();
        }
        if (explodeEffect) {
            var effect = explodeEffect.GetEffect(transform.position);
        }
    }
}