using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleAI : MonoBehaviour
{
    [SerializeField] private float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(transform.up.x * speed * Time.deltaTime, transform.up.y * speed * Time.deltaTime, 0);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
