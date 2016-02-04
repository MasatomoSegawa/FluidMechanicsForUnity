using UnityEngine;
using System.Collections;

public class Smoke : MonoBehaviour {

	private Vector3 affectWind;

    public Rigidbody2D myPhysics;

	public float k = 0.5f;

	// ゆらぎ
	public float f_Max;
	public float f_Min;

	private float deltaT;
	private float deltaX;
	private float deltaY;

	public Vector2 windSpeed;

	public Vector2 currentVelocity;


    void Start()
    {
        myPhysics = GetComponent<Rigidbody2D>();

		deltaT = FluidMechanicsController.Instance.deltaT;

    }

	void Update(){

        if (!float.IsNaN(myPhysics.velocity.x) && !float.IsNaN(myPhysics.velocity.y))
        { 

            Vector2 temp = ((windSpeed - myPhysics.velocity) * k);
            if (!float.IsNaN(temp.x) && !float.IsNaN(temp.y))
            {
                myPhysics.velocity += ((windSpeed - myPhysics.velocity) * k) * deltaT;

				myPhysics.velocity = new Vector2 (myPhysics.velocity.x * deltaX, myPhysics.velocity.y * deltaY);

                currentVelocity = myPhysics.velocity;
            }
        }
	}
		
	void LateUpdate(){
		windSpeed = Vector3.zero;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
	
		if (other.GetComponent<PhysicsRoom> () != null) {
			Vector3 wind = other.GetComponent<PhysicsRoom> ().velocity;
			this.windSpeed += new Vector2 (wind.x, wind.y);
		}

	}
		    
	void OnTriggerStay2D(Collider2D other)
	{
	
		if (other.GetComponent<PhysicsRoom> () != null) {
			Vector3 wind = other.GetComponent<PhysicsRoom> ().velocity;
			this.windSpeed += new Vector2 (wind.x, wind.y);
		}
			
	}

	/*
	void OnDrawGizmos(){


		Vector2 direction = velocity.normalized;
		float length = 5.0f;

		//Debug.Log (transform.position + direction * length);

		Gizmos.color = Color.red;
		Gizmos.DrawLine (transform.position, transform.position + direction * length);

	}*/

}
