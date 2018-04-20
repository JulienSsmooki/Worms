using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
* @JulienLopez
* @MenuManager.cs
* @Le script s'attache sur un Canvas.
*   - Permet de gerer le canvas du menu.
*   - Ne fonctionne qu'avec le NetworkManager.
*/

public class MenuManager : MonoBehaviour {

    #region Public Variables

    public GameObject Panel_Co_Serv;

    public GameObject Panel_Co_Room;
    public Text textPlayerName;

    public GameObject Panel_Room;
    public GameObject TextWormsTeam;
    public GameObject DropDownWormsTeam;

    public GameObject Panel_Option;

    public NetworkManager NetManager;

    public List<GameObject> imageMaps = new List<GameObject>();

    #endregion


    #region MonoBehaviour CallBacks

    private void Awake()
    {
        //Récupère ou créer un NetworkManager
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

    /// <summary>
    /// Désactive tout les panels
    /// </summary>
    private void ResetPanel()
    {
        Panel_Co_Serv.SetActive(false);
        Panel_Co_Room.SetActive(false);
        Panel_Room.SetActive(false);
        Panel_Option.SetActive(false);
    }

    /// <summary>
    /// Active le panel de connexion au serveur
    /// </summary>
    public void Active_Co_Serv()
    {
        ResetPanel();
        Panel_Co_Serv.SetActive(true);
    }

    /// <summary>
    /// Active le panel de connexion à une room
    /// </summary>
    public void Active_Co_Room()
    {
        ResetPanel();
        Panel_Co_Room.SetActive(true);

        textPlayerName.text = "Welcome, " + PhotonNetwork.playerName;
    }

    /// <summary>
    /// Active le panel du lobby d'attente dans la room
    /// </summary>
    public void Active_Room()
    {
        ResetPanel();
        Panel_Room.SetActive(true);
    }

    /// <summary>
    /// Active le panel d'option
    /// </summary>
    public void Active_Option()
    {
        ResetPanel();
        Panel_Option.SetActive(true);
    }
    
    /// <summary>
    /// Active la possibilité de set le nombre de worms par team
    /// </summary>
    /// <param name="activated"></param>
    public void Active_Worms(bool activated)
    {
        TextWormsTeam.SetActive(activated);
        DropDownWormsTeam.SetActive(activated);
    }

    /// <summary>
    /// Quitte l'application
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
    
    /// <summary>
    /// Pour l'UI button => connexion au serveur
    /// </summary>
    public void Connect()
    {
        NetManager.Connect();
    }

    /// <summary>
    /// Pour l'UI button => connexion a une room
    /// </summary>
    public void SearchGame()
    {
        NetManager.SearchGame();
    }

    /// <summary>
    /// Pour l'UI dropdown => type de partie désiré
    /// </summary>
    public void SetToG(Dropdown change)
    {
        NetManager.SetToG(change);
    }

    /// <summary>
    /// Pour l'UI dropdown => nombre de worms par team
    /// </summary>
    public void SetWPT(Dropdown change)
    {
        NetManager.SetWPT(change);
    }

    /// <summary>
    /// Pour l'UI dropdown => map désiré
    /// </summary>
    public void SetMapName(Dropdown change)
    {
        NetManager.SetMapName(change);
        Active_Image_Map(change.value);
    }

    /// <summary>
    /// Pour l'UI button => deconnexion de la room
    /// </summary>
    public void BackToLobby()
    {
        Active_Worms(false);
        NetManager.BackToLobby();
    }

    /// <summary>
    /// Pour l'UI button => deconnexion du serveur
    /// </summary>
    public void BackToConnect()
    {
        NetManager.BackToConnect();
    }

    /// <summary>
    /// Pour l'UI dropdown => set l'image de la map correspondant
    /// </summary>
    private void Active_Image_Map(int numMap)
    {
        foreach (GameObject image in imageMaps)
        {
            image.SetActive(false);
        }
        imageMaps[numMap].SetActive(true);
        imageMaps[numMap + 4].SetActive(true);
    }

    #endregion
}
