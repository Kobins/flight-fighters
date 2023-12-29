using UnityEngine;

public class Area : MonoBehaviour {
    public delegate void AreaEnterEvent(Area area, ArmoredVehicle vehicle);
    public AreaEnterEvent OnAreaEnter;
    private void OnTriggerEnter(Collider other) {
        if (OnAreaEnter != null) {
            ArmoredVehicle vehicle;
            if (other.TryGetComponent(out vehicle)
                || other.transform.parent != null && other.transform.parent.TryGetComponent(out vehicle)
            ) {
                OnAreaEnter(this, vehicle);
                OnAreaEnter = null;
                return;
            }
        }
    }
}
