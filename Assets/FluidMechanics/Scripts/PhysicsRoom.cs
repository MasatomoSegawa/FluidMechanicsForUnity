using UnityEngine;

public class PhysicsRoom : MonoBehaviour {

    public Vector2 velocity;
    public float presser;
    public GameObject arrowSprite;
    public float scale;
    public bool isWall = false;

    public float Left;
    public float Right;
    public float Up;
    public float Down;

    public void UpdateVelocity(float left,float right, float up, float down,float VelX, float VelY)
    {

        Left = left;
        Right = right;
        Up = up;
        Down = down;

        if (isWall == false)
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
