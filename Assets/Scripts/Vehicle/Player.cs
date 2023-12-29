using System;
using System.Linq;
using UI.VR;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour {

    [HideInInspector] public Aeroplane aeroplane;
    [HideInInspector] public GameController controller;

    public LayerMask enemyLayerMask;
    public float enemyFindMaxRange = 10000f;
    
    public AudioClip lockOnSound;
    public AudioClip lockOnAlertSound;

    private AudioSource lockOnAudioSource;
    private AudioSource lockOnAlertAudioSource;

    private Camera _camera;

    public void Init(GameController controller, Aeroplane aeroplane) {
        this.controller = controller;
        this.aeroplane = aeroplane;
        aeroplane.cockpitUI.Init(this);
        lockOnSound = controller.lockOnSound;
        lockOnAlertSound = controller.lockOnAlertSound;
        enemyLayerMask = LayerMask.GetMask("Enemy");
        name = "Player";
    }

    private void Start() {
        _camera = Camera.main;
        if (!_camera) {
            Debug.LogWarning("NO CAMERA !!!");
            return;
        }
        aeroplane.OnDeath += OnDeath;
        aeroplane.OnDamage += OnDamage;
        aeroplane.OnWeaponMiss += OnWeaponMiss;
        aeroplane.OnWeaponHit += OnHit;
        for (int i = 0; i < aeroplane.attachedWeapons.Length; i++) {
            aeroplane.attachedWeapons[i].OnGetShootPosition = OnGetShootPosition;

            if (aeroplane.attachedWeapons[i] is VRWeapon vrWeapon) {
                vrWeapon.AttachedCameraTransform = _camera.transform;
            }
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
        
        // 앞쪽 보고 있으면 미사일(무기0)
        // 뒤쪽 보고 있으면 플레어(무기1)
        var dot = aeroplane.transform.forward.Dot(_camera.transform.forward);
        aeroplane.SelectWeapon(dot > 0f ? 0 : 1);
        if (VRUtil.IsTriggerPressed) {
            aeroplane.TriggerWeapon();
        }

        // 나를 락온중인 미사일이 있다면 락온 중 판정
        bool isLocked = false;
        for (int i = 0; i < LockOnMissile.Missiles.Count; i++) {
            if (LockOnMissile.Missiles[i].target.gameObject == aeroplane.gameObject) {
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

        // 내가 락온중이라면 -> 모든 무기 중 Lock하고 있는 게 있다면
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


    private UnityAction _onWaypointEnd;
    private bool _repeatWaypoint;
    private WaypointHandler _waypoint;
    private int _waypointIndex;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="waypoint"></param>
    /// <param name="nearest">가장 가까운 웨이포인트부터 시작 여부입니다.</param>
    /// <param name="repeat">반복 여부입니다.</param>
    /// <param name="onWaypointEnd">웨이포인트가 끝나면 호출되는 콜백입니다.</param>
    public void SetWaypoint(WaypointHandler waypoint, bool nearest, bool repeat, UnityAction onWaypointEnd = null) {
        // 원래 들고 있던 waypoint 제거
        if (_waypoint) {
            _waypoint.gameObject.SetActive(false);
            _onWaypointEnd = null;
        }
        _waypoint = waypoint;
        _waypoint.gameObject.SetActive(true);

        // 가장 가까운 위치
        if (nearest) {
            var minimumIndex = 0;
            var minimumDistance = Vector3.Distance(_waypoint.Waypoints[0].transform.position, aeroplane.transform.position);
            for (int i = 1; i < _waypoint.Waypoints.Count; i++) {
                var distance = Vector3.Distance(_waypoint.Waypoints[i].transform.position, aeroplane.transform.position);
                if (distance < minimumDistance) {
                    minimumIndex = i;
                    minimumDistance = distance;
                }
            }

            _waypointIndex = minimumIndex;
        }
        else {
            _waypointIndex = 0;
        }
        

        _repeatWaypoint = repeat;
        _onWaypointEnd = onWaypointEnd;
        
    }

    private Collider[] _colliderCache = new Collider[20];
    private Transform _targetEnemy = null;
    private void FixedUpdate() {
        UpdateEnemy();
        Move();
    }

    private void UpdateEnemy() {
        if (!_targetEnemy || !_targetEnemy.gameObject.activeSelf) {
            _targetEnemy = null;

            var position = transform.position;
            int count = Physics.OverlapSphereNonAlloc(position, enemyFindMaxRange, _colliderCache, enemyLayerMask);
            if(count <= 0) return;
            var minimum = _colliderCache[0];
            var minimumDistance = position.DistanceSquared(minimum.transform.position);
            for (int i = 1; i < count; i++) {
                var distance = position.DistanceSquared(_colliderCache[i].transform.position);
                if (distance < minimumDistance) {
                    minimumDistance = distance;
                    minimum = _colliderCache[i];
                }
            }

            _targetEnemy = minimum.transform;
        }
    }

    private void Move() {
        if (controller.ended) {
            aeroplane.Input(0, 0, 0, 0);
            return;
        }

        Transform target;
        if (_waypoint) {
            target = _waypoint.Waypoints[_waypointIndex].transform;
        }
        else {
            target = _targetEnemy;
            if (!target) {
                aeroplane.Input(0f, 0f, 0f, 0f);
                return;
            }
        }

        var position = transform.position;
        var targetWorldPosition = target.position;
        Debug.DrawLine(position, targetWorldPosition, Color.yellow);
        var length = (targetWorldPosition - position).magnitude;
        Debug.DrawRay(position, transform.forward * length, Color.cyan);
        Debug.DrawRay(position, transform.up * length, Color.green);
        Debug.DrawRay(position, transform.right * length, Color.red);
        var targetLocalDirection = transform.InverseTransformDirection(targetWorldPosition - position);
        // 1. 일단 로컬 기준 위 쪽을 바라보게 만든 뒤
        // 2. pitch로 방향만 맞춤
        // 3. roll 평형이루기

        float roll, yaw = 0f;
        // 3 -> 거의 다 맞췄으면 roll 평형 이루기
        var dot = Vector3.forward.Dot(targetLocalDirection.normalized);
        if (dot >= 0.95) {
            roll = -aeroplane.rollAngle / 45f;
            if (Mathf.Abs(roll) <= 1f) {
                yaw = Vector3.SignedAngle(Vector3.forward, targetLocalDirection.ProjectedXZPlane(), Vector3.up);
            }
        }
        // 아니면 위 쪽을 바라보도록 roll
        else {
            // 1 -> 로컬 업벡터와 목표 업벡터 사잇각 비교
            var upAngle = Vector3.SignedAngle(Vector3.up, targetLocalDirection.ProjectedXYPlane(), Vector3.forward);
            // SignedAngle은 왼손으로 감는 방향이 +임
            // -면 왼쪽으로, +면 오른쪽으로
            roll = -upAngle / 45f;
            if (roll > 0.2f) { // 10도 이상 차이나면 일단 돌리는거 먼저
                aeroplane.Input(1f, 0f, roll, 0f);
                // Debug.Log($"roll first: {roll}");
                return;
            }
        }
        
        // 2 -> pitch로 방향 맞추기
        var pitchAngle = Vector3.SignedAngle(Vector3.forward, targetLocalDirection.ProjectedYZPlane(), Vector3.right);
        var pitch = -pitchAngle / 90f;

        aeroplane.Input(1f, yaw, roll, pitch);
        // Debug.Log($"yrp: {yaw}, {roll}, {pitch}, dot: {dot}");
    }

    private void OnTriggerEnter(Collider other) {
        if(!_waypoint) return;
        if (_waypoint.GetWaypointIndex(other.gameObject) == _waypointIndex) {
            NextWaypoint();    
        }
    }

    private void NextWaypoint() {
        var nextIndex = _waypointIndex + 1;
        if (nextIndex >= _waypoint.Waypoints.Count) {
            if (_repeatWaypoint) {
                nextIndex = 0;
            }
            else {
                _onWaypointEnd?.Invoke();
                _onWaypointEnd = null;
                _waypoint = null;
                return;
            }
        }
        _waypointIndex = nextIndex;
    }

    public void OnGetShootPosition(Vector3 pos, out Vector3 target, float maxDst) {
        var cameraTransform = controller.cameraController.transform;
        var ray = new Ray(cameraTransform.position, cameraTransform.forward);
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

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyFindMaxRange);
    }
}
