using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Teleport tweaks")]
    [SerializeField] private float maxTeleportDistance = 1;

    [Header("Game Feel")]
    [SerializeField] private float timeSlow = 0.5f;
    [SerializeField] private float speedMod = 1f;

    [Header("Needed")]
    [SerializeField] private GameObject teleportTargetGraphic = null;
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
            teleportTargetGraphic.SetActive(true);
            //velocity thing for graphic
            RayCastCheck ttRayCastCheck = teleportTarget.GetComponent<RayCastCheck>();
            RayCastCheck ttgRayCastCheck = teleportTargetGraphic.GetComponent<RayCastCheck>();

            if (ttRayCastCheck.distIndex != ttgRayCastCheck.distIndex && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > 0 && ttRayCastCheck.distances[ttRayCastCheck.distIndex] > ttgRayCastCheck.distances[ttgRayCastCheck.distIndex])
            {
                teleportTargetGraphic.transform.position = teleportTarget.transform.position;
            }
            else
            {
                Vector2 speed = (teleportPoint - new Vector2(teleportTargetGraphic.transform.position.x, teleportTargetGraphic.transform.position.y)) * speedMod * Time.deltaTime;
                teleportTargetGraphic.GetComponent<Rigidbody2D>().velocity = speed;
                teleportTarget.transform.position = teleportPoint;
            }

            //Debug
            //Debug.DrawLine(beginPointPos, endPointPos);
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

            //graphics
            teleportTargetGraphic.SetActive(false);
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
            teleportTargetGraphic.transform.position = transform.position;
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
        teleportTargetGraphic.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        beginPhaseMouse = true;
    }
}
