using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    #region Public Variables

    public List<GameObject> P1Worms = new List<GameObject>();
    public List<GameObject> P2Worms = new List<GameObject>();
    public List<GameObject> P3Worms = new List<GameObject>();
    public List<GameObject> P4Worms = new List<GameObject>();

    #endregion



    #region Private Variables

    NetworkManager NetManager;

    PhotonView view;

    #endregion



    #region MonoBehaviour CallBacks

    private void Awake()
    {
        NetManager = FindObjectOfType<NetworkManager>();

        if (PhotonNetwork.isMasterClient)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
            NetManager.hashSet = PhotonNetwork.room.CustomProperties;

            for (int i = 0; i < (int)NetManager.ToG; i++)
            {
                for (int j = 0; j < (int)NetManager.hashSet["NbrWorms"]; j++)
                {
                    GameObject tmp = PhotonNetwork.Instantiate("PlayerPrefab",new Vector2(
                        Random.Range( spawns[(i * (int)NetManager.ToG)].transform.position.x, spawns[(i * (int)NetManager.ToG) + 1].transform.position.x),
                        Random.Range( spawns[(i * (int)NetManager.ToG)].transform.position.y, spawns[(i * (int)NetManager.ToG) + 1].transform.position.y))
                        , Quaternion.identity, 0);
                    tmp.name = tmp.name + i;
                    tmp.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.playerList[i]);
                    switch (i)
                    {
                        case 0:
                            P1Worms.Add(tmp);
                            break;
                        case 1:
                            P2Worms.Add(tmp);
                            break;
                        case 2:
                            P3Worms.Add(tmp);
                            break;
                        case 3:
                            P4Worms.Add(tmp);
                            break;
                        default: break;
                    }
                }
            }
        }
    }
    
    private void Start()
    {
        if(!PhotonNetwork.isMasterClient)
        {
            StartCoroutine(FindWorms());
        }
    }

    public IEnumerator FindWorms()
    {
        while(true)
        {
            GameObject[] worms = GameObject.FindGameObjectsWithTag("Player");
            if(worms != null && worms.Length > 0)
            {
                foreach (GameObject worm in worms)
                {
                    switch (worm.GetComponent<PhotonView>().ownerId - 1)
                    {
                        case 0:
                            P1Worms.Add(worm);
                            break;
                        case 1:
                            P2Worms.Add(worm);
                            break;
                        case 2:
                            P3Worms.Add(worm);
                            break;
                        case 3:
                            P4Worms.Add(worm);
                            break;
                        default: break;
                    }
                }
                Debug.Log("Find !");
                yield break;
            }
            Debug.Log("Don't find !");
            yield return 0;
        }
    }

    #endregion

}
