using UnityEngine;

public class PhysicsRoom : MonoBehaviour {

    public Vector2 velocity;
    public float presser;
    public GameObject arrowSprite;
    public float scale;
	public RoomType myType;

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

			if (Input.GetKeyDown (KeyCode.B)) {
				Debug.Log (angle);
			}

			#region Scale
			// スケールの更新.
			float mag = velocity.sqrMagnitude;
			float newScale = Mathf.Clamp(mag * scale, 0.1f, 3.6f);
			Vector2 localScale = new Vector2 (newScale, 1.0f);
			if (!float.IsNaN (localScale.x) && !float.IsNaN (localScale.y)) {
				arrowSprite.transform.localScale = new Vector2 (newScale, 1.0f);

				arrowSprite.transform.localScale = Vector2.ClampMagnitude (arrowSprite.transform.localScale, 10.0f);
			}
			#endregion

			#region rotation
			// Quaternionの整合性チェック.
			Quaternion rotation = Quaternion.AngleAxis (angle, new Vector3 (0.0f, 0.0f, 1.0f));
			if (rotation != null && 
				!float.IsNaN (rotation.w) && 
				!float.IsNaN (rotation.x) && 
				!float.IsNaN (rotation.y) && 
				!float.IsNaN (rotation.z)) {  

				// 角度の更新.
				arrowSprite.transform.rotation = Quaternion.AngleAxis (angle, new Vector3 (0.0f, 0.0f, 1.0f));
			}
			#endregion

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
	
}
