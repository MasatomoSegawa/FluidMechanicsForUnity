using UnityEngine;
using System.Collections;

public class Play : GameLogic {

	public override void Init_GameLogic ()
	{
		base.Init_GameLogic ();
		Debug.Log ("GamePlay!");
		SoundManager.Instance.PlayBGM ("Nanahira");
	}

	public override void Update_GameLogic ()
	{
		base.Update_GameLogic ();
		if (Input.GetKeyDown (KeyCode.A)) {
			GameModel.Instance.ChangeState (GameLogicState.End);
		}

	}

	public override void End_GameLogic ()
	{
		base.End_GameLogic ();
		Debug.Log ("GamePlay終了!");
	}

}
