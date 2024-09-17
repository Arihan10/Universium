using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance; 
    
    [SerializeField] TMP_InputField roomNameInputField, playerNameInputField;

    [SerializeField] TMP_Text roomNameText; 

    [SerializeField] Transform roomListContent, playerListContent;

    [SerializeField] GameObject roomListItemPrefab, playerListItemPrefab, startGameButton;

    private void Awake() {
        instance = this; 
    }

    // Start is called before the first frame update
    void Start()
    {
        MenuManager.instance.OpenMenu("LoadingMenu"); 
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true; 
    }

    public override void OnJoinedLobby() {
        Debug.Log("Joined Lobby");
        MenuManager.instance.OpenMenu("PlayMenu");

        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
        playerNameInputField.text = PhotonNetwork.NickName; 
    }

    public void CreateRoom() {
        if (string.IsNullOrEmpty(roomNameInputField.text)) return;
        RoomOptions rmOptions = new RoomOptions() { CleanupCacheOnLeave = false }; 
        PhotonNetwork.CreateRoom(roomNameInputField.text, rmOptions);
        MenuManager.instance.OpenMenu("LoadingMenu"); 
    }

    public void StartGame() {
        PhotonNetwork.LoadLevel(1); 
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("LoadingMenu"); 
    }

    public override void OnJoinedRoom() { 
        MenuManager.instance.OpenMenu("RoomMenu");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        foreach (Transform trans in playerListContent) {
            Destroy(trans.gameObject); 
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; ++i) {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient); 
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); 
    }

    public override void OnLeftRoom() {
        MenuManager.instance.OpenMenu("PlayMenu"); 
    }

    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("LoadingMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        foreach (Transform trans in roomListContent) {
            Destroy(trans.gameObject); 
        }
        for (int i = 0; i < roomList.Count; ++i) {
            if (roomList[i].RemovedFromList) continue; 
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]); 
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer); 
    }

    public void OnUsernameInputValueChanged() {
        PhotonNetwork.NickName = playerNameInputField.text; 
    }
}
