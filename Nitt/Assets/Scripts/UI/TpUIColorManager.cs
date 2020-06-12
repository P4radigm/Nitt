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
    public Color staticColor;
    private OptionsManager oM;

    // Start is called before the first frame update
    void Start()
    {
        oM = OptionsManager.instance;
        staticColor = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);
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

        if (oM.flashingColours) { currentColor = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1); }
        else { currentColor = staticColor; }
        

        mainDP.startColor = new ParticleSystem.MinMaxGradient(new Color(currentColor.r - 0.1f, currentColor.g - 0.1f, currentColor.b - 0.1f, 1), currentColor);
        afterTpBar.color = currentColor;
        tpLossParticlesMain.startColor = currentColor;

        if (rainbowOn)
        {
            GetComponent<SpriteRenderer>().color = currentColor;
        }
    }
}
