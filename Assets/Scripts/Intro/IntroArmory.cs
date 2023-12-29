using UnityEngine;
using UnityEngine.UI;

public class IntroArmory : MonoBehaviour {
    [HideInInspector] public IntroManager introManager;
    public Button prevButton;
    public Button nextButton;
    public Text currentAeroplaneName;

    private void Awake() {
        prevButton.onClick.AddListener(delegate { Prev(); });
        nextButton.onClick.AddListener(delegate { Next(); });
    }

    public void Init(IntroManager introManager) {
        this.introManager = introManager;
        UpdateStatus();
    }

    public void UpdateStatus() {
        int index = introManager.aeroplanes.IndexOf(introManager.selectedAeroplane);
        for (int i = 0; i < introManager.centerAeroplanes.Length; i++) {
            introManager.centerAeroplanes[i].gameObject.SetActive(i == index);
        }
        ValidateButton(index);
    }

    public void Prev() {
        int index = introManager.aeroplanes.IndexOf(introManager.selectedAeroplane);
        if (index <= 0) {
            return;
        }
        var plane = introManager.aeroplanes[index - 1];
        var oldCenterPlane = introManager.centerAeroplanes[index];
        var centerPlane = introManager.centerAeroplanes[index - 1];

        oldCenterPlane.gameObject.SetActive(false);
        centerPlane.gameObject.SetActive(true);
        introManager.selectedAeroplane = plane;

        ValidateButton(--index);
    }

    public void Next() {
        int index = introManager.aeroplanes.IndexOf(introManager.selectedAeroplane);
        if (index >= introManager.aeroplanes.Count - 1) {
            return;
        }
        var plane = introManager.aeroplanes[index + 1];
        var oldCenterPlane = introManager.centerAeroplanes[index];
        var centerPlane = introManager.centerAeroplanes[index + 1];

        oldCenterPlane.gameObject.SetActive(false);
        centerPlane.gameObject.SetActive(true);
        introManager.selectedAeroplane = plane;

        ValidateButton(++index);
    }

    public void ValidateButton(int index) {
        prevButton.gameObject.SetActive(index > 0);
        nextButton.gameObject.SetActive(index < introManager.aeroplanes.Count - 1);
        currentAeroplaneName.text = introManager.selectedAeroplane.name;
    }

}
