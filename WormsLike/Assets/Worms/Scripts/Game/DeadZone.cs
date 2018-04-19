using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour {

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.tag == "Player")
            other.GetComponent<PlayerController>().lifePoint -= 1.0f;
    }
}
