using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LockOnMissile : MonoBehaviour {
    public static readonly List<LockOnMissile> Missiles = new List<LockOnMissile>();

    [HideInInspector] public LockOnMissileLauncher launcher;
    [HideInInspector] public ArmoredVehicle shooted;
    public GameObject target;
    public bool isTargetingVehicle = true;

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
    [Header("Flare-Related")] 
    public float flareRecognizeAngle = 40f;

    private float _flareRecognizeAngleInCos;
    
    CustomEffect trail;

    float time;
    float speed;
    float moved;

    List<Collider> colliders;

    private void Awake() {
        _flareRecognizeAngleInCos = Mathf.Acos(flareRecognizeAngle * Mathf.Deg2Rad);
        colliders = new List<Collider>();
        foreach (var component in GetComponents<Collider>()) {
            colliders.Add(component);
        }
        foreach (var component in GetComponentsInChildren<Collider>()) {
            colliders.Add(component);
        }
        Missiles.Add(this);
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

        isTargetingVehicle = true;
    }

    void FixedUpdate() {
        var t = transform;
        var position = t.position;
        var forward = t.forward;
        if (target && isTargetingVehicle) {
            Flare findFlare = null;
            foreach (var flare in Flare.Flares.Values) {
                // 같은 개체가 쏘지 않은 플레어에 대해 
                if(flare.Launcher.attached == launcher.attached) continue;
                var missileToFlare = flare.transform.position - position;
                var directionToFlare = missileToFlare.normalized;
                var dot = forward.Dot(directionToFlare);
                // 일정 시야각 내에 존재하면 그 플레어를 우선해 따라감
                if (dot > _flareRecognizeAngleInCos) {
                    findFlare = flare;
                    break;
                }
            }

            if (findFlare) {
                target = findFlare.gameObject;
                isTargetingVehicle = false;
            }
        }
        if (target && target.gameObject.activeInHierarchy) {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(target.transform.position - position),
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
        if (!target || !target.gameObject.activeInHierarchy) {
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
        // Debug.Log($"missile {gameObject.name}::OnTriggerEnter({other.name})");
        // 지형 충돌
        if (other.CompareTag("Terrain")) {
            launcher.attached.OnWeaponMiss?.Invoke(launcher);
            gameObject.SetActive(false);
            return;
        }
        // 미사일 기만
        if (isTargetingVehicle && other.CompareTag("Flare")) {
            var flare = other.GetComponent<Flare>();
            if (!flare) {
                Debug.LogWarning($"object {other.name} has tag Flare, but doesn't have component Flare");
                return;
            }
            // 플레어와 미사일의 발사자가 다른 경우에만
            if (flare.Launcher.attached != launcher.attached) {
                // Debug.Log($"missile {gameObject.name} targeting flare {flare.gameObject.name}");
                // 플레어를 따라가도록
                isTargetingVehicle = false;
                target = flare.gameObject;
            }
            return;
        }
        var vehicle = other.transform.GetComponent<ArmoredVehicle>();
        if (vehicle) {
            DamageVehicle(vehicle);
            return;
        }
        if (other.transform.parent) {
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
        Missiles.Remove(this);
        if (trail) {
            trail.Particle.Stop();
        }
        if (explodeEffect) {
            var effect = explodeEffect.GetEffect(transform.position);
        }
    }
}