using System;
using UnityEngine;

public class FlareLauncher : Weapon {
    [Header("Flare")]
    public Flare flarePrefab;
    public int maxAmmo = 2;
    public float reloadTime = 3f;
    
    private int _ammo;
    private float _reloadingDelay;

    private float _flareRotateAngle;
    
    private void Start() {
        _ammo = maxAmmo;
        shootDelay = 0;
        _reloadingDelay = reloadTime;
        _flareRotateAngle = 360f / m_ShootPerTrigger;
    }

    private void Update() {
        if (isSlotDummy) return;
        //항상 1발씩 장전
        if (_ammo < maxAmmo) {
            if (_reloadingDelay > 0) {
                _reloadingDelay -= TimeManager.deltaTime;
            }
            if (_reloadingDelay <= 0) {
                _ammo++;
                _reloadingDelay = reloadTime;
            }
            if (attachedWeaponSlot) {
                //한번 꽉 차면 한번 장전되는 효과
                attachedWeaponSlot.image.SetFilled(1 - _reloadingDelay / reloadTime);
            }
        } else {
            if (attachedWeaponSlot) {
                attachedWeaponSlot.image.SetFilled(0);
            }
        }
        if (shootDelay > 0) {
            shootDelay -= TimeManager.deltaTime;
        }
    }


    public override bool OnTrigger() {
        if (shootDelay > 0
            || _ammo <= 0
           ) return false;
        shootDelay = m_ShootInterval;
        --_ammo;
        return true;
    }

    private Vector3 _lastShootDirection = Vector3.right;
    public override void OnShoot() {
        // 발사지점에서 특정 방향으로 
        var shootPosition = GetNextShootPosition();
        var shootLocalDirection = _lastShootDirection.RotateAroundZ(_flareRotateAngle);
        var shootWorldDirection = transform.TransformDirection(shootLocalDirection);
        var flare = Instantiate(flarePrefab, shootPosition.position, Quaternion.LookRotation(shootWorldDirection));
        flare.Init(this, attached, shootWorldDirection);
    }
}