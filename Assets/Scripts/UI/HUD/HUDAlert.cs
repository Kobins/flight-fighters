using UnityEngine;
using UnityEngine.UI;

public class HUDAlert : MonoBehaviour {

    [HideInInspector]
    public Player player;

    private Text message;
    private Image border;

    private void Start() {
        border = GetComponentInChildren<Image>();
        message = GetComponent<Text>();
        gameObject.SetActive(false);
    }

    public void Alert(string text, float duration, Color color) {
        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
        }
        t = duration;
        message.text = text;
        message.color = color;
        border.color = color;
    }

    float t;
    private void Update() {
        if (t > 0) {
            t -= TimeManager.deltaTime;
        }
        if (t <= 0) {
            gameObject.SetActive(false);
        }
    }

}
