using UnityEngine;
using UnityEngine.UI;

public class UIWeaponSlotImage : MonoBehaviour {
    public RawImage image;
    public Image selectedSquare;
    public Image filledSquare;

    public void SetFilled(float ratio) {
        filledSquare.rectTransform.localScale = new Vector2(1, ratio);
    }

    public void SetFilledColor(Color color) {
        filledSquare.color = color;
    }
}
