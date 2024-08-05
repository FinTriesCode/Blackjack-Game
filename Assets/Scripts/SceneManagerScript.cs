using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void StartMainLevel()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void InstructionsScreen()
    {
        SceneManager.LoadScene("InstructionsScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
