using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum TypeOfGame
{
    _1vs1 = 2,
    _1vs1vs1 = 3,
    _1vs1vs1vs1 = 4
};

public class NetworkManager : Photon.PunBehaviour
{
    #region Public Variables
    public MenuManager controlPanel;

    public Text feedbackText;

    public byte maxPlayersPerRoom = 4;

    public TypeOfGame ToG = TypeOfGame._1vs1;

    public Text textTimerBeforeLaunch;
    public Text textRoomName;
    #endregion



    #region Private Variables
    bool isConnecting;

    string gameVersion = "1";

    List<string> listNameRoomOpenOfType = new List<string>();

    float timerBeforeLaunch = 10.0f;

    Hashtable hashSet;
    #endregion



    #region MonoBehaviour CallBacks

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.automaticallySyncScene = true;
    }

    void Update()
    {
        if (PhotonNetwork.inRoom && PhotonNetwork.room.PlayerCount == (int)ToG)
        {
            if (timerBeforeLaunch > 0.0f)
                timerBeforeLaunch -= Time.deltaTime;
            else
                timerBeforeLaunch = 0.0f;

            textTimerBeforeLaunch.text = "Time before starting : " + (int)timerBeforeLaunch;

            if (timerBeforeLaunch <= 0.0f)
            {
                PhotonNetwork.LoadLevel("GameRoom" + (int)ToG);
            }
        }
    }
    #endregion



    #region Public Methods

    public void Connect()
    {
        feedbackText.text = "Info :";
        
        isConnecting = true;
        
        controlPanel.Active_Co_Room();

        LogFeedback("Connecting...");
        PhotonNetwork.ConnectUsingSettings(gameVersion);
        
        if (PhotonNetwork.connected)
        {
            //LogFeedback("Joining Room...");
            //PhotonNetwork.JoinOrCreateRoom("", new RoomOptions(), TypedLobby.Default);
           //PhotonNetwork.JoinRandomRoom();
        }
    }

    public void SearchGame()
    {
        UpdateListRoom();
        if (PhotonNetwork.connected)
        {
            if (listNameRoomOpenOfType.Count > 0)
            {
                int randRoom = Random.Range(0, listNameRoomOpenOfType.Count);
                PhotonNetwork.JoinRoom(listNameRoomOpenOfType[randRoom]);
            }
            else
            {
                RoomOptions option = new RoomOptions();
                option.MaxPlayers = (byte)ToG;
                option.IsOpen = true;
                option.IsVisible = true;
                option.PlayerTtl = 5 * 60 * 1000;
                //hashSet = new Hashtable(5)
                //{
                //    { "PlayerName1" , PhotonNetwork.room.CustomProperties["PlayerName1"] },
                //    { "PlayerName2" , PhotonNetwork.room.CustomProperties["PlayerName2"] },
                //    { "PlayerName3" , PhotonNetwork.room.CustomProperties["PlayerName3"] },
                //    { "PlayerName4" , PhotonNetwork.room.CustomProperties["PlayerName4"] },
                //    { "TypeOfGame", PhotonNetwork.room.CustomProperties["TypeOfGame"] }
                //};
                hashSet = new Hashtable();
                hashSet["TypeOfGame"] = ToG;
                hashSet["PlayerName1"] = "";
                hashSet["PlayerName2"] = "";
                hashSet["PlayerName3"] = "";
                hashSet["PlayerName4"] = "";

                option.CustomRoomProperties = hashSet;

                string[] lobbyProps = { "TypeOfGame", "PlayerName1", "PlayerName2", "PlayerName3", "PlayerName4" };
                option.CustomRoomPropertiesForLobby = lobbyProps;

                PhotonNetwork.CreateRoom(ToG.ToString() + " | Game n°" + PhotonNetwork.countOfRooms, option, TypedLobby.Default);
            }

            controlPanel.Active_Room();
        }
    }

    void UpdateListRoom()
    {
        listNameRoomOpenOfType.RemoveRange(0, listNameRoomOpenOfType.Count);

        RoomInfo[] roomInfos = PhotonNetwork.GetRoomList();
        foreach (RoomInfo info in roomInfos)
        {
            if((TypeOfGame)info.CustomProperties["TypeOfGame"] == ToG)
            {
                listNameRoomOpenOfType.Add(info.Name);
            }
        }
    }
    
    public void SetToG(Dropdown change)
    {
        ToG = (TypeOfGame)(change.value + 2);
    }
    
    void LogFeedback(string message)
    {
        if (feedbackText == null)
        {
            return;
        }
        
        feedbackText.text += System.Environment.NewLine + message;
    }

    #endregion



    #region Photon.PunBehaviour CallBacks

    public override void OnConnectedToMaster()
    {
        
        if (isConnecting)
        {
            LogFeedback("<Color=Blue>OnConnectedToMaster</Color>: Connected");
        }
    }
    
    public override void OnDisconnectedFromPhoton()
    {
        LogFeedback("<Color=Red>OnDisconnectedFromPhoton</Color>: Disconnected");

        isConnecting = false;

        controlPanel.Active_Co_Serv();

    }
    
    public override void OnJoinedRoom()
    {
        textRoomName.text = PhotonNetwork.room.Name.Substring(1);

        if (hashSet != null)
        {
            PhotonNetwork.room.SetCustomProperties(hashSet);
        }
        
    }

    #endregion
}
