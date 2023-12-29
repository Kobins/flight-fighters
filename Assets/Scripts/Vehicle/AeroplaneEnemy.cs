using UnityEngine;

public class AeroplaneEnemy : Enemy {
    Aeroplane aeroplane;
    ArmoredVehicle target;

    public float minAltitude = 100f;
    public float targetFaceAngle = 150f;

    private void Start() {
        aeroplane = GetComponent<Aeroplane>();
        if (player) target = player.aeroplane;
    }
    
    void Update() {
        if (aeroplane.currentWeapon != null) {
            aeroplane.currentWeapon.Trigger();
        }
        //일정 고도 미만/내 앞에 땅이면 기수 상승
        if (aeroplane.altitude < minAltitude
            || Physics.Raycast(new Ray(transform.position, aeroplane.transform.forward), 3000f, LayerMask.GetMask("Terrain"))
        ) {
            aeroplane.Input(1, 0, 0, 1f);
            return;
        }
        if (aeroplane.landingGear) {
            aeroplane.LandingGear(false);
        }
        //목표 없으면 수평유지/뺑뺑돌기
        if (target == null || !target.gameObject.activeInHierarchy) {
            aeroplane.Input(0, 1, 0, -aeroplane.pitchAngle / 90);
            return;
        }

        var targetTransform = target.transform;
        var targetPosition = targetTransform.position;
        var position = aeroplane.rigidbody.transform.position;
        float pitch;
        //목표보다 높이 있을 때 기수 하강
        if (position.y > targetPosition.y) {
            pitch = -0.5f;
        }
        //목표보다 낮게 있을 때 기수 상승
        else if (position.y < targetPosition.y) {
            pitch = 0.5f;
        } else {
            pitch = -aeroplane.pitchAngle / 90;
        }

        float roll = -aeroplane.rollAngle / 90;
        var xzForward = aeroplane.xzForward;
        var targetXZForward = targetTransform.forward.ProjectedXZPlane().normalized;
        float toTargetAngle = Vector3.SignedAngle(
            xzForward,
            (targetPosition - position).ProjectedXZPlane().normalized,
            Vector3.up
        );
        // 너무 서로를 노려보고 있으면 안되므로 ...
        if (Mathf.Abs(toTargetAngle) <= 180f - targetFaceAngle) {
            var forwardAngle = Vector3.SignedAngle(xzForward, targetXZForward, Vector3.up);
            if (forwardAngle < -targetFaceAngle) {
                aeroplane.Input(1, -0.5f, roll, pitch);
                return;
            }else if (forwardAngle > targetFaceAngle) {
                aeroplane.Input(1, 0.5f, roll, pitch);
                return;
            }
        }
        
        //-180 ~ 180
        float yaw;
        if (toTargetAngle < 0) { //왼쪽으로
            yaw = -0.5f;
        } else if (toTargetAngle > 0) { //오른쪽으로
            yaw = +0.5f;
        } else {
            yaw = 0;
        }
        aeroplane.Input(1, yaw, roll, pitch);
    }
}
