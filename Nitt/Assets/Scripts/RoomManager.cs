﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { 
    Needle,
    Wasp,
    Bat,
    Slug,
    Mole
}

public class RoomManager : MonoBehaviour

{
    [System.Serializable]
    public class EnemyToBeSpawned
    {
        public EnemyType enemyType;
        public Transform spawnTransform;
    }

    [Header("Entrance/Exits")]
    public GameObject topEntrance;
    public GameObject rightEntrance;
    public GameObject downEntrance;
    public GameObject leftEntrance;

    [Header("Enemy Spawn Options")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private EnemyToBeSpawned[] enemiesToBeSpawned;
    [SerializeField] private float totalSpawnTime;
    [SerializeField] private float lastPauzeTime;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    [Header("VFX")]
    [SerializeField] private ParticleSystem enemySpawnVFX;
    [SerializeField] private ParticleSystem enemyAfterSpawnVFX;

    public bool isCompleted;
    public bool hasPlayer;
    private PlayerBehaviour2 pB;
    private Camera mainCamera;

    private bool spawnedEnemiesYet;
    private GameManager gm;
    private Shader enemyShader;
    //private Material EnemyMat = new Material(Shader.Find("ShaderGraphs/ColorCycle"));

    private Coroutine spawnEnemiesRoutine = null;

    void Start()
    {
        gm = GameManager.instance;
        pB = FindObjectOfType<PlayerBehaviour2>();
        mainCamera = FindObjectOfType<Camera>();

        topEntrance.SetActive(false);
        rightEntrance.SetActive(false);
        downEntrance.SetActive(false);
        leftEntrance.SetActive(false);

        enemyShader = Shader.Find("NittShader/ColorCycle");

        if(gm.spawnedRooms.Count == 0)
        {
            gm.spawnedRooms.Add(gameObject);
        }
    }

    void Update()
    {
        if (hasPlayer)
        {
            UpdateEnemyList();
            gm.activeRoom = gameObject;

            if (!spawnedEnemiesYet)
            {
                if(spawnEnemiesRoutine == null)
                {
                    spawnEnemiesRoutine = StartCoroutine(SpawnEnemiesIE());
                }
            }

            if (spawnedEnemies.Count == 0 && spawnedEnemiesYet)
            {
                isCompleted = true;

                topEntrance.SetActive(true);
                rightEntrance.SetActive(true);
                downEntrance.SetActive(true);
                leftEntrance.SetActive(true);
            }
        }
    }

    private IEnumerator SpawnEnemiesIE()
    {

        for (int i = 0; i < enemiesToBeSpawned.Length; i++)
        {

            Vector3 vec3SpawnPoint = new Vector3(enemiesToBeSpawned[i].spawnTransform.position.x, enemiesToBeSpawned[i].spawnTransform.position.y, 0);
            Quaternion spawnRot = enemiesToBeSpawned[i].spawnTransform.rotation;

            ParticleSystem spawnFX = Instantiate(enemySpawnVFX, vec3SpawnPoint, Quaternion.identity);
            var spawnFXMain = spawnFX.main;
            spawnFXMain.simulationSpeed = totalSpawnTime / enemiesToBeSpawned.Length;
            spawnFX.Play(true);

            yield return new WaitForSeconds(totalSpawnTime / enemiesToBeSpawned.Length);
            Destroy(spawnFX.gameObject);
            GameObject Enemy = Instantiate(enemyPrefabs[(int)enemiesToBeSpawned[i].enemyType], vec3SpawnPoint, spawnRot, transform);

            ParticleSystem afterSpawnFX = Instantiate(enemyAfterSpawnVFX, vec3SpawnPoint, Quaternion.identity);
            afterSpawnFX.Play(true);
            Destroy(afterSpawnFX, 0.3f);

            DisableEnemy(Enemy);

            //Set Material with offset
            Renderer[] prefabRenderers = Enemy.GetComponentsInChildren<Renderer>();
            if (prefabRenderers.Length != 0)
            {
                for (int j = 0; j < prefabRenderers.Length; j++)
                {
                    Material enemyMat = new Material(enemyShader);
                    enemyMat.SetFloat("_ColorOffset", Random.Range(0, 100f));
                    prefabRenderers[j].material = enemyMat;
                }
            }

            //Particles
            spawnedEnemies.Add(Enemy);
        }

        yield return new WaitForSeconds(lastPauzeTime);

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            EnableEnemy(spawnedEnemies[i]);
        }

        spawnedEnemiesYet = true;

        yield return null;
    }

