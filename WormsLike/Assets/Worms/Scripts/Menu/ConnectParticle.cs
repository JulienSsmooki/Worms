using UnityEngine;

/*
* @JulienLopez
* @ConnectParticle.cs
* @Le script s'attache sur un gameObject vide avec un particle system en enfant.
*   - Permet de lancer un feedback lors de la connexion (pour les connexion trop lente).
*/

public class ConnectParticle : MonoBehaviour {
    
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
        if(NetManager == null && !netInit)//Récupère le NetworkManager
        {
            NetManager = FindObjectOfType<NetworkManager>();
            netInit = true;
        }
        if(NetManager != null)//Se set dans le NetworkManager
        {
            NetManager.connectParticle = this;
            NetManager = null;
        }

        if (isAnimating)//Si l'anim est activé
        {
            transform.Rotate(0f, 0f, 360.0f * Time.deltaTime);
            
            particles.transform.localPosition = Vector3.MoveTowards(particles.transform.localPosition, offset, 0.5f * Time.deltaTime);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Lance l'anim d'attente de la connexion
    /// </summary>
    public void StartWaitingAnimation()
    {
        isAnimating = true;
        offset = new Vector3(0.5f, 0.0f, 0.0f);
        particles.SetActive(true);
    }
    /// <summary>
    /// Stop l'anim d'attente de la connexion
    /// </summary>
    public void StopWaitingAnimation()
    {
        particles.SetActive(false);
    }

    #endregion
}
