using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    #region Public Variables
    

    #endregion



    #region Private Variables

    NetworkManager NetManager;

    #endregion



    #region MonoBehaviour CallBacks

    private void Awake()
    {
        NetManager = FindObjectOfType<NetworkManager>();
        
    }

    #endregion


    #region Photon.PunBehaviour CallBacks
    

    #endregion
}
