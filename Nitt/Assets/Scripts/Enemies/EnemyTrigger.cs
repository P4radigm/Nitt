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
    private GameManager gm = GameManager.instance;

    // Start is called before the first frame update
    void Start()
    {
        HP = initialHP;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        PlayerBehaviour2 pB = collision.gameObject.GetComponent<PlayerBehaviour2>();

        if (pB != null)
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
                    pB.teleportCells += cellRegenAmount;
                    Destroy(parentObject);
                }
            }
            else
            {
                pB.hitPoints -= damageAmount;

                if (takesContactDamage)
                {
                    float projectedHP = HP - totalContactDamage;

                    if (projectedHP > 0)
                    {
                        HP = projectedHP;
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
