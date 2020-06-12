using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastCheck1 : MonoBehaviour
{
    public int distIndex;
    public RaycastHit2D[] rayCastHits = new RaycastHit2D[4];
    public float[] distances = new float[4];

    // Update is called once per frame
    void Update()
    {
        rayCastHits[0] = Physics2D.Raycast(transform.position, Vector2.up, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.up * 1000, Color.magenta);
        rayCastHits[1] = Physics2D.Raycast(transform.position, Vector2.right, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.right * 1000, Color.magenta);
        rayCastHits[2] = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.down * 1000, Color.magenta);
        rayCastHits[3] = Physics2D.Raycast(transform.position, Vector2.left, Mathf.Infinity, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.left * 1000, Color.magenta);

        if (rayCastHits[0].collider != null) { distances[0] = rayCastHits[0].distance; }
        else { distances[0] = Mathf.Infinity; }

        if (rayCastHits[1].collider != null) { distances[1] = rayCastHits[1].distance; }
        else { distances[1] = Mathf.Infinity; }

        if (rayCastHits[2].collider != null) { distances[2] = rayCastHits[2].distance; }
        else { distances[2] = Mathf.Infinity; }

        if (rayCastHits[3].collider != null) { distances[3] = rayCastHits[3].distance; }
        else { distances[3] = Mathf.Infinity; }

        float maxDist = Mathf.Infinity;

        for (int i = 0; i < distances.Length; i++)
        {
            if (distances[i] < maxDist)
            {
                maxDist = distances[i];
                distIndex = i;
            }
        }
    }
}
