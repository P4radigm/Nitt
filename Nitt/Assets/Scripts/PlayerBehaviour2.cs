using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PlayerBehaviour2 : MonoBehaviour
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
    public int currency;

    [Header("Player Stats Tweaks")]
    [SerializeField] int maxHitPoints;
    [SerializeField] int maxTeleportCells;
    [SerializeField] float initialTpDamageOutput;
    [SerializeField] float inititalcontactDamageOutput;
    [SerializeField] int startCurrency;

    [Header("Needed")]
    [SerializeField] private GameObject teleportTargetGraphic = null;
    [SerializeField] private GameObject teleportTarget = null;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider teleportSlider;

    private SpriteRenderer[] spR;
    private float fixedDeltaTime;
    private bool notTeleported = false;
    private bool beginPhaseMouse = true;

    //Teleport Point Calculation
    private Vector2 teleportPoint = Vector2.zero;
    private Vector2 beginPointPos = Vector2.zero;
    private Vector2 endPointPos = Vector2.zero;
    private Vector2 moveDirection = Vector2.up;
    [HideInInspector] public Vector2 lastGroundPos = Vector2.zero;
    [HideInInspector] public bool justTP = false;
    private float moveDirectionDistance = 0;
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
        currency = startCurrency;

        healthSlider.maxValue = maxHitPoints;
        teleportSlider.maxValue = maxTeleportCells;

        spR = teleportTargetGraphic.GetComponentsInChildren<SpriteRenderer>();
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
            for (int i = 0; i < spR.Length; i++)
            {
                spR[i].enabled = true;
            }

            RayCastCheck1 ttRayCastCheck = teleportTarget.GetComponent<RayCastCheck1>();
            RayCastCheck1 ttgRayCastCheck = teleportTargetGraphic.GetComponent<RayCastCheck1>();

            teleportTarget.transform.position = teleportPoint;

            Vector2 tptpNeededAngle = teleportTarget.transform.position - teleportTargetGraphic.transform.position;
            float tptpNeededDistance = Vector2.Distance(teleportTargetGraphic.transform.position, teleportTarget.transform.position);
            RaycastHit2D tptpNeeded = Physics2D.Raycast(teleportTargetGraphic.transform.position, tptpNeededAngle.normalized, tptpNeededDistance, 1 << LayerMask.NameToLayer("Environment"));

            if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] <= ttgRayCastCheck.distances[ttgRayCastCheck.distIndex] && tptpNeeded.collider != null)
            {
                teleportTargetGraphic.transform.position = ttRayCastCheck.rayCastHits[ttRayCastCheck.distIndex].point + (ttRayCastCheck.rayCastHits[ttRayCastCheck.distIndex].normal) * (transform.localScale.x / 2);
            }
            else if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > ttgRayCastCheck.distances[ttgRayCastCheck.distIndex] && tptpNeeded.collider != null)
            {
                teleportTargetGraphic.transform.position = teleportTarget.transform.position;
            }
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
            if (groundRay.collider != null && groundRay.collider.tag == "Ground")
            {
                lastGroundPos = transform.position;
            }
            //graphics
            for (int i = 0; i < spR.Length; i++)
            {
                spR[i].enabled = false;
            }
            //teleportTargetGraphic.SetActive(false);
        }

        if (hitPoints > maxHitPoints)
        {
            hitPoints = maxHitPoints;
        }

        if (teleportCells > maxTeleportCells)
        {
            teleportCells = maxTeleportCells;
        }

        healthSlider.value = hitPoints;
        teleportSlider.value = teleportCells;

        if (hitPoints == 0)
        {
            Death();
        }
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
            //teleportTargetGraphic.transform.position = transform.position;
        }

        endPointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        moveDirection = endPointPos - beginPointPos;
        moveDirection.Normalize();
        moveDirectionAngle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;

        moveDirectionDistance = Mathf.Clamp(Vector2.Distance(endPointPos, beginPointPos), 0f, maxTeleportDistance);

        teleportPoint = new Vector2(transform.position.x, transform.position.y) + (moveDirection * moveDirectionDistance);
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
