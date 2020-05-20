using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PlayerBehaviour : MonoBehaviour
{
    [Header("Teleport tweaks")]
    [SerializeField] private float maxTeleportDistance = 1;
    [SerializeField] private float distanceThreshold = 0.1f;
    [SerializeField] private float justTPTimer = 0.1f;

    [Header("Game Feel")]
    [SerializeField] private float timeSlow = 0.5f;

    [Header("Player Stats")]
    public int hitPoints;
    public int teleportCells;
    public float tpDamageOutput;
    public float contactDamageOutput;

    [Header("Player Start Stats")]
    [SerializeField] int maxHitPoints;
    [SerializeField] int maxTeleportCells;
    [SerializeField] float initialTpDamageOutput;
    [SerializeField] float inititalcontactDamageOutput;

    [Header("Needed")]
    [SerializeField] private GameObject teleportTargetGraphic = null;
    [SerializeField] private GameObject teleportTarget = null;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider teleportSlider;

    private float fixedDeltaTime;
    private bool notTeleported = false;
    private bool beginPhaseMouse = true;

    //Teleport Point Calculation
    private Vector2 teleportPoint = Vector2.zero;
    private Vector2 beginPointPos = Vector2.zero;
    private Vector2 endPointPos = Vector2.zero;
    private Vector2 moveDirection = Vector2.up;
    private Vector2 tpPoint = Vector2.zero;
    [HideInInspector] public Vector2 lastGroundPos = Vector2.zero;
    [HideInInspector] public bool justTP = false;
    private float moveDirectionDistance = 0;
    private float lastShortestDistance = 0;
    private int lastDistIndex = 0;
    private bool edgeTPNeeded = true;
    float moveDirectionAngle = 0;

    private Rigidbody2D playerRigidbody2D;

    private void Awake()
    {
        //Screen.SetResolution(2160, 1080, 0, 60);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        Input.simulateMouseWithTouches = true;
        playerRigidbody2D = GetComponent<Rigidbody2D>();

        hitPoints = maxHitPoints;
        teleportCells = maxTeleportCells;
        tpDamageOutput = initialTpDamageOutput;
        contactDamageOutput = inititalcontactDamageOutput;

        healthSlider.maxValue = maxHitPoints;
        teleportSlider.maxValue = maxTeleportCells;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        //Player Input
        if (Input.GetMouseButton(0) && teleportCells > 0)
        {
            TimeSlow();
            CalcTargetPos();

            notTeleported = true;

            //graphics
            teleportTargetGraphic.SetActive(true);
            //velocity thing for graphic
            RayCastCheck ttRayCastCheck = teleportTarget.GetComponent<RayCastCheck>();
            RayCastCheck ttgRayCastCheck = teleportTargetGraphic.GetComponent<RayCastCheck>();

            teleportTarget.transform.position = teleportPoint;

            if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] <= ttgRayCastCheck.distances[ttgRayCastCheck.distIndex])
            {
                teleportTargetGraphic.transform.position = ttRayCastCheck.rayCastHits[ttRayCastCheck.distIndex].point + (ttRayCastCheck.rayCastHits[ttRayCastCheck.distIndex].normal) * (transform.localScale.x / 2);
            }
            else if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > ttgRayCastCheck.distances[ttgRayCastCheck.distIndex])
            {
                teleportTargetGraphic.transform.position = teleportTarget.transform.position;
            }
            else if (ttRayCastCheck.distances[ttRayCastCheck.distIndex] <= 0 && lastShortestDistance <= 0.5f && lastShortestDistance != 0f && ttgRayCastCheck.distances[ttgRayCastCheck.distIndex] != lastShortestDistance && edgeTPNeeded == true)
            {
                edgeTPNeeded = false;
                Debug.Log(lastShortestDistance);
                teleportTargetGraphic.transform.position = ttgRayCastCheck.rayCastHits[lastDistIndex].point + ttgRayCastCheck.rayCastHits[lastDistIndex].normal*0.5f;
            }
            else if(ttRayCastCheck.distances[ttRayCastCheck.distIndex] >= 0)
            {
                teleportTargetGraphic.transform.position = tpPoint;

                edgeTPNeeded = true;
                lastShortestDistance = ttgRayCastCheck.distances[ttgRayCastCheck.distIndex];
                lastDistIndex = ttgRayCastCheck.distIndex;
            }
            else
            {

            }

            //Debug
            Debug.DrawLine(beginPointPos, endPointPos, Color.magenta);
            Debug.DrawLine(transform.position, teleportPoint, Color.red);
            //Debug.Log((Quaternion.Euler(0, 0, 90) * moveDirection).magnitude);
        }
        else
        {
            if (notTeleported)
            {
                TimeContinue();
                Teleport();
                
                notTeleported = false;
            }

            //GroundCheck
            RaycastHit2D groundRay = Physics2D.Raycast(transform.position, Vector2.down, 0.6f);
            if(groundRay.collider != null && groundRay.collider.tag == "Ground")
            {
                lastGroundPos = transform.position;
            }
            //graphics
            teleportTargetGraphic.SetActive(false);
        }

        if (hitPoints > maxHitPoints)
        {
            hitPoints = maxHitPoints;
        }

        if(teleportCells > maxTeleportCells)
        {
            teleportCells = maxTeleportCells;
        }

        healthSlider.value = hitPoints;
        teleportSlider.value = teleportCells;

        if(hitPoints == 0)
        {
            Death();
        }

        //Debug.Log(justTP);
    }

    private void TimeSlow()
    {
        Time.timeScale = timeSlow;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    private void TimeContinue()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    private void CalcTargetPos()
    {
        if (beginPhaseMouse == true)
        {
            beginPointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            beginPhaseMouse = false;
            teleportTargetGraphic.transform.position = transform.position;
        }

        endPointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        moveDirection = endPointPos - beginPointPos;
        moveDirection.Normalize();
        moveDirectionAngle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;

        moveDirectionDistance = Mathf.Clamp(Vector2.Distance(endPointPos, beginPointPos), 0f, maxTeleportDistance);

        teleportPoint = new Vector2(transform.position.x, transform.position.y) + (moveDirection * moveDirectionDistance);

        tpPoint = teleportPoint;

        RayCastCheck rcC = teleportTargetGraphic.GetComponent<RayCastCheck>();

        #region CollisionCheck

        if (rcC.rayCastHits[0].distance <= 0.5f && rcC.rayCastHitsC[0].distance > 0.5f && rcC.rayCastHitsC[1].distance > 0.5f)
        {
            //up
            if (tpPoint.y > rcC.rayCastHits[0].point.y - 0.5f)
            {
                tpPoint.y = rcC.rayCastHits[0].point.y - 0.5f;
            }
        }

        if (rcC.rayCastHits[1].distance <= 0.5f && rcC.rayCastHitsC[2].distance > 0.5f && rcC.rayCastHitsC[3].distance > 0.5f)
        {
            //right
            if (tpPoint.x > rcC.rayCastHits[1].point.x - 0.5f)
            {
                tpPoint.x = rcC.rayCastHits[1].point.x - 0.5f;
            }
        }

        if (rcC.rayCastHits[2].distance <= 0.5f && rcC.rayCastHitsC[4].distance > 0.5f && rcC.rayCastHitsC[5].distance > 0.5f)
        {
            //down
            if (tpPoint.y < rcC.rayCastHits[2].point.y + 0.5f)
            {
                tpPoint.y = rcC.rayCastHits[2].point.y + 0.5f;
            }
        }

        if (rcC.rayCastHits[3].distance <= 0.5f && rcC.rayCastHitsC[6].distance > 0.5f && rcC.rayCastHitsC[7].distance > 0.5f)
        {
            //left
            if (tpPoint.x < rcC.rayCastHits[3].point.x + 0.5f)
            {
                tpPoint.x = rcC.rayCastHits[3].point.x + 0.5f;
            }
        }

        if (rcC.rayCastHitsC[0].distance <= 0.5f && rcC.rayCastHits[0].distance > 0.5f && rcC.rayCastHitsC[1].distance > 0.5f)
        {
            //up
            if (tpPoint.x < rcC.rayCastHitsC[0].point.y - 0.5f)
            {
                tpPoint.x = rcC.rayCastHitsC[0].point.y - 0.5f;
            }
        }

        if (rcC.rayCastHitsC[1].distance <= 0.5f && rcC.rayCastHits[0].distance > 0.5f && rcC.rayCastHitsC[0].distance > 0.5f)
        {
            //up
            if (tpPoint.x < rcC.rayCastHitsC[1].point.y - 0.5f)
            {
                tpPoint.x = rcC.rayCastHitsC[1].point.y - 0.5f;
            }
        }

        if (rcC.rayCastHitsC[2].distance <= 0.5f && rcC.rayCastHits[1].distance > 0.5f && rcC.rayCastHitsC[3].distance > 0.5f)
        {
            //right
            if (tpPoint.x > rcC.rayCastHitsC[2].point.x - 0.5f)
            {
                tpPoint.x = rcC.rayCastHitsC[2].point.x - 0.5f;
            }
        }

        if (rcC.rayCastHitsC[3].distance <= 0.5f && rcC.rayCastHits[1].distance > 0.5f && rcC.rayCastHitsC[2].distance > 0.5f)
        {
            //right
            if (tpPoint.x > rcC.rayCastHitsC[3].point.x - 0.5f)
            {
                tpPoint.x = rcC.rayCastHitsC[3].point.x - 0.5f;
            }
        }

        if (rcC.rayCastHitsC[4].distance <= 0.5f && rcC.rayCastHits[2].distance > 0.5f && rcC.rayCastHitsC[5].distance > 0.5f)
        {
            //down
            if (tpPoint.y < rcC.rayCastHitsC[4].point.y + 0.5f)
            {
                tpPoint.y = rcC.rayCastHitsC[4].point.y + 0.5f;
            }
        }

        if (rcC.rayCastHitsC[5].distance <= 0.5f && rcC.rayCastHits[2].distance > 0.5f && rcC.rayCastHitsC[4].distance > 0.5f)
        {
            //down
            if (tpPoint.y < rcC.rayCastHitsC[5].point.y + 0.5f)
            {
                tpPoint.y = rcC.rayCastHitsC[5].point.y + 0.5f;
            }
        }

        if (rcC.rayCastHitsC[6].distance <= 0.5f && rcC.rayCastHits[3].distance > 0.5f && rcC.rayCastHitsC[7].distance > 0.5f)
        {
            //left
            if (tpPoint.x < rcC.rayCastHitsC[6].point.x + 0.5f)
            {
                tpPoint.x = rcC.rayCastHitsC[6].point.x + 0.5f;
            }
        }

        if (rcC.rayCastHitsC[7].distance <= 0.5f && rcC.rayCastHits[3].distance > 0.5f && rcC.rayCastHitsC[6].distance > 0.5f)
        {
            //left
            if (tpPoint.x < rcC.rayCastHitsC[7].point.x + 0.5f)
            {
                tpPoint.x = rcC.rayCastHitsC[7].point.x + 0.5f;
            }
        }

        #endregion

        #region OldCalc
        //when teleportPoint is in a wall
        //RaycastHit2D wallCollisionCheck = Physics2D.Raycast(transform.position, moveDirection, Vector2.Distance(transform.position, teleportPoint), 1 << 8);
        //Debug.Log(wallCollisionCheck.collider);
        //if (wallCollisionCheck.collider != null)
        //{
        //    float deltaY = transform.position.y - wallCollisionCheck.point.y;
        //    float deltaX = transform.position.x - wallCollisionCheck.point.x;

        //    float tangentPointOffset = (0.5f * deltaX)/deltaY;

        //    if(deltaX < deltaY)
        //    {
        //        tpPoint = wallCollisionCheck.point + wallCollisionCheck.normal * 0.5f + Vector2.Perpendicular(wallCollisionCheck.normal) * -tangentPointOffset;
        //    }
        //    else
        //    {
        //        tpPoint = wallCollisionCheck.point + wallCollisionCheck.normal * 0.5f + Vector2.Perpendicular(wallCollisionCheck.normal) * -tangentPointOffset;
        //    }
        //}
        //else
        //{
        //    tpPoint = teleportPoint;
        //}
        #endregion
    }

    private void Teleport()
    {
        transform.position = teleportTargetGraphic.transform.position;
        playerRigidbody2D.velocity = Vector2.zero;
        //teleportTargetGraphic.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        justTP = true;
        StartCoroutine(JustTPCooldown());

        teleportCells--;

        beginPhaseMouse = true;
    }

    public void EnvironmentDamage()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = lastGroundPos;
        hitPoints--;
    }

    private void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator JustTPCooldown()
    {
        yield return new WaitForSeconds(justTPTimer);
        justTP = false;
    }
}
