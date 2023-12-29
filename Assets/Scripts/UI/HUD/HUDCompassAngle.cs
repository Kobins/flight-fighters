using UnityEngine;

public class HUDCompassAngle : MonoBehaviour {

    new RectTransform transform;

    private void Awake() {
        transform = GetComponent<RectTransform>();
    }

    public void SetAngle(int angle) {
        transform.anchoredPosition = new Vector2(angle * HUDCompass.COMPASS_ANGLE_DISTANCE, 0);
        if (angle % 45 != 0) {
            transform.sizeDelta = new Vector2(2, 10);
        } else {
            transform.sizeDelta = new Vector2(2, 20);
        }
    }
}
