using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportCellPickup : MonoBehaviour
{
    [SerializeField] private int teleportCellAmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerBehaviour pB = collision.GetComponent<PlayerBehaviour>();
        if (pB != null)
        {
            pB.teleportCells += teleportCellAmount;
            Destroy(gameObject);
        }
    }
}

