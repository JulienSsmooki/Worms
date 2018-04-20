using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Debut,
    SelectionWorm,
    Action,
    Fin
}


public class GameManager : MonoBehaviour {

    #region Public Variables

    public new Camera camera;
    
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
    #endregion
    

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        camera = Camera.main;
        viewCamera = camera.GetComponent<PhotonView>();
        view = GetComponent<PhotonView>();
        NetManager = FindObjectOfType<NetworkManager>();
        nbrTeamAlive = (int)NetManager.ToG;

        if (PhotonNetwork.isMasterClient)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
            List<Vector3> spawnsPos = new List<Vector3>();
            foreach (GameObject spawn in spawns)
            {
                spawnsPos.Add(spawn.transform.position);
            }


            NetManager.hashSet = PhotonNetwork.room.CustomProperties;
            
            view.RPC("UpdateLocalWormsList", PhotonTargets.AllBuffered, spawnsPos.ToArray());

            view.RPC("UpdatePlayerTurn", PhotonTargets.All, PhotonNetwork.playerList[0], 0);
        }

        StartCoroutine(WormsAreYouAlive());
    }
    
    public IEnumerator WormsAreYouAlive()
    {
        do
        {

            yield return new WaitForSeconds(0.5f);

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

        if (!sendOnlyOneDie)
        {
            view.RPC("AnyoneAreLoose", PhotonTargets.All, null);
            sendOnlyOneDie = true;
        }
        yield break;
    }

    private void Update()
    {
        if (PhotonNetwork.inRoom)
        {
            if (selectedWorm != null)
            {
                if (arrow != null)
                {
                    if (arrow.activeSelf)
                        arrow.transform.position = selectedWorm.transform.position + Vector3.up * 0.3f;
                    else
                        arrow.SetActive(true);
                }

                if (playerTurn != null && playerTurn != viewCamera.owner && teamIsAlive)
                {
                    viewCamera.TransferOwnership(playerTurn.ID);
                    camera.transform.position = new Vector3(selectedWorm.transform.position.x, selectedWorm.transform.position.y, camera.transform.position.z);
                }
            }

            if (PhotonNetwork.player == playerTurn)
            {
                if (teamIsAlive)
                {
                    switch (phase)
                    {
                        case GamePhase.Debut:
                            timerSelection = maxTimerSelect;
                            timerAction = maxTimerAction;

                            phase = GamePhase.SelectionWorm;
                            break;
                        case GamePhase.SelectionWorm:
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

                                    } while (selectedWorm == null);

                                    if (arrow == null)
                                        arrow = Instantiate(Resources.Load("GreenArrow"), selectedWorm.transform.position + Vector3.up * 0.3f, Quaternion.identity) as GameObject;

                                }
                            }
                            else
                            {
                                phase = GamePhase.Action;
                            }

                            break;
                        case GamePhase.Action:
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
                                view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Fin);
                            }
                            break;
                        case GamePhase.Fin:
                            selectedWorm = null;
                            indexSelectWorms = 0;
                            idPlayerToPlay = ++idPlayerToPlay % (int)NetManager.ToG;

                            view.RPC("NextPlayerTurn", PhotonTargets.MasterClient, idPlayerToPlay);

                            view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Debut);

                            break;
                    }
                    if (selectedWorm != null)
                    {
                        if (localWormsPC[indexSelectWorms].isAlive)
                        {
                            if (localWormsPC[indexSelectWorms].missileScript != null)
                                camera.transform.position = new Vector3(localWormsPC[indexSelectWorms].missileScript.transform.position.x, localWormsPC[indexSelectWorms].missileScript.transform.position.y, camera.transform.position.z);
                            else
                                camera.transform.position = new Vector3(selectedWorm.transform.position.x, selectedWorm.transform.position.y, camera.transform.position.z);
                        }
                        else
                        {
                            if (phase != GamePhase.SelectionWorm)
                            {
                                camera.transform.position = Vector3.forward * -10.0f;

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
                    idPlayerToPlay = ++idPlayerToPlay % (int)NetManager.ToG;

                    view.RPC("NextPlayerTurn", PhotonTargets.MasterClient, idPlayerToPlay);

                    view.RPC("NextPhase", PhotonTargets.AllBuffered, GamePhase.Debut);
                }
            }
            else
            {
                if (arrow != null && arrow.activeSelf)
                    arrow.SetActive(false);
            }
        }
    }


    #endregion


    #region Photon.PunBehaviour RPCs

    [PunRPC]
    public void NextPhase(GamePhase newPhase)
    {
        phase = newPhase;

        if(newPhase == GamePhase.Fin)
            localWormsPC[indexSelectWorms].isControledWorms = false;

    }


    [PunRPC]
    public void UpdatePlayerTurn(PhotonPlayer _playerTurn, int _idPlayerToPlay)
    {
        playerTurn = _playerTurn;
        idPlayerToPlay = _idPlayerToPlay;
        Debug.Log(playerTurn + " " + idPlayerToPlay);
    }

    [PunRPC]
    public void NextPlayerTurn(int _idPlayerToPlay)
    {
        view.RPC("UpdatePlayerTurn", PhotonTargets.All, PhotonNetwork.playerList[_idPlayerToPlay], _idPlayerToPlay);
        Debug.Log("On master : " + PhotonNetwork.playerList[_idPlayerToPlay] + _idPlayerToPlay);
    }

    [PunRPC]
    public void UpdateLocalWormsList(Vector3[] _spawns)
    {
        for (int j = 0; j < (int)NetManager.hashSet["NbrWorms"]; j++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("PlayerPrefab", new Vector2(
                Random.Range(_spawns[(PhotonNetwork.player.ID -1)].x, _spawns[((PhotonNetwork.player.ID -1) + 1)].x),
                Random.Range(_spawns[(PhotonNetwork.player.ID -1)].y, _spawns[((PhotonNetwork.player.ID -1) + 1)].y))
                , Quaternion.identity, 0);
            tmp.name = tmp.name + (PhotonNetwork.player.ID - 1);
            
            localWormsPC.Add(tmp.GetComponent<PlayerController>());
        }
        
    }

    [PunRPC]
    public void AnyoneAreLoose()
    {
        if(nbrTeamAlive > 1)
            nbrTeamAlive--;
    }

    #endregion
}
