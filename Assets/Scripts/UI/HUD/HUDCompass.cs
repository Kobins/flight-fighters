using Core.VR;
using UnityEngine;
using UnityEngine.UI;

public class HUDCompass : MonoBehaviour {
    public float compassAngleDistance = 10f;

    public Vector2 normalSize = new Vector2(0.1f, 1f);
    public Vector2 bigSize = new Vector2(0.1f, 2f);
    public Vector2 direction8Position = new Vector2(0f, 0f);
    public float compassParentPositionY = 1f;
    
    [HideInInspector] public Player player;

    [SerializeField] private Text compassAngle = default;
    [SerializeField] private RectTransform compassParent = default;
    [SerializeField] private HUDCompassAngle compassPrefab = default;
    [SerializeField] private Text compassDirectionPrefab = default;

    private Camera _camera;
    
    void Start() {
        _camera = Camera.main;
        for (int i = -90; i <= 360 + 90; i += 5) {
            var angle = Instantiate(compassPrefab, compassParent);
            angle.name = "Compass_" + i;
            bool isBig = i % 45 == 0;
            angle.SetAngle(i, compassAngleDistance, isBig ? bigSize : normalSize);
            if (isBig) {
                var direction = Instantiate(compassDirectionPrefab, angle.transform);
                direction.rectTransform.anchoredPosition = direction8Position;
                direction.text = GetDirection(i);
            }
        }
    }

    string GetDirection(int angle) {
        switch ((angle + 360) % 360) {
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
        float angle = (_camera.transform.forward.DirectionToYawInDegrees() + 360) % 360;
        compassAngle.text = angle.ToString("0");
        compassParent.anchoredPosition = new Vector2(angle * -compassAngleDistance, compassParentPositionY);
    }
}
