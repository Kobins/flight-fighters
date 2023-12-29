using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIEnemyTargetMarker : MonoBehaviour {

    [HideInInspector] public Image image;
    public Text distanceText;
    public Image arrowImage;

    [HideInInspector] public new RectTransform transform;


    [HideInInspector] public float width, height;
    [HideInInspector] public float widthRadius, heightRadius;

    [HideInInspector] public float arrowWidth, arrowHeight;
    [HideInInspector] public float arrowWidthRadius, arrowHeightRadius;

    public Color GetColor() {
        return image.color;
    }

    public void SetColor(Color color) {
        image.color = color;
        distanceText.color = color;
        arrowImage.color = color;
    }

    public void SetDistance(float distance) {
        distanceText.text = ((int)distance).ToString();
    }

    private void Awake() {
        transform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        width = transform.rect.width;
        height = transform.rect.height;
        widthRadius = width / 2;
        heightRadius = height / 2;

        arrowWidth = arrowImage.rectTransform.rect.width;
        arrowHeight = arrowImage.rectTransform.rect.height;
        arrowWidthRadius = arrowWidth / 2;
        arrowHeightRadius = arrowHeight / 2;
    }
}