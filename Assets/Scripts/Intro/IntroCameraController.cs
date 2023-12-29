using UnityEngine;

public class IntroCameraController : MonoBehaviour {
    [HideInInspector] public IntroManager introManager;
    [HideInInspector] public GameObject centerAeroplane;
    public float rotateSpeed = 30f;

    public void Init(IntroManager introManager) {
        this.introManager = introManager;
        centerAeroplane = introManager.centerAeroplaneParent;
    }

    void Update() {
        if (centerAeroplane == null) {
            return;
        }
        transform.RotateAround(centerAeroplane.transform.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
