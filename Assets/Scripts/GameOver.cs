using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public TMP_Text scoreText = null;
    public TMP_Text killsText = null;

    private void Awake()
    {
        scoreText.text = "Score: " + GameController.instance.playerScore.ToString();
        killsText.text = "Demons Killed: " + GameController.instance.playerKills.ToString();
    }
    public void MainMenu(){
        SceneManager.LoadScene(0);
    }
}
