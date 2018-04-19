using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    #region Public Variables

    public bool isControledWorms = false;
    public bool isOnFire = false;

    [Range(0.0f, 2.0f)] public float scaleDeplacement = 1.0f;

    public float lifePoint = 10.0f;
    public bool isAlive = true;

    public Text lifeText;
    #endregion
    

    #region Private Variables

    NetworkManager NetManager;

    GameManager GM;

    Rigidbody2D rb2D;
    Vector2 velocity = Vector2.zero;
    bool isGrounded = false;
    
    Missile missileScript;
    Vector3 dirMouse = Vector3.zero;
    float shootForce = 10.0f;

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
        if (int.Parse(lifeText.text) != lifePoint)
            lifeText.text = lifePoint.ToString();

        if(isControledWorms)
        {
            //Fire
            if(Input.GetButton("Jump"))
            {
                dirMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - (transform.position + Vector3.up * 0.2f);
                dirMouse.Normalize();

                if (!isOnFire)
                {
                    isOnFire = true;
                    GameObject missile = PhotonNetwork.Instantiate("MissilePrefab", transform.position + Vector3.up * 0.2f, Quaternion.identity, 0);
                    missileScript = missile.GetComponent<Missile>();
                    missileScript.SetDirPui(dirMouse, shootForce);
                }
                else
                {
                    if(missileScript != null)
                        missileScript.SetDirPui(dirMouse, shootForce);
                }
            }
            else
            {

                Vector2 moveX = Vector2.zero;
                moveX.x = Input.GetAxis("Horizontal") * scaleDeplacement * Time.deltaTime + rb2D.position.x;

                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
                {
                    velocity.y = 1.0f;
                }
                if (!isGrounded)
                {
                    velocity.y -= rb2D.gravityScale * Time.deltaTime;
                }

                rb2D.velocity = velocity;
                moveX.y = rb2D.position.y;
                rb2D.position = moveX;


                if (isOnFire)
                {
                    if (missileScript != null)
                    {
                        missileScript.rb2D.bodyType = RigidbodyType2D.Dynamic;
                        missileScript.col2D.enabled = true;
                        missileScript.Launch();
                        missileScript = null;
                    }
                    isOnFire = false;
                }
            }

        }

        //Update life
        if(lifePoint <= 0 && isAlive)
        {
            isControledWorms = false;
            isAlive = false;
            transform.position = new Vector3(10000, 10000);
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.velocity = Vector3.zero;
            GM.StopAllCoroutines();
            GM.StartCoroutine(GM.WormsAreYouAlive());
        }
    }

    

    void OnCollisionEnter2D(Collision2D coll)
    {
        if(Vector2.Dot( -transform.up, coll.contacts[0].point) > 0)
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
