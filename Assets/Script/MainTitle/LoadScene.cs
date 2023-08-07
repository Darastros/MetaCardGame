using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public bool started = false;
    public bool tryToStart = false;
    public void LoadMainScene()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadPartyScene()
    {
        SceneManager.LoadScene(1);
    }
    
    public void StartGame()
    {
        tryToStart = true;
    }

    public void Update()
    {
        if (tryToStart && !started && FMODUnity.RuntimeManager.HasBankLoaded("Master"))
        {
            GameManager.Instance.animator.SetTrigger("OnStart");
            GameManager.Instance.PlayStartFightSFX();
            started = true;
        }
    }
}
