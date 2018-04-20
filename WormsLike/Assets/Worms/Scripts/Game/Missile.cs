using UnityEngine;

/*
* @JulienLopez
* @Missile.cs
* @Le script s'attache sur le GameObject du missile.
*   - Permet de gerer le missile créer par un worm.
*/

public class Missile : MonoBehaviour {

    #region Public Variables

    public Vector3 direction = Vector3.zero;
    public float puissance = 10.0f;

    public Rigidbody2D rb2D;
    public BoxCollider2D col2D;
    public PhotonView view;

    public PlayerController launcher;
    #endregion


    #region Private Variables

    GameObject terrain;

    #endregion


    #region MonoBehaviour CallBacks
    private void OnDestroy()
    {
        //Cas où le missile est détruit à la fin du timer
        if (launcher != null)
        {
            //Désactive tout ce qui est liée au tir
            launcher.isOnFire = false;
            launcher.feedTarget.SetActive(false);
        }
    }


    private void Awake()
    {
        terrain = GameObject.FindGameObjectWithTag("Ground");
        view = GetComponent<PhotonView>();
        rb2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if(rb2D.velocity.magnitude > 0.0f)//Update de la rotation lors du parcours du missil en l'air
        {
            Quaternion rot = Quaternion.LookRotation(transform.forward, new Vector3(transform.forward.x + rb2D.velocity.x, transform.forward.y + rb2D.velocity.y, 0.0f));
            transform.rotation = rot;
        }
    }

    /// <summary>
    /// Set la direction et la puissance du missile.
    /// </summary>
    /// <param name="_dir"></param>
    /// <param name="_pui"></param>
    public void SetDirPui(Vector3 _dir, float _pui)
    {
        direction = _dir;
        puissance = _pui;

        Quaternion rot = Quaternion.LookRotation(transform.forward, new Vector3(transform.forward.x, transform.forward.y, 0.0f) + direction);
        transform.rotation = rot;
    }

    /// <summary>
    /// Lance le missile.
    /// </summary>
    public void Launch()
    {
        direction.Normalize();
        rb2D.velocity = direction * puissance;
    }

    /// <summary>
    /// Collision avec le terrain envoie la position exact à tout les joueurs pour amputé
    /// le terrain de manière déterministe.
    /// </summary>
    /// <param name="coll"></param>
    void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.tag == "Ground")
        {
            Vector3 impactPoint = coll.contacts[0].point;

            view.RPC("UpdateTexture", PhotonTargets.All, impactPoint);

        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Test si un worms se trouve dans le rayon d'impact du missile.
    /// Si oui => perte de vie.
    /// </summary>
    /// <param name="_impactPoint"></param>
    /// <param name="_pixelsPerUnit"></param>
    public void TutchingWorms(Vector3 _impactPoint, float _pixelsPerUnit)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(_impactPoint.x,_impactPoint.y), 70.0f / _pixelsPerUnit);
        if (hitColliders != null)
        {
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag == "Player")
                {
                    PlayerController pc = collider.gameObject.GetComponent<PlayerController>();
                    if(Vector3.Magnitude(collider.gameObject.transform.position - _impactPoint) < (70.0f / _pixelsPerUnit)) //Impacte proche
                    {
                        pc.lifePoint -= (0.7f - Vector3.Magnitude(collider.gameObject.transform.position - _impactPoint)) * 100.0f;
                    }
                    if(pc.lifePoint < 0.0f)
                    {
                        pc.lifePoint = 0.0f;
                    }
                }
            }
        }
    }

    #endregion


    #region Photon.PunBehaviour RPCs
    /// <summary>
    /// RPC : Destruction du terrain à un point précis.
    /// </summary>
    /// <param name="_impactPoint"></param>
    [PunRPC]
    private void UpdateTexture(Vector3 _impactPoint)
    {
        //Récupere le terrain et la position de l'impact
        Sprite spriteTerrain = terrain.GetComponent<SpriteRenderer>().sprite;

        Texture2D tex = spriteTerrain.texture;

        Vector3 pos = _impactPoint;

        pos *= spriteTerrain.pixelsPerUnit;

        pos.x += tex.width / 2.0f;
        pos.y += tex.height / 2.0f;

        TutchingWorms(_impactPoint, spriteTerrain.pixelsPerUnit);

        //Détruit le terrain
        int offset = 80;
        for (int Y = (int)pos.y - offset; Y < (int)pos.y + offset; Y++)
        {
            for (int X = (int)pos.x - offset; X < (int)pos.x + offset; X++)
            {
                Color Couleur = tex.GetPixel(X, Y);
                if (Couleur.a != 0)
                {
                    float Sqrt = Mathf.Sqrt(((Y - pos.y) * (Y - pos.y)) + ((X - pos.x) * (X - pos.x)));

                    if (Sqrt < 70)
                        tex.SetPixel(X, Y, Color.black);
                    if (Sqrt < 65)
                        tex.SetPixel(X, Y, Color.clear);

                }
            }
        }
        tex.Apply();

        //Update le collider
        Destroy(terrain.GetComponent<PolygonCollider2D>());
        terrain.AddComponent<PolygonCollider2D>();

        //Détruit le missile
        rb2D.bodyType = RigidbodyType2D.Kinematic;
        rb2D.velocity = Vector2.zero;
        Destroy(gameObject);
    }
    
    #endregion
}
