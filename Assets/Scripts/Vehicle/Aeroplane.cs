using System.Collections.Generic;
using UI.HUD;
using UnityEngine;

//Standard Assets의 AeroplaneController 기반
[RequireComponent(typeof(Rigidbody))]
public class Aeroplane : ArmoredVehicle {
    private void OnCollisionEnter(Collision collision) {
        if (!collision.collider.CompareTag("Terrain")) return;
        Damage(collision.impulse.magnitude);
    }

    [Header("Object Binding")] 
    public Transform cameraPosition;
    public UICockpit cockpitUI;

    [Header("AttackAngle Effects")]
    public float attackAngle = 0.5f;
    public List<CustomEffect> attackAngleEffects;

    [Header("Afterburner Effects")]
    public float afterburnerMin;
    public float afterburnerMax;
    public List<ParticleSystem> afterburners;

    [Header("Sounds")]
    public AudioClip engineSound;
    public float engineSoundPitchMin = 0.5f;
    public float engineSoundPitchMax = 2f;
    public float engineSoundVolume = 1f;
    public AudioClip windSound;
    public float windSoundPitchBase = 0.5f;
    public float windSoundPitchFactor = 0.005f;
    public float windSoundVolumeMin = 0f;
    public float windSoundVolumeMax = 1f;
    [HideInInspector] public AudioSource engineAudioSource;
    [HideInInspector] public AudioSource windAudioSource;

    [Header("Landing Gears")]
    public Animator[] landingGearAnimators;

    [HideInInspector] public new Rigidbody rigidbody;

    [Header("Engine and Throttle")]
    [SerializeField] public float m_MaxEnginePower = 300f;
    [SerializeField] private float m_ThrottleChangeSpeed = 0.3f;

    [Header("Aerodynamic")]
    [SerializeField] private float m_AerodynamicEffect = 0.02f;
    [SerializeField] private float m_DragEffectByAirBrakes = 3f;
    [SerializeField] private float m_DragEffectBySpeed = 0.001f;
    [SerializeField] private float m_LiftByForwardSpeed = 0.002f;
    [SerializeField] private float m_ZeroLiftSpeedPoint = 300f;
    [SerializeField] private float m_LandingGearFactor = 0.8f;

    [Header("Turn Sensivity")]
    [SerializeField] private float m_YawSensivity = 0.2f;
    [SerializeField] private float m_PitchSensivity = 1f;
    [SerializeField] private float m_RollSensivity = 1f;

    private float initialDrag;
    private float initialAngularDrag;

    public float throttleInput { get; private set; }
    public float yawInput { get; private set; }
    public float rollInput { get; private set; }
    public float pitchInput { get; private set; }
    public bool airBrake { get; private set; }
    public float throttle { get; private set; }
    public float enginePower { get; private set; }
    public float MaxEnginePower { get => m_MaxEnginePower; }
    public float forwardSpeed { get; private set; }
    public float angleDifference { get; private set; }
    public Vector3 xzForward { get; private set; }
    public Vector3 xzRight { get; private set; }
    public float yawAngle { get; private set; }
    public float pitchAngle { get; private set; }
    public float rollAngle { get; private set; }
    public float bankedTurnAmount { get; private set; }
    public float altitude { get; private set; }
    public bool landingGear { get; private set; } = true;

    protected override void Awake() {
        base.Awake();
        rigidbody = GetComponent<Rigidbody>();
        initialDrag = rigidbody.drag;
        initialAngularDrag = rigidbody.angularDrag;

        if (engineSound) {
            engineAudioSource = gameObject.AddComponent<AudioSource>();
            engineAudioSource.loop = true;
            engineAudioSource.clip = engineSound;
            engineAudioSource.Play();
        }
        if (windSound) {
            windAudioSource = gameObject.AddComponent<AudioSource>();
            windAudioSource.loop = true;
            windAudioSource.clip = windSound;
            windAudioSource.Play();
        }
    }

    private void OnEnable() {
        hp = maxHp;
    }

    public void Input(float throttle, float yaw, float roll, float pitch) {
        throttleInput = throttle;
        yawInput = yaw;
        rollInput = roll;
        pitchInput = pitch;

        //-1과 1로 제한
        throttleInput = Mathf.Clamp(throttleInput, -1, 1);
        yawInput = Mathf.Clamp(yawInput, -1, 1);
        rollInput = Mathf.Clamp(rollInput, -1, 1);
        pitchInput = Mathf.Clamp(pitchInput, -1, 1);

        //에어 브레이크
        airBrake = throttleInput < 0;

        Move();
    }

