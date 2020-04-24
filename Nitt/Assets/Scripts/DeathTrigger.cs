using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerBehaviour pB = collision.GetComponent<PlayerBehaviour>();
        if(pB != null) { pB.EnvironmentDamage(); }
    }

}
