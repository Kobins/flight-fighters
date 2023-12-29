using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour {

    [HideInInspector] public GameMap map;
    [HideInInspector] public GameController controller;
    public List<List<Enemy>> enemyPool;

    public void Init(GameController controller) {
        this.controller = controller;
        map = controller.map;

        enemyPool = new List<List<Enemy>>(map.enemyPalette.Count);
        for (int i = 0; i < map.enemyPalette.Count; i++) {
            enemyPool.Add(new List<Enemy>());
        }
        //전부 돌면서 적 종류별로 생성될 수 있는 갯수 세기
        map.actions.ForEach((action) => {
            if (action.type.Id != (int)ActionEnum.CREATE_ENEMY) {
                return;
            }
            int index = (int)action.param[0];
            enemyPool[index].Capacity++;
        });
        for (int index = 0; index < enemyPool.Count; index++) {
            var paletteEnemy = map.enemyPalette[index];
            for (int i = 0; i < enemyPool[index].Capacity; i++) {
                var enemy = Instantiate(paletteEnemy, transform);
                enemy.gameObject.SetActive(false);
                enemyPool[index].Add(enemy);
            }
        }
    }
    public Enemy GetEnemy(int index) {
        if (index < 0 || index >= map.enemyPalette.Count) {
            throw new System.IndexOutOfRangeException("Invalid enemy palette index: " + index);
        }
        for (int i = 0; i < enemyPool[index].Count; i++) {
            if (enemyPool[index][i].gameObject.activeInHierarchy) {
                continue;
            }
            var enemy = enemyPool[index][i];
            enemy.gameObject.SetActive(true);
            return enemy;
        }
        throw new System.Exception("There are no usable enemy instance: " + index);
    }
}
