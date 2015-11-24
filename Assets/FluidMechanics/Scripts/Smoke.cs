using UnityEngine;

public class Smoke : MonoBehaviour {

    public Vector2 velocity;

    public Rigidbody2D myPhysics;

    void Start()
    {
        myPhysics = GetComponent<Rigidbody2D>();
    }
    
    public void UpdateVelocity(Vector2 windSpeed)
    {

        velocity = windSpeed;

        Vector2 nowVelocity = myPhysics.velocity;

        myPhysics.velocity = 0.5f * (nowVelocity - windSpeed) * 1.0f / 1.0f;

    }

}
