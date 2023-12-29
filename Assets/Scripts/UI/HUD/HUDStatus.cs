using UnityEngine;
using UnityEngine.UI;

public class HUDStatus : MonoBehaviour {

    public Player player;

    public Image hp;
    public Image enginePower;

    void Update() {
        if (player == null || !player.gameObject.activeSelf) return;
        hp.rectTransform.localScale = new Vector2(player.aeroplane.hp / player.aeroplane.maxHp, 1f);
        enginePower.rectTransform.localScale = new Vector2(player.aeroplane.enginePower / player.aeroplane.MaxEnginePower, 1f);
    }
}
