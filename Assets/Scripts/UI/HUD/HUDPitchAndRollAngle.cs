using UnityEngine;
using UnityEngine.UI;

public class HUDPitchAndRollAngle : MonoBehaviour {
    new RectTransform transform;
    [SerializeField] private Text leftAngleText = default;
    [SerializeField] private Text rightAngleText = default;
    [SerializeField] private RectTransform leftAngled = default;
    [SerializeField] private RectTransform rightAngled = default;

    [SerializeField] private RectTransform leftVertical = default;
    [SerializeField] private RectTransform rightVertical = default;

    private void Awake() {
        transform = GetComponent<RectTransform>();
    }

    public void SetAngle(int angle) {
        angle = -angle;
        transform.anchoredPosition = new Vector2(0, angle * HUDPitchAndRoll.PAR_DISTANCE);
        leftAngleText.text = angle.ToString();
        rightAngleText.text = angle.ToString();
        if (angle > 0) { //위로 꺾이게
            leftAngled.rotation = Quaternion.Euler(0, 0, -(float)angle / 2);
            rightAngled.rotation = Quaternion.Euler(0, 0, (float)angle / 2);
        } else if (angle == 0) { //수평
            //수직 막대기 세로크기 0
            leftVertical.sizeDelta = new Vector2(1, 0);
            rightVertical.sizeDelta = new Vector2(1, 0);
        } else if (angle < 0) { //아래로 꺾이게
            leftAngled.rotation = Quaternion.Euler(0, 0, (float)angle / 2);
            rightAngled.rotation = Quaternion.Euler(0, 0, -(float)angle / 2);
            //수직 막대기 상하반전
            leftVertical.rotation = Quaternion.Euler(180, 0, 0);
            rightVertical.rotation = Quaternion.Euler(180, 0, 0);
        }
    }
}
