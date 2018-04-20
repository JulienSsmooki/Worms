using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* @JulienLopez
* @GameManager.cs
* @Le script s'attache sur un gameObject vide.
*   - Permet de gerer le
*/

/// <summary>
/// Enum de phase de la partie
/// </summary>
public enum GamePhase
{
    Debut,
    SelectionWorm,
    Action,
    Fin
}


public class GameManager : MonoBehaviour
{

    #region Public Variables
    
    public List<PlayerController> localWormsPC = new List<PlayerController>();

    public float timerSelection;
    public float timerAction;

    public GamePhase phase = GamePhase.Debut;
    public int idPlayerToPlay = 0;
    public PhotonPlayer playerTurn;

    public GameObject selectedWorm;
    public int indexSelectWorms = 0;

    public int nbrTeamAlive = 0;
    public bool teamIsAlive = true;
    #endregion


    #region Private Variables

    float maxTimerSelect = 5.0f;
    float maxTimerAction = 50.0f;

    NetworkManager NetManager;

    PhotonView view;
    PhotonView viewCamera;

    bool wasShoot = false;

    GameObject arrow;

    bool sendOnlyOneDie = false;

    GamePhase previousPhase = GamePhase.Debut;
    float preventChangePlayerTooFast = 0.0f;
    #endregion


    #region MonoBehaviour CallBacks

    private void Awake()
    {
        viewCamera = Camera.main.GetComponent<PhotonView>();
        view = GetComponent<PhotonView>();
        NetManager = FindObjectOfType<NetworkManager>();
        nbrTeamAlive = (int)NetManager.ToG;
        NetManager.hashSet = PhotonNetwork.room.CustomProperties;

        //Envoie à tous les spawns et le premier client à jouer
        if (PhotonNetwork.isMasterClient)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
            List<Vector3> spawnsPos = new List<Vector3>();
            foreach (GameObject spawn in spawns)
            {
                spawnsPos.Add(spawn.transform.position);
            }
            
            view.RPC("UpdateLocalWormsList", PhotonTargets.AllBuffered, spawnsPos.ToArray());

            view.RPC("UpdatePlayerTurn", PhotonTargets.AllBuffered, PhotonNetwork.playerList[0], 0);
        }

