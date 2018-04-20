using UnityEngine;
using UnityEngine.UI;

/*
* @JulienLopez
* @PlayerController.cs
* @Le script s'attache sur le GameObject du joueur.
*   - Permet de gerer le worm sélectionner par le GameManager
*/

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
    Rigidbody2D rb2D;
    Vector2 velocity = Vector2.zero;
    bool isGrounded = false;
    
    Vector3 dirMouse = Vector3.zero;
    float shootForce = 1.0f;

    Animator anim;
    SpriteRenderer spriteRender;
    bool isFlip = false;

    PhotonView view;

    #endregion

    
    #region MonoBehaviour CallBacks

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (int.Parse(lifeText.text) != (int)lifePoint)//Update la vie du worm
            lifeText.text = ((int)lifePoint).ToString();

        if(isControledWorms)//S'il est controler (set par le GameManager)
        {
            //Fire
            if(Input.GetButton("Jump"))
            {
                //"Désactive" le rigidbody2D
                if(rb2D.bodyType == RigidbodyType2D.Dynamic)
                    rb2D.bodyType = RigidbodyType2D.Static;
                
                //Direction donnée par la souris
                dirMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - (transform.position + Vector3.up * 0.2f);
                dirMouse.Normalize();
                dirMouse.z = 0.0f;

                //S'il n'est pas créer => créer le missile / récupere son script / Init direction-puissance / active le feedback de direction
                if (!isOnFire)
                {
                    isOnFire = true;
                    shootForce = 1.0f;
                    GameObject missile = PhotonNetwork.Instantiate("MissilePrefab", transform.position + Vector3.up * 0.2f, Quaternion.identity, 0);
                    missileScript = missile.GetComponent<Missile>();
                    missileScript.launcher = this;
                    missileScript.SetDirPui(dirMouse, shootForce);

                    feedTarget.SetActive(true);
                }
                else
                {
                    //Update de la direction du missile et du feedback
                    if(missileScript != null)
                        missileScript.SetDirPui(dirMouse, shootForce);

                    feedTarget.transform.localPosition = Vector3.up * 20.0f;

                    Quaternion rot = Quaternion.LookRotation(feedTarget.transform.forward, new Vector3(feedTarget.transform.forward.x, feedTarget.transform.forward.y, 0.0f) + dirMouse);
                    feedTarget.transform.rotation = rot;

                    feedTarget.transform.localPosition += dirMouse.normalized * (5.0f * shootForce);
                    
                }
                
                //Augmente la puissance avec le temps
                if (shootForce < 10.0f)
                    shootForce += Time.deltaTime * 1.5f;
                else
                    shootForce = 10.0f;

            }
            else
            {
                //"Réactive" le rigidbody2D
                if (rb2D.bodyType == RigidbodyType2D.Static)
                    rb2D.bodyType = RigidbodyType2D.Dynamic;
               
                //Déplacement horizontal
                Vector2 moveX = Vector2.zero;
                float InputHorizontal = Input.GetAxis("Horizontal");
                moveX.x = InputHorizontal * scaleDeplacement * Time.deltaTime + rb2D.position.x;

                //Flip du sprite (gauche, droit)
                if (InputHorizontal > 0)
                    isFlip = true;
                else if (InputHorizontal < 0)
                    isFlip = false;

                anim.SetFloat("Speed", Mathf.Abs(InputHorizontal));

                view.RPC("UpdateFlipSprite", PhotonTargets.AllBuffered, isFlip);

                //Physic2D => test si le worm est grounded
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

                //Jump
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
                
                if (isOnFire)//Lance le missile
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
        if(lifePoint < 1 && isAlive)
        {
            isControledWorms = false;
            isAlive = false;
            rb2D.velocity = Vector3.zero;
            anim.SetBool("isDead", true);
            lifeText.gameObject.SetActive(false);
        }
    }
    
    #endregion


    #region Photon.PunBehaviour CallBacks
    /// <summary>
    /// Flip le sprite du worm actuellement déplacer
    /// </summary>
    /// <param name="_isFlip"></param>
    [PunRPC]
    public void UpdateFlipSprite(bool _isFlip)
    {
        spriteRender.flipX = _isFlip;
    }

    #endregion
}
