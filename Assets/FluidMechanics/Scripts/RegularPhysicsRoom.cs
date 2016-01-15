using UnityEngine;
using System.Collections;

public class RegularPhysicsRoom : MonoBehaviour {

    public Vector2 vel;
    public RoomType myType;
    public GameObject arrowSprite;
    public float scale;
    public float p;
	public Vector2 constantVelocity;

    public void UpdateVariables(float VelX, float VelY,float p)
    {

        if (myType == RoomType.Normal)
        {

            this.p = p;

            float angle = Mathf.Atan2(VelX * 100.0f, VelY * 100.0f) * 100.0f;
            vel = new Vector2(VelX, VelY);

            if (Input.GetKeyDown(KeyCode.B))
                Debug.Log(angle);

            float mag = vel.sqrMagnitude;

            // スケールの更新.
            if (mag < 10.0f)
            {
                float newScale = Mathf.Clamp(mag * scale, 0.1f, 3.6f);
                arrowSprite.transform.localScale = new Vector2(newScale, 1.0f);
            }

            // 角度の更新.
            arrowSprite.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0.0f, 0.0f, 1.0f));

        }

    }

	void OnTriggerEnter2D(Collider2D other){

		Debug.Log (other.name);

		if (other.tag == "Smoke") {

			Destroy (other);

		}

	}

}
