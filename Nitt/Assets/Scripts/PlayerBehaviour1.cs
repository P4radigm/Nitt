using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour1 : MonoBehaviour
{
    [Header("Teleport tweaks")]
    [SerializeField] private float maxTeleportDistance = 1;

    [Header("Game Feel")]
    [SerializeField] private float timeSlow = 0.5f;

    [Header("Needed")]
    [SerializeField] private GameObject teleportTarget = null;



    private float fixedDeltaTime;
    private bool notTeleported = false;
    private bool beginPhaseMouse = true;

    //Teleport Point Calculation
    private Vector2 teleportPoint = Vector2.zero;
    private Vector2 beginPointPos = Vector2.zero;
    private Vector2 endPointPos = Vector2.zero;
    private Vector2 moveDirection = Vector2.up;
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

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Player Input
        if (Input.GetMouseButton(0))
        {
            TimeSlow();
            CalcTargetPos();

            notTeleported = true;

            //graphics
            teleportTarget.SetActive(true);
            teleportTarget.transform.position = teleportPoint;

            //Debug
            //Debug.DrawLine(beginPointPos, endPointPos);
        }
        else
        {
            if (notTeleported)
            {
                TimeContinue();
                Teleport();
                
                notTeleported = false;
            }

            //graphics
            teleportTarget.SetActive(false);
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
        }

        endPointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        moveDirection = endPointPos - beginPointPos;
        moveDirection.Normalize();
        moveDirectionAngle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;

        moveDirectionDistance = Mathf.Clamp(Vector2.Distance(endPointPos, beginPointPos), 0f, maxTeleportDistance);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, Mathf.Infinity);

        if(hit.collider != null)
        {
            float distance = Mathf.Abs(Vector2.Distance(transform.position, hit.point));
            if(distance < maxTeleportDistance)
            {
                Vector2 Offset = hit.normal * (teleportTarget.transform.localScale/2);

                teleportPoint = hit.point + Offset;
            }
            else
            {
                teleportPoint = new Vector2(transform.position.x, transform.position.y) + (moveDirection * moveDirectionDistance);
            }
        }
        else
        {
            teleportPoint = new Vector2(transform.position.x, transform.position.y) + (moveDirection * moveDirectionDistance);
        }

        //Debug.Log(hit.collider);
        Debug.DrawLine(transform.position, hit.point, Color.green);
        Debug.DrawLine(hit.point, hit.point+hit.normal, Color.red);
    }

    private void Teleport()
    {
        transform.position = teleportPoint;
        playerRigidbody2D.velocity = Vector2.zero;

        beginPhaseMouse = true;
    }
}
