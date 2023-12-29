using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDRadar : MonoBehaviour {
    [HideInInspector]
    public Player player;

    public string[] directions = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

    public float maxRange = 4000f;
    float radarRadius;
    public Text directionMarkerPrefab;
    public Image enemyMarkerPrefab;
    public Image areaMarkerPrefab;

    public List<Text> directionMarkers;
    public Dictionary<Transform, Image> markers;

    private void Awake() {
        var rect = GetComponent<RectTransform>();
        radarRadius = rect.sizeDelta.x / 2;
        directionMarkers = new List<Text>();
        for (int i = 0; i < directions.Length; i++) {
            var mark = Instantiate(directionMarkerPrefab, transform);
            mark.name = directions[i];
            mark.text = directions[i];
            directionMarkers.Add(mark);
        }
        markers = new Dictionary<Transform, Image>();
    }
    float yaw;
    void Update() {
        yaw = (player.aeroplane.yawAngle + 360) % 360;
        //8방위
        for (int i = 0; i < directionMarkers.Count; i++) {
            var xzDelta = Vector3.forward;
            xzDelta.RotateAroundY(i * -45);
            xzDelta.RotateAroundY(yaw);
            var position = new Vector2(xzDelta.x, xzDelta.z) * radarRadius;
            directionMarkers[i].rectTransform.anchoredPosition = position;
        }
        //마커 청소
        var toRemove = new List<Transform>();
        foreach (var target in markers.Keys) {
            if (target == null || !target.gameObject.activeInHierarchy) {
                if (target != null) {
                    markers[target].gameObject.SetActive(false);
                }
                toRemove.Add(target);
            }
        }
        for (int i = 0; i < toRemove.Count; i++) {
            markers.Remove(toRemove[i]);
        }
        //구역
        var areas = new List<Area>(player.controller.map.areaPalette);
        areas.RemoveAll((a) =>
            a == null
            || !a.gameObject.activeInHierarchy
        );
        IterateMarker(areas, areaMarkerPrefab);
        //차량
        var vehicles = new List<ArmoredVehicle>(ArmoredVehicle.Vehicles);
        vehicles.RemoveAll((e) =>
            e == null
            || e.Equals(player.aeroplane)
            || !e.gameObject.activeInHierarchy
        );
        IterateMarker(vehicles, enemyMarkerPrefab);
    }

    public void IterateMarker<T>(List<T> objects, Image markerPrefab) where T : MonoBehaviour {
        Image marker;
        for (int i = 0; i < objects.Count; i++) {
            if (!markers.ContainsKey(objects[i].transform)) {
                marker = Instantiate(markerPrefab, transform);
                markers[objects[i].transform] = marker;
            } else {
                marker = markers[objects[i].transform];
            }
            SetMarkerPosition(objects[i].transform, marker);
        }
    }

    public void SetMarkerPosition(Transform target, Image marker) {
        var deltaPosition = (target.position - player.transform.position);
        var xzDelta = deltaPosition.ProjectedXZPlane();
        xzDelta.RotateAroundY(yaw); // 벡터를 원점 기준으로 y축으로 yaw만큼 회전변환
        xzDelta = Vector3.ClampMagnitude(xzDelta, maxRange); // 길이 제한
        var position = new Vector2(xzDelta.x, xzDelta.z) / maxRange * radarRadius;
        marker.rectTransform.anchoredPosition = position;
        var xzForward = target.forward.ProjectedXZPlane().normalized;
        xzForward.RotateAroundY(yaw - 90); //화살표 이미지가 위쪽을 보기 때문에
        marker.rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(xzForward.z, xzForward.x) * Mathf.Rad2Deg);
    }
}