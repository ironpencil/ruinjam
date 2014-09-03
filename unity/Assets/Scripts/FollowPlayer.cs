using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	public Transform player;
	public Vector2 maxFollowDistance;

	public Vector2 playerPosition = Vector2.zero;
	public Vector2 cameraPosition = Vector2.zero;
	public Vector2 currentDistance = Vector2.zero;

	public Vector2 minCameraBounds = new Vector2(-100, -100);
	public Vector2 maxCameraBounds = new Vector2(100, 100);

	public bool DoFollowY = false;

	// Use this for initialization
	void Start () {
		//this.transform.position = player.position;
	}

	void FixedUpdate() {

		//use FixedUpdate to lerp camera towards player, avoids jittery sprites

		playerPosition = player.transform.position;
		cameraPosition = this.transform.position;
		currentDistance = cameraPosition - playerPosition;

		float distanceX = Mathf.Abs (currentDistance.x);
		float distanceY = Mathf.Abs (currentDistance.y);

		float newPositionX = Mathf.Lerp (cameraPosition.x, playerPosition.x, distanceX * Time.deltaTime * 2.0f);
		float newPositionY = cameraPosition.y;

		if (DoFollowY) {
			newPositionY = Mathf.Lerp (cameraPosition.y, playerPosition.y, distanceY * Time.deltaTime * 2.0f);
		}

		this.transform.position = new Vector3(newPositionX, newPositionY, this.transform.position.z);
	}

	void LateUpdate() {

		//use LateUpdate to keep player within max screen bounds, avoids FixedUpdate camera falling behind

		playerPosition = player.transform.position;
		cameraPosition = this.transform.position;
		currentDistance = cameraPosition - playerPosition;
		
		float distanceX = Mathf.Abs (currentDistance.x);
		float distanceY = Mathf.Abs (currentDistance.y);

		//don't allow max follow distance to be negative
		if (maxFollowDistance.x < 0.0f || maxFollowDistance.y < 0.0f) {
			maxFollowDistance = new Vector2(Mathf.Max(0.0f, maxFollowDistance.x), Mathf.Max(0.0f, maxFollowDistance.y));
		}

		//have to convert maxFollowDistance because viewport starts at bottom left
		// and maxFollowDistance is meant to be a percentage of the screen
		//(basically converting 0.0f-1.0f scale to 0.5f-1.0f scale)
		float maxViewX = (maxFollowDistance.x * 0.5f) + 0.5f;
		float maxViewY = (maxFollowDistance.y * 0.5f) + 0.5f;

		//get another point that is maxFollowDistance away from our camera
		Vector3 maxBounds = camera.ViewportToWorldPoint(new Vector3(maxViewX, maxViewY, 0));

		//maxDistance holds the distance, in world units, that the player is allowed to be from the camera
		Vector2 maxDistance = ((Vector2)maxBounds) - cameraPosition;

		float newX = cameraPosition.x;
		float newY = cameraPosition.y;

		//if the distance to the player from the camera is greater then our maxDistance,
		// make up the difference...

		//...for x...
		if (distanceX > maxDistance.x) {
			newX = newX - ((distanceX - maxDistance.x) * Mathf.Sign(currentDistance.x));
		}

		//...and for y
		if (distanceY > maxDistance.y) {
			newY = newY - ((distanceY - maxDistance.y) * Mathf.Sign(currentDistance.y));
		}

		if (newX < minCameraBounds.x) {
						newX = minCameraBounds.x;
				}
		if (newX > maxCameraBounds.x) {
			newX = maxCameraBounds.x;
		}

		if (newY < minCameraBounds.y) {
			newY = minCameraBounds.y;
		}

		if (newY > maxCameraBounds.y) {
			newY = maxCameraBounds.y;
		}

		//if our newly calculated positions are different from our current camera, move it
		if (cameraPosition.x != newX || cameraPosition.y != newY) {
			this.transform.position = new Vector3(newX, newY, this.transform.position.z);
		}
	}
}
