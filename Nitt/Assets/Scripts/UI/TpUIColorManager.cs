using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpUIColorManager : MonoBehaviour
{
    [HideInInspector] public bool rainbowOn = false;
    [SerializeField] private ParticleSystem drainParticles;
    [SerializeField] private SpriteRenderer afterTpBar;
    [SerializeField] private ParticleSystem tpLossParticles;
    [SerializeField] private float newColTime;

    private float newColTimer;
    public Color currentColor; 

    // Start is called before the first frame update
    void Start()
    {
        newColTimer = newColTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(newColTimer <= 0)
        {
            UpdateColor();
            newColTimer = newColTime;
        }
        else
        {
            newColTimer -= Time.deltaTime;
        }
    }

    private void UpdateColor()
    {
        ParticleSystem.MainModule mainDP = drainParticles.gameObject.GetComponent<ParticleSystem>().main;
        ParticleSystem.MainModule tpLossParticlesMain = tpLossParticles.gameObject.GetComponent<ParticleSystem>().main;

        currentColor = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);

        mainDP.startColor = new ParticleSystem.MinMaxGradient(new Color(currentColor.r - 0.1f, currentColor.g - 0.1f, currentColor.b - 0.1f, 1), currentColor);
        afterTpBar.color = currentColor;
        tpLossParticlesMain.startColor = currentColor;

        if (rainbowOn)
        {
            GetComponent<SpriteRenderer>().color = currentColor;
        }
    }
}
