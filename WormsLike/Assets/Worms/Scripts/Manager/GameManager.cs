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

    public List<GameObject> localWorms = new List<GameObject>();

    public float timerSelection;
    public float timerAction;

    public GamePhase phase = GamePhase.Debut;
    public int idPlayerToPlay = 0;
    public PhotonPlayer playerTurn;

    public GameObject selectedWorm;
    public int indexSelectWorms = 0;

    #endregion
    

    #region Private Variables

    float maxTimerSelect = 5.0f;
    float maxTimerAction = 50.0f;

    NetworkManager NetManager;

    PhotonView view;
    PhotonView viewCamera;

    PlayerController controlPlayer;


    bool isListClean = false;
    #endregion
    

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        camera = Camera.main;
        viewCamera = camera.GetComponent<PhotonView>();
        view = GetComponent<PhotonView>();
        NetManager = FindObjectOfType<NetworkManager>();

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

        }
    }
    
    private void Start()
    {
        if(PhotonNetwork.isMasterClient)
        {
            playerTurn = PhotonNetwork.playerList[0];
        }
    }
    
    private void Update()
    {

        if (selectedWorm != null && playerTurn != null)
        {
            if (playerTurn != viewCamera.owner)
            {
                viewCamera.TransferOwnership(playerTurn.ID);
            }
            camera.transform.position = new Vector3(selectedWorm.transform.position.x, selectedWorm.transform.position.y, camera.transform.position.z);
        }
        if (PhotonNetwork.isMasterClient)
        {
            view.RPC("UpdatePlayerTurn", PhotonTargets.AllBuffered, playerTurn);
        }

        switch (phase)
        {
            case GamePhase.Debut:
                timerSelection = maxTimerSelect;
                timerAction = maxTimerAction;
                selectedWorm = null;
                
                phase = GamePhase.SelectionWorm;
                break;
            case GamePhase.SelectionWorm:
                if (timerSelection >= 0.0f)
                {
                    timerSelection -= Time.deltaTime;
                    if (PhotonNetwork.player == playerTurn && isListClean)
                    {
                        int maxWorms = 0;
                        
                        selectedWorm = localWorms[indexSelectWorms];
                        maxWorms = localWorms.Count;
                        
                        if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow) && indexSelectWorms < maxWorms)
                        {
                            indexSelectWorms = ++indexSelectWorms % maxWorms;

                        }
                        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) && indexSelectWorms > 0)
                        {
                            indexSelectWorms = --indexSelectWorms % maxWorms;
                            
                        }
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
                    if (PhotonNetwork.player == playerTurn && isListClean)
                    {
                        if (controlPlayer == null && selectedWorm != null)
                        {
                            controlPlayer = selectedWorm.GetComponent<PlayerController>();
                            controlPlayer.isControledWorms = true;
                        }
                    }
                }
                else
                {
                    if (controlPlayer != null)
                    {
                        controlPlayer.isControledWorms = false;
                        controlPlayer = null;
                    }
                    phase = GamePhase.Fin;
                }
                break;
            case GamePhase.Fin:
                indexSelectWorms = 0;
                idPlayerToPlay = ++idPlayerToPlay % (int)NetManager.ToG;

                view.RPC("NextPlayerTurn", PhotonTargets.MasterClient, idPlayerToPlay);
                
                view.RPC("SecureEndTurn", PhotonTargets.AllBuffered, null);

                break;
        }
    }


    #endregion


    #region Photon.PunBehaviour RPCs

    [PunRPC]
    public void SecureEndTurn()
    {
        phase = GamePhase.Debut;
    }


    [PunRPC]
    public void UpdatePlayerTurn(PhotonPlayer _playerTurn)
    {
        playerTurn = _playerTurn;
    }

    [PunRPC]
    public void NextPlayerTurn(int _idPlayerToPlay)
    {
        playerTurn = PhotonNetwork.playerList[_idPlayerToPlay];
    }

    [PunRPC]
    public void UpdateLocalWormsList(Vector3[] _spawns)
    {
        for (int j = 0; j < (int)NetManager.hashSet["NbrWorms"]; j++)
        {
            GameObject tmp = PhotonNetwork.Instantiate("PlayerPrefab", new Vector2(
                Random.Range(_spawns[((PhotonNetwork.player.ID -1) * (int)NetManager.ToG)].x, _spawns[((PhotonNetwork.player.ID -1) * (int)NetManager.ToG) + 1].x),
                Random.Range(_spawns[((PhotonNetwork.player.ID -1) * (int)NetManager.ToG)].y, _spawns[((PhotonNetwork.player.ID -1) * (int)NetManager.ToG) + 1].y))
                , Quaternion.identity, 0);
            tmp.name = tmp.name + (PhotonNetwork.player.ID - 1);

            localWorms.Add(tmp);

        }

        isListClean = true;
    }


    #endregion
}
