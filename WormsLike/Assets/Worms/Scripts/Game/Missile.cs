using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {

    #region Public Variables

    public Vector3 direction = Vector3.zero;
    public float puissance = 10.0f;

    public Rigidbody2D rb2D;
    public BoxCollider2D col2D;
    #endregion


    #region Private Variables

    PhotonView view;

    #endregion


    #region MonoBehaviour CallBacks

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        rb2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();
    }

    public void SetDirPui(Vector3 _dir, float _pui)
    {
        direction = _dir;
        direction.z = 0.0f;
        puissance = _pui;

        Quaternion rot = Quaternion.LookRotation(transform.forward, new Vector3(transform.forward.x, transform.forward.y, 0.0f) + direction);
        transform.rotation = rot;
    }

    public void Launch()
    {
        direction.Normalize();
        rb2D.velocity = direction * puissance;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.tag == "Ground")
        {
            //Destroy(collision.gameObject.GetComponent<PolygonCollider2D>());
            
            UpdateTexture(coll.gameObject.GetComponent<SpriteRenderer>().sprite);

            PhotonNetwork.Destroy(view);
        }
    }

    private void UpdateTexture(Sprite terrain)
    {
        Texture2D tex = terrain.texture;

        Vector3 pos = transform.position;

        pos.x -= tex.width / 2.0f;
        pos.y -= tex.height / 2.0f;

        pos.x *= tex.width;
        pos.y *= tex.height;

        pos.x *= tex.texelSize.x;
        pos.y *= tex.texelSize.y;

        pos.x = Mathf.Abs(pos.x);
        pos.y = Mathf.Abs(pos.y);
        

        Debug.Log(pos.x + " | " + pos.y);

        for (int Y = (int)pos.y - 30; Y < (int)pos.y + 30; Y++)
        {
            for (int X = (int)pos.x - 30; X < (int)pos.x + 30; X++)
            {
                Color Couleur = tex.GetPixel(X, Y);
                if (Couleur.a != 0)
                {
                    tex.SetPixel(X, Y, Color.clear);
                }
            }
        }
        tex.Apply();
    }

    #endregion


    #region Photon.PunBehaviour CallBacks


    #endregion
}
