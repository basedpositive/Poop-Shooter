using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Zhan.SimpleFPSShoter {
public class OptionScreen : MonoBehaviour
{
    public Toggle fullscreenTog, vsyncTog;

    public List<ResItems> resolutions = new List<ResItems>();
    private int selectedResolution;

    public Text resolutionLabel;

    // Start is called before the first frame update
    void Start()
    {
        fullscreenTog.isOn = Screen.fullScreen;

        if(QualitySettings.vSyncCount == 0) {
            vsyncTog.isOn = false;
        }
        else {
            vsyncTog.isOn = true;
        }
        bool foundRes = false;
        for(int i = 0; i < resolutions.Count; i++) {
            if(Screen.width == resolutions[i].h_ && Screen.height == resolutions[i].v_){
                foundRes = true;

                selectedResolution = i;

                UpdateResolutionLabel();
            }
        }
        if (!foundRes) {
            ResItems newResolution = new ResItems();
            newResolution.h_ = Screen.width;
            newResolution.v_ = Screen.height;

            resolutions.Add(newResolution);
            selectedResolution = resolutions.Count - 1;

            UpdateResolutionLabel();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResolutionLeft(){
        selectedResolution--;
        if (selectedResolution < 0) {
            selectedResolution = 0;
        }
        UpdateResolutionLabel();
    }

    public void ResolutionRight(){
        selectedResolution++;
        if (selectedResolution > resolutions.Count - 1){
            selectedResolution = resolutions.Count - 1;
        }
        UpdateResolutionLabel();
    }

    public void UpdateResolutionLabel() {
        resolutionLabel.text = resolutions[selectedResolution].h_.ToString() + " x " + resolutions[selectedResolution].v_.ToString();
    }

    public void ApplyOptions() {
        // Screen.fullScreen = fullscreenTog.isOn;

        if (vsyncTog.isOn) {
            QualitySettings.vSyncCount = 1;
        }
        else {
            QualitySettings.vSyncCount = 0;
        }
        Screen.SetResolution(resolutions[selectedResolution].h_, resolutions[selectedResolution].v_, fullscreenTog.isOn);
    } 

    [System.Serializable]
    public class ResItems {
        public int h_, v_;
    }
}
}