        //Lance une coroutine de test si l'équipe est encore en vie
        StartCoroutine(WormsAreYouAlive());
    }

    /// <summary>
    /// Coroutine de test des worms pour savoir s'il en reste au moins un en vie
    /// </summary>
    /// <returns></returns>
    public IEnumerator WormsAreYouAlive()
    {
        do
        {

            yield return 0;

            bool anyoneAlive = false;
            if (localWormsPC != null && localWormsPC.Count > 0)
            {
                foreach (PlayerController pc in localWormsPC)
                {
                    if (pc.isAlive)
                        anyoneAlive = true;
                }
            }
            else
            {
                anyoneAlive = true;
            }
            teamIsAlive = anyoneAlive;
        } while (teamIsAlive);

        //Aucun worms en vie => mort du joueur
        if (!sendOnlyOneDie)
        {
            view.RPC("AnyoneAreLoose", PhotonTargets.All, null);
            sendOnlyOneDie = true;
        }
    }

    private void Update()
    {
        if (PhotonNetwork.inRoom)
        {
            if(PhotonNetwork.isMasterClient)
                preventChangePlayerTooFast += Time.deltaTime;

            if (selectedWorm != null)
            {
                //Active la flèche feedback du worms actif
                if (arrow != null)
                {
                    if (arrow.activeSelf)
                        arrow.transform.position = selectedWorm.transform.position + Vector3.up * 0.3f;
                    else
                        arrow.SetActive(true);
                }

            }

            if (PhotonNetwork.player == playerTurn)
            {

                //Prend le controle de la caméra
                if (playerTurn != viewCamera.owner && teamIsAlive)
                {
                    viewCamera.TransferOwnership(playerTurn.ID);
                    Camera.main.transform.position = new Vector3(selectedWorm.transform.position.x, selectedWorm.transform.position.y, Camera.main.transform.position.z);
                }

                //Phase de gamepay (GameLoop)
                switch (phase)
                {
                    case GamePhase.Debut:
                        timerSelection = maxTimerSelect;
                        timerAction = maxTimerAction;

                        phase = GamePhase.SelectionWorm;
                        break;
                    case GamePhase.SelectionWorm:
                        if (teamIsAlive)
                        {
                            if (timerSelection >= 0.0f)
                            {
                                timerSelection -= Time.deltaTime;
                                if (localWormsPC.Count > 0)
                                {
                                    int maxWorms = (int)NetManager.hashSet["NbrWorms"];

                                    if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow)) && indexSelectWorms < maxWorms)
                                    {
                                        indexSelectWorms = ++indexSelectWorms % maxWorms;

                                    }
                                    else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && indexSelectWorms > 0)
                                    {
                                        indexSelectWorms = --indexSelectWorms % maxWorms;

                                    }

                                    do
                                    {
                                        if (localWormsPC[indexSelectWorms].isAlive)
                                        {
                                            selectedWorm = localWormsPC[indexSelectWorms].gameObject;
                                        }
                                        else
                                        {
                                            indexSelectWorms = ++indexSelectWorms % maxWorms;
                                        }

                                        if (!teamIsAlive)
                                            break;

                                    } while (selectedWorm == null);

                                    if (arrow == null)
                                        arrow = Instantiate(Resources.Load("GreenArrow"), selectedWorm.transform.position + Vector3.up * 0.3f, Quaternion.identity) as GameObject;

                                }
                            }
                            else
                            {
                                phase = GamePhase.Action;
                            }
                        }
                        else
                        {
                            view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Action);
                        }
                        break;
                    case GamePhase.Action:
                        if (teamIsAlive)
                        {
                            if (timerAction >= 0.0f)
                            {
                                timerAction -= Time.deltaTime;

                                if (!localWormsPC[indexSelectWorms].isControledWorms)
                                {
                                    localWormsPC[indexSelectWorms].isControledWorms = true;
                                }
                                else
                                {
                                    if (localWormsPC[indexSelectWorms].isOnFire)
                                    {
                                        wasShoot = true;
                                    }
                                    if (!localWormsPC[indexSelectWorms].isOnFire && wasShoot && localWormsPC[indexSelectWorms].missileScript == null)
                                    {
                                        wasShoot = false;

                                        view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Fin);
                                    }
                                }
                            }
                            else
                            {
                                if (localWormsPC[indexSelectWorms].missileScript != null)
                                    PhotonNetwork.Destroy(localWormsPC[indexSelectWorms].missileScript.gameObject);

                                view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Fin);
                            }

                        }
                        else
                        {
                            view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Fin);
                        }
                        break;
                    case GamePhase.Fin:
                        if (previousPhase == GamePhase.Action)
                        {
                            selectedWorm = null;
                            indexSelectWorms = 0;
                            idPlayerToPlay = ++idPlayerToPlay % (int)NetManager.ToG;

                            view.RPC("NextPlayerTurn", PhotonTargets.MasterClient, idPlayerToPlay);
                        }
                        view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Debut);

                        break;
                }
                if (selectedWorm != null)
                {
                    //Update Camera
                    if (localWormsPC[indexSelectWorms].isAlive)
                    {
                        //Suit le missile tiré sinon le worms sélectionné
                        if (localWormsPC[indexSelectWorms].missileScript != null)
                            Camera.main.transform.position = new Vector3(localWormsPC[indexSelectWorms].missileScript.transform.position.x, localWormsPC[indexSelectWorms].missileScript.transform.position.y, Camera.main.transform.position.z);
                        else
                            Camera.main.transform.position = new Vector3(selectedWorm.transform.position.x, selectedWorm.transform.position.y, Camera.main.transform.position.z);
                    }
                    else
                    {
                        //Si malheureusement le worms sélectioné meure
                        if (phase != GamePhase.SelectionWorm)
                        {
                            Camera.main.transform.position = Vector3.forward * -10.0f;

                            view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Fin);
                        }
                        else
                        {
                            indexSelectWorms = ++indexSelectWorms % (int)NetManager.hashSet["NbrWorms"]; ;
                        }
                    }
                }
            }
            else
            {
                //Désactive la flèche feedback
                if (arrow != null && arrow.activeSelf)
                    arrow.SetActive(false);
            }
        }
    }


    #endregion


    #region Photon.PunBehaviour RPCs
    /// <summary>
    /// Passe à la phase en paramètre
    /// </summary>
    /// <param name="newPhase"></param>
    [PunRPC]
    public void NextPhase(GamePhase newPhase)
    {
        previousPhase = phase;
        phase = newPhase;

        if (newPhase == GamePhase.Fin)
            localWormsPC[indexSelectWorms].isControledWorms = false;

    }

    /// <summary>
    /// Récupère le joueur qui doit jouer du master
    /// </summary>
    /// <param name="_playerTurn"></param>
    /// <param name="_idPlayerToPlay"></param>
    [PunRPC]
    public void UpdatePlayerTurn(PhotonPlayer _playerTurn, int _idPlayerToPlay)
    {
        playerTurn = _playerTurn;
        idPlayerToPlay = _idPlayerToPlay;
    }

    /// <summary>
    /// Master => send le nouveau joueur à jouer
    /// </summary>
    /// <param name="_idPlayerToPlay"></param>
    [PunRPC]
    public void NextPlayerTurn(int _idPlayerToPlay)
    {
        if (preventChangePlayerTooFast > 0.35f)
        {
            view.RPC("UpdatePlayerTurn", PhotonTargets.All, PhotonNetwork.playerList[_idPlayerToPlay], _idPlayerToPlay);
            preventChangePlayerTooFast = 0.0f;
        }
    }

    /// <summary>
    /// Créer et récupère la liste locale des worms possédé par le joueur
    /// </summary>
    /// <param name="_spawns"></param>
    [PunRPC]
    public void UpdateLocalWormsList(Vector3[] _spawns)
    {
        for (int j = 0; j < (int)NetManager.hashSet["NbrWorms"]; j++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("PlayerPrefab", new Vector2(
                Random.Range(_spawns[(PhotonNetwork.player.ID - 1)].x, _spawns[((PhotonNetwork.player.ID - 1) + 1)].x),
                Random.Range(_spawns[(PhotonNetwork.player.ID - 1)].y, _spawns[((PhotonNetwork.player.ID - 1) + 1)].y))
                , Quaternion.identity, 0);
            tmp.name = tmp.name + (PhotonNetwork.player.ID - 1);

            localWormsPC.Add(tmp.GetComponent<PlayerController>());
        }

    }

    /// <summary>
    /// Lorsqu'un joueur est mort réduit le nombre de team en vie
    /// </summary>
    [PunRPC]
    public void AnyoneAreLoose()
    {
        if (nbrTeamAlive > 1)
            nbrTeamAlive--;
    }

    #endregion
}
