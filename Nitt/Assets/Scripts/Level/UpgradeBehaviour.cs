using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeBehaviour : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;

    private SpriteRenderer sp;
    private Collider2D col;
    private Rigidbody2D rb;

    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;

        sp = GetComponent<SpriteRenderer>();
        if(sp == null) { sp = gameObject.AddComponent<SpriteRenderer>(); }

        col = GetComponent<Collider2D>();
        if(col == null) { col = gameObject.AddComponent<BoxCollider2D>(); }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) { rb = gameObject.AddComponent<Rigidbody2D>(); }

        gameObject.tag = "Upgrade";

        rb.isKinematic = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerBehaviour2 pB = collision.gameObject.GetComponent<PlayerBehaviour2>();

        if (pB != null)
        {
            gm.StartCoroutine(gm.ShowUpgradeType(upgradeType));
            pB.GotUpgrade(upgradeType);
            Destroy(gameObject);
        }
    }
}
