using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public enum TypeOfGame
{
    _1vs1 = 2,
    _1vs1vs1 = 3,
    _1vs1vs1vs1 = 4
};

public class NetworkManager : Photon.PunBehaviour
{
    #region Public Variables
    
    public byte maxPlayersPerRoom = 4;

    public TypeOfGame ToG = TypeOfGame._1vs1;
    public string mapName = "Garden";

    public List<Text> listPlayerName = new List<Text>();

    public Text textTimerBeforeLaunch;
    public Text textRoomName;

    public ConnectParticle connectParticle;

    public Hashtable hashSet;

    public bool isGameStarted = false;
    public int NbrWormsPT = 1;
    #endregion



    #region Private Variables
    MenuManager controlPanel;
    
    bool isConnecting;
    
    string gameVersion = "1";

    List<string> listNameRoomOpenOfType = new List<string>();

    float timerBeforeLaunch = 1.0f;
        
    string playername = "PlayerName";
    
    #endregion



    #region MonoBehaviour CallBacks

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.automaticallySyncScene = true;

        SceneManager.sceneLoaded += OnStartGame;
        
        DontDestroyOnLoad(this);
    }
    
    public void OnStartGame(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex > 0)//Is in game scene
        {
            isGameStarted = true;
            listPlayerName.RemoveRange(0, listPlayerName.Count);
        }
        else
        {
            controlPanel = FindObjectOfType<MenuManager>();
        }
    }

    void Update()
    {
        if (PhotonNetwork.inRoom && !isGameStarted)
        {
            hashSet = PhotonNetwork.room.CustomProperties;

            for (int i = 1; i < 5; i++)
            {
                if (i < PhotonNetwork.room.PlayerCount + 1)
                {
                    if (!listPlayerName[i - 1].gameObject.activeSelf)
                    {
                        listPlayerName[i - 1].gameObject.SetActive(true);
                    }
                    listPlayerName[i - 1].text = (string)hashSet[(playername + i.ToString())];
                }
                else
                {
                    if (listPlayerName[i - 1].gameObject.activeSelf)
                    {
                        listPlayerName[i - 1].text = "";
                        listPlayerName[i - 1].gameObject.SetActive(false);
                    }
                }
            }

            if (PhotonNetwork.room.PlayerCount == (int)ToG)
            {
                if (timerBeforeLaunch > 0.0f)
                    timerBeforeLaunch -= Time.deltaTime;
                else
                    timerBeforeLaunch = 0.0f;

                textTimerBeforeLaunch.text = "Time before starting : " + (int)timerBeforeLaunch;

                if (timerBeforeLaunch <= 0.0f)
                {
                    PhotonNetwork.LoadLevel(mapName + (int)ToG);
                }
            }
            else
            {
                timerBeforeLaunch = 1.0f;
                textTimerBeforeLaunch.text = "Time before starting : XX";
            }
        }
    }
    #endregion



    #region Public Methods

    public void Connect()
    {
        isConnecting = true;
        connectParticle.StartWaitingAnimation();
        PhotonNetwork.ConnectUsingSettings(gameVersion);
        
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
               
                hashSet = new Hashtable();
                hashSet["TypeOfGame"] = ToG;
                hashSet["MapName"] = mapName;
                hashSet["NbrWorms"] = NbrWormsPT;
                hashSet["PlayerName1"] = "";
                hashSet["PlayerName2"] = "";
                hashSet["PlayerName3"] = "";
                hashSet["PlayerName4"] = "";

                option.CustomRoomProperties = hashSet;

                string[] lobbyProps = { "TypeOfGame", "MapName", "NbrWorms", "PlayerName1", "PlayerName2", "PlayerName3", "PlayerName4" };
                option.CustomRoomPropertiesForLobby = lobbyProps;

                PhotonNetwork.CreateRoom(ToG.ToString() + " | " + mapName +" n°" + PhotonNetwork.countOfRooms, option, TypedLobby.Default);
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
            if((TypeOfGame)info.CustomProperties["TypeOfGame"] == ToG && (string)info.CustomProperties["MapName"] == mapName)
            {
                listNameRoomOpenOfType.Add(info.Name);
            }
        }
    }
    
    public void SetToG(Dropdown change)
    {
        ToG = (TypeOfGame)(change.value + 2);
    }

    public void SetWPT(Dropdown change)
    {
        NbrWormsPT = int.Parse(change.options[change.value].text);

        hashSet["NbrWorms"] = NbrWormsPT;
        PhotonNetwork.room.SetCustomProperties(hashSet);
    }

    public void SetMapName(Dropdown change)
    {
        mapName = change.options[change.value].text;
    }

    public void BackToLobby()
    {
        if(PhotonNetwork.LeaveRoom())
            controlPanel.Active_Co_Room();
    }

    public void BackToConnect()
    {
        if (PhotonNetwork.LeaveLobby())
        {
            PhotonNetwork.Disconnect();
            controlPanel.Active_Co_Serv();
        }
    }

    #endregion



    #region Photon.PunBehaviour CallBacks

    public override void OnConnectedToPhoton()
    {
        base.OnConnectedToPhoton();

        controlPanel.Active_Co_Room();

        connectParticle.StopWaitingAnimation();
    }

    public override void OnDisconnectedFromPhoton()
    {
        isConnecting = false;
        
    }
    
    public override void OnJoinedRoom()
    {
        textRoomName.text = PhotonNetwork.room.Name.Substring(1);
        
        if (hashSet != null)
        {
            hashSet[playername + PhotonNetwork.room.PlayerCount] = PhotonNetwork.playerName;

            PhotonNetwork.room.SetCustomProperties(hashSet);
        }
        else
        {
            hashSet = PhotonNetwork.room.CustomProperties;
            hashSet[playername + PhotonNetwork.room.PlayerCount] = PhotonNetwork.playerName;

            PhotonNetwork.room.SetCustomProperties(hashSet);
        }

        if (PhotonNetwork.isMasterClient)
            controlPanel.Active_Worms(true);

    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);
        if (hashSet != null)
        {
            hashSet[playername + PhotonNetwork.room.PlayerCount] = newPlayer.NickName;

            PhotonNetwork.room.SetCustomProperties(hashSet);
        }
        else
        {
            hashSet = PhotonNetwork.room.CustomProperties;
            hashSet[playername + PhotonNetwork.room.PlayerCount] = newPlayer.NickName;

            PhotonNetwork.room.SetCustomProperties(hashSet);
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        if(PhotonNetwork.isMasterClient)
        {
            for (int i = 1; i < 5; i++)
            {
                if ((string)hashSet[playername + i] == otherPlayer.NickName)
                {
                    hashSet[playername + i] = "";
                    for (int j = 4; j > i; j--)
                    {
                        if ((string)hashSet[playername + j] != "")
                            hashSet[playername + j] = hashSet[playername + i];

                    }
                    PhotonNetwork.room.SetCustomProperties(hashSet);
                    i = 5;
                }
            }
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if(isGameStarted)
            PhotonNetwork.LoadLevel(0);

    }

    #endregion
}
