using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Zhan.SimpleFPSShoter {
public class BringUpSettings : MonoBehaviour
{

    public GameObject setting;
    public bool issettingactive;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (issettingactive == false) {
                Pause();
            }

            else {
                Resume();
            }
        }
    }

    public void Pause() {
        setting.SetActive(true);
        issettingactive = true;
        this.GetComponent<MouseLook>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume() {
        setting.SetActive(false);
        issettingactive = false;
        this.GetComponent<MouseLook>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
}
