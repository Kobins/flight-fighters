using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapEnemyGenerator))]
public class EnemyGeneratorEditor : Editor {
    public MapEnemyGenerator selected;
    private void OnEnable() {
        // target은 Editor에 있는 변수로 선택한 오브젝트를 받아옴.
        if (AssetDatabase.Contains(target)) {
            selected = null;
        } else {
            // target은 Object형이므로 Enemy로 형변환
            selected = (MapEnemyGenerator)target;
        }
    }

    public override void OnInspectorGUI() {
        if (selected == null)
            return;
        if (GUILayout.Button("Generate Action")) {
            var split = selected.name.Split('/');
            string group = "default";
            int index = 0;
            if (split.Length == 2) {
                var paletteName = split[0];
                if (selected.transform.parent != null && selected.transform.parent.TryGetComponent<GameMap>(out var map)) {
                    for (int i = 0; i < map.enemyPalette.Count; i++) {
                        if (map.enemyPalette[i].name.Equals(paletteName)) {
                            index = i;
                            break;
                        }
                    }
                }
                group = split[1];
            }
            string generated = selected.GenerateActionString(index, group);
            Debug.Log("Copied to your clipboard : " + generated);
            EditorGUIUtility.systemCopyBuffer = generated;
        }

    }

}
