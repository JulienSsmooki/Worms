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
    public GameObject feedTarget;
    public Missile missileScript;
    #endregion
    

    #region Private Variables

    NetworkManager NetManager;

    GameManager GM;

    Rigidbody2D rb2D;
    Vector2 velocity = Vector2.zero;
    bool isGrounded = false;
    
    Vector3 dirMouse = Vector3.zero;
    float shootForce = 1.0f;

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
                if(rb2D.bodyType == RigidbodyType2D.Dynamic)
                    rb2D.bodyType = RigidbodyType2D.Static;

                if (shootForce < 10.0f)
                    shootForce += Time.deltaTime * 1.5f;
                else
                    shootForce = 10.0f;

                dirMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - (transform.position + Vector3.up * 0.2f);
                dirMouse.Normalize();
                dirMouse.z = 0.0f;

                if (!isOnFire)
                {
                    isOnFire = true;
                    GameObject missile = PhotonNetwork.Instantiate("MissilePrefab", transform.position + Vector3.up * 0.2f, Quaternion.identity, 0);
                    missileScript = missile.GetComponent<Missile>();
                    missileScript.SetDirPui(dirMouse, shootForce);

                    feedTarget.SetActive(true);
                }
                else
                {
                    if(missileScript != null)
                        missileScript.SetDirPui(dirMouse, shootForce);

                    feedTarget.transform.localPosition = Vector3.up * 20.0f;

                    Quaternion rot = Quaternion.LookRotation(feedTarget.transform.forward, new Vector3(feedTarget.transform.forward.x, feedTarget.transform.forward.y, 0.0f) + dirMouse);
                    feedTarget.transform.rotation = rot;

                    feedTarget.transform.localPosition += dirMouse.normalized * (5.0f * shootForce);
                    
                }
            }
            else
            {
                if (rb2D.bodyType == RigidbodyType2D.Static)
                    rb2D.bodyType = RigidbodyType2D.Dynamic;


                Vector2 moveX = Vector2.zero;
                moveX.x = Input.GetAxis("Horizontal") * scaleDeplacement * Time.deltaTime + rb2D.position.x;

                Debug.DrawLine(transform.position, transform.position - transform.up, Color.red, 1.0f);
                RaycastHit2D hit;
                hit = Physics2D.BoxCast(transform.position - (Vector3.up * 0.115f), new Vector2(0.2f,0.01f), 0.0f, - transform.up, 0.0f);
                if (hit.collider != null)
                {
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }

                if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
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
                        shootForce = 1.0f;
                        missileScript.rb2D.bodyType = RigidbodyType2D.Dynamic;
                        missileScript.col2D.enabled = true;
                        missileScript.Launch();
                    }
                    feedTarget.SetActive(false);
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
    
    #endregion


    #region Photon.PunBehaviour CallBacks


    #endregion
}
