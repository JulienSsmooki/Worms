using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour {

    #region Public Variables

    public Text textToG;
    public Text textPlayerTurn;
    public Text textPhase;
    public Text textTimerPhase;
    public GameObject textWin;
    public GameObject textLoose;

    public GameManager gm;
    #endregion


    #region Private Variables

    NetworkManager NetManager;

    float timerDisconnect = 0.0f;

    Color[] tabColorPlayer = new Color[] { Color.black, Color.blue, Color.red, Color.yellow };

    bool isSetUpColorWorms = false;

    #endregion


    #region MonoBehaviour CallBacks

    private void Awake()
    {
        NetManager = FindObjectOfType<NetworkManager>();
    }

    private void Start()
    {
        textToG.text = "ToG : " + NetManager.ToG.ToString().Substring(1) + " | Player : " + PhotonNetwork.player.NickName;

        textToG.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        textPlayerTurn.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        textPhase.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        textTimerPhase.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        
    }

    private void Update()
    {
        if(gm.localWormsPC.Count > 0 && !isSetUpColorWorms)
        {
            foreach (PlayerController pc in gm.localWormsPC)
            {
                pc.lifeText.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
            }
            isSetUpColorWorms = true;
        }

        textPlayerTurn.text = "Player Turn : " + gm.playerTurn;
        if (gm.playerTurn == PhotonNetwork.player)
        {
            if (!textPhase.gameObject.activeSelf)
                textPhase.gameObject.SetActive(true);
            textPhase.text = "Phase : " + gm.phase;
            if (gm.phase == GamePhase.SelectionWorm)
            {
                if (!textTimerPhase.gameObject.activeSelf)
                    textTimerPhase.gameObject.SetActive(true);
                textTimerPhase.text = "Timer : " + (int)gm.timerSelection;
            }
            else if (gm.phase == GamePhase.Action)
            {
                if (!textTimerPhase.gameObject.activeSelf)
                    textTimerPhase.gameObject.SetActive(true);
                textTimerPhase.text = "Timer : " + (int)gm.timerAction;
            }
            else
            {
                if (textTimerPhase.gameObject.activeSelf)
                    textTimerPhase.gameObject.SetActive(false);
            }
        }
        else
        {
            if (textTimerPhase.gameObject.activeSelf)
                textTimerPhase.gameObject.SetActive(false);
            if (textPhase.gameObject.activeSelf)
                textPhase.gameObject.SetActive(false);
        }

        if(gm.nbrTeamAlive == 1 || (gm.playerTurn != null && gm.playerTurn.IsInactive))
        {
            timerDisconnect += Time.deltaTime;
            if (gm.teamIsAlive)
            {
                textWin.SetActive(true);
            }
            else
            {
                textLoose.SetActive(true);
            }
        }

        if (timerDisconnect > 2.0f && PhotonNetwork.inRoom)
            PhotonNetwork.LeaveRoom();
    }

    #endregion


    #region Photon.PunBehaviour CallBacks


    #endregion
}
