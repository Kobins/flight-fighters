using System.Collections;
using Core.VR;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadUpDisplay : MonoBehaviour {

    private Player _player;

    public HUDCompass compassHUD = default;
    public HUDAlert alert = default;
    public Text radioName;
    public Text radioText;

    public void Init(Player player) {
        _player = player;
        compassHUD.player = player;
        alert.player = player;

        radioName.gameObject.SetActive(false);
    }

    public void Radio(string name, string message, float duration) {
        radioName.gameObject.SetActive(true);
        radioName.text = name;
        radioText.text = message;
        StartCoroutine(RadioCoroutine(duration));
    }

    IEnumerator RadioCoroutine(float duration) {
        yield return new WaitForSeconds(duration);
        radioName.gameObject.SetActive(false);
    }
}
