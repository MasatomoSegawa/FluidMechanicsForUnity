using UnityEngine;

public class Smoke : MonoBehaviour {

	public Vector3 velocity;

    public Rigidbody2D myPhysics;

	public float k = 0.5f;

	// ゆらぎ
	public float f_Max;
	public float f_Min;

	private float deltaT;

    void Start()
    {
        myPhysics = GetComponent<Rigidbody2D>();

		deltaT = FluidMechanicsController.Instance.deltaT;

    }
    
	public void UpdateVelocity(Vector3 windSpeed)
    {

		velocity = k * (velocity - windSpeed) * 1.0f / 1.0f * deltaT;

		Vector3 f_vector = new Vector3 (Random.Range (f_Min, f_Max), Random.Range (f_Min, f_Max), 0.0f);

		myPhysics.MovePosition (transform.position + velocity + f_vector);

    }

	void OnDrawGizmos(){

		Vector3 direction = velocity.normalized;
		float length = 5.0f;

		//Debug.Log (transform.position + direction * length);

		Gizmos.color = Color.red;
		Gizmos.DrawLine (transform.position, transform.position + direction * length);

	}

}
