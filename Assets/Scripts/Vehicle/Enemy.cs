using UnityEngine;

public class Enemy : MonoBehaviour {

    [HideInInspector] public Player player;
    [HideInInspector] public ArmoredVehicle vehicle;

    private void Awake() {
        foreach (var component in gameObject.GetComponents(typeof(ArmoredVehicle))) {
            vehicle = (ArmoredVehicle)component;
            break;
        }
        if (vehicle == null) {
            gameObject.SetActive(false);
            Debug.LogError("Enemy Object " + name + " doesn't have Entity object");
            return;
        }
    }
}
