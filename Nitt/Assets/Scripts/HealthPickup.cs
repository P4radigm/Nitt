using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int hitPointsAmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerBehaviour pB = collision.GetComponent<PlayerBehaviour>();
        if(pB != null) 
        { 
            pB.hitPoints += hitPointsAmount;
            Destroy(gameObject);
        }
    }
}
