using UnityEngine;
using System.Collections;

/// <summary>
/// ゲームロジックの状態を管理する列挙体.
/// </summary>
public enum GameLogicState{
	Init,Play,End,
}

/// <summary>
/// ゲームロジック用に使うスタティッククラス.
/// ゲームの進行を止めたり、状態を管理する.
/// </summary>
public class GameModel : Singleton<GameModel> {

	public static bool _isStop;
	public static bool isStop{
		get{ return _isStop; }
		set{ _isStop = value; }
	}

	// 現在のゲームの状態.
	public static GameLogicState gameLogicState = GameLogicState.Init;

	// 現在のゲームロジックのスクリプト.
	GameLogic currentGameLogic;

	void Start(){
		currentGameLogic = this.GetComponent<Init> ();
		currentGameLogic.Init_GameLogic ();
	}

	/// <summary>
	/// ゲームロジックの状態を遷移させるメソッド.
	/// </summary>
	/// <param name="nextState">Next state.</param>
	public void ChangeState(GameLogicState nextState){

		// 前のゲームロジックの終了処理.
		currentGameLogic.End_GameLogic ();

		gameLogicState = nextState;
		switch (gameLogicState) {
		case GameLogicState.Init:
			currentGameLogic = this.GetComponent<Init> ();
			break;

		case GameLogicState.Play:
			currentGameLogic = this.GetComponent<Play> ();
			break;

		case GameLogicState.End:
			currentGameLogic = this.GetComponent<End> ();
			break;
		}

		// 新しいゲームロジックを初期化.
		currentGameLogic.Init_GameLogic ();

	}

}
