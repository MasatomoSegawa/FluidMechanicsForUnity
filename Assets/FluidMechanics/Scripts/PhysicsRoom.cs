using UnityEngine;

public class PhysicsRoom : MonoBehaviour {

    public Vector2 velocity;
    public float presser;
    public GameObject arrowSprite;
    public float scale;
	public RoomType myType;

	public Vector2 constantVelocity;

    public float Left;
    public float Right;
    public float Up;
    public float Down;

	public void UpdateVelocity(float VelX, float VelY)
	{

		if (myType == RoomType.Normal)
		{

			float angle = Mathf.Atan2(VelX * 100.0f,VelY * 100.0f) * 100.0f;
			velocity = new Vector2(VelX, VelY);

			float mag = velocity.sqrMagnitude;

			// スケールの更新.
			if (mag < 10.0f)
			{
				float newScale = Mathf.Clamp(mag * scale, 0.1f, 3.6f);
				arrowSprite.transform.localScale = new Vector2(newScale, 1.0f);
			}

			// 角度の更新.
			arrowSprite.transform.rotation = Quaternion.AngleAxis (angle, new Vector3 (0.0f, 0.0f, 1.0f));



		}


	}

    public void UpdateVelocity(float left,float right, float up, float down,float VelX, float VelY)
    {

        Left = left;
        Right = right;
        Up = up;
        Down = down;

		if (myType == RoomType.Normal)
        {

            velocity = new Vector2(VelX, VelY);

            float mag = velocity.sqrMagnitude;
            float theta = Mathf.Atan2(velocity.normalized.x , velocity.normalized.y);

            Quaternion currentQuaternion = Quaternion.AngleAxis(theta, new Vector3(0.0f, 0.0f, 1.0f));

            if (mag < 10.0f)
            {
                float newScale = Mathf.Clamp(mag * scale, 0.1f, 3.6f);
                arrowSprite.transform.localScale = new Vector2(newScale, 1.0f);
            }

            arrowSprite.transform.rotation = currentQuaternion;

            //arrowSprite.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), angle);

        }


    }
	
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if(other.tag == "Smoke")
        {
			Smoke smoke = other.GetComponent<Smoke>();
			smoke.Test (velocity);

        }

    }

    void OnTriggerStay2D(Collider2D other)
    {

        if (other.tag == "Smoke")
        {
			Debug.Log (velocity);
			//Smoke smoke = other.GetComponent<Smoke>();
			//smoke.UpdateVelocity(velocity);

        }

    }

}