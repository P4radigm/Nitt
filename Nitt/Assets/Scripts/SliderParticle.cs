using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderParticle : MonoBehaviour
{
    [SerializeField] private Slider attachedSlider;
    [SerializeField] private float xStart = 110f;
    [SerializeField] private float xEnd = 110f;
    [SerializeField] private float maxOffset;
    [SerializeField] private float minOffset;
    [SerializeField] private float xOffset;

    private ParticleSystem pS;
    private float sliderMaxValue;
    private float sliderMinValue;
    private float startY;
    private float startZ;

    // Start is called before the first frame update
    void Start()
    {
        xStart += xOffset;
        xEnd += xOffset;

        pS = GetComponent<ParticleSystem>();
        sliderMaxValue = attachedSlider.maxValue;
        sliderMinValue = attachedSlider.minValue;
        startY = transform.localPosition.y;
        startZ = transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        //Pos update
        float _newXPos = xEnd - (attachedSlider.value / sliderMaxValue * (Mathf.Abs(xStart)+Mathf.Abs(xEnd)));
        Vector3 _newPos = new Vector3(_newXPos, startY, startZ);
        transform.localPosition = _newPos;


        if (attachedSlider.value >= sliderMaxValue - maxOffset || attachedSlider.value <= sliderMinValue + minOffset)
        {
            //Debug.Log("pS Stopped");
            if (!pS.isStopped)
            {
                pS.Stop();
            }
        }
        else
        {
            if (!pS.isPlaying)
            {
                //pS.Play();
            }
        }
    }
}
