using UnityEngine;

namespace Core {
    public class Waypoint : MonoBehaviour {
        public WaypointHandler Handler { get; private set; } = null;
        public int Index { get; private set; } = -1;

        public void Init(WaypointHandler handler, int index) {
            Handler = handler;
            Index = index;
        }
    }
}