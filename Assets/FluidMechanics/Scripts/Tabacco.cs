using UnityEngine;
using System.Collections;

public class Tabacco : MonoBehaviour {

	public float extractRangeTheta;
	public float initSmokeOfSpeed;

	public GameObject smoke;

	[Header("排出のタイミング")]
	public float extractDuration;

	private bool isExtractSmoke = false;
	private float nextTime;

	// Use this for initialization
	void Start () {
	
		smoke = Resources.Load ("smoke") as GameObject;

	}

	public void StartExtractSmoke(){
		isExtractSmoke = true;

		nextTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (isExtractSmoke == true && nextTime + extractDuration <= Time.time) {
			nextTime = Time.time + extractDuration;
			ExtractSmoke ();
		}

	}

	void ExtractSmoke(){

		GameObject smokeObject = Instantiate (smoke,transform.position,Quaternion.identity) as GameObject;

		float randRange = Random.Range (-extractRangeTheta, extractRangeTheta);
		Vector3 direction = Quaternion.Euler(0.0f, 0.0f, randRange) * transform.up;
		//* Quaternion.AngleAxis (Random.Range (-extractRangeTheta, extractRangeTheta), Vector3.up);

		// 初期速度の初期化.
		smokeObject.GetComponent<Smoke> ().velocity = direction.normalized * initSmokeOfSpeed;

	}



}