using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum RoomTypeEnum
{
    L,
    T,
    R,
    B,
    LR,
    TB,
    LT,
    LB,
    RT,
    RB,
    TBL,
    TBR,
    LRT,
    LRB,
    LRBT
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [HideInInspector] public Camera mainCam;
    [SerializeField] [Range(0f, 0.5f)] private float freezeDuration;
    private bool isFrozen = false;
    private float pendingFreezeDuration = 0f;
    private Coroutine freezeRoutine = null;

    [Header("Room Management")]
    [HideInInspector] public GameObject activeRoom = null;
    [HideInInspector]  public GameObject[][] topRooms;
    [HideInInspector]  public GameObject[][] rightRooms;
    [HideInInspector]  public GameObject[][] bottomRooms;
    [HideInInspector]  public GameObject[][] leftRooms;
    public List<GameObject> spawnedRooms = new List<GameObject>();
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

    [Header("UpgradeValues")]
    public int maxHPCellAmntInc;
    public float maxTpjAmntInc;
    public float tpjRegenAmntInc;
    public float tpRangeAmntInc;
    public float tpTimeSlowAmntDec;
    public float tpDamageAmntInc;
    public float contactDamageAmntInc;

    [Header("Enemy Options")]
    public float totalEnemySpawnTime;
    public float pauzeAfterLastEnemy;
    public GameObject[] enemyPrefabs;


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

    [Header("VFX")]
    [SerializeField] private ParticleSystem onEnemyDeathParticles;
    public ParticleSystem enemySpawnFX;
    public ParticleSystem afterEnemySpawnFX;


    // Start is called before the first frame update
    void Start()
    {
        topRooms = new GameObject[8][];
        topRooms[0] = RoomsB;
        topRooms[1] = RoomsTB;
        topRooms[2] = RoomsLB;
        topRooms[3] = RoomsRB;
        topRooms[4] = RoomsTBL;
        topRooms[5] = RoomsTBR;
        topRooms[6] = RoomsLRB;
        topRooms[7] = RoomsLRBT;

        rightRooms = new GameObject[8][];
        rightRooms[0] = RoomsL;
        rightRooms[1] = RoomsLR;
        rightRooms[2] = RoomsLT;
        rightRooms[3] = RoomsLB;
        rightRooms[4] = RoomsTBL;
        rightRooms[5] = RoomsLRT;
        rightRooms[6] = RoomsLRB;
        rightRooms[7] = RoomsLRBT;

        bottomRooms = new GameObject[8][];
        bottomRooms[0] = RoomsT;
        bottomRooms[1] = RoomsTB;
        bottomRooms[2] = RoomsLT;
        bottomRooms[3] = RoomsRT;
        bottomRooms[4] = RoomsTBL;
        bottomRooms[5] = RoomsTBR;
        bottomRooms[6] = RoomsLRT;
        bottomRooms[7] = RoomsLRBT;

        leftRooms = new GameObject[8][];
        leftRooms[0] = RoomsR;
        leftRooms[1] = RoomsLR;
        leftRooms[2] = RoomsRT;
        leftRooms[3] = RoomsRB;
        leftRooms[4] = RoomsTBR;
        leftRooms[5] = RoomsLRT;
        leftRooms[6] = RoomsLRB;
        leftRooms[7] = RoomsLRBT;


        mainCam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pendingFreezeDuration > 0 && !isFrozen)
        {
            if (freezeRoutine == null)
            {
                freezeRoutine = StartCoroutine(DoFreezeIE()); 
            }
        }
    }

    public void EnemyDeath(GameObject enemyThatDies)
    {
        Vector3 lastPos = enemyThatDies.transform.position;
        Quaternion lastRot = enemyThatDies.transform.rotation;

        Destroy(enemyThatDies);

        ParticleSystem onDeathFX = Instantiate(onEnemyDeathParticles, lastPos, lastRot);
        onDeathFX.Play(true);
        Destroy(onDeathFX.gameObject, 1f);
    }

    public void AddNewGridGraph(Vector3 ggPos)
    {
        AstarData data = AstarPath.active.data;

        GridGraph gg = data.gridGraph;

        gg.center = ggPos;

        AstarPath.active.Scan();
    }

    public void Freeze()
    {
        pendingFreezeDuration = freezeDuration;
    }

    IEnumerator DoFreezeIE()
    {
        isFrozen = true;
        var originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(freezeDuration);

        Time.timeScale = originalTimeScale;
        pendingFreezeDuration = 0;
        isFrozen = false;
        freezeRoutine = null;
    }
}
