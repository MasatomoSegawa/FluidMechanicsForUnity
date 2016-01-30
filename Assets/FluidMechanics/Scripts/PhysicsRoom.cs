using UnityEngine;

public class PhysicsRoom : MonoBehaviour
{

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

        velocity = new Vector2(VelX, VelY);

        /*
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


            if (!float.IsNaN(transform.rotation.x) && !float.IsNaN(transform.rotation.y) && !float.IsNaN(transform.rotation.z))
            {
                //Do stuff

                Quaternion temp = Quaternion.AngleAxis(angle, new Vector3(0.0f, 0.0f, 1.0f));

                if (!float.IsNaN(temp.x) && !float.IsNaN(temp.y) && !float.IsNaN(temp.z))
                {


                    // 角度の更新.
                    arrowSprite.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0.0f, 0.0f, 1.0f));
                }

            }

		}*/


    }

    public float mag;
    Vector2 LeftSideVelocity, RightSideVelocity, UpSideVelocity, DownSideVelocity;

    void OnDrawGizmos()
    {

        Vector3 direction = velocity.normalized;
        Vector3 rightDirection = RightSideVelocity.normalized;
        Vector3 leftDirection = LeftSideVelocity.normalized;
        Vector3 upDirection = UpSideVelocity.normalized;
        Vector3 downDirection = DownSideVelocity.normalized;
        float length = Mathf.Clamp(velocity.magnitude, 1.0f, 2.5f);

        //Debug.Log (transform.position + direction * length);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + direction * length);
        Gizmos.DrawCube(transform.position + direction * length, new Vector3(0.3f, 0.3f, 0.0f));

        Gizmos.color = Color.blue;
        // 左
        Gizmos.DrawLine(transform.position - Vector3.left * 2.935f,
            transform.position - Vector3.left * 2.935f + Vector3.right * Left * 2.0f);
        //Gizmos.DrawCube (transform.position + direction * length, new Vector3 (0.3f, 0.3f, 0.0f));
    }

    public void UpdateVelocity(float left, float right, float up, float down, float VelX, float VelY)
    {

        Debug.Log("?");
        Debug.Log(left);

        Left = left;
        Right = right;
        Up = up;
        Down = down;

        velocity = new Vector2(VelX, VelY);
        UpSideVelocity = new Vector2(0.0f, Up);
        DownSideVelocity = new Vector2(0.0f, Down);
        RightSideVelocity = new Vector2(right, 0.0f);
        LeftSideVelocity = new Vector2(left, 0.0f);

        if (myType == RoomType.Normal)
        {

            mag = velocity.sqrMagnitude;
            float theta = Mathf.Atan2(velocity.normalized.x, velocity.normalized.y);

            /*
			Quaternion currentQuaternion = Quaternion.AngleAxis(theta, new Vector3(0.0f, 0.0f, -1.0f));

            if (mag < 10.0f)
            {
                float newScale = Mathf.Clamp(mag * scale, 0.1f, 3.6f);
                arrowSprite.transform.localScale = new Vector2(newScale, 1.0f);
            }

			//arrowSprite.transform.rotation = currentQuaternion;

			arrowSprite.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), theta);
			*/
        }


    }


    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Smoke" && myType == RoomType.OutLet)
        {
            Destroy(other.gameObject);
        }

    }

}