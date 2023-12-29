using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum IntroStatus {
    MAIN,
    SELECT_MAP,
    ARMORY,
    HOW_TO_PLAY
}

public class IntroManager : MonoBehaviour {


    // public IntroCameraController cameraController;
    public IntroStatus status = IntroStatus.MAIN;

    [Header("Background")]
    public GameObject centerAeroplaneParent;
    [HideInInspector] public Aeroplane[] centerAeroplanes;

    [Header("Title UI Parents")]
    public GameObject titleButton;
    public GameObject mapScrolls;
    public GameObject howToPlay;

    [Header("Bottom Title Buttons")]
    public Button startButton;
    public Button armoryButton;
    public Button howToPlayButton;

    [Header("Bottom Back Button")]
    public GameObject backButtonParent;
    public Button backButton;

    [Header("Map")]
    public GameMap[] maps;
    public IntroMapButton mapButtonPrefab;
    public RectTransform mapButtonParent;
    public GameMap selectedMap;

    [Header("Aeroplanes")]
    public Aeroplane defaultAeroplane;
    public List<Aeroplane> aeroplanes;
    public IntroArmory armory;
    public Aeroplane selectedAeroplane;

    [Header("Loading Screeen")]
    public IntroLoadingScreen loadingScreenCanvas;


    void Start() {
        status = IntroStatus.MAIN;
        for (int i = 0; i < maps.Length; i++) {
            var button = Instantiate(mapButtonPrefab, mapButtonParent);
            var map = maps[i];
            button.SetMap(map);
            button.GetComponent<Button>().onClick.AddListener(delegate { BtnSelectMap(map); });
        }
        centerAeroplanes = new Aeroplane[aeroplanes.Count];
        for (int i = 0; i < aeroplanes.Count; i++) {
            var planePrefab = aeroplanes[i];
            var centerPlane = Instantiate(planePrefab, centerAeroplaneParent.transform);
            centerPlane.transform.position = new Vector3(0, 0, 0);
            centerPlane.rigidbody.useGravity = false;
            centerPlane.gameObject.SetActive(false);
            foreach (var audioSource in centerPlane.GetComponents<AudioSource>()) {
                audioSource.Stop();
                audioSource.volume = 0f;
            }
            centerAeroplanes[i] = centerPlane;
        }
        GameObject gameControllerObj = GameObject.Find("GameManager");
        if (gameControllerObj != null) {
            GameController controller = gameControllerObj.GetComponent<GameController>();
            selectedAeroplane = controller.selectedAeroplane;
            Destroy(gameControllerObj);
        } else {
            selectedAeroplane = defaultAeroplane;
        }
        // cameraController.Init(this);
        armory.Init(this);

        startButton.onClick.AddListener(delegate { BtnStart(); });
        armoryButton.onClick.AddListener(delegate { BtnArmory(); });
        howToPlayButton.onClick.AddListener(delegate { BtnHowToPlay(); });
        backButton.onClick.AddListener(delegate { BtnBack(); });
    }

    void Update() {
        if (Input.GetKey(KeyCode.Escape)) {
            BtnBack();
        }
    }

    public void BtnSelectMap(GameMap map) {
        DontDestroyOnLoad(loadingScreenCanvas);
        loadingScreenCanvas.gameObject.SetActive(true);
        loadingScreenCanvas.Init(map);
        selectedMap = map;
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Game");
    }

    public void BtnSelectAeroplane(Aeroplane plane) {
        selectedAeroplane = plane;
    }

    public void BtnStart() {
        titleButton.SetActive(false);
        mapScrolls.SetActive(true);
        backButtonParent.SetActive(true);
    }

    public void BtnArmory() {
        titleButton.SetActive(false);
        armory.gameObject.SetActive(true);
        backButtonParent.SetActive(true);
    }

    public void BtnHowToPlay() {
        titleButton.SetActive(false);
        howToPlay.SetActive(true);
        backButtonParent.SetActive(true);
    }

    public void BtnBack() {
        titleButton.SetActive(true);
        armory.gameObject.SetActive(false);
        mapScrolls.SetActive(false);
        howToPlay.SetActive(false);
        backButtonParent.SetActive(false);
    }
}
