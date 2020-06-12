using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuFunctionality : MonoBehaviour
{
    [SerializeField] private Canvas tapToPlay;
    [SerializeField] private Canvas options;
    [SerializeField] private SpriteRenderer yFlashingColours;
    [SerializeField] private SpriteRenderer nFlashingColours;
    [SerializeField] private SpriteRenderer yScreenDistortion;
    [SerializeField] private SpriteRenderer nScreenDistortion;
    private OptionsManager oM;

    // Start is called before the first frame update
    void Start()
    {
        oM = OptionsManager.instance;
        Input.simulateMouseWithTouches = true;
    }

    private void Update()
    {
        if (oM.flashingColours)
        {
            yFlashingColours.gameObject.SetActive(true);
            nFlashingColours.gameObject.SetActive(false);
        }
        else
        {
            yFlashingColours.gameObject.SetActive(false);
            nFlashingColours.gameObject.SetActive(true);
        }

        if (oM.screenDistortion)
        {
            yScreenDistortion.gameObject.SetActive(true);
            nScreenDistortion.gameObject.SetActive(false);
        }
        else
        {
            yScreenDistortion.gameObject.SetActive(false);
            nScreenDistortion.gameObject.SetActive(true);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenOptions()
    {
        tapToPlay.gameObject.SetActive(false);
        options.gameObject.SetActive(true);
    }

    public void CloseOptions()
    {
        tapToPlay.gameObject.SetActive(true);
        options.gameObject.SetActive(false);
    }

    public void ChangeFlashingColours()
    {
        if (oM.flashingColours) 
        {
            oM.flashingColours = false; 
        }
        else 
        { 
            oM.flashingColours = true; 
        }
    }

    public void ChangeScreenDistortion()
    {
        if (oM.screenDistortion)
        {
            oM.screenDistortion = false;
        }
        else
        {
            oM.screenDistortion = true;
        }
    }
}
