using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    #region Public Variables

    public bool isControledWorms = false;

    [Range(0.0f, 2.0f)] public float scaleDeplacement = 1.0f;

    public float lifePoint = 100.0f;

    #endregion



    #region Private Variables

    NetworkManager NetManager;

    GameManager GM;

    Rigidbody2D rb2D;
    Vector2 velocity = Vector2.zero;
    bool isGrounded = false;

    #endregion



    #region MonoBehaviour CallBacks

    private void Awake()
    {
        NetManager = FindObjectOfType<NetworkManager>();
        GM = FindObjectOfType<GameManager>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(isControledWorms)
        {
            Vector2 moveX = Vector2.zero;
            moveX.x = Input.GetAxis("Horizontal") * scaleDeplacement * Time.deltaTime + rb2D.position.x;

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = 1.0f;
            }
            if(!isGrounded)
            {
                velocity.y -= rb2D.gravityScale * Time.deltaTime;
            }

            rb2D.velocity = velocity;
            moveX.y = rb2D.position.y;
            rb2D.position = moveX;
        }
    }

    

    void OnCollisionEnter2D(Collision2D coll)
    {
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        isGrounded = false;
    }

    #endregion


    #region Photon.PunBehaviour CallBacks


    #endregion
}
