using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AfterTpUIBar : MonoBehaviour
{
    [SerializeField] private float maxLength;
    private PlayerBehaviour2 pB;


    // Start is called before the first frame update
    void Start()
    {
        pB = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour2>();
    }

    // Update is called once per frame
    void Update()
    {
        float maxTPJ = pB.maxTeleportJuice;
        float TPJCost = pB.baseTpjuiceCost;

        float newLength = maxLength / (maxTPJ / TPJCost);

        transform.localScale = new Vector3(newLength, transform.localScale.y, transform.localScale.z);
        transform.localPosition = new Vector3(newLength / 2, 0, 0);
    }
}
