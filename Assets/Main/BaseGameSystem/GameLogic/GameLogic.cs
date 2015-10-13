using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour {

	bool On = false;

	void Update(){

		if (On != false) {
			Update_GameLogic ();
		}

	}

	public virtual void Init_GameLogic(){
		On = true;
		Debug.Log ("Init_GameLogic");
	}

	public virtual void Update_GameLogic(){

	}

	public virtual void End_GameLogic(){
		On = false;
		Debug.Log ("End_GameLogic");
	}

}
