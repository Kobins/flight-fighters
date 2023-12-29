using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Events;

public class WaypointHandler : MonoBehaviour {
    [SerializeField] private List<Waypoint> _waypoints = new List<Waypoint>();

    public List<Waypoint> Waypoints => _waypoints;

    public UnityAction OnEnd;
    
    public static int Layer { get; private set; } = 0;
    
    private void Start() {
        var count = _waypoints.Count;
        for (int i = 0; i < count; i++) {
            _waypoints[i].Init(this, i);
            var r = _waypoints[i].GetComponent<MeshRenderer>();
            if(!r || !r.enabled) continue;
            r.enabled = false;
        }

        Layer = LayerMask.NameToLayer("Waypoint");
        
        // 부모는 기본 꺼짐
        gameObject.SetActive(false);
    }


    public int GetWaypointIndex(GameObject target) {
        if ((target.layer & Layer) == 0) {
            return -1;
        }
        var waypoint = target.GetComponent<Waypoint>();
        if (!waypoint || waypoint.Handler != this) {
            return -1;
        }
        return waypoint.Index;
    }

    [ContextMenu("Bind Child Objects")]
    private void Bind() {
        var layer = LayerMask.NameToLayer("Waypoint");
        _waypoints.Clear();
        var children = GetComponentsInChildren<Transform>();
        _waypoints.Capacity = children.Length;
        var thisTransform = transform;
        int index = 0;
        for (int i = 0; i < children.Length; i++) {
            if(children[i] == thisTransform) continue;
            var c = children[i].GetComponent<Collider>();
            if (!c) {
                Debug.LogWarning($"failed to binding child waypoint {children[i].gameObject.name}");
                continue;
            }

            var waypoint = c.GetOrAddComponent<Waypoint>();
            waypoint.Init(this, index);
            _waypoints.Add(waypoint);
            var o = c.gameObject;
            o.name = $"Waypoint_{index}";
            o.layer = layer;
            ++index;
        }
    }

    
    private static Color _startColor = Color.magenta;
    private static Color _endColor = Color.cyan;
    private void OnDrawGizmos() {
        var count = _waypoints.Count;
        if(count <= 1) return;
        
        for (int i = 0; i < count; i++) {
            var from = _waypoints[i];
            var to = _waypoints[(i + 1) % count];
            Gizmos.color = Color.Lerp(_startColor, _endColor, (float)(i) / (count-1));
            Gizmos.DrawLine(from.transform.position, to.transform.position);
        }
    }
}
