using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [Header("Object to Destroy")]
    [SerializeField] private GameObject parentObject;
    [SerializeField] private ParticleSystem onDeathParticles;

    [Header("Collision Settings")]
    [SerializeField] private int cellRegenAmount;
    [SerializeField] private int damageAmount;
    [SerializeField] private float initialHP;
    [SerializeField] private bool takesContactDamage;
    [SerializeField] private float initialContactDamage;

    [HideInInspector] public bool active = true;

    private float noPlayerHitCooldownTime = 0;
    private float HP;
    private GameManager gm = GameManager.instance;

    // Start is called before the first frame update
    void Start()
    {
        active = true;
        HP = initialHP;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        PlayerBehaviour2 pB = collision.gameObject.GetComponent<PlayerBehaviour2>();

        if (pB != null && active && !pB.playerNotHittable)
        {
            float playerTpDamageOutput = pB.tpDamageOutput;
            float playerContactDamageOutput = pB.contactDamageOutput;
            float totalContactDamage = initialContactDamage + playerContactDamageOutput;

            if (pB.justTP == true)
            {
                float projectedHP = HP - playerTpDamageOutput;

                if (projectedHP > 0)
                {
                    HP = projectedHP;
                }
                else
                {
                    //pB.teleportJuice += cellRegenAmount;
                    gm.hitEnemy = true;
                    //Debug.Log(cellRegenAmount);
                    pB.CheckCombo(cellRegenAmount);
                    //gm.Freeze(0.08f);
                    gm.EnemyDeath(parentObject);
                }
            }
            else
            {
                pB.hitPoints -= damageAmount;
                pB.StartCoroutine(pB.OnDamage());

                gm.healthLossParticles.Clear();
                gm.healthLossParticles.Play();

                //gm.Freeze(0.2f);

                if (takesContactDamage)
                {
                    float projectedHP = HP - totalContactDamage;

                    if (projectedHP > 0)
                    {
                        HP = projectedHP;
                    }
                    else
                    {
                        gm.EnemyDeath(parentObject);
                    }
                }
            }
        }
    }
}
