using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    GameObject Panel_Co_Serv;

    GameObject Panel_Co_Room;
    Text textPlayerName;

    GameObject Panel_Room;
    GameObject TextWormsTeam;
    GameObject DropDownWormsTeam;

    GameObject Panel_Option;

    public NetworkManager NetManager;

    private void Awake()
    {
        NetManager = FindObjectOfType<NetworkManager>();
        if(NetManager == null)
        {
            GameObject netManagerGO = Instantiate(Resources.Load("NetworkManager") as GameObject);
            NetManager = netManagerGO.GetComponent<NetworkManager>();
        }
    }

    private void Start()
    {
        Panel_Co_Serv = transform.GetChild(1).gameObject;
        Panel_Co_Room = transform.GetChild(2).gameObject;
        textPlayerName = Panel_Co_Room.transform.GetChild(0).GetComponent<Text>();
        Panel_Room = transform.GetChild(3).gameObject;
        TextWormsTeam = Panel_Room.transform.GetChild(0).gameObject;
        DropDownWormsTeam = Panel_Room.transform.GetChild(1).gameObject;
        Panel_Option = transform.GetChild(4).gameObject;
        
        NetManager.listPlayerName.Add(Panel_Room.transform.GetChild(2).GetComponent<Text>());
        NetManager.listPlayerName.Add(Panel_Room.transform.GetChild(3).GetComponent<Text>());
        NetManager.listPlayerName.Add(Panel_Room.transform.GetChild(4).GetComponent<Text>());
        NetManager.listPlayerName.Add(Panel_Room.transform.GetChild(5).GetComponent<Text>());
        NetManager.textTimerBeforeLaunch = Panel_Room.transform.GetChild(6).GetComponent<Text>();
        NetManager.textRoomName = Panel_Room.transform.GetChild(7).GetComponent<Text>();

        if (NetManager.isGameStarted)
        {
            Active_Co_Room();
            NetManager.isGameStarted = false;
        }
    }

    private void ResetPanel()
    {
        Panel_Co_Serv.SetActive(false);
        Panel_Co_Room.SetActive(false);
        Panel_Room.SetActive(false);
        Panel_Option.SetActive(false);
    }

    public void Active_Co_Serv()
    {
        ResetPanel();
        Panel_Co_Serv.SetActive(true);
    }

    public void Active_Co_Room()
    {
        ResetPanel();
        Panel_Co_Room.SetActive(true);

        textPlayerName.text = "Welcome, " + PhotonNetwork.playerName;
    }

    public void Active_Room()
    {
        ResetPanel();
        Panel_Room.SetActive(true);
    }

    public void Active_Option()
    {
        ResetPanel();
        Panel_Option.SetActive(true);
    }
    

    public void Active_Worms()
    {
        TextWormsTeam.SetActive(true);
        DropDownWormsTeam.SetActive(true);
    }

    public void Connect()
    {
        NetManager.Connect();
    }

    public void SearchGame()
    {
        NetManager.SearchGame();
    }

    public void SetToG(Dropdown change)
    {
        NetManager.SetToG(change);
    }

    public void SetWPT(Dropdown change)
    {
        NetManager.SetWPT(change);
    }

    public void BackToLobby()
    {
        NetManager.BackToLobby();
    }

    public void BackToConnect()
    {
        NetManager.BackToConnect();
    }

}
