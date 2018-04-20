using UnityEngine;
using UnityEngine.UI;

/*
* @JulienLopez
* @HUDManager.cs
* @Le script s'attache sur un Canvas.
*   - Permet de gerer le Hud pendant le cours de la partie.
*/

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

    Color[] tabColorPlayer = new Color[] { new Color(0.0f,0.5f,0.0f,1.0f), new Color(1.0f, 0.2f, 1.0f, 1.0f), Color.red, Color.yellow };

    bool isSetUpColorWorms = false;
    bool isSetUpDeath = false;
    bool isSetUpEndOfGame = false;

    #endregion


    #region MonoBehaviour CallBacks

    private void Awake()
    {
        NetManager = FindObjectOfType<NetworkManager>();
    }

    private void Start()
    {
        textToG.text = "ToG : " + NetManager.ToG.ToString().Substring(1) + " | Player : " + PhotonNetwork.player.NickName + " | ALIVE !";

        //Set la couleur du HUD
        textToG.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        textPlayerTurn.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        textPhase.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        textTimerPhase.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
        
    }

    private void Update()
    {
        if(!gm.teamIsAlive && !isSetUpDeath)//Si le joueur n'a plus de worms en vie set l'hud à died
        {
            textToG.text = "ToG : " + NetManager.ToG.ToString().Substring(1) + " | Player : " + PhotonNetwork.player.NickName + " | DIED !";
            isSetUpDeath = true;
        }

        //Set les text de vie de nos worms de la même couleur que le HUD
        if(gm.localWormsPC.Count > 0 && !isSetUpColorWorms)
        {
            foreach (PlayerController pc in gm.localWormsPC)
            {
                pc.lifeText.color = tabColorPlayer[PhotonNetwork.player.ID - 1];
            }
            isSetUpColorWorms = true;
        }

        //Set les paramètres du HUD
        textPlayerTurn.text = "Player Turn : " + gm.playerTurn;
        if (gm.playerTurn == PhotonNetwork.player && !isSetUpEndOfGame)
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

        //Fin de partie => un seul joueur en vie ou un joueur est inactif
        if(gm.nbrTeamAlive == 1 || (gm.playerTurn != null && gm.playerTurn.IsInactive))
        {
            Desaative_HUD();
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

        //Quitte la room
        if (timerDisconnect > 2.0f && PhotonNetwork.inRoom)
            PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Désactive le HUD en fin de partie
    /// </summary>
    private void Desaative_HUD()
    {
        if(!isSetUpEndOfGame)
        {
            isSetUpEndOfGame = true;
            textToG.gameObject.SetActive(false);
            textPlayerTurn.gameObject.SetActive(false);
            textPhase.gameObject.SetActive(false);
            textTimerPhase.gameObject.SetActive(false);
        }
    }

    #endregion
    
}
