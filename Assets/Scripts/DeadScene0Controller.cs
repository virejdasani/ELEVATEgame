using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;



public class DeadScene0Controller : MonoBehaviour
{
    

    public void TryAgain()
    {
        SceneManager.LoadScene("Level0");
    }
    public void QuitGame()
    {
        Debug.Log("Application quit");
        Application.Quit();
    }
    public void BackToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    

    
    
}
