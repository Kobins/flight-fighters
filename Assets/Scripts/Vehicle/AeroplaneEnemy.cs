using UnityEngine;

public class AeroplaneEnemy : Enemy {
    Aeroplane aeroplane;
    ArmoredVehicle target;

    public float minAltitude = 100f;

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
        var position = aeroplane.rigidbody.transform.position;
        float pitch;
        //목표보다 높이 있을 때 기수 하강
        if (position.y > target.transform.position.y) {
            pitch = -0.5f;
        }
        //목표보다 낮게 있을 때 기수 상승
        else if (position.y < target.transform.position.y) {
            pitch = 0.5f;
        } else {
            pitch = -aeroplane.pitchAngle / 90;
        }
        //-180 ~ 180
        float angle = Vector3.SignedAngle(
            aeroplane.xzForward,
            (target.transform.position - position).ProjectedXZPlane().normalized,
            Vector3.up
        );
        float yaw;
        if (angle < 0) { //왼쪽으로
            yaw = -1;
        } else if (angle > 0) { //오른쪽으로
            yaw = +1;
        } else {
            yaw = 0;
        }
        aeroplane.Input(1, yaw, -aeroplane.rollAngle / 90, pitch);
    }
}
