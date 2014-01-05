using UnityEngine;
using System.Collections;

namespace Tinkerbox {	
	public class PlayerAgent : Agent {
		protected override void OnDamage (Vector2 source)
		{
			//knockback
			rigidbody2D.velocity = rigidbody2D.velocity + (source * 30f);
		}
	}
}
