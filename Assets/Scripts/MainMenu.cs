using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public TextMeshProUGUI cornAmount;
    private string completionPrompt;

    private void Start()
    {
        completionPrompt = "You have collected " + PlayerController.collectedCorn + "/14 sacred corn.";
        cornAmount.text = completionPrompt.ToString();
    }

    private void Update()
    {
        
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene("Intro");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Start");
    }

    public void GoToControlsMenu()
    {
        SceneManager.LoadScene("Controls");
    }
}
