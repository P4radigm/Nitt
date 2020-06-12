using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BatAI : MonoBehaviour
{
    private Transform target;
    [Header("Needed")]
    [SerializeField] private GameObject tpTarget;

    [Header("WayFinding Settings")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float nextWaypointDistance = 3f;

    [Header("TP Settings")]
    [SerializeField] private float teleportDistance = 2f;
    [Space]
    [SerializeField] private float startTpCooldownTime = 10f;
    [Space]
    [SerializeField] private float brakeTimer = 1f;
    [SerializeField] private float teleportTimer = 1f;
    [Space]
    [SerializeField] private float newDrag = 3f;
    [SerializeField] private float newAngDrag = 3f;


    private float tpCooldownTime = 0;
    private float intialDrag;
    private float intialAngDrag;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool tpAttackInProgress = false;

    private Seeker seeker;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        tpTarget.GetComponent<Renderer>().sortingOrder = 25;
        tpTarget.SetActive(false);
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        intialDrag = rb.drag;
        intialAngDrag = rb.angularDrag;

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error && p.CompleteState != PathCompleteState.Partial)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(path == null)
        {
            if(Vector2.Distance(rb.position, target.position) < teleportDistance && tpCooldownTime <= 0)
            {
                StartCoroutine(TeleportAttack());
                tpCooldownTime = startTpCooldownTime;
            }
            else
            {
                tpCooldownTime -= Time.deltaTime;
            }

            return;
        }

        //if(path.CompleteState == PathCompleteState.Partial)
        //{
        //    return;
        //}

        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
        }
        else
        {
            reachedEndOfPath = false;
        }

        if(Vector2.Distance(rb.position, target.position) > teleportDistance && !tpAttackInProgress)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = direction * speed * Time.deltaTime;

            rb.AddForce(force);

            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if(distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
        }
        else if(tpCooldownTime <= 0)
        {
            StartCoroutine(TeleportAttack());
            tpCooldownTime = startTpCooldownTime;
        }
        else
        {
            tpCooldownTime -= Time.deltaTime;
        }

    }

    private IEnumerator TeleportAttack()
    {
        tpAttackInProgress = true;
        Collider2D col = GetComponent<Collider2D>();
        //col.isTrigger = true;

        //stop moving
        rb.drag = newDrag;
        rb.angularDrag = newAngDrag;
        yield return new WaitForSeconds(brakeTimer);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        //show target
        tpTarget.SetActive(true);
        tpTarget.transform.position = target.position;

        //give player time to respond
        yield return new WaitForSeconds(teleportTimer);

        //tp to target
        rb.position = tpTarget.transform.position;
        rb.drag = intialDrag;
        tpTarget.SetActive(false);


        col.isTrigger = false;
        tpAttackInProgress = false;

        yield return null;
    }
}