    public void ToggleLandingGear() => LandingGear(!landingGear);

    public void LandingGear(bool landingGear) {
        this.landingGear = landingGear;
        for (int i = 0; i < landingGearAnimators.Length; i++) {
            landingGearAnimators[i].SetTrigger(landingGear ? "landing" : "takeoff");
        }
    }

    void Move() {
        CalculateForward();
        AdjustRigidbody();
        ApplyForces();
        ApplyTorques();
        CalculateAngles();
        CalculateAltitude();
        CalculateEffects();
        CalculateSounds();
    }

    void CalculateForward() {

        //전방 속도 계산 (실제 기체 속도에서 기체가 기수 방향의 값만 가져오기)
        var localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        forwardSpeed = Mathf.Max(0, localVelocity.z);

        if (landingGear) {
            forwardSpeed *= m_LandingGearFactor;
        }

        //Mathf.Clamp01(value) == Mathf.Clamp(0, 1, value)
        //스로틀 입력에 따라 스로틀 조정
        //입력 * 스로틀 변경 속도 * dt
        throttle = Mathf.Clamp01(throttle + throttleInput * TimeManager.deltaTime * m_ThrottleChangeSpeed);

        //엔진 출력 == 스로틀(0~1) * 최대 엔진 파워
        enginePower = throttle * m_MaxEnginePower;
    }

    void AdjustRigidbody() {
        //속도에 비례해서 공기 저항 증가
        float dragBySpeed = rigidbody.velocity.magnitude * m_DragEffectBySpeed;
        //에어 브레이크는 기체의 공기 저항을 증가시켜 작동
        rigidbody.drag = (initialDrag + dragBySpeed) * (airBrake ? m_DragEffectByAirBrakes : 1);
        //전방 속도 비례해 회전 저항 증가
        //높은 속도에서 조금 느리게 회전함 -> 관성
        rigidbody.angularDrag = initialAngularDrag * forwardSpeed;

        //기수 방향 벡터와 기체 속도 방향벡터간 내적값 (==벡터간 각도 cos값)
        angleDifference = Vector3.Dot(transform.forward, rigidbody.velocity.normalized);
        //구간 [0, 90]의 cos값은 1에서 시작해서 0까지 점점 감소하는 형태를 가지고 있음.
        //cos^2값은 그 정도가 더 심해짐 -> 차이가 커질 수록 작아짐
        angleDifference *= angleDifference;

        //기수 방향과 기체 속도 방향 차이를 선형 보간
        //* 그냥 AddForce만 하면 이전에 가해진 힘의 영향으로 밀림
        //* 방향을 보간해주면 이전에 가해진 힘의 영향이 줄어듦
        //angleDifference는 방향 차이가 커질 수록 작아지기 때문에 너무 차이가 커지면 보정이 거의 일어나지 않음
        //>> 90도에 가까워지면 실속 상태에 빠질 수 있음
        rigidbody.velocity = Vector3.Lerp(
            rigidbody.velocity,
            transform.forward * forwardSpeed,
            angleDifference * forwardSpeed * m_AerodynamicEffect * TimeManager.deltaTime
        );
        if (rigidbody.velocity.sqrMagnitude == 0) return;
        //비행기의 실제 회전값을 기체 속도로 구면 보간
        //미미한 작용이나, 실속 상태에서 자동 회복 효과
        rigidbody.rotation = Quaternion.Slerp(
            rigidbody.rotation,
            Quaternion.LookRotation(rigidbody.velocity, transform.up),
            m_AerodynamicEffect * TimeManager.deltaTime
        );
    }

