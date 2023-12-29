using System;
using System.Collections.Generic;
using UnityEngine;

public class UIEnemyTarget : MonoBehaviour {
    [HideInInspector] public Player player;
    [HideInInspector] public GameController controller;

    public float minSize = 0.2f;
    public float maxSize = 1.2f;
    public float farDistance = 10000f;
    public float nearDistance = 500f;
    public float offScreenRadius = 1;
    public UIEnemyTargetMarker markerPrefab;
    public Dictionary<Enemy, UIEnemyTargetMarker> marker;

    private Camera _camera;
    private Canvas _canvas;
    private RectTransform _rectTransform;
    private float _projectionDistance;
    private Vector3 _inversedScale;
    private Rect _rect;
    
    private void Awake() {
        marker = new Dictionary<Enemy, UIEnemyTargetMarker>();
    }

    private void Start() {
        _camera = Camera.main;
        _canvas = GetComponent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        _projectionDistance = (_camera.transform.position - _canvas.transform.position).magnitude;
        var scale = _rectTransform.localScale;
        _inversedScale = new Vector3(
            1f / scale.x,
            1f / scale.y,
            1f / scale.z
        );
        var originalRect = _rectTransform.rect;
        _rect = new Rect(originalRect);
        var center = new Vector2(0, 0);
        _rect.xMin = center.x - (originalRect.width / 2) * scale.x;
        _rect.xMax = center.x + (originalRect.width / 2) * scale.x;
        _rect.yMin = center.y - (originalRect.height / 2) * scale.y;
        _rect.yMax = center.y + (originalRect.height / 2) * scale.y;
    }

    public void Init(GameController controller) {
        this.controller = controller;
        player = controller.player;
    }
    void FixedUpdate() {
        UIEnemyTargetMarker mark;
        var cameraTransform = _camera.transform;
        var cameraForward = cameraTransform.forward;
        var cameraRight = cameraTransform.right;
        var cameraUp = cameraTransform.up;
        foreach (var key in controller.enemies.Keys) {
            foreach (var enemy in controller.enemies[key]) {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) {
                    if (marker.ContainsKey(enemy)) {
                        marker[enemy].gameObject.SetActive(false);
                        marker[enemy].arrowImage.gameObject.SetActive(false);
                    }
                    marker.Remove(enemy);
                    continue;
                }
                if (!marker.ContainsKey(enemy)) {
                    mark = marker[enemy] = Instantiate(markerPrefab, transform);
                    mark.name = "Mark_" + enemy.name;
                    var arrowTransform = mark.arrowImage.transform;
                    arrowTransform.SetParent(_rectTransform);
                    arrowTransform.localScale = Vector3.one;
                } else {
                    mark = marker[enemy];
                }
                player.aeroplane.currentWeapon.OnMarkerUpdate(mark, enemy);
                var cameraToEnemy = enemy.transform.position - cameraTransform.position;
                // 카메라 중심으로, 캔버스에 투영
                var depth = cameraForward.Dot(cameraToEnemy);
                var x = cameraRight.Dot(cameraToEnemy); 
                var y = cameraUp.Dot(cameraToEnemy); 
                var multiplier = _projectionDistance / depth;
                var pos = new Vector3(
                    x * multiplier,
                    y * multiplier,
                    depth
                );
                // Debug.Log($"pos: {pos} / x: {x}, y: {y}, z: {depth}");
                ////뒤에 있는 경우
                bool isBackCamera = pos.z < 0;
                // if (isBackCamera) {
                    // pos.x = -1000f;
                    // pos.y = -1000f;
                // }
                // pos.x -= mark.widthRadius; // TODO
                // pos.y -= mark.heightRadius;
                //적이 완전히 화면 밖으로 벗어난 경우 : 화살표 표시
                if (isBackCamera
                || pos.x + mark.widthRadius < _rect.xMin || pos.x > _rect.xMax + mark.widthRadius
                || pos.y + mark.heightRadius < _rect.yMin || pos.y > _rect.yMax + mark.heightRadius) {
                    pos.z = -1000;
                    mark.transform.localPosition = pos;

                    //로컬좌표계 방향 중 xy값만 가져오기
                    Vector3 local = cameraTransform.InverseTransformDirection(cameraToEnemy.normalized); 
                    local.z = 0; local.Normalize();
                    mark.arrowImage.rectTransform.localPosition = local * offScreenRadius;
                    var angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg - 90;
                    mark.arrowImage.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
                    if (!mark.arrowImage.gameObject.activeInHierarchy) {
                        mark.arrowImage.gameObject.SetActive(true);
                    }
                    continue;
                }
                if (mark.arrowImage.gameObject.activeInHierarchy) {
                    mark.arrowImage.gameObject.SetActive(false);
                }
                float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                mark.SetDistance(distance);
                float sizeFactor = Mathf.Lerp(minSize, maxSize, (1 - Mathf.InverseLerp(nearDistance, farDistance, distance)));
                mark.transform.localScale = Vector2.one * sizeFactor;
                pos.z = 0;
                mark.transform.localPosition = pos.MultiplyEach(_inversedScale);
            }
        }
    }
}
