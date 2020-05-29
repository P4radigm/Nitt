using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorLocation { 
    Top,
    Right,
    Bottom,
    Left
}


public class DoorBehaviour : MonoBehaviour
{
    private RoomManager roomManager;
    public DoorLocation doorLocation;

    private void Start()
    {
        RoomManager rm = transform.parent.GetComponent<RoomManager>();

        if(rm != null) { roomManager = rm; }
        else { Debug.LogError("Door: " + gameObject + "not placed in a room"); }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            PlayerBehaviour2 pB = collision.GetComponent<PlayerBehaviour2>();

            if(pB != null)
            {
                if (roomManager.doorOn && !roomManager.justEntered)
                {
                    Debug.Log("Hit Registered");
                    roomManager.ChangeRoom(this);
                    Debug.LogWarning("Player entered door: " + gameObject);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if (roomManager.justEntered)
            {
                roomManager.justEntered = false;
            }
        }
    }
}
