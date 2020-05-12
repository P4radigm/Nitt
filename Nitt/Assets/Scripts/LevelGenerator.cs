using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private float localLeftChance;
    [SerializeField] private float localRightChance;
    [SerializeField] private float localBottomChance;
    [Space]
    [SerializeField] private float moveAmountX;
    [SerializeField] private float moveAmountY;
    [Space]
    [SerializeField] private int maxX;
    [SerializeField] private int minX;
    [SerializeField] private int maxY;
    [SerializeField] private int minY;

    [Header("Speed")]
    [SerializeField] private float startTimeBtwRoom = 0.25f;
    private float timeBtwRoom;

    [Header("Rooms")]
    [SerializeField] private GameObject[] RoomsL;
    [SerializeField] private GameObject[] RoomsT;
    [SerializeField] private GameObject[] RoomsR;
    [SerializeField] private GameObject[] RoomsB;
    [Space]
    [SerializeField] private GameObject[] RoomsLR;
    [SerializeField] private GameObject[] RoomsTB;
    [SerializeField] private GameObject[] RoomsLT;
    [SerializeField] private GameObject[] RoomsLB;
    [SerializeField] private GameObject[] RoomsRT;
    [SerializeField] private GameObject[] RoomsRB;
    [Space]
    [SerializeField] private GameObject[] RoomsTBL;
    [SerializeField] private GameObject[] RoomsTBR;
    [SerializeField] private GameObject[] RoomsLRT;
    [SerializeField] private GameObject[] RoomsLRB;
    [Space]
    [SerializeField] private GameObject[] RoomsLRBT;


    [Header("Needed")]
    [SerializeField] private Transform[] leftStartingPositions;
    [SerializeField] private Transform[] topStartingPositions;
    [SerializeField] private Transform[] rightStartingPositions;
    [SerializeField] private Transform[] bottomStartingPositions;

    private List<GameObject> spawnedRooms = new List<GameObject>();
    private int currentListIndex = 0;
    private Transform[][] startingPosArrays;
    private GameObject[][] Rooms;
    private Vector3 prevGeneratorPos;
    private float direction = 1f;
    private int startingSide;
    private int startingPos;
    private bool generationFinished = false;
    private bool firstDistanceCalculated = false;
    private bool correctRooms = false;

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

    void Start()
    {
        startingPosArrays = new Transform[4][];
        startingPosArrays[0] = rightStartingPositions;
        startingPosArrays[1] = leftStartingPositions;
        startingPosArrays[2] = topStartingPositions;
        startingPosArrays[3] = bottomStartingPositions;

        Rooms = new GameObject[15][];
        Rooms[0] = RoomsL;
        Rooms[1] = RoomsT;
        Rooms[2] = RoomsR;
        Rooms[3] = RoomsB;
        Rooms[4] = RoomsLR;
        Rooms[5] = RoomsTB;
        Rooms[6] = RoomsLT;
        Rooms[7] = RoomsLB;
        Rooms[8] = RoomsRT;
        Rooms[9] = RoomsRB;
        Rooms[10] = RoomsTBL;
        Rooms[11] = RoomsTBR;
        Rooms[12] = RoomsLRT;
        Rooms[13] = RoomsLRB;
        Rooms[14] = RoomsLRBT;

        //Pick random starting side
        startingSide = Random.Range(0, startingPosArrays.Length);
        startingPos = Random.Range(0, startingPosArrays[startingSide].Length);

        //spawn starting room
        transform.position = startingPosArrays[startingSide][startingPos].position;
        prevGeneratorPos = transform.position;
        GameObject startingRoom = Instantiate(Rooms[startingSide][Random.Range(0, Rooms[startingSide].Length)], transform.position, Quaternion.identity, transform.parent);

        //add starting room to the spawned rooms list and add to current list index
        spawnedRooms.Add(startingRoom);
        currentListIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeBtwRoom <= 0 && !generationFinished)
        {
            //main path spawner
            if (startingSide == 0) { MoveLeft(); }
            else if (startingSide == 1) { MoveRight(); }
            else if (startingSide == 2) { MoveBottom(); }
            else if (startingSide == 3) { MoveTop(); }
            else { Debug.LogError("Out of range error: startingSide"); }
            timeBtwRoom = startTimeBtwRoom;
        }
        else if (!generationFinished)
        {
            timeBtwRoom -= Time.deltaTime;
        }
        else if (!correctRooms)
        {
            int beginCount = spawnedRooms.Count;
            Debug.Log("beginCount = " + beginCount);

            //update rooms to correct shape
            for (int i = 0; i < beginCount; i++)
            {
                RoomType rt = spawnedRooms[i].GetComponent<RoomType>();

                rt.CheckRoom(spawnedRooms, i);

                if (rt.afterCheckType != rt.roomType)
                {
                    GameObject badRoom = spawnedRooms[i];
                    Vector3 curPos = spawnedRooms[i].transform.position;
                    spawnedRooms.Remove(badRoom);
                    Destroy(badRoom);
                    GameObject correctRoom = Instantiate(Rooms[rt.afterCheckType][Random.Range(0, Rooms[rt.afterCheckType].Length)], curPos, Quaternion.identity, transform.parent);
                    spawnedRooms.Insert(i, correctRoom);
                }
            }
            correctRooms = true;
        }


    }

    private void MoveRight()
    {
        //float worldLeftChance = 0f;
        float worldTopChance = localRightChance;
        float worldRightChance = localBottomChance;
        float worldBottomChance = localLeftChance;

        if (!firstDistanceCalculated) { direction = Random.Range(0, worldTopChance + worldBottomChance + worldRightChance); firstDistanceCalculated = true; }
        //Debug.Log(direction);

        if (direction >= 0 && direction < worldTopChance)
        {
            //Go BOTTOM, local RIGHT
            if(transform.position.y < maxY)
            {
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
                transform.position = newPos;

                float rightOrDown = Random.Range(0, worldTopChance + worldRightChance);

                if(rightOrDown >= 0 && rightOrDown < worldTopChance)
                {
                    direction = Random.Range(0, worldTopChance);
                }
                else
                {
                    direction = Random.Range(worldTopChance + worldBottomChance, worldTopChance + worldBottomChance + worldRightChance);
                }            
            }
            else
            {
                direction = Random.Range(worldTopChance + worldBottomChance, worldTopChance + worldBottomChance + worldRightChance);
            }
        }
        else if (direction >= worldTopChance && direction < worldTopChance + worldBottomChance)
        {
            //Go TOP, local LEFT
            if(transform.position.y > minY)
            {
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
                transform.position = newPos;

                direction = Random.Range(worldTopChance, worldTopChance + worldBottomChance + worldRightChance);
            }
            else
            {
                direction = Random.Range(worldTopChance + worldBottomChance, worldTopChance + worldBottomChance + worldRightChance);
            }
        }
        else if (direction >= worldTopChance + worldBottomChance && direction < worldTopChance + worldBottomChance + worldRightChance)
        {
            //Go RIGHT, local BOTTOM
            if(transform.position.x < maxX)
            {
                Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                transform.position = newPos;

                direction = Random.Range(0, worldTopChance + worldBottomChance + worldRightChance);
            }
            else
            {
                generationFinished = true;
            }
        }
        else
        {
            Debug.LogError("Out of range exception: direction");
        }

        if (!generationFinished && prevGeneratorPos != transform.position) 
        {
            prevGeneratorPos = transform.position;
            GameObject newRoom = Instantiate(RoomsLRBT[1], transform.position, Quaternion.identity, transform.parent);
            spawnedRooms.Add(newRoom);
        }
    }

    private void MoveBottom()
    {
        float worldLeftChance = localLeftChance;
        //float worldTopChance = 0f;
        float worldRightChance = localRightChance;
        float worldBottomChance = localBottomChance;

        if (!firstDistanceCalculated) { direction = Random.Range(0, worldRightChance + worldLeftChance + worldBottomChance); firstDistanceCalculated = true; }
        //Debug.Log(direction);

        if (direction >= 0 && direction < worldRightChance)
        {
            //Go RIGHT, local RIGHT
            if (transform.position.x < maxX)
            {
                Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                transform.position = newPos;

                float rightOrDown = Random.Range(0, worldRightChance + worldBottomChance);

                if (rightOrDown >= 0 && rightOrDown < worldRightChance)
                {
                    direction = Random.Range(0, worldRightChance);
                }
                else
                {
                    direction = Random.Range(worldRightChance + worldLeftChance, worldRightChance + worldLeftChance + worldBottomChance);
                }
            }
            else
            {
                direction = Random.Range(worldRightChance + worldLeftChance, worldRightChance + worldLeftChance + worldBottomChance);
            }
        }
        else if (direction >= worldRightChance && direction < worldRightChance + worldLeftChance)
        {
            //Go LEFT, local LEFT
            if (transform.position.x > minX)
            {
                Vector2 newPos = new Vector2(transform.position.x - moveAmountX, transform.position.y);
                transform.position = newPos;

                direction = Random.Range(worldRightChance, worldRightChance + worldLeftChance + worldBottomChance);
            }
            else
            {
                direction = Random.Range(worldRightChance + worldLeftChance, worldRightChance + worldLeftChance + worldBottomChance);
            }
        }
        else if (direction >= worldRightChance + worldLeftChance && direction < worldRightChance + worldLeftChance + worldBottomChance)
        {
            //Go BOTTOM, local BOTTOM
            if (transform.position.y > minY)
            {
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
                transform.position = newPos;

                direction = Random.Range(0, worldRightChance + worldLeftChance + worldBottomChance);
            }
            else
            {
                generationFinished = true;
            }
        }
        else
        {
            Debug.LogError("Out of range exception: direction");
        }

        if (!generationFinished && prevGeneratorPos != transform.position)
        {
            prevGeneratorPos = transform.position;
            GameObject newRoom = Instantiate(RoomsLRBT[1], transform.position, Quaternion.identity, transform.parent);
            spawnedRooms.Add(newRoom);
        }
    }

    private void MoveLeft()
    {
        float worldLeftChance = localBottomChance;
        float worldTopChance = localRightChance;
        //float worldRightChance = 0f;
        float worldBottomChance = localLeftChance;

        if (!firstDistanceCalculated) { direction = Random.Range(0, worldTopChance + worldBottomChance + worldLeftChance); firstDistanceCalculated = true; }
        //Debug.Log(direction);

        if (direction >= 0 && direction < worldTopChance)
        {
            //Go TOP, local RIGHT
            if (transform.position.y < maxY)
            {
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
                transform.position = newPos;

                float rightOrDown = Random.Range(0, worldTopChance + worldLeftChance);

                if (rightOrDown >= 0 && rightOrDown < worldTopChance)
                {
                    direction = Random.Range(0, worldTopChance);
                }
                else
                {
                    direction = Random.Range(worldTopChance + worldBottomChance, worldTopChance + worldBottomChance + worldLeftChance);
                }
            }
            else
            {
                direction = Random.Range(worldTopChance + worldBottomChance, worldTopChance + worldBottomChance + worldLeftChance);
            }
        }
        else if (direction >= worldTopChance && direction < worldTopChance + worldBottomChance)
        {
            //Go BOTTOM, local LEFT
            if (transform.position.y > minY)
            {
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
                transform.position = newPos;

                direction = Random.Range(worldTopChance, worldTopChance + worldBottomChance + worldLeftChance);
            }
            else
            {
                direction = Random.Range(worldTopChance + worldBottomChance, worldTopChance + worldBottomChance + worldLeftChance);
            }
        }
        else if (direction >= worldTopChance + worldBottomChance && direction < worldTopChance + worldBottomChance + worldLeftChance)
        {
            //Go LEFT, local BOTTOM
            if (transform.position.x > minX)
            {
                Vector2 newPos = new Vector2(transform.position.x - moveAmountX, transform.position.y);
                transform.position = newPos;

                direction = Random.Range(0, worldTopChance + worldBottomChance + worldLeftChance);
            }
            else
            {
                generationFinished = true;
            }
        }
        else
        {
            Debug.LogError("Out of range exception: direction");
        }

        if (!generationFinished && prevGeneratorPos != transform.position)
        {
            prevGeneratorPos = transform.position;
            GameObject newRoom = Instantiate(RoomsLRBT[1], transform.position, Quaternion.identity, transform.parent);
            spawnedRooms.Add(newRoom);
        }
    }

    private void MoveTop()
    {
        float worldLeftChance = localRightChance;
        float worldTopChance = localBottomChance;
        float worldRightChance = localLeftChance;
        //float worldBottomChance = 0f;

        if (!firstDistanceCalculated) { direction = Random.Range(0, worldLeftChance + worldRightChance + worldTopChance); firstDistanceCalculated = true; }
        //Debug.Log(direction);

        if (direction >= 0 && direction < worldLeftChance)
        {
            //Go LEFT, local RIGHT
            if (transform.position.x > minX)
            {
                Vector2 newPos = new Vector2(transform.position.x - moveAmountX, transform.position.y);
                transform.position = newPos;

                float rightOrDown = Random.Range(0, worldLeftChance + worldTopChance);

                if (rightOrDown >= 0 && rightOrDown < worldLeftChance)
                {
                    direction = Random.Range(0, worldLeftChance);
                }
                else
                {
                    direction = Random.Range(worldLeftChance + worldRightChance, worldLeftChance + worldRightChance + worldTopChance);
                }
            }
            else
            {
                direction = Random.Range(worldLeftChance + worldRightChance, worldLeftChance + worldRightChance + worldTopChance);
            }
        }
        else if (direction >= worldLeftChance && direction < worldLeftChance + worldRightChance)
        {
            //Go RIGHT, local LEFT
            if (transform.position.x < maxX)
            {
                Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
                transform.position = newPos;

                direction = Random.Range(worldLeftChance, worldLeftChance + worldRightChance + worldTopChance);
            }
            else
            {
                direction = Random.Range(worldLeftChance + worldRightChance, worldLeftChance + worldRightChance + worldTopChance);
            }
        }
        else if (direction >= worldLeftChance + worldRightChance && direction < worldLeftChance + worldRightChance + worldTopChance)
        {
            //Go TOP, local BOTTOM
            if (transform.position.y < maxY)
            {
                Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
                transform.position = newPos;

                direction = Random.Range(0, worldRightChance + worldLeftChance + worldTopChance);
            }
            else
            {
                generationFinished = true;
            }
        }
        else
        {
            Debug.LogError("Out of range exception: direction");
        }

        if (!generationFinished && prevGeneratorPos != transform.position)
        {
            prevGeneratorPos = transform.position;
            GameObject newRoom = Instantiate(RoomsLRBT[1], transform.position, Quaternion.identity, transform.parent);
            spawnedRooms.Add(newRoom);
        }
    }

    //private void Move()
    //{
    //    if (direction >= 0 && direction < goLeftChance && startingSide != 0)
    //    {
    //        //Move LEFT
    //        if(transform.position.x > minX)
    //        {
    //            Vector2 newPos = new Vector2(transform.position.x - moveAmountX, transform.position.y);
    //            transform.position = newPos;
    //        }
    //        else if (startingSide != 2)
    //        {
    //            direction = Random.Range(goLeftChance, goLeftChance + goTopChance + goRightChance + goBottomChance);
    //        }
    //        else
    //        {
    //            generationFinished = true;
    //        }
    //    }
    //    else if (direction >= goLeftChance && direction < goLeftChance + goTopChance && startingSide != 1)
    //    {
    //        //move TOP
    //        if(transform.position.y < maxY)
    //        {
    //            Vector2 newPos = new Vector2(transform.position.x, transform.position.y + moveAmountY);
    //            transform.position = newPos;
    //        }
    //        else if (startingSide != 3)
    //        {
    //            direction = Random.Range(goLeftChance + goTopChance, goLeftChance + goTopChance + goRightChance + goBottomChance);
    //        }
    //        else
    //        {
    //            generationFinished = true;
    //        }
    //    }
    //    else if (direction >= goLeftChance + goTopChance && direction < goLeftChance + goTopChance + goRightChance && startingSide != 2)
    //    {
    //        //move RIGHT
    //        if(transform.position.x < maxX)
    //        {
    //            Vector2 newPos = new Vector2(transform.position.x + moveAmountX, transform.position.y);
    //            transform.position = newPos;
    //        }
    //        else if (startingSide != 0)
    //        {
    //            direction = Random.Range(goLeftChance + goTopChance + goRightChance, goLeftChance + goTopChance + goRightChance + goBottomChance);
    //        }
    //        else
    //        {
    //            generationFinished = true;
    //        }
    //    }
    //    else if (direction >= goLeftChance + goTopChance + goRightChance && direction <= goLeftChance + goTopChance + goRightChance + goBottomChance && startingSide != 3)
    //    {
    //        //move DOWN
    //        if(transform.position.y > minY)
    //        {
    //            Vector2 newPos = new Vector2(transform.position.x, transform.position.y - moveAmountY);
    //            transform.position = newPos;
    //        }
    //        else
    //        {
    //            generationFinished = true;
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("Out of range exception: direction");
    //    }


    //    Instantiate(startingRoomsB[0], transform.position, Quaternion.identity, transform.parent);
    //    direction = Random.Range(0, goLeftChance + goTopChance + goRightChance + goBottomChance);
    //}
}
