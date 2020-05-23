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
        public Transform spawnTransform;
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

    [Header("VFX")]
    [SerializeField] private ParticleSystem enemySpawnVFX;

    public bool isCompleted;
    public bool hasPlayer;

    private bool spawnedEnemiesYet;
    private GameManager gm;
    private Shader enemyShader;
    //private Material EnemyMat = new Material(Shader.Find("ShaderGraphs/ColorCycle"));

    private Coroutine spawnEnemiesRoutine = null;

    void Start()
    {
        gm = GameManager.instance;

        enemyShader = Shader.Find("NittShader/ColorCycle");
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

            Vector3 vec3SpawnPoint = new Vector3(enemiesToBeSpawned[i].spawnTransform.position.x, enemiesToBeSpawned[i].spawnTransform.position.y, 0);

            ParticleSystem spawnFX = Instantiate(enemySpawnVFX, vec3SpawnPoint, Quaternion.identity);
            spawnFX.Play(true);

            yield return new WaitForSeconds(secondsBetweenSpawns);
            Destroy(spawnFX);
            GameObject Enemy = Instantiate(enemyPrefabs[(int)enemiesToBeSpawned[i].enemyType], vec3SpawnPoint, Quaternion.identity, transform);

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

        yield return new WaitForSeconds(secondsBetweenSpawns);

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
