using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Com.Zhan.SimpleFPSShoter {
public class MainMenu : MonoBehaviour
{
    public Launcher launcher;

    public GameObject optionsScreen;

    public void Start() {
        Pause.paused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void JoinMatch() {
        launcher.Join();
    }

    public void CreateMatch() {
        launcher.Create();
    }

    public void OpenOptions() {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions() {
        optionsScreen.SetActive(false);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
}
