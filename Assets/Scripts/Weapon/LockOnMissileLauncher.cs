using System.Collections.Generic;
using UnityEngine;

public class LockOnMissileLauncher : Weapon, VRWeapon {

    [Header("Missile")]
    public LockOnMissile missilePrefab;

    [Header("Performance")]
    public float m_MaxLockAngle = 30f;
    public float m_MaxLockRange = 500f;
    public int m_MaxAmmo = 2;
    public float m_LockTime = 2f;
    public float m_ReloadTime = 3f;

    [HideInInspector] public ArmoredVehicle target;
    int ammo;
    float reloadingDelay;
    float currentLockTime;

    // VR 플레이어는 카메라 forward와 기체 forward가 다르므로 ...
    public Transform AttachedCameraTransform { get; set; }
    
    private void Start() {
        if(!AttachedCameraTransform) AttachedCameraTransform = attached.transform;
        ammo = m_MaxAmmo;
        shootDelay = 0;
        reloadingDelay = m_ReloadTime;
        currentLockTime = m_LockTime;
    }


    void Update() {
        if (isSlotDummy) return;
        //항상 1발씩 장전
        if (ammo < m_MaxAmmo) {
            if (reloadingDelay > 0) {
                reloadingDelay -= TimeManager.deltaTime;
            }
            if (reloadingDelay <= 0) {
                ammo++;
                reloadingDelay = m_ReloadTime;
            }
            if (attachedWeaponSlot) {
                //한번 꽉 차면 한번 장전되는 효과
                attachedWeaponSlot.image.SetFilled(1 - reloadingDelay / m_ReloadTime);
            }
        } else {
            if (attachedWeaponSlot) {
                attachedWeaponSlot.image.SetFilled(0);
            }
        }
        if (shootDelay > 0) {
            shootDelay -= TimeManager.deltaTime;
        }
        //무기 들고있지 않으면 락온 안됨
        if (!holding) {
            target = null;
            return;
        }
        var attachedCameraTransform = AttachedCameraTransform;
        var attachedObject = attached.gameObject;
        var vehicles = new List<ArmoredVehicle>(ArmoredVehicle.Vehicles);
        vehicles.RemoveAll((e) => {
            var targetObject = e.gameObject;
            var targetTransform = e.transform;
            var attachedToTarget = (targetTransform.position - attachedCameraTransform.position);
            return !e
                || targetObject.GetInstanceID() == attachedObject.GetInstanceID()
                //같은 Enemy일 경우
                || targetObject.CompareTag(attachedObject.tag)
                || !targetObject.activeInHierarchy
                //최대 락온 거리 벗어난 경우
                || attachedToTarget.magnitude > m_MaxLockRange
                //지정 시야각 벗어난 경우
                || Vector3.Angle(attachedToTarget.normalized, attachedCameraTransform.forward) > m_MaxLockAngle;
        });
        if (vehicles.Count <= 0) {
            target = null;
        }
        //타겟이 원래 없었거나 감지가능 리스트에서 없어진 경우
        if (target == null || !vehicles.Contains(target)) {
            currentLockTime = m_LockTime;
            float nearest = m_MaxLockAngle + 100;
            foreach (var entity in vehicles) {
                //가장 기체 방향에 가까운 적 찾기
                if (Vector3.Angle((entity.transform.position - attachedCameraTransform.position), attachedCameraTransform.forward) < nearest) {
                    target = entity;
                }
            }
        }
        if (target == null) return;
        if (currentLockTime > 0) {
            currentLockTime -= TimeManager.deltaTime;
        }
    }
    public override bool OnTrigger() {
        if (target == null
            || !target.gameObject.activeInHierarchy
            || currentLockTime > 0
            || shootDelay > 0
            || ammo <= 0
        ) return false;
        shootDelay = m_ShootInterval;
        ammo--;
        return true;
    }

    public override void OnShoot() {
        var shootPos = GetNextShootPosition();
        var missile = Instantiate(missilePrefab, shootPos.position, shootPos.rotation);
        missile.launcher = this;
        missile.shooted = attached;
        missile.target = target.gameObject;
        missile.isTargetingVehicle = true;
    }

    public override void OnMarkerUpdate(UIEnemyTargetMarker mark, Enemy enemy) {
        if (target == null || !target.gameObject.Equals(enemy.gameObject)) {
            if (mark.GetColor() != Color.green) {
                mark.SetColor(Color.green);
            }
            return;
        }
        if (currentLockTime > 0) {
            mark.SetColor(Color.yellow);
        } else {
            mark.SetColor(Color.red);
        }
    }

    public override bool IsLocking() {
        return target != null && currentLockTime <= 0;
    }
}
