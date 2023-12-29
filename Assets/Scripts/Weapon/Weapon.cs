using UnityEngine;

public delegate void ShootPositionEvent(Vector3 position, out Vector3 targetPoint, float maxDst);
public abstract class Weapon : MonoBehaviour {
    [HideInInspector] public ArmoredVehicle attached;
    public string m_DisplayName;
    public int m_ShootPerTrigger;
    public float m_ShootInterval;
    public float m_HitMarkDuration;
    public float m_MaxRange = 1500f;
    public Transform[] m_ShootPositions;

    [HideInInspector] public bool holding;
    protected float shootDelay;
    protected int shootPositionIndex;

    [Header("Weapon Slot View Settings")]
    public Vector3 m_SlotViewPosition = new Vector3(0, 0, 0);
    public Vector3 m_SlotViewRotation = new Vector3(-22.5f, 22.5f, 0);
    public Vector3 m_SlotViewScale = new Vector3(1, 1, 1);
    public UIWeaponSlotImage slotPrefab;
    [HideInInspector] public bool isSlotDummy;
    [HideInInspector] public WeaponSlot attachedWeaponSlot;

    private void Awake() {
        holding = false;
        shootDelay = 0f;
        if (m_ShootPositions == null || m_ShootPositions.Length <= 0) {
            m_ShootPositions = new Transform[] { transform };
        }
        shootPositionIndex = 0;
    }
    public void Trigger() {
        if (OnTrigger()) {
            Shoot();
        }
    }
    void Shoot() {
        for (int i = 0; i < m_ShootPerTrigger; i++) {
            OnShoot();
        }
    }
    public abstract bool OnTrigger();
    public abstract void OnShoot();
    public virtual void OnMarkerUpdate(UIEnemyTargetMarker mark, Enemy enemy) {
        if (mark.GetColor() != Color.green) {
            mark.SetColor(Color.green);
        }
    }
    public virtual bool IsLocking() { return false; }
    public ShootPositionEvent OnGetShootPosition;

    public Transform GetNextShootPosition() {
        var pos = m_ShootPositions[(shootPositionIndex + 1) % m_ShootPositions.Length];
        if (OnGetShootPosition != null) {
            var shootPosition = pos.position;
            OnGetShootPosition(shootPosition, out Vector3 targetPoint, m_MaxRange);
            pos.rotation = Quaternion.LookRotation((targetPoint - shootPosition));
        } else {
            pos.rotation = Quaternion.LookRotation(attached.transform.forward);
        }
        return pos;
    }
}