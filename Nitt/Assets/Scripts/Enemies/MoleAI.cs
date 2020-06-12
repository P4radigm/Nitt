using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleAI : MonoBehaviour
{
    [SerializeField] float speed = 100f;
    [SerializeField] float startShootCooldown;
    [SerializeField] bool shootyBoi;
    [SerializeField] float rayCastLength = 0.295f;

    [SerializeField] GameObject needlePrefab;
    [SerializeField] float instantiateOffset;

    private Vector2 direction = Vector2.zero;
    private float shootCooldown;

    private Shader enemyShader;
    private OptionsManager oM;

    // Start is called before the first frame update
    void Start()
    {
        oM = OptionsManager.instance;
        enemyShader = Shader.Find("NittShader/ColorCycle");

        shootCooldown = startShootCooldown;
        shootCooldown = Random.Range(1, startShootCooldown);

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
    void Update()
    {
        transform.position += new Vector3(direction.x * speed * Time.deltaTime, direction.y * speed * Time.deltaTime);

        if(shootCooldown <= 0)
        {
            if (shootyBoi)
            {
                Shoot();
            }
            shootCooldown = startShootCooldown;
        }
        else
        {
            shootCooldown -= Time.deltaTime;
        }

        RaycastHit2D rayCastHitUp = Physics2D.Raycast(transform.position, Vector2.up, rayCastLength, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.up * rayCastLength, Color.cyan);
        RaycastHit2D rayCastHitRight = Physics2D.Raycast(transform.position, Vector2.right, rayCastLength, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.right * rayCastLength, Color.cyan);
        RaycastHit2D rayCastHitDown = Physics2D.Raycast(transform.position, Vector2.down, rayCastLength, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.down * rayCastLength, Color.cyan);
        RaycastHit2D rayCastHitLeft = Physics2D.Raycast(transform.position, Vector2.left, rayCastLength, 1 << LayerMask.NameToLayer("Environment"));
        Debug.DrawRay(transform.position, Vector2.left * rayCastLength, Color.cyan);

        if(rayCastHitUp.collider != null)
        {
            Vector2 newDir = Vector2.Reflect(direction, rayCastHitUp.normal);
            direction = newDir;
        }

        if (rayCastHitRight.collider != null)
        {
            Vector2 newDir = Vector2.Reflect(direction, rayCastHitRight.normal);
            direction = newDir;
        }

        if (rayCastHitDown.collider != null)
        {
            Vector2 newDir = Vector2.Reflect(direction, rayCastHitDown.normal);
            direction = newDir;
        }

        if (rayCastHitLeft.collider != null)
        {
            Vector2 newDir = Vector2.Reflect(direction, rayCastHitLeft.normal);
            direction = newDir;
        }
    }

    private void Shoot()
    {
        GameObject needle =  Instantiate(needlePrefab, transform.position + new Vector3(instantiateOffset, instantiateOffset, 0), Quaternion.Euler(0, 0, -45), transform.parent);
        GameObject needle1 = Instantiate(needlePrefab, transform.position + new Vector3(instantiateOffset, -instantiateOffset, 0), Quaternion.Euler(0, 0, -135), transform.parent);
        GameObject needle2 = Instantiate(needlePrefab, transform.position + new Vector3(-instantiateOffset, -instantiateOffset, 0), Quaternion.Euler(0, 0, 135), transform.parent);
        GameObject needle3 = Instantiate(needlePrefab, transform.position + new Vector3(-instantiateOffset, instantiateOffset, 0), Quaternion.Euler(0, 0, 45), transform.parent);

        Material needleMat = new Material(enemyShader);
        Material needle1Mat = new Material(enemyShader);
        Material needle2Mat = new Material(enemyShader);
        Material needle3Mat = new Material(enemyShader);

        needleMat.SetFloat("_ColorOffset", Random.Range(0, 100f));
        needle1Mat.SetFloat("_ColorOffset", Random.Range(0, 100f));
        needle2Mat.SetFloat("_ColorOffset", Random.Range(0, 100f));
        needle3Mat.SetFloat("_ColorOffset", Random.Range(0, 100f));

        if (!oM.flashingColours) 
        {
            needleMat.SetFloat("_ScrollSpeed", 0); 
            needle1Mat.SetFloat("_ScrollSpeed", 0); 
            needle2Mat.SetFloat("_ScrollSpeed", 0); 
            needle3Mat.SetFloat("_ScrollSpeed", 0); 
        }

        needle.GetComponentInChildren<Renderer>().material = needleMat;
        needle1.GetComponentInChildren<Renderer>().material = needle1Mat;
        needle2.GetComponentInChildren<Renderer>().material = needle2Mat;
        needle3.GetComponentInChildren<Renderer>().material = needle3Mat;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(collision);
        Vector2 newDir = Vector2.Reflect(direction, collision.GetContact(0).normal);
        direction = newDir;      
    }
}
