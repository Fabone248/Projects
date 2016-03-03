using UnityEngine;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour {

    public int speed = 1;

    Rigidbody rigidbody;
    Vector3 velocity;
    
	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>(); 
	}
	
	// Update is called once per frame
	void Update () {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;
	}

    void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
    }
}
