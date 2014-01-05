using UnityEngine;
using System.Collections;

namespace Tinkerbox {
  [RequireComponent(typeof(Rigidbody2D))]
  public class SimpleTopDownMove : MonoBehaviour {

    public float acceleration = 50f;

  	
	void Start() {
		rigidbody2D.gravityScale = 0;
	}

  	void FixedUpdate () {
  		rigidbody2D.AddForce(new Vector2(Input.GetAxis("Horizontal") * acceleration, Input.GetAxis("Vertical") * acceleration));
		
  	}
  }
}
