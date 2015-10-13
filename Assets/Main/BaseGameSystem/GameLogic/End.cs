using UnityEngine;
using System.Collections;

public class End : GameLogic {

	public override void Init_GameLogic ()
	{
		base.Init_GameLogic ();
		Debug.Log ("おわりのはじまり!");

	}

	public override void Update_GameLogic ()
	{
		base.Update_GameLogic ();
		if (Input.GetKeyDown (KeyCode.A)) {
			FadeManager.Instance.LoadLevel ("Main", 1.0f);
			SoundManager.Instance.FadeOutBGM ("Nanahira");
		}
	}

	public override void End_GameLogic ()
	{
		base.End_GameLogic ();
		Debug.Log ("End_End!!");

	}

}
