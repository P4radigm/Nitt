using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlugAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float maxDistanceGround;
    [SerializeField] private float minDistanceWall;

    [Header("Shoot Wasp Settings")]
    [SerializeField] private AnimationCurve waspGFXCurve;
    [SerializeField] private float maxYWaspGFX;
    [SerializeField] private float waspChargeTime;
    [SerializeField] private float pauzeBeforeShoot;
    [SerializeField] private float afterShotCooldown;

    [Header("Needed")]
    [SerializeField] private GameObject rightCheck;
    [SerializeField] private GameObject leftCheck;
    [SerializeField] private GameObject waspGraphic;
    [SerializeField] private GameObject waspPrefab;

    private bool readyToFire = false;
    private Vector2 direction = Vector2.zero;
    private Vector2 initialWaspPos;

    private Coroutine waspGFXroutine;
    private Coroutine ShootWaspRoutine = null;


    // Start is called before the first frame update
    void Start()
    {
        initialWaspPos = waspGraphic.transform.localPosition;

        RaycastHit2D initGroundCheck = Physics2D.Raycast(transform.position, transform.up * -1);

        if(initGroundCheck.collider != null)
        {
            transform.position = initGroundCheck.point;
        }

        int randDir = Random.Range(0, 2);

        if(randDir == 0)
        {
            direction = transform.right;
        }
        else if (randDir == 1)
        {
            direction = transform.right * -1;
        }

        StartWaspGFX();
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToFire)
        {
            CheckForPlayer();
        }

        //movement
        CalcDir();

        if(direction != Vector2.zero)
        {
            transform.position += new Vector3(direction.x * patrolSpeed * Time.deltaTime, direction.y * patrolSpeed * Time.deltaTime);
        }
    }

    private void CalcDir()
    {
        RaycastHit2D groundCheckRight = Physics2D.Raycast(rightCheck.transform.position, transform.up * -1, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment") | 1 << LayerMask.NameToLayer("Player"));
        RaycastHit2D groundCheckLeft = Physics2D.Raycast(leftCheck.transform.position, transform.up * -1, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment") | 1 << LayerMask.NameToLayer("Player"));
        RaycastHit2D wallCheckRight = Physics2D.Raycast(rightCheck.transform.position, transform.right, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment") | 1 << LayerMask.NameToLayer("Player"));
        RaycastHit2D wallCheckLeft = Physics2D.Raycast(leftCheck.transform.position, transform.right * -1, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment") | 1 << LayerMask.NameToLayer("Player"));

        if (groundCheckRight.distance > maxDistanceGround || wallCheckRight.distance <= minDistanceWall)
        {
            direction = transform.right * -1;
        }
        else if (groundCheckLeft.distance > maxDistanceGround || wallCheckLeft.distance <= minDistanceWall)
        {
            direction = transform.right;
        }
    }

    private void CheckForPlayer()
    {
        RaycastHit2D checkForPlayer = Physics2D.Raycast(transform.position, transform.up, Mathf.Infinity, 1 << LayerMask.NameToLayer("Player"));

        if(checkForPlayer.collider != null)
        {
            //fire Wasp
            if (ShootWaspRoutine == null) { ShootWaspRoutine = StartCoroutine(IEShootWasp()); }
        }
    }

    private IEnumerator IEShootWasp()
    {
        //stop moving
        Vector2 oldDir = direction;
        direction = Vector2.zero;
        yield return new WaitForSeconds(pauzeBeforeShoot);

        //shoot
        Instantiate(waspPrefab, waspGraphic.transform.position, Quaternion.identity, transform.parent);
        waspGraphic.SetActive(false);
        readyToFire = false;

        yield return new WaitForSeconds(afterShotCooldown);

        //start moving again
        direction = oldDir;
        StartWaspGFX();

        ShootWaspRoutine = null;

        yield return null;
    }

    private void StartWaspGFX()
    {
        if (waspGFXroutine != null) { StopCoroutine(waspGFXroutine); }
        waspGFXroutine = StartCoroutine(IEWaspGFX());
    }

    private IEnumerator IEWaspGFX()
    {
        float lerpTime = 0;

        waspGraphic.SetActive(true);
        waspGraphic.transform.localPosition = initialWaspPos;

        while (lerpTime < 1)
        {
            //move 0.140 in pos y dir
            lerpTime += Time.deltaTime / waspChargeTime;
            float evaluatedLerpTime = waspGFXCurve.Evaluate(lerpTime);
            float newY = Mathf.Lerp(initialWaspPos.y, initialWaspPos.y + maxYWaspGFX, evaluatedLerpTime);
            waspGraphic.transform.localPosition = new Vector3(0, newY, 0); 
            yield return null;
        }

        readyToFire = true;

        yield return null;
    }
}
