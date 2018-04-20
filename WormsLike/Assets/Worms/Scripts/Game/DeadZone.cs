using UnityEngine;

/*
* @JulienLopez
* @DeadZone.cs
* @Le script s'attache sur le terrain possédent un collider2D en trigger.
*   - Permet de gerer les sorties de terrain des joueurs.
*/

public class DeadZone : MonoBehaviour {

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Player")//S'il s'agit du joueur => le tue
            other.GetComponent<PlayerController>().lifePoint -= 5.0f;
    }
}
