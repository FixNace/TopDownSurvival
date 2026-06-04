using UnityEngine;
using System.Collections;

public class AbilityZone : MonoBehaviour
{
    private AbilityType type;
    private float duration;
    private CircleCollider2D zoneCollider;

    public void Initialize(AbilityType abilityType, float time)
    {
        type = abilityType;
        duration = time;
        zoneCollider = GetComponent<CircleCollider2D>();

        if (type == AbilityType.TankShield)
        {
            zoneCollider.isTrigger = false;
            gameObject.layer = LayerMask.NameToLayer("PlayerObstacle");
        }
        else
        {
            zoneCollider.isTrigger = true;
        }

        StartCoroutine(LifeRoutine());
    }

    IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (zoneCollider.isTrigger && other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                if (type == AbilityType.ReloadBuff) pc.SetDamageBuff(true); // Теперь это Урон!
                if (type == AbilityType.VampireAura) pc.SetVampireBuff(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (zoneCollider.isTrigger && other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                if (type == AbilityType.ReloadBuff) pc.SetDamageBuff(false);
                if (type == AbilityType.VampireAura) pc.SetVampireBuff(false);
            }
        }
    }
}