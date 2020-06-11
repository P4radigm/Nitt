using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    [SerializeField] private float newColTime;

    private float newColTimer;
    private Color currentColor;
    private Scene activeScene;
    [HideInInspector] public int score;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        newColTimer = newColTime;
    }

    // Update is called once per frame
    void Update()
    {
        activeScene = SceneManager.GetActiveScene();

        if (activeScene == SceneManager.GetSceneByName("Death"))
        {
            if(scoreText == null) { scoreText = FindObjectOfType<TextMeshProUGUI>(); }

            if (newColTimer <= 0)
            {
                UpdateColor();
                newColTimer = newColTime;
            }
            else
            {
                newColTimer -= Time.deltaTime;
            }

            scoreText.text = score.ToString();
        }
        else if(activeScene == SceneManager.GetSceneByName("Menu"))
        {
            Destroy(this.gameObject);
        }
        else
        {
            newColTimer = newColTime;
        }
    }

    private void UpdateColor()
    {
        currentColor = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);

        scoreText.color = currentColor;
    }
}
