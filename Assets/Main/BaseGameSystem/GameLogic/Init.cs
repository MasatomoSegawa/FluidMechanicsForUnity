using UnityEngine;
using System.Collections;

public class Init : GameLogic {

	public override void Init_GameLogic ()
	{
		base.Init_GameLogic ();
		Debug.Log ("初期化!");
	}

	public override void Update_GameLogic ()
	{
		base.Update_GameLogic ();

		if (Input.GetKeyDown (KeyCode.A)) {
			GameModel.Instance.ChangeState (GameLogicState.Play);
		}

	}

	public override void End_GameLogic ()
	{
		base.End_GameLogic ();
		Debug.Log ("初期化狩猟!");
	}

}