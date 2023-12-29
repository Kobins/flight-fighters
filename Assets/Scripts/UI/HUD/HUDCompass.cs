using UnityEngine;
using UnityEngine.UI;

public class HUDCompass : MonoBehaviour {
    public static readonly float COMPASS_ANGLE_DISTANCE = 10f;

    [HideInInspector]
    public Player player;

    [SerializeField] private Text compassAngle = default;
    [SerializeField] private RectTransform compassParent = default;
    [SerializeField] private HUDCompassAngle compassPrefab = default;
    [SerializeField] private Text compassDirectionPrefab = default;

    void Start() {
        for (int i = -90; i <= 360 + 90; i += 5) {
            var angle = Instantiate(compassPrefab, compassParent);
            angle.name = "Compass_" + i;
            angle.SetAngle(i);
            if (i % 45 == 0) {
                var direction = Instantiate(compassDirectionPrefab, angle.transform);
                direction.rectTransform.anchoredPosition = new Vector2(0, 25);
                direction.text = GetDirection(i);
            }
        }
    }

    string GetDirection(int angle) {
        switch (((angle + 360) % 360)) {
            case -90: return "W";
            case -45: return "NW";

            case 0: return "N";
            case 45: return "NE";
            case 90: return "E";
            case 135: return "SE";
            case 180: return "S";
            case 225: return "SW";
            case 270: return "W";
            case 315: return "NW";
            case 360: return "N";

            case 405: return "NE";
            case 450: return "E";
        }
        return "";
    }
    void LateUpdate() {
        if (player == null) return;
        float angle = (player.aeroplane.yawAngle + 360) % 360;
        compassAngle.text = angle.ToString("0");
        compassParent.anchoredPosition = new Vector2(angle * -COMPASS_ANGLE_DISTANCE, 10);
    }
}
