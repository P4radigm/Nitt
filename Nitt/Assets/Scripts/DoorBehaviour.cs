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
    [SerializeField] RoomManager roomManager;
    public DoorLocation doorLocation;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            roomManager.ChangeRoom(this);
            Debug.LogWarning("Player entered door: " + gameObject);
        }
    }
}