    private void UpdateEnemyList()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if(spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    public void ChangeRoom(DoorBehaviour door)
    {
        if(door.doorLocation == DoorLocation.Top)
        {
            GameObject _newRoom = null;

            //disable player script
            pB.enabled = false;

            //check if there's not already a room on top of this one
            bool _roomAlreadyThere = false;

            for (int i = 0; i < gm.spawnedRooms.Count; i++)
            {
                if(gm.spawnedRooms[i].transform.position == transform.position + new Vector3(0, 20, 0))
                {
                    _roomAlreadyThere = true;
                    _newRoom = gm.spawnedRooms[i];
                    _newRoom.SetActive(true);
                }
            }

            //spawn room on top of this one with a bottom door
            if (!_roomAlreadyThere)
            {
                int randomRoomType = Random.Range(0, 8);
                GameObject[] randomRoomTypeArray = gm.topRooms[randomRoomType];
                int randomRoom = Random.Range(0, randomRoomTypeArray.Length);

                _newRoom = Instantiate(randomRoomTypeArray[randomRoom], transform.position + new Vector3(0, 20, 0), Quaternion.identity);
                gm.spawnedRooms.Add(_newRoom);
            }

            //update camera
            mainCamera.transform.position += new Vector3(0, 20, 0);

            //move player to bottom door of new room (with offset)
            pB.transform.position = _newRoom.GetComponent<RoomManager>().downEntrance.transform.position + new Vector3(0, 0.5f, 0);

            //enable player script
            pB.enabled = true;
        }
        else if (door.doorLocation == DoorLocation.Right)
        {
            GameObject _newRoom = null;

            //disable player script
            pB.enabled = false;

            //check if there's not already a room on top of this one
            bool _roomAlreadyThere = false;

            for (int i = 0; i < gm.spawnedRooms.Count; i++)
            {
                if (gm.spawnedRooms[i].transform.position == transform.position + new Vector3(13, 0, 0))
                {
                    _roomAlreadyThere = true;
                    _newRoom = gm.spawnedRooms[i];
                    _newRoom.SetActive(true);
                }
            }

            //spawn room on top of this one with a bottom door
            if (!_roomAlreadyThere)
            {
                int randomRoomType = Random.Range(0, 8);
                GameObject[] randomRoomTypeArray = gm.rightRooms[randomRoomType];
                int randomRoom = Random.Range(0, randomRoomTypeArray.Length);

                _newRoom = Instantiate(randomRoomTypeArray[randomRoom], transform.position + new Vector3(13, 0, 0), Quaternion.identity);
                gm.spawnedRooms.Add(_newRoom);
            }

            //update camera
            mainCamera.transform.position += new Vector3(13, 0, 0);

            //move player to bottom door of new room (with offset)
            pB.transform.position = _newRoom.GetComponent<RoomManager>().leftEntrance.transform.position + new Vector3(0.5f, 0, 0);

            //enable player script
            pB.enabled = true;
        }
        else if (door.doorLocation == DoorLocation.Bottom)
        {
            GameObject _newRoom = null;

            //disable player script
            pB.enabled = false;

            //check if there's not already a room on top of this one
            bool _roomAlreadyThere = false;

            for (int i = 0; i < gm.spawnedRooms.Count; i++)
            {
                if (gm.spawnedRooms[i].transform.position == transform.position + new Vector3(0, -20, 0))
                {
                    _roomAlreadyThere = true;
                    _newRoom = gm.spawnedRooms[i];
                    _newRoom.SetActive(true);
                }
            }

            //spawn room on top of this one with a bottom door
            if (!_roomAlreadyThere)
            {
                int randomRoomType = Random.Range(0, 8);
                GameObject[] randomRoomTypeArray = gm.bottomRooms[randomRoomType];
                int randomRoom = Random.Range(0, randomRoomTypeArray.Length);

                _newRoom = Instantiate(randomRoomTypeArray[randomRoom], transform.position + new Vector3(0, -20, 0), Quaternion.identity);
                gm.spawnedRooms.Add(_newRoom);
            }

            //update camera
            mainCamera.transform.position += new Vector3(0, -20, 0);

            //move player to bottom door of new room (with offset)
            pB.transform.position = _newRoom.GetComponent<RoomManager>().topEntrance.transform.position + new Vector3(0, -0.5f, 0);

            //enable player script
            pB.enabled = true;
        }
        else if (door.doorLocation == DoorLocation.Left)
        {
            GameObject _newRoom = null;

            //disable player script
            pB.enabled = false;

            //check if there's not already a room on top of this one
            bool _roomAlreadyThere = false;

            for (int i = 0; i < gm.spawnedRooms.Count; i++)
            {
                if (gm.spawnedRooms[i].transform.position == transform.position + new Vector3(-13, 0, 0))
                {
                    _roomAlreadyThere = true;
                    _newRoom = gm.spawnedRooms[i];
                    _newRoom.SetActive(true);
                }
            }

            //spawn room on top of this one with a bottom door
            if (!_roomAlreadyThere)
            {
                int randomRoomType = Random.Range(0, 8);
                GameObject[] randomRoomTypeArray = gm.leftRooms[randomRoomType];
                int randomRoom = Random.Range(0, randomRoomTypeArray.Length);

                _newRoom = Instantiate(randomRoomTypeArray[randomRoom], transform.position + new Vector3(-13, 0, 0), Quaternion.identity);
                gm.spawnedRooms.Add(_newRoom);
            }

            //update camera
            mainCamera.transform.position += new Vector3(-13, 0, 0);

            //move player to bottom door of new room (with offset)
            pB.transform.position = _newRoom.GetComponent<RoomManager>().rightEntrance.transform.position + new Vector3(-0.5f, 0, 0);

            //enable player script
            pB.enabled = true;
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Debug.LogWarning("Player has entered: " + gameObject);
            hasPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.LogWarning("Player has left: " + gameObject);
            hasPlayer = false;
        }
    }

    private void DisableEnemy(GameObject enemy)
    {
        EnemyTrigger eT = enemy.GetComponentInChildren<EnemyTrigger>();
        eT.active = false;

        Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        NeedleAI n = enemy.GetComponent<NeedleAI>();
        WaspAI w = enemy.GetComponent<WaspAI>();
        BatAI b = enemy.GetComponent<BatAI>();
        MoleAI m = enemy.GetComponent<MoleAI>();
        SlugAI s = enemy.GetComponent<SlugAI>();

        if(n != null)
        {
            n.enabled = false;
        }
        else if(w != null)
        {
            w.enabled = false;
        }
        else if (b != null)
        {
            b.enabled = false;
        }
        else if (m != null)
        {
            m.enabled = false;
        }
        else if (s != null)
        {
            s.enabled = false;
        }
        else
        {
            Debug.LogWarning(enemy + " is no enemy");
        }
    }

    private void EnableEnemy(GameObject enemy)
    {
        EnemyTrigger eT = enemy.GetComponentInChildren<EnemyTrigger>();
        eT.active = true;

        Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = true;
        }

        NeedleAI n = enemy.GetComponent<NeedleAI>();
        WaspAI w = enemy.GetComponent<WaspAI>();
        BatAI b = enemy.GetComponent<BatAI>();
        MoleAI m = enemy.GetComponent<MoleAI>();
        SlugAI s = enemy.GetComponent<SlugAI>();

        if (n != null)
        {
            n.enabled = true;
        }
        else if (w != null)
        {
            w.enabled = true;
        }
        else if (b != null)
        {
            b.enabled = true;
        }
        else if (m != null)
        {
            m.enabled = true;
        }
        else if (s != null)
        {
            s.enabled = true;
        }
        else
        {
            Debug.LogWarning(enemy + " is no enemy");
        }
    }
}