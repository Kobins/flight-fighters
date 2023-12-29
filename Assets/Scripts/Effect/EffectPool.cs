using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour {
    static EffectPool instance;
    static int POOL_EXPAND = 10;

    public List<CustomEffect> poolEffectList;
    public static EffectPool GetInstance() => instance;

    Dictionary<string, GameObject> poolParent;
    Dictionary<string, List<CustomEffect>> effectPool;

    void Start() {
        instance = this;
        poolParent = new Dictionary<string, GameObject>();
        effectPool = new Dictionary<string, List<CustomEffect>>();

        foreach (var obj in poolEffectList) {
            CreatePool(obj);
        }
    }

    public void CreatePool(CustomEffect obj) {
        poolParent[obj.name] = new GameObject(obj.name + "_Parent");
        poolParent[obj.name].transform.parent = transform;
        effectPool[obj.name] = new List<CustomEffect>(POOL_EXPAND);
        ExpandPool(obj);
    }

    void ExpandPool(CustomEffect obj) {
        CustomEffect dup;
        int number = effectPool[obj.name].Count;
        effectPool[obj.name].Capacity += POOL_EXPAND;
        for (int i = 0; i < POOL_EXPAND; i++) {
            dup = Instantiate(obj, poolParent[obj.name].transform);
            dup.gameObject.SetActive(false);
            dup.name = obj.name + "_" + (number + i).ToString("00");
            effectPool[obj.name].Add(dup);
        }

    }

    public CustomEffect GetEffect(CustomEffect obj) {
        if (!effectPool.ContainsKey(obj.name)) {
            CreatePool(obj);
        }
        for (int i = 0; i < effectPool[obj.name].Count; i++) {
            if (effectPool[obj.name][i] == null) continue;
            if (effectPool[obj.name][i].gameObject.activeSelf) {
                continue;
            }
            effectPool[obj.name][i].gameObject.SetActive(true);
            return effectPool[obj.name][i];
        }
        ExpandPool(obj);
        return GetEffect(obj);
    }
}
