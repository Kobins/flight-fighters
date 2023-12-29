using UnityEngine;

public class HUDPitchAndRoll : MonoBehaviour {
    private Aeroplane _aeroplane;

    [SerializeField] private RectTransform angleListParent = default;
    [SerializeField] private HUDPitchAndRollAngle anglePrefab = default;

    public float barDistance = 10;
    public int angleUnit = 5;
    
    private float _scaleFactor;
    
    void Start() {
        for (int i = -90; i <= 90; i += angleUnit) {
            var angle = Instantiate(anglePrefab, angleListParent);
            angle.name = "Angle_" + i;
            angle.SetAngle(i, barDistance);
        }

        _scaleFactor = angleListParent.lossyScale.y;
    }

    public void Init(Aeroplane aeroplane) {
        _aeroplane = aeroplane;
    }

    void LateUpdate() {
        if (!_aeroplane) return;
        //pitch 회전
        angleListParent.anchoredPosition = new Vector2(0, _aeroplane.pitchAngle * -barDistance * _scaleFactor);
        //roll 회전 -> angleListParent의 부모이기 때문에 pitch에는 영향없이 회전
        transform.localRotation = Quaternion.Euler(0, 0, _aeroplane.rollAngle);
    }
}
