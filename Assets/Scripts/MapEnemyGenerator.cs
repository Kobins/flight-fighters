using UnityEngine;

public class MapEnemyGenerator : MonoBehaviour {
    private void Awake() {
        gameObject.SetActive(false);
    }

    public string GenerateActionString(int index, string group) {
        return MapActionType.CREATE_ENEMY.Name + "|" + index + "|" + transform.position.x + "|" + transform.position.y + "|" + transform.position.z + "|" + transform.rotation.eulerAngles.y + "|" + group;
    }

}
