using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIHeadUpDisplay : MonoBehaviour {

    private Player player;

    [Header("Top")]
    public HUDCompass compassHUD = default;
    public HUDAlert alert = default;
    public Text radioName;
    public Text radioText;
    [Header("Center")]
    public HUDPitchAndRoll pitchAndRollHUD = default;
    public HUDStatus status = default;
    [Header("Bottom")]
    public HUDRadar radar = default;
    public Text altitude = default;
    public Text speed = default;
    public Text landingGear = default;

    public void SetPlayer(Player player) {
        this.player = player;
        pitchAndRollHUD.player = player;
        compassHUD.player = player;
        status.player = player;
        radar.player = player;
        alert.player = player;

        radioName.gameObject.SetActive(false);
    }

    void LateUpdate() {
        altitude.text = ((int)player.aeroplane.altitude).ToString();
        speed.text = ((int)player.aeroplane.rigidbody.velocity.magnitude).ToString();
        landingGear.text = player.aeroplane.landingGear ? "OPENED" : "CLOSED";
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
