using UnityEngine;
using System.Collections;

/* Tinkerbox.SmoothCamera
 * 
 * A Smooth Following Camera With Settable Boundries 
 * 
 * This script can either grab the tagged player or 
 * accept an assigned transform in the inspector.
 * 
 * It zooms out when the target is moving fast to
 * allow the user to see what's coming up. It also
 * has settable camera boundries to avoid showing off
 * unfinished areas of the level.
 * 
 */

namespace Tinkerbox {
	public class SmoothCamera : MonoBehaviour {

		// Assignable Target, Leave null to user player tag...
		public Transform target;
		public Rect boundries = new Rect(0,0,10,10);

		//Tweakables
		public float movementLagFactor = 0.1f;
		public float maximumZoom = 1.1f;
		public float zoomSpeedThreshold = 0.1f;

		//We keep some initial values around as a baseline value.
		private float initialZoom;

		//We use this to calculate zoom
		private Vector2 lastPlayerPosition;

		public void Start() {
			if(target == null) {
				try {
					target = GameObject.FindWithTag("Player").transform;
				} catch {
					Debug.LogError("Nothing in this scene has a player tag, and the target of the camera has not been assigned");
				}
			}

			//initialization
			initialZoom  = camera.orthographicSize;
		}

		public void FixedUpdate() {
			//Quadratic interpolation of camera position.
			transform.position = Vector3.Lerp(transform.position, target.position, movementLagFactor);
			//Reset Z distance for 2D
			transform.position = new Vector3(transform.position.x, transform.position.y, -10);
		}

		public void OnDrawGizmos() {
		}
	}
}
