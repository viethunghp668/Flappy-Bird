using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[SerializeField] private float thrust, minTiltSmooth, maxTiltSmooth, hoverDistance, hoverSpeed;
	[SerializeField] private float jumpForce = 1.2f, gravity = 0.7f;
	private bool start, isDead = false;
	private float timer, tiltSmooth, y;
	private Rigidbody2D playerRigid;
	private Quaternion downRotation, upRotation;
	[SerializeField] private SpriteRenderer playerSpriteRenderer;

	void Start () {
		tiltSmooth = maxTiltSmooth;
		playerRigid = GetComponent<Rigidbody2D> ();
		downRotation = Quaternion.Euler (0, 0, -90);
		upRotation = Quaternion.Euler (0, 0, 35);
	}

	void Update () {
		if (!start) {
			// Hover the player before starting the game
			timer += Time.deltaTime;
			y = hoverDistance * Mathf.Sin (hoverSpeed * timer);
			transform.localPosition = new Vector3 (0, y, 0);
		} else {
			// Rotate downward while falling
			transform.rotation = Quaternion.Lerp (transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
		}
		// Limit the rotation that can occur to the player
		transform.rotation = new Quaternion (transform.rotation.x, transform.rotation.y, Mathf.Clamp (transform.rotation.z, downRotation.z, upRotation.z), transform.rotation.w);
	}

	void LateUpdate () {
		if(isDead) return;
		Vector3 velocity = transform.position;
		velocity.y -= gravity * Time.deltaTime;
		if (GameManager.Instance.GameState ()) {
			if (Input.GetMouseButtonDown (0)) {
				if(!start){
					// This code checks the first tap. After first tap the tutorial image is removed and game starts
					start = true;
					GameManager.Instance.GetReady ();
					GetComponent<Animator>().speed = 2;
				}
				// playerRigid.gravityScale = 1f;
				tiltSmooth = minTiltSmooth;
				transform.rotation = upRotation;
				playerRigid.velocity = Vector2.zero;
				velocity.y += jumpForce;
				// Push the player upwards
				// playerRigid.AddForce (Vector2.up * thrust);
				SoundManager.Instance.PlayTheAudio("Flap");
			}
		}
		if (playerRigid.velocity.y < -1f) {
			// Increase gravity so that downward motion is faster than upward motion
			tiltSmooth = maxTiltSmooth;
			// playerRigid.gravityScale = 2f;
		}
		transform.position = velocity;
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.transform.CompareTag ("Score")) {
			Destroy (col.gameObject);
			GameManager.Instance.UpdateScore ();
		} else if (col.transform.CompareTag ("Obstacle")) {
			// Destroy the Obstacles after they reach a certain area on the screen
			foreach (Transform child in col.transform.parent.transform) {
				child.gameObject.GetComponent<BoxCollider2D> ().enabled = false;
			}
			KillPlayer ();
		}
	}

	void OnCollisionEnter2D (Collision2D col) {
		if (col.transform.CompareTag ("Ground")) {
			playerRigid.simulated = false;
			KillPlayer ();
			transform.rotation = downRotation;
		}
	}

	public void KillPlayer () {
		if(isDead) return;
		isDead = true;
		StartCoroutine(SetGrayScaleGradually());
		GameManager.Instance.EndGame ();
		playerRigid.velocity = Vector2.zero;
		// Stop the flapping animation
		GetComponent<Animator> ().enabled = false;
	}

	IEnumerator SetGrayScaleGradually() { 
		float elapsedTime = 0f;
        float phaseDuration = 1f;
        float percentComplete = elapsedTime / phaseDuration;
        while (percentComplete < 1) {
			float currentGrayScaleRatio = Mathf.Lerp(1, 0, percentComplete);
            playerSpriteRenderer.material.SetFloat("_GrayScaleRatio", currentGrayScaleRatio);
            elapsedTime += Time.deltaTime;
            percentComplete = elapsedTime / phaseDuration;
            yield return null;
        }
	}

}