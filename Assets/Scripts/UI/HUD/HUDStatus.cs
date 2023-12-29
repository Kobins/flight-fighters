using UnityEngine;
using UnityEngine.UI;

public class HUDStatus : MonoBehaviour {


    public Image hp;
    public Image enginePower;

    private Player _player;

    public void Init(Player player) {
        _player = player;
    }
    void Update() {
        if (_player == null || !_player.gameObject.activeSelf) return;
        hp.rectTransform.localScale = new Vector2(_player.aeroplane.hp / _player.aeroplane.maxHp, 1f);
        enginePower.rectTransform.localScale = new Vector2(_player.aeroplane.enginePower / _player.aeroplane.MaxEnginePower, 1f);
    }
}
