using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    public int roomType;

    public int afterCheckType = -1;

    //Room types:
    //0 -> L
    //1 -> T
    //2 -> R
    //3 -> B
    //4 -> LR
    //5 -> TB
    //6 -> LT
    //7 -> LB
    //8 -> RT
    //9 -> RB
    //10 -> TBL
    //11 -> TBR
    //12 -> LRT
    //13 -> LRB
    //14 -> LRBT

    public void DestroyRoom()
    {
        Destroy(this.gameObject);
    }

    public void CheckRoom(List<GameObject> currentSpawnedRooms, int curListIndex)
    {
        //RaycastHit2D upRay = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + Vector2.up * 2, Vector2.up, 22f, ~(1 << LayerMask.NameToLayer("Rooms")));
        //RaycastHit2D rightRay = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + Vector2.right * 2, Vector2.right, 15f, ~(1 << LayerMask.NameToLayer("Rooms")));
        //RaycastHit2D downRay = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + Vector2.down * 2, Vector2.down, 22f, ~(1 << LayerMask.NameToLayer("Rooms")));
        //RaycastHit2D leftRay = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + Vector2.left * 2, Vector2.left, 15f, ~(1 << LayerMask.NameToLayer("Rooms")));

        bool upRoom = false;
        bool rightRoom = false;
        bool downRoom = false;
        bool leftRoom = false;

        for (int i = 0; i < currentSpawnedRooms.Count; i++)
        {

            //Check up
            if (currentSpawnedRooms[i].transform.position == transform.position + new Vector3(0, 20, 0))
            {
                upRoom = true;
            }

            //Check right
            if (currentSpawnedRooms[i].transform.position == transform.position + new Vector3(13, 0, 0))
            {
                rightRoom = true;
            }

            //Check down
            if (currentSpawnedRooms[i].transform.position == transform.position + new Vector3(0, -20, 0))
            {
                downRoom = true;
            }

            //Check left
            if (currentSpawnedRooms[i].transform.position == transform.position + new Vector3(-13, 0, 0))
            {
                leftRoom = true;
            }
        }

        if (upRoom == true && rightRoom == true && downRoom == true && leftRoom == true)
        {
            //14 -> LRBT
            afterCheckType = 14;
        }
        else if (upRoom == false && rightRoom == true && downRoom == true && leftRoom == true)
        {
            //13 -> LRB
            afterCheckType = 13;
        }        
        else if (upRoom == true && rightRoom == true && downRoom == false && leftRoom == true)
        {
            //12 -> LRT
            afterCheckType = 12;
        }
        else if (upRoom == true && rightRoom == true && downRoom == true && leftRoom == false)
        {
            //11 -> TBR
            afterCheckType = 11;
        }
        else if (upRoom == true && rightRoom == false && downRoom == true && leftRoom == true)
        {
            //10 -> TBL
            afterCheckType = 10;
        }
        else if (upRoom == false && rightRoom == true && downRoom == true && leftRoom == false)
        {
            //9 -> RB
            afterCheckType = 9;
        }
        else if (upRoom == true && rightRoom == true && downRoom == false && leftRoom == false)
        {
            //8 -> RT
            afterCheckType = 8;
        }
        else if (upRoom == false && rightRoom == false && downRoom == true && leftRoom == true)
        {
            //7 -> LB
            afterCheckType = 7;
        }
        else if (upRoom == true && rightRoom == false && downRoom == false && leftRoom == true)
        {
            //6 -> LT
            afterCheckType = 6;
        }
        else if (upRoom == true && rightRoom == false && downRoom == true && leftRoom == false)
        {
            //5 -> TB
            afterCheckType = 5;
        }
        else if (upRoom == false && rightRoom == true && downRoom == false && leftRoom == true)
        {
            //4 -> LR
            afterCheckType = 4;
        }
        else if (upRoom == false && rightRoom == false && downRoom == true && leftRoom == false)
        {
            //3 -> B
            afterCheckType = 3;
        }
        else if (upRoom == false && rightRoom == true && downRoom == false && leftRoom == false)
        {
            //2 -> R
            afterCheckType = 2;
        }
        else if (upRoom == true && rightRoom == false && downRoom == false && leftRoom == false)
        {
            //1 -> T
            afterCheckType = 1;
        }
        else if (upRoom == false && rightRoom == false && downRoom == false && leftRoom == true)
        {
            //0 -> L
            afterCheckType = 0;
        }
        else
        {
            Debug.LogError("No Raycast Hit????");
        }

        //Debug.Log(curListIndex + ": upRoom = " + upRoom);
        //Debug.Log(curListIndex + ": rightRoom = " + rightRoom);
        //Debug.Log(curListIndex + ": downRoom = " + downRoom);
        //Debug.Log(curListIndex + ": leftRoom = " + leftRoom);
        //Debug.LogWarning(curListIndex + ": afterCheckType = " + afterCheckType);
    }
}
