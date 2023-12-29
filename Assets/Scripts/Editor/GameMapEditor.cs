using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameMap))]
public class GameMapEditor : Editor {
    public MapEnemyGenerator generatorPrefab;

    public GameMap selected;
    private void OnEnable() {
        // target은 Editor에 있는 변수로 선택한 오브젝트를 받아옴.
        if (AssetDatabase.Contains(target)) {
            selected = null;
        } else {
            // target은 Object형이므로 Enemy로 형변환
            selected = (GameMap)target;
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (selected == null)
            return;
        EditorGUILayout.Space();
        var cmd = EditorGUIUtility.systemCopyBuffer;
        //Debug.Log("cmd: " + cmd);
        bool valid = false;
        string name = "", group = "default";
        float x = 0f, y = 0f, z = 0f, yaw = 0f;
        if (cmd != null && cmd.Length > 0) {
            var split = cmd.Split('|');
            if (split.Length >= MapActionType.CREATE_ENEMY.minimumParamLength) {
                name = selected.enemyPalette[int.Parse(split[1])].name;
                x = float.Parse(split[2]);
                y = float.Parse(split[3]);
                z = float.Parse(split[4]);
                yaw = float.Parse(split[5]);
                if (split.Length > 6) {
                    group = split[6];
                }
                valid = true;
            }
        }
        //Debug.Log("valid: " + valid + "(" + x + "," + y + "," + z + ")");
        if (GUILayout.Button("Create Enemy Generator in Your Clipboard")) {
            if (valid) {
                var generator = Instantiate(generatorPrefab, new Vector3(x, y, z), Quaternion.Euler(0, yaw, 0), selected.transform);
                generator.name = name + "/" + group;
            }
        }
    }

}
