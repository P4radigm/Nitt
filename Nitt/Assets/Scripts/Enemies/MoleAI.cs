using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleAI : MonoBehaviour
{
    [SerializeField] float speed = 100f;

    private Vector2 direction = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        int startDir = Random.Range(0, 4);
        if(startDir == 0)
        {
            direction = new Vector2(0.5f, 0.5f);
        }
        else if (startDir == 1)
        {
            direction = new Vector2(0.5f, -0.5f);
        }
        else if (startDir == 2)
        {
            direction = new Vector2(-0.5f, -0.5f);
        }
        else if (startDir == 3)
        {
            direction = new Vector2(-0.5f, 0.5f);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += new Vector3(direction.x * speed * Time.deltaTime, direction.y * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision);
        Vector2 newDir = Vector2.Reflect(direction, collision.GetContact(0).normal);
        direction = newDir;      
    }
}
