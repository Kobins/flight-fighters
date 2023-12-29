using UnityEngine;

public class Player : MonoBehaviour {

    [HideInInspector] public Aeroplane aeroplane;
    [HideInInspector] public GameController controller;

    public AudioClip lockOnSound;
    public AudioClip lockOnAlertSound;

    private AudioSource lockOnAudioSource;
    private AudioSource lockOnAlertAudioSource;

    public void Init(GameController controller, Aeroplane aeroplane) {
        this.controller = controller;
        this.aeroplane = aeroplane;
        lockOnSound = controller.lockOnSound;
        lockOnAlertSound = controller.lockOnAlertSound;
        name = "Player";
    }

    private void Start() {
        aeroplane.OnDeath += OnDeath;
        aeroplane.OnDamage += OnDamage;
        aeroplane.OnWeaponMiss += OnWeaponMiss;
        aeroplane.OnWeaponHit += OnHit;
        for (int i = 0; i < aeroplane.attachedWeapons.Length; i++) {
            aeroplane.attachedWeapons[i].OnGetShootPosition = OnGetShootPosition;
        }

        if (lockOnSound) {
            lockOnAudioSource = gameObject.AddComponent<AudioSource>();
            lockOnAudioSource.clip = lockOnSound;
            lockOnAudioSource.loop = true;
            lockOnAudioSource.playOnAwake = false;
        }

        if (lockOnAlertSound) {
            lockOnAlertAudioSource = gameObject.AddComponent<AudioSource>();
            lockOnAlertAudioSource.clip = lockOnAlertSound;
            lockOnAlertAudioSource.loop = true;
            lockOnAlertAudioSource.playOnAwake = false;
        }

    }

    private void Update() {
        if (controller.ended) return;
        //0~9 무기 선택
        for (KeyCode key = KeyCode.Alpha0; key < KeyCode.Alpha9; key++) {
            if (Input.GetKeyDown(key)) {
                aeroplane.SelectWeapon(key);
                break;
            }
        }
        if (Input.GetKey(KeyCode.Space)) {
            aeroplane.TriggerWeapon();
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            aeroplane.ToggleLandingGear();
        }

        bool isLocked = false;
        for (int i = 0; i < LockOnMissile.missiles.Count; i++) {
            if (LockOnMissile.missiles[i].target.gameObject.Equals(aeroplane.gameObject)) {
                isLocked = true;
                break;
            }
        }
        if (isLocked) {
            controller.headUpDisplay.alert.Alert("MISSILE ALERT", 1f, Color.red);
            if (lockOnAlertSound && !lockOnAlertAudioSource.isPlaying) {
                lockOnAlertAudioSource.Play();
            }
        } else {
            if (lockOnAlertSound) {
                lockOnAlertAudioSource.Stop();
            }
        }

        bool isLocking = false;
        for (int i = 0; i < aeroplane.attachedWeapons.Length; i++) {
            if (aeroplane.attachedWeapons[i].IsLocking()) {
                isLocking = true;
                break;
            }
        }
        if (isLocking) {
            if (lockOnSound && !lockOnAudioSource.isPlaying) {
                lockOnAudioSource.Play();
            }
        } else {
            if (lockOnSound) {
                lockOnAudioSource.Stop();
            }
        }
    }

    private void FixedUpdate() {
        if (controller.ended) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            aeroplane.Input(-1, 0, 0, 0);
            return;
        }
        if (Input.GetMouseButton(0)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            aeroplane.Input(
                Input.GetAxis("Vertical"),
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Mouse Y")
            );
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            aeroplane.Input(
                //전진/후진
                Input.GetAxis("Vertical"),
                //좌/우 (Y축)
                Input.GetAxis("Horizontal"),
                //roll (Z축)
                Input.GetAxis("Aeroplane Roll"),
                //기수 상승/기수 하강(X축)
                Input.GetAxis("Aeroplane Pitch")
            );
        }
    }

    public void OnGetShootPosition(Vector3 pos, out Vector3 target, float maxDst) {
        var center = new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        var ray = Camera.main.ScreenPointToRay(center);
        if (Physics.Raycast(ray, out var hit)) {
            target = hit.point;
        } else {
            target = ray.GetPoint(maxDst);
        }
    }

    public void OnWeaponMiss(Weapon weapon) {
        controller.headUpDisplay.alert.Alert("MISSED", 2f, Color.yellow);
    }

    public void OnDamage(ArmoredVehicle attacker, ArmoredVehicle victim, float amount) {
        controller.cameraController.Shake(amount / aeroplane.maxHp, 1f);
    }

    public void OnHit(Weapon weapon, ArmoredVehicle victim, float amount) {
        if (victim.hp <= 0 || !victim.gameObject.activeInHierarchy) {
            controller.headUpDisplay.alert.Alert("DESTROYED", 2f, Color.green);
        } else {
            controller.headUpDisplay.alert.Alert("HITTED", weapon.m_HitMarkDuration, Color.green);
        }
    }

    public void OnDeath(ArmoredVehicle attacker, ArmoredVehicle victim) {
        controller.GameEnd(false);
    }
}
