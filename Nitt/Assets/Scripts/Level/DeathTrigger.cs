using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerBehaviour2 pB = collision.GetComponent<PlayerBehaviour2>();
        if(pB != null) { pB.EnvironmentDamage(); }
    }

}
