using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringText : MonoBehaviour
{
    [SerializeField] private float normalScrollSpeed = 10;
    private Shader enemyShader;
    private OptionsManager oM;
    private Material newMat;
    private Renderer R;

    private bool switched = false;

    // Start is called before the first frame update
    void Start()
    {
        oM = OptionsManager.instance;
        enemyShader = Shader.Find("NittShader/ColorCycle");

        R = GetComponent<Renderer>();

        newMat = new Material(enemyShader);

        newMat.SetFloat("_ColorOffset", Random.Range(0, 100f));

        if (!oM.flashingColours)
        {
            newMat.SetFloat("_ScrollSpeed", 0);
        }
        else
        {
            newMat.SetFloat("_ScrollSpeed", normalScrollSpeed);
        }

        R.material = newMat;
    }

    void Update()
    {        
        if (!oM.flashingColours && switched)
        {
            newMat.SetFloat("_ColorOffset", Random.Range(0, 100f));
            newMat.SetFloat("_ScrollSpeed", 0);
            R.material = newMat;
            switched = false;
        }
        
        if(oM.flashingColours && !switched)
        {
            newMat.SetFloat("_ColorOffset", Random.Range(0, 100f));
            newMat.SetFloat("_ScrollSpeed", normalScrollSpeed);
            R.material = newMat;
            switched = true;
        }
    }
}
