using UnityEngine;

public class HUDPitchAndRoll : MonoBehaviour {
    [HideInInspector]
    public Player player;

    [SerializeField] private RectTransform angleListParent = default;
    [SerializeField] private HUDPitchAndRollAngle anglePrefab = default;

    public static readonly float PAR_DISTANCE = 10;

    void Start() {
        for (int i = -90; i <= 90; i += 5) {
            var angle = Instantiate(anglePrefab, angleListParent);
            angle.name = "Angle_" + i;
            angle.SetAngle(i);
        }
    }

    void LateUpdate() {
        if (player == null) return;
        //pitch 회전
        angleListParent.anchoredPosition = new Vector2(0, player.aeroplane.pitchAngle * -PAR_DISTANCE);
        //roll 회전 -> angleListParent의 부모이기 때문에 pitch에는 영향없이 회전
        transform.rotation = Quaternion.Euler(0, 0, player.aeroplane.rollAngle);
    }
}
