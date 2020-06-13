using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public bool IsPaused;
    public GameObject PauseMenu;
    
    // Start is called before the first frame update
    void Start()
    {
        IsPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsPaused)
        {
            PauseMenu.SetActive(true);
            Time.timeScale = 0f;
        } else
        {
            PauseMenu.SetActive(false);
            Time.timeScale = 1f;
        }

        if(Input.GetButtonDown("Cancel"))
        {
            IsPaused = !IsPaused;
        }
    }

    public void LeaveGame()
    {
        SceneManager.LoadScene("Start");
    }

    public void ContinueGame()
    {
        IsPaused = false;
    }
}
