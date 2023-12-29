using System.Collections.Generic;
using UnityEngine;

public class UIEnemyTarget : MonoBehaviour {
    [HideInInspector] public Player player;
    [HideInInspector] public GameController controller;

    public float minSize = 0.2f;
    public float maxSize = 1.2f;
    public float farDistance = 10000f;
    public float nearDistance = 500f;
    public UIEnemyTargetMarker markerPrefab;
    public Dictionary<Enemy, UIEnemyTargetMarker> marker;

    private void Awake() {
        marker = new Dictionary<Enemy, UIEnemyTargetMarker>();
    }

    public void Init(GameController controller) {
        this.controller = controller;
        player = controller.player;
    }
    void FixedUpdate() {
        UIEnemyTargetMarker mark;
        foreach (var key in controller.enemies.Keys) {
            foreach (var enemy in controller.enemies[key]) {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) {
                    if (marker.ContainsKey(enemy)) {
                        marker[enemy].gameObject.SetActive(false);
                    }
                    marker.Remove(enemy);
                    continue;
                }
                if (!marker.ContainsKey(enemy)) {
                    mark = marker[enemy] = Instantiate(markerPrefab, transform);
                    mark.name = "Mark_" + enemy.name;
                } else {
                    mark = marker[enemy];
                }
                player.aeroplane.currentWeapon.OnMarkerUpdate(mark, enemy);
                var pos = Camera.main.WorldToScreenPoint(enemy.transform.position);
                ////뒤에 있는 경우
                bool isBackCamera = pos.z < 0;
                if (isBackCamera) {
                    pos.x = -1000f;
                    pos.y = -1000f;
                }
                pos.x -= mark.widthRadius;
                pos.y -= mark.heightRadius;
                //적이 완전히 화면 밖으로 벗어난 경우 : 화살표 표시
                if (isBackCamera
                || pos.x + mark.width < 0 || pos.x > Screen.width + mark.widthRadius
                || pos.y + mark.height < 0 || pos.y > Screen.height + mark.heightRadius) {
                    pos.z = -1000;
                    mark.transform.position = pos;

                    //로컬좌표계 방향 중 xy값만 가져오기
                    var local = player.transform.InverseTransformDirection((enemy.transform.position - player.transform.position).normalized);
                    local.z = 0;
                    local.Normalize();
                    mark.arrowImage.rectTransform.position = local * 100 + new Vector3(Screen.width / 2, Screen.height / 2);
                    var angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg - 90;
                    mark.arrowImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
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
                mark.transform.position = pos;
            }
        }
    }
}
