using UnityEngine;

public class PhysicsRoom : MonoBehaviour {

    public Vector2 velocity;
    public float presser;
    public GameObject arrowSprite;
    public float scale;
    public bool isWall = false;

    public void UpdateVelocity(float VelX,float VelY)
    {

        float mag = Mathf.Sqrt(VelX * VelX + VelY * VelY);
        float theta = Mathf.Atan2(VelY, VelX);

        velocity = new Vector2(VelX, VelY);
        
        if(mag < 10.0f)
        {
            arrowSprite.transform.localScale = new Vector2(1.0f * mag * scale, 1.0f);
        }

        arrowSprite.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), theta);
        
    }
	
}
