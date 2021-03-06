﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthLossParticle : MonoBehaviour
{
    [SerializeField] private float maxLength;
    private PlayerBehaviour2 pB;
    private ParticleSystem pS;
    private ParticleSystem.ShapeModule shapeM;
    [SerializeField] private bool isHP;

    // Start is called before the first frame update
    void Start()
    {
        pS = GetComponent<ParticleSystem>();
        shapeM = pS.shape;
        pB = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isHP)
        {
            //Debug.Log(pS.isPlaying);
            float maxHP = pB.maxHitPoints;
            float damageInc = 1;

            float newLength = maxLength / (maxHP / damageInc);

            shapeM.scale = new Vector3(newLength, shapeM.scale.y, shapeM.scale.z);
            shapeM.position = new Vector3(newLength / 2, 0, 0);
        }
        else if (!isHP)
        {
            float maxTPJ = pB.maxTeleportJuice;
            float TPJCost = pB.baseTpjuiceCost;

            float newLength = maxLength / (maxTPJ / TPJCost);

            shapeM.scale = new Vector3(newLength, shapeM.scale.y, shapeM.scale.z);
            shapeM.position = new Vector3(newLength / 2, 0, 0);
        }
    }
}
