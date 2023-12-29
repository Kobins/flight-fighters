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

    public void SetAngle(int angle, float distance) {
        angle = -angle;
        transform.anchoredPosition = new Vector2(0, angle * distance);
        leftAngleText.text = angle.ToString();
        rightAngleText.text = angle.ToString();
        //위로 꺾이게
        if (angle > 0) { 
            leftAngled.localRotation = Quaternion.Euler(0, 0, -(float)angle / 2);
            rightAngled.localRotation = Quaternion.Euler(0, 0, (float)angle / 2);
        }
        //수평
        else if (angle == 0) { 
            //수직 막대기 세로크기 0
            leftVertical.sizeDelta = new Vector2(1, 0);
            rightVertical.sizeDelta = new Vector2(1, 0);
        }
        //아래로 꺾이게 
        else { 
            leftAngled.localRotation = Quaternion.Euler(0, 0, (float)angle / 2);
            rightAngled.localRotation = Quaternion.Euler(0, 0, -(float)angle / 2);
            //수직 막대기 상하반전
            leftVertical.localRotation = Quaternion.Euler(180, 0, 0);
            rightVertical.localRotation = Quaternion.Euler(180, 0, 0);
        }
    }
}
