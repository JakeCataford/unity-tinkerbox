using UnityEngine;
using System.Collections;


namespace Tinkerbox {
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class FollowerAgent : Agent {

		public float acceleration = 10.0f;

		protected override void OnSpawn () {
			rigidbody2D.drag = 10.0f;
			rigidbody2D.gravityScale = 0;
		}

		protected override void InSight () {
			rigidbody2D.AddForce ((targetPlayer.transform.position - transform.position).normalized * acceleration);
		}

		protected override void OnTouchPlayer(GameObject player) {
			player.GetComponent<Agent> ().Damage (1, (targetPlayer.transform.position - transform.position).normalized);
		}

	}
}
