using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Com.Zhan.SimpleFPSShoter {

    [System.Serializable]
    public class ProfileData {
    public string username;
    public int level;
    public int xp;

    public ProfileData() {
        this.username = "DEFAULT";
        this.level = 0;
        this.xp = 0;
    }

    public ProfileData(string u, int l, int x) {
        this.username = u;
        this.level = l;
        this.xp = x;
    }

    object[] ConvertToObjectArr() {
        object[] ret = new object[3];
        return ret;
    }
    }

public class Launcher : MonoBehaviourPunCallbacks
{
    public InputField usernameField;
    public static ProfileData myProfile = new ProfileData();

    public void Awake() {
        PhotonNetwork.AutomaticallySyncScene = true;

        myProfile = Data.LoadProfile();
        usernameField.text = myProfile.username;

        Connect();
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected!");
        // Join();
        base.OnConnectedToMaster();
    }

    public override void OnJoinedRoom() {
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Connect() {
        Debug.Log("trying to connect...");
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Join() {
        PhotonNetwork.JoinRandomRoom();
    }

    public void Create() {
        PhotonNetwork.CreateRoom("");
    }

    public void StartGame() {
        if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.username = "RANDOM_USER_" + Random.Range(100, 1000);
            }
            else
            {
                myProfile.username = usernameField.text;
            }

        if(PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            Data.SaveProfile(myProfile);
            PhotonNetwork.LoadLevel(1);
        }
    }
}
}
