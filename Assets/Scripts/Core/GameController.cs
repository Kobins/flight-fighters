using System.Collections;
using System.Collections.Generic;
using Core.VR;
using UI.VR;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    [HideInInspector] public Player player;
    [HideInInspector] public GameMap map;
    [HideInInspector] public Aeroplane selectedAeroplane;
    [HideInInspector] public GameMap selectedMap;

    [Header("Enemy Pool")]
    public EnemyPool enemyPool;

    [Header("Sounds")]
    public AudioClip lockOnSound;
    public AudioClip lockOnAlertSound;

    [Header("Default Values")]
    [SerializeField] private Aeroplane defaultAeroplane = default;
    [SerializeField] private GameMap defaultMap = default;

    [Header("Camera")]
    public VRCameraController cameraController;

    private Camera _camera;
    private VRCameraPointer _cameraPointer;

    [Header("UI")]
    public UIHeadUpDisplay headUpDisplay;
    public WeaponSlotView weaponSlotView;
    public UIEnemyTarget enemyTarget;
    public UIGameEndScreen gameEndScreen;

    void Start() {
        //이전 씬에서 넘어온 정보 받기
        Aeroplane plane;
        GameObject introManagerObj = GameObject.Find("IntroManager");
        if (introManagerObj == null) { //없으면 기본 맵 / 전투기
            selectedAeroplane = defaultAeroplane;
            selectedMap = defaultMap;
        } else {
            IntroManager introManager = introManagerObj.GetComponent<IntroManager>();
            selectedAeroplane = introManager.selectedAeroplane;
            selectedMap = introManager.selectedMap;
            Destroy(introManagerObj.gameObject);
        }
        plane = Instantiate(selectedAeroplane);
        plane.name = selectedAeroplane.name;
        map = Instantiate(selectedMap);
        map.name = selectedMap.name;
        map.gameObject.SetActive(true);
        player = plane.gameObject.AddComponent<Player>();
        player.Init(this, plane);
        plane.transform.position = map.planeStartPosition.position;
        plane.transform.rotation = map.planeStartPosition.rotation;

        //초기화
        enemyPool.Init(this);
        cameraController.player = player;
        _camera = Camera.main;
        _cameraPointer = _camera.GetComponent<VRCameraPointer>();
        _cameraPointer.Reticle.gameObject.SetActive(false);
        headUpDisplay.Init(player);
        if(weaponSlotView)
            weaponSlotView.Init(plane);
        enemyTarget.Init(this);
        gameEndScreen.Init(this);

        for (int i = 0; i < map.areaPalette.Count; i++) {
            map.areaPalette[i].gameObject.SetActive(false);
        }

        ended = false;
        time = 0f;
        waitFor = 0f;
        waitForCondition = false;
        actionIterator = map.actions.GetEnumerator();
        enemies = new Dictionary<string, List<Enemy>>();
        TimeManager.timeScale = 1f;

        loaded = false;
        StartCoroutine(StartWaitCoroutine());
    }


    private IEnumerator StartWaitCoroutine() {
        //이전 씬에서 넘어온 로딩 패널 대기
        GameObject screenObject = GameObject.Find("LoadingScreenCanvas");
        if (screenObject != null) {
            IntroLoadingScreen screen = screenObject.GetComponentInChildren<IntroLoadingScreen>();
            screen.Loaded();
            yield return new WaitWhile(() => screenObject.activeInHierarchy);
        }
        loaded = true;
    }

    private IEnumerator<MapAction> actionIterator;

    public bool ended;
    private bool loaded;
    public float time;
    float waitFor;
    bool waitForCondition;
    public Dictionary<string, List<Enemy>> enemies;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ForceGameEnd();
        }
        if (!loaded) {
            return;
        }
        if (ended) { return; }
        time += TimeManager.deltaTime;
        if (waitFor > 0f) {
            waitFor -= TimeManager.deltaTime;
            return;
        }
        if (waitForCondition) { return; }
        if (!actionIterator.MoveNext()) { return; }
        actionIterator.Current.Execute(this);
    }
    public void Wait(float delay) { waitFor = delay; }
    public void WaitFor(IEnumerator coroutine) {
        waitForCondition = true;
        StartCoroutine(coroutine);
    }
    public IEnumerator WaitForTakeOff(float altitude) {
        yield return new WaitWhile(() => {
            return player.aeroplane.altitude < altitude;
        });
        waitForCondition = false;
    }
    public IEnumerator WaitForEnemyKill(string group) {
        yield return new WaitWhile(() => {
            if (!enemies.ContainsKey(group) || enemies[group].Count <= 0) {
                return false;
            }
            for (int i = 0; i < enemies[group].Count; i++) {
                if (enemies[group][i] != null && enemies[group][i].gameObject.activeInHierarchy) {
                    return true;
                }
            }
            return false;
        });
        waitForCondition = false;
    }
    public IEnumerator WaitForEnterArea(int index) {
        var area = map.areaPalette[index];
        bool entered = false;
        area.OnAreaEnter = (a, v) => {
            entered = true;
        };
        yield return new WaitUntil(() => {
            return entered;
        });
        waitForCondition = false;
    }
    public IEnumerator WaitForWaypointEnd(int index, bool nearest) {
        var waypoint = map.waypoints[index];
        bool end = false;
        player.SetWaypoint(waypoint, nearest, false, () => {
            end = true;
        });
        yield return new WaitUntil(() => {
            return end;
        });
        waitForCondition = false;
    }

    public void RepeatWaypoint(int index, bool nearest) {
        var waypoint = map.waypoints[index];
        player.SetWaypoint(waypoint, nearest, true);
    }
    public void GenerateEnemy(int index, float x, float y, float z, float yaw, string group = "default") {
        var enemy = enemyPool.GetEnemy(index);
        enemy.transform.position = new Vector3(x, y, z);
        enemy.transform.rotation = Quaternion.Euler(0, yaw, 0);
        enemy.player = player;
        if (!enemies.ContainsKey(group)) { enemies.Add(group, new List<Enemy>()); }
        enemies[group].Add(enemy);
    }
    public void DestroyAllEnemy() {
        foreach (var group in enemies.Keys) {
            DestroyEnemy(group);
        }
    }
    public void DestroyEnemy(string group) {
        foreach (var enemy in enemies[group]) {
            Destroy(enemy.gameObject);
        }
    }
    public void EnableArea(int index) {
        map.areaPalette[index].gameObject.SetActive(true);
    }
    public void DisableArea(int index) {
        map.areaPalette[index].gameObject.SetActive(false);
    }
    public void Radio(string name, string text, float duration) {
        headUpDisplay.Radio(name, text, duration);
    }

    [ContextMenu("Force Game End")]
    private void ForceGameEnd() => GameEnd(false);
    
    public void GameEnd(bool isClear) {
        ended = true;
        headUpDisplay.gameObject.SetActive(false);
        enemyTarget.gameObject.SetActive(false);
        _cameraPointer.Reticle.gameObject.SetActive(true);
        gameEndScreen.gameObject.SetActive(true);
        gameEndScreen.GameEnd(isClear);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var missiles = new List<LockOnMissile>(LockOnMissile.Missiles);
        foreach (var missile in missiles) {
            missile.gameObject.SetActive(false);
        }
        LockOnMissile.Missiles.Clear();
    }
    public void GoHome() {
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Intro");
    }
}
