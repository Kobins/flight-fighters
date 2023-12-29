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

    const string CLEAR_TEXT = "CLEAR!";
    const string FAILED_TEXT = "MISSION FAILED";

    private void Awake() {
        panelImage = GetComponent<Image>();
        panelColor = panelImage.color;
        panelImage.color = new Color(0, 0, 0, 0);
        clearText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        homeButton.onClick.AddListener(delegate { controller.GoHome(); });
        homeButton.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    public void Init(GameController controller) {
        this.controller = controller;
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
    }

}
