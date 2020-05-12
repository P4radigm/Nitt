using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatBehaviour: MonoBehaviour
{
    [SerializeField] float batSpeed;
    [SerializeField] int damageAmount;
    [SerializeField] int cellRegenAmount;
    [SerializeField] bool noiseOn;
    [SerializeField] Vector2 noiseResetRange;
    [SerializeField] Vector2 noiseTimeRange;
    private Transform player;
    private Vector3 direction = Vector3.zero;
    private Vector3 directionOffset = Vector3.zero;
    private bool goAway = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        if (noiseOn) { RandomOffset(); }
    }

    // Update is called once per frame
    void Update()
    {
        if (!goAway)
        {
            direction = Vector3.Normalize(Vector3.Normalize(player.position - transform.position) + directionOffset);
        }
        else
        {
            direction = Vector3.Normalize((Vector3.Normalize(player.position - transform.position) + directionOffset)*-1);
        }
        transform.position += direction * batSpeed * Time.deltaTime;
    }

    void RandomOffset()
    {
        float randomTime = Random.Range(noiseResetRange.x, noiseResetRange.y);

        directionOffset = new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0);

        Invoke("CorrectAgain", randomTime);
    }

    void CorrectAgain()
    {
        float randomTime = Random.Range(noiseTimeRange.x, noiseTimeRange.y);

        directionOffset = Vector3.zero;

        Invoke("RandomOffset", randomTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        

        PlayerBehaviour pB = collision.GetComponent<PlayerBehaviour>();
        if (pB != null)
        {
            if(pB.justTP == true)
            {
                pB.teleportCells += cellRegenAmount;
            }
            else
            {
                pB.hitPoints -= damageAmount;
            }

            Destroy(gameObject);
        }    
    }
}
