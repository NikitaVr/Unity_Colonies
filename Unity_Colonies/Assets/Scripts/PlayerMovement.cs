using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public Rigidbody2D playerRigidBody;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        playerRigidBody = GetComponent<Rigidbody2D>();
        float movementX = Input.GetAxis("Horizontal");
        float movementY = Input.GetAxis("Vertical");
        int speed = 5;
        Vector2 direction = new Vector2(movementX, movementY);
        Vector2 velocity = direction * speed;
        playerRigidBody.velocity = velocity;
    }
}
