using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public GameObject Panel_Co_Serv;


    public GameObject Panel_Co_Room;
    public Text textPlayerName;

    public GameObject Panel_Room;
    public GameObject TextWormsTeam;
    public GameObject DropDownWormsTeam;

    public GameObject Panel_Option;
    
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
}
