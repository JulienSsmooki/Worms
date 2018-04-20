using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectParticle : MonoBehaviour {

    #region Public Variables


    #endregion

    #region Private Variables

    NetworkManager NetManager;

    GameObject particles;

    Vector3 offset;
    
    bool isAnimating;

    bool netInit = false;
    #endregion

    #region MonoBehaviour CallBacks
    
    void Awake()
    {
        particles = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if(NetManager == null && !netInit)
        {
            NetManager = FindObjectOfType<NetworkManager>();
            netInit = true;
        }
        if(NetManager != null)
        {
            NetManager.connectParticle = this;
            NetManager = null;
        }

        if (isAnimating)
        {
            transform.Rotate(0f, 0f, 360.0f * Time.deltaTime);
            
            particles.transform.localPosition = Vector3.MoveTowards(particles.transform.localPosition, offset, 0.5f * Time.deltaTime);
        }
    }
    #endregion

    #region Public Methods
    
    public void StartWaitingAnimation()
    {
        isAnimating = true;
        offset = new Vector3(0.5f, 0.0f, 0.0f);
        particles.SetActive(true);
    }
    
    public void StopWaitingAnimation()
    {
        particles.SetActive(false);
    }

    #endregion
}
