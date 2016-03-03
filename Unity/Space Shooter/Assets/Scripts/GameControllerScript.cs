using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameControllerScript : MonoBehaviour {

    public GameObject[] hazards;
    public Vector3 spawnValues;
    public int hazardCount;
    public float spawnWait;
    public float startWait;
    public float waveWait;

    public Text scoreText;
    public Text restartText;
    public Text gameOverText;

    private bool gameOver;
    private bool restart;
    private int score;

    // Use this for initialization
    void Start()
    {
        gameOver = false;
        restart = false;

        gameOverText.text = "";
        restartText.text = "";
        score = 0;

        UpdateScore();

        StartCoroutine(SpawnWaves());
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            
        }
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);

        while(!gameOver)
        {
            
            for (int i = 0; i < hazardCount ; ++i)
            {
                GameObject hazard = hazards[Random.Range(0, hazards.Length)];

                Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(hazard, spawnPosition, spawnRotation);
                yield return new WaitForSeconds(spawnWait);

                if (gameOver)
                {
                    restartText.text = "Press 'R' to restart";
                    restart = true;
                    break;
                }
            }

            yield return new WaitForSeconds(waveWait);
        }

        restartText.text = "Press 'R' to restart";
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        UpdateScore();
    }

    public void GameOver()
    {
        gameOverText.text = "Game Over";
        gameOver = true;
    }
}
