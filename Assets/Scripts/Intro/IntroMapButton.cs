using UnityEngine;
using UnityEngine.UI;

public class IntroMapButton : MonoBehaviour {

    GameMap map;
    public Image thumbnail;
    public Text displayName;
    public Text author;

    public void SetMap(GameMap map) {
        thumbnail.sprite = map.thumbnail;
        displayName.text = map.displayName;
        author.text = map.author;
    }
}
