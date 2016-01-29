using UnityEngine;
using System.Collections;

public class Tabacco : MonoBehaviour {

	public float extractRangeTheta;
	public float initSmokeOfSpeed;

	public GameObject smoke;

	[Header("排出のタイミング")]
	public float extractDuration;

	public bool isExtractSmoke = true;
	public float nextTime;

	[Header("最大排出数")]
	public int maxExtractNumber = 20;

	public int currentExtractedNumber = 0;

	// Use this for initialization
	void Start () {
	
		smoke = Resources.Load ("smoke") as GameObject;

	}

	public void StartExtractSmoke(){
		isExtractSmoke = true;

		nextTime = Time.time;

        Debug.Log("test");
	}
	
	// Update is called once per frame
	void Update () {

        Debug.Log(isExtractSmoke);
		if (isExtractSmoke == true && nextTime + extractDuration <= Time.time && currentExtractedNumber < maxExtractNumber) {
			nextTime = Time.time + extractDuration;
			ExtractSmoke ();
            Debug.Log("yes");
		}

	}

	void ExtractSmoke(){

		GameObject smokeObject = Instantiate (smoke,transform.position,Quaternion.identity) as GameObject;

		float randRange = Random.Range (-extractRangeTheta, extractRangeTheta);
		Vector3 direction = Quaternion.Euler(0.0f, 0.0f, randRange) * transform.up;
		//* Quaternion.AngleAxis (Random.Range (-extractRangeTheta, extractRangeTheta), Vector3.up);

		// 初期速度の初期化.
		smokeObject.GetComponent<Smoke> ().GetComponent<Rigidbody2D>().velocity = direction.normalized * initSmokeOfSpeed;

		currentExtractedNumber++;
	}



}