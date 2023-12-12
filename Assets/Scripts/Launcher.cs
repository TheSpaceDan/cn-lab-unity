using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    public GameObject loadingScreen;
    public TMP_Text loadingText;
    
    public GameObject roomScreen;
    public TMP_InputField roomNameInput;

    public GameObject createdRoomScreen;
    public TMP_Text roomNameText;

    public GameObject roomBrowserScreen;
    public RoomButtonScript roomButton;
    private List<RoomButtonScript> allRoomButtons = new List<RoomButtonScript>();
    
    void Awake()    
    {
        instance = this;
    }
    
    void Start()
    {
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting...";
        
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        loadingText.text = "Joining Lobby...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        loadingScreen.SetActive(false);
    }
    
    public void OpenCreateRoomScreen()
    {
        roomScreen.SetActive(true);
    }
    
    public void CloseCreateRoomScreen()
    {
        roomScreen.SetActive(false);
    }
    
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            return;
        }
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 6;
        
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
        
        loadingScreen.SetActive(true);
        roomScreen.SetActive(false);
        loadingText.text = "Creating Room...";
    }

    public override void OnCreatedRoom()
    {
        loadingScreen.SetActive(false);
        roomNameText.text = "Room " + PhotonNetwork.CurrentRoom.Name;
        createdRoomScreen.SetActive(true);
    }

    public void LeaveRoom()
    {
        createdRoomScreen.SetActive(false);
        loadingScreen.SetActive(true);
        loadingText.text = "Leaving Room...";
        
        PhotonNetwork.LeaveRoom();
    }
    
    public void OpenRoomBrowser()   
    {
        roomBrowserScreen.SetActive(true);
    }
    
    public void CloseRoomBrowser()
    {
        roomBrowserScreen.SetActive(false);
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButtonScript button in allRoomButtons)
        {
            Destroy(button.gameObject);
        }
        
        allRoomButtons.Clear();
        
        roomButton.gameObject.SetActive(false);
        
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButtonScript newButton = Instantiate(roomButton, roomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                
                allRoomButtons.Add(newButton);
            } 
        }
    }
}
