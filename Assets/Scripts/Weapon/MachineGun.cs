using UnityEngine;

public class MachineGun : Weapon {
    public CustomEffect lineEffect;
    public CustomEffect shootEffect;
    public CustomEffect hitGroundEffect;
    public CustomEffect hitVehicleEffect;


    public float m_Spread = 0.2f;
    public float m_Damage = 5f;

    public float m_MaxHeat = 400f;
    public float m_HeatPerShoot = 1f;
    public float m_HeatDecreasePerSeconds = 100f;

    public float m_OverHeatDuration = 2f;

    float heat = 0f;
    float overHeating = 0f;

    void Start() {
        overHeating = 0;
    }

    static readonly Color red = new Color(1, 0, 0, 0.5f);
    static readonly Color yellow = new Color(1, 0.92f, 0.016f, 0.5f);

    void Update() {
        if (slotViewing) return;
        if (shootDelay > 0) {
            shootDelay -= TimeManager.deltaTime;
        } else { //열은 발사 딜레이중이 아닐 때 감소함
            if (overHeating > 0) { //과열 지속
                overHeating -= TimeManager.deltaTime;
            } else if (heat > 0) {
                heat -= m_HeatDecreasePerSeconds * TimeManager.deltaTime;
                if (heat < 0) heat = 0;
                if (attachedWeaponSlot) {
                    attachedWeaponSlot.image.SetFilledColor(yellow);
                }
            }
        }
        if (attachedWeaponSlot) {
            attachedWeaponSlot.image.SetFilled(heat / m_MaxHeat);
        }
    }

    public override bool OnTrigger() {
        if (shootDelay > 0 || overHeating > 0) return false;
        heat += m_HeatPerShoot;
        if (heat > m_MaxHeat) {
            if (attachedWeaponSlot)
                attachedWeaponSlot.image.SetFilledColor(red);
            heat = m_MaxHeat;
            overHeating = m_OverHeatDuration;
        }
        return true;
    }

    public override void OnShoot() {
        var shootPos = GetShootPosition();
        if (shootEffect) {
            shootEffect.GetEffect(shootPos.position);
        }
        var direction = shootPos.forward;
        direction += Random.insideUnitSphere * 0.1f * m_Spread;
        direction.Normalize();
        var raycast = Physics.RaycastAll(shootPos.position, direction, m_MaxRange);
        Vector3 hitted = shootPos.position + direction * m_MaxRange;
        for (int i = 0; i < raycast.Length; i++) {
            if (raycast[i].transform.gameObject.Equals(attached.gameObject) || raycast[i].transform.tag == attached.tag) continue;
            hitted = raycast[i].point;
            var entity = raycast[i].transform.GetComponent<ArmoredVehicle>();
            if (entity != null) {
                if (hitVehicleEffect) {
                    hitVehicleEffect.GetEffect(hitted);
                }
                entity.Damage(attached, entity, m_Damage);
                attached.OnWeaponHit?.Invoke(this, entity, m_Damage);
                break;
            }
            if (hitGroundEffect) {
                hitGroundEffect.GetEffect(hitted);
            }
            break;
        }
        if (lineEffect) {
            var line = lineEffect.GetEffect();
            line.SetLine(shootPos.position, hitted, m_ShootInterval);
        }
    }
    public override void OnMarkerUpdate(UIEnemyTargetMarker mark, Enemy enemy) {
        if (Vector3.Distance(attached.transform.position, enemy.transform.position) > m_MaxRange) {
            mark.SetColor(Color.green);
        } else {
            mark.SetColor(Color.yellow);
        }
    }
}