    void ApplyForces() {
        var forces = Vector3.zero;
        //전방 가속
        forces += enginePower * transform.forward;
        //양력 방향 == 기체 수직
        var liftDirection = Vector3.Cross(rigidbody.velocity, transform.right).normalized;

        //기본적으로 속도가 증가할 수록 양력은 증가
        //[0, inf) 구간의 속도^2 * 양력값 -> y=x^2 그래프와 일치
        var liftFactor = forwardSpeed * forwardSpeed * m_LiftByForwardSpeed;

        //Lerp(start, end, ratio) == value
        //InverseLerp(start, end, value) == ratio

        //비행기가 ZeroLiftSpeedPoint에 가까워질 수록 다시 감소함
        //실제로는 이륙 직후 조종사가 플랩을 후퇴할 때 일어남 - 항력을 감소시키지만 양력도 또한 감소함
        //플랩을 실제로 시뮬레이트하지 않기 때문에, 간단하게 구현됨

        //0~1의 값 -> forwardSpeed가 m_ZeroLiftPoint 가까워질 수록 0에 가까워짐
        //[0,1] 구간의 y = -x+1의 그래프와 일치
        var zeroLiftFactor = Mathf.InverseLerp(m_ZeroLiftSpeedPoint, 0, forwardSpeed);

        //양력 = 전방속도^2 * 양력 * 양력감소 * 기수-속도 일치율 (실속 시 양력 약해짐)
        var liftPower = liftFactor * zeroLiftFactor * angleDifference;

        //힘 총합
        forces += liftPower * liftDirection;
        rigidbody.AddForce(forces);
    }

    void ApplyTorques() {
        var t = transform;
        var torque = Vector3.zero;

        //각 축에 대한 yaw, pitch, roll 회전
        torque += (-pitchInput * m_PitchSensivity) * t.right;
        torque += (yawInput * m_YawSensivity) * t.up;
        torque += (-rollInput * m_RollSensivity) * t.forward;

        //전체 토크는 전방 속도와 곱해져서 고속 상황에서 회전력이 높음.
        //즉, 저속 상황에서는 반대로 회전력이 낮음.
        //기체의 기수(nose) 방향으로 기체가 움직이지 않을 경우(실속 중 하강)도 회전력이 낮음.
        //>> 실속 중 하강하는 경우는 회전력이 감소함 >> 자동 회복을 기다려야 함
        rigidbody.AddTorque(torque * (forwardSpeed * angleDifference));

    }

    void CalculateAngles() {
        xzForward = new Vector3(transform.forward.x, 0, transform.forward.z);
        //pitch == 방향벡터를 xz평면에 투영한 벡터의 길이와 방향벡터의 y값
        pitchAngle = Mathf.Atan2(transform.forward.y, xzForward.magnitude) * Mathf.Rad2Deg;
        xzForward.Normalize();
        //yaw == xz평면 북쪽 벡터와 xz평면 방향벡터 사이의 각 -> -180 ~ 180 사이의 값
        yawAngle = Vector3.SignedAngle(Vector3.forward, xzForward, Vector3.up);

        xzRight = Vector3.Cross(Vector3.up, xzForward);
        var localXZRight = transform.InverseTransformDirection(xzRight);
        rollAngle = Mathf.Atan2(localXZRight.y, localXZRight.x) * Mathf.Rad2Deg;
    }

    void CalculateAltitude() {
        //높이 계산
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        for (int i = 0; i < hits.Length; i++) {
            if (!hits[i].collider.CompareTag("Terrain")) continue;
            altitude = transform.position.y - hits[i].point.y;
            break;
        }
    }

    void CalculateEffects() {
        //pitch 입력이 일정 이상일 때 받음각 효과(양쪽 날개 trail)
        bool angleDif = Mathf.Abs(pitchInput) > attackAngle;
        for (int i = 0; i < attackAngleEffects.Count; i++) {
            attackAngleEffects[i].gameObject.SetActive(angleDif);
        }
        //애프터버너
        for (int i = 0; i < afterburners.Count; i++) {
            var main = afterburners[i].main;
            var curve = main.startSize;
            curve.constant = Mathf.Lerp(afterburnerMin, afterburnerMax, throttle);
            main.startSize = curve;
        }
    }
    void CalculateSounds() {
        float engine = Mathf.InverseLerp(0, m_MaxEnginePower, enginePower);
        if (engineSound) {
            engineAudioSource.pitch = Mathf.Lerp(engineSoundPitchMin, engineSoundPitchMax, engine);
        }
        if (windSound) {
            windAudioSource.pitch = windSoundPitchBase + forwardSpeed * windSoundPitchFactor;
            windAudioSource.volume = Mathf.Lerp(windSoundVolumeMin, windSoundVolumeMax, engine);
        }
    }
}
