using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGameEndScreen : MonoBehaviour {
    [HideInInspector] public GameController controller;
    private Image panelImage;
    public Color panelColor;

    [Header("Buttons")]
    public Button homeButton;

    [Header("Left")]
    public Text clearText;
    public Text descriptionText;

    [Header("Right")]
    public Text mapNameText;
    public Text mapAuthorText;
    public Text aeroplaneText;
    public Text elapsedTimeText;
    public Text elapsedTimeValueText;

    const string CLEAR_TEXT = "CLEAR!";
    const string FAILED_TEXT = "MISSION FAILED";

    private void Awake() {
        panelImage = GetComponent<Image>();
        panelColor = panelImage.color;
        panelImage.color = new Color(0, 0, 0, 0);
        clearText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        mapNameText.gameObject.SetActive(false);
        mapAuthorText.gameObject.SetActive(false);
        aeroplaneText.gameObject.SetActive(false);
        elapsedTimeText.gameObject.SetActive(false);
        elapsedTimeValueText.gameObject.SetActive(false);
        homeButton.onClick.AddListener(delegate { controller.GoHome(); });
        homeButton.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    public void Init(GameController controller) {
        this.controller = controller;
        mapNameText.text = controller.map.displayName;
        mapAuthorText.text = controller.map.author;
        aeroplaneText.text = controller.selectedAeroplane.name;
    }
    public void SetDescription(string text) {
        descriptionText.text = text;
    }

    public void GameEnd(bool isClear) {
        clearText.text = isClear ? CLEAR_TEXT : FAILED_TEXT;
        StartCoroutine(GameEndCoroutine());
    }
    const float epsilon = 0.01f;
    IEnumerator GameEndCoroutine() {
        while (true) {
            TimeManager.timeScale = Mathf.Lerp(TimeManager.timeScale, 0f, 10f * Time.deltaTime);
            panelImage.color = Color.Lerp(panelImage.color, panelColor, 10f * Time.deltaTime);
            if (panelImage.color.IsSimilar(panelColor)) {
                break;
            }
            yield return null;
        }
        TimeManager.timeScale = 0f;
        panelImage.color = panelColor;
        clearText.gameObject.SetActive(true);
        descriptionText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        homeButton.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        mapNameText.gameObject.SetActive(true);
        mapAuthorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        aeroplaneText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        elapsedTimeText.gameObject.SetActive(true);
        elapsedTimeValueText.text = "00:00:00";
        elapsedTimeValueText.gameObject.SetActive(true);
        float time = 0f;
        while (true) {
            time = Mathf.Lerp(time, controller.time, 0.1f);
            elapsedTimeValueText.text = GetTimeText(time);
            if (controller.time - time <= epsilon) {
                break;
            }
            yield return null;
        }
    }

    string GetTimeText(float time) {
        int hour = (int)(time / 3600);
        int min = (int)(time / 60) % 60;
        int sec = (int)(time % 60);
        return hour.ToString("00") + ":" + min.ToString("00") + ":" + sec.ToString("00");
    }

}
