using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    [Header("Object to Destroy")]
    [SerializeField] private GameObject parentObject;

    [Header("Collision Settings")]
    [SerializeField] private int cellRegenAmount;
    [SerializeField] private int damageAmount;
    [SerializeField] private float initialHP;
    [SerializeField] private bool takesContactDamage;
    [SerializeField] private float initialContactDamage;

    private float noPlayerHitCooldownTime = 0;
    private float HP;

    // Start is called before the first frame update
    void Start()
    {
        HP = initialHP;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        PlayerBehaviour pB = collision.gameObject.GetComponent<PlayerBehaviour>();
        float playerTpDamageOutput = 0;
        float playerContactDamageOutput = 0;
        float projectedHP = HP;

        if (pB != null)
        {
            playerTpDamageOutput = pB.tpDamageOutput;
            playerContactDamageOutput = pB.contactDamageOutput;
            projectedHP = HP - playerTpDamageOutput;
        }

        float totalContactDamage = initialContactDamage + playerContactDamageOutput;

        if (pB != null)
        {
            if (pB.justTP == true)
            {
                if (projectedHP > 0)
                {
                    HP = projectedHP;
                }
                else
                {
                    pB.teleportCells += cellRegenAmount;
                    Destroy(parentObject);
                }
            }
            else
            {
                pB.hitPoints -= damageAmount;

                if (takesContactDamage)
                {
                    float pHP = HP - totalContactDamage;
                    if (pHP > 0)
                    {
                        HP = pHP;
                    }
                    else
                    {
                        Destroy(parentObject);
                    }
                }
            }
        }
    }
}
