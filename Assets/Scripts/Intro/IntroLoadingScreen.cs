using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IntroLoadingScreen : MonoBehaviour, IPointerClickHandler {

    public Text mapName;
    public Text mapAuthor;
    public Text mapDescription;
    public Text statusText;
    bool isLoaded = false;

    public void Init(GameMap currentMap) {
        statusText.text = "Loading...";
        isLoaded = false;
        mapName.text = currentMap.displayName;
        mapAuthor.text = currentMap.author;
        mapDescription.text = currentMap.description;
    }

    public void Loaded() {
        isLoaded = true;
        statusText.text = "Click to continue";
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!isLoaded) return;
        gameObject.SetActive(false);
    }
}