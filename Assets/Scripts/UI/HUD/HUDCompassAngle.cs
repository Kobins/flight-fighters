using UnityEngine;

public class HUDCompassAngle : MonoBehaviour {

    new RectTransform transform;

    private void Awake() {
        transform = GetComponent<RectTransform>();
    }

    public void SetAngle(int angle, float angleDistance, Vector2 scale) {
        transform.anchoredPosition = new Vector2(angle * angleDistance, 0);
        transform.sizeDelta = scale;
    }
}
