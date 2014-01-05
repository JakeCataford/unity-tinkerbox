using UnityEngine;
using System.Collections.Generic;

namespace Tinkerbox {
	public class Agent : MonoBehaviour {

		public enum State { IDLE, ENGAGING, DEAD };

		//Interface Methods
		protected virtual void OnSpawn() {}
		protected virtual void OnSight() {}
		protected virtual void InSight() {}
		protected virtual void OnLoseSight() {}
		protected virtual void OnStartIdle() {}
		protected virtual void Idle() {}
		protected virtual void OnDamage(Vector2 source) {}
		protected virtual void OnDie(Vector2 damageSource) {}
		protected virtual void Always() {}
		protected virtual void OnTouchPlayer(GameObject player) {}

		public int hitPoints = 5;
		public State coreState = State.IDLE;
		public float visionRange = 2f;
		public GameObject targetPlayer;
		public bool DebugAgent = false;
		public float selfCollisionBuffer = 1.1f;

		void Start() {
			OnSpawn ();
		}

		void Update() {
			List<GameObject> playersInSight = PlayersInSight ();
			if (coreState == State.IDLE && playersInSight.Count > 0) {
				OnSight ();
				coreState = State.ENGAGING;
				targetPlayer = playersInSight [Mathf.FloorToInt (Random.value * playersInSight.Count)];
			} else if (coreState == State.IDLE) {
				Idle ();
			}

			if(coreState == State.ENGAGING) {
				//check if we can still see the player...
				RaycastHit2D hit = Physics2D.Raycast(this.transform.position + ((targetPlayer.transform.position - transform.position).normalized * selfCollisionBuffer), targetPlayer.transform.position - transform.position);
				if(hit && hit.collider.tag == "Player") {
					InSight();
				} else {
					OnLoseSight();
					//Otherwise try to assign a new target...
					if(playersInSight.Count > 0) {
						targetPlayer = playersInSight[Mathf.FloorToInt(Random.value * playersInSight.Count)];
					} else {
						//or give up and idle...
						coreState = State.IDLE;
						OnStartIdle();
					}
				}
			}

			Always ();
		}

		public void Damage(int points, Vector2 sourceDirection) {
			hitPoints -= points;
			//We send the source vector of the damage to calculate knockback and other stuff...
			if (points > 0) {
				OnDamage (sourceDirection);
			} else {
				OnDie (sourceDirection);
			}
		}

		void OnDrawGizmos() {
			if (DebugAgent) {
				if (coreState == State.IDLE) {
						Gizmos.color = Color.green;
				}

				if (coreState == State.ENGAGING) {
						Gizmos.color = Color.red;
				}

				if (coreState == State.DEAD) {
						Gizmos.color = Color.gray;
				}

				Gizmos.DrawWireCube (transform.position, Vector3.one);
				Gizmos.DrawWireSphere (transform.position, visionRange);
				if (coreState == State.ENGAGING) {
						RaycastHit2D hit = Physics2D.Raycast (this.transform.position, targetPlayer.transform.position - transform.position);
						if (hit) {
								Gizmos.DrawLine (this.transform.position, hit.point);
						}
				}
			}
		}

		void OnCollisionEnter2D(Collision2D col) {
			if (col.collider.tag == "Player") {
				OnTouchPlayer(col.collider.gameObject);
			}
		}

		private List<GameObject> PlayersInSight() {
			Collider2D[] colliders = Physics2D.OverlapCircleAll (transform.position, visionRange);
			List<GameObject> playersInSight = new List<GameObject> ();
			foreach (Collider2D collider in colliders) {
				if(collider.tag == "Player") {
					playersInSight.Add(collider.gameObject);
				}
			}
			return playersInSight;
		}

	}
}
