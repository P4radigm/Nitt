using System.Collections;
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
        public Vector2 spawnPos;
    }

    [Header("Entrance/Exits")]
    [SerializeField] private GameObject topEntrance;
    [SerializeField] private GameObject rightEntrance;
    [SerializeField] private GameObject downEntrance;
    [SerializeField] private GameObject leftEntrance;

    [Header("Enemy Spawn Options")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private EnemyToBeSpawned[] enemiesToBeSpawned;
    [SerializeField] private float secondsBetweenSpawns;
    private List<GameObject> spawnedEnemies = new List<GameObject>();


    public bool isCompleted;
    public bool hasPlayer;

    private bool spawnedEnemiesYet;
    private GameManager gm = GameManager.instance;

    private Coroutine spawnEnemiesRoutine = null;

    void Start()
    {
        
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

            if (spawnedEnemies.Count == 0)
            {
                isCompleted = true;
            }
        }
    }

    private IEnumerator SpawnEnemiesIE()
    {

        for (int i = 0; i < enemiesToBeSpawned.Length; i++)
        {
            Vector3 vec3SpawnPoint = new Vector3(enemiesToBeSpawned[i].spawnPos.x, enemiesToBeSpawned[i].spawnPos.y, 0);
            yield return new WaitForSeconds(secondsBetweenSpawns);
            GameObject Enemy = Instantiate(enemyPrefabs[(int)enemiesToBeSpawned[i].enemyType], vec3SpawnPoint, Quaternion.identity, transform);
            //Particles
            spawnedEnemies.Add(Enemy);
        }

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
}
