	using UnityEngine;
using System.Collections.Generic;

public struct RoomInformation{

	public int ROOM_MAX_X;
	public int ROOM_MAX_Y;
	public List<List<GameObject>> physicsRooms;

	public Vector2Int[] inletPositions;
	public Vector2Int[] outletPostions;
}

[System.Serializable]
public struct Vector2Int{
	public int x, y;

	public Vector2Int(int x,int y){
		this.x = x;
		this.y = y;
	}

	public string ToString(){
		return "(" + x + "," + y + ")";
	}
}

public struct InletOutletInitPositions{

	//public int[] InletPos;
	//public int[] OutletPos;

	public Vector2Int[] InletPos;
	public Vector2Int[] OutletPos;

}

[SerializeField]
public enum RoomType{
	Normal = 0,Wall = 1,InLet = 2, OutLet = 3,
}

/// <summary>
/// 部屋のレベルデータをパースするクラス.
/// 以下csvファイル構成
/// 0 - 空
/// 1 - 壁
/// "方向" + I - 流入口
/// "方向" + O - 流出口
/// ←↑↓→ - 強制排出(吸気口/排気口)
/// S - セーフルーム
/// T - タバコ
/// </summary>
public class PhysicsRoomLevelImportor : MonoBehaviour {

	const float ROOM_SPRITE_LENGTH = 2.935f;

	public float velocityScale = 1.0f;

	private InletOutletInitPositions RandomNumbers(int XMax ,int YMin, int YMax, int OutletNumber, int InletNumber){

		InletOutletInitPositions inletOutletInitPositions = new InletOutletInitPositions ();
		//inletOutletInitPositions.InletPos = new int[InletNumber];
		//inletOutletInitPositions.OutletPos = new int[OutletNumber];

		inletOutletInitPositions.InletPos = new Vector2Int[InletNumber];
		inletOutletInitPositions.OutletPos = new Vector2Int[OutletNumber];

		int cnt = YMax;
		List<Vector2Int> numbers = new List<Vector2Int>(cnt);
		for(int i = 0; i < YMax; i++)
		{
			numbers.Add(new Vector2Int(0, i + YMin));
			numbers.Add(new Vector2Int(XMax, i + YMin));
		}

		System.Random rnd = new System.Random((int)Time.time);

        cnt = YMax;
		for(int i = 0; i < InletNumber; i++)
		{
            int index = Random.Range(0, numbers.Count - 1);
            inletOutletInitPositions.InletPos[i] = numbers[index];
			cnt--;
			numbers.RemoveAt(index);
		}

        rnd = new System.Random((int)Time.time);

        cnt = YMax;
        for (int i = 0; i < OutletNumber; i++)
        {
            int index = Random.Range(0, numbers.Count - 1);
            inletOutletInitPositions.OutletPos[i] = numbers[index];
            cnt--;
            numbers.RemoveAt(index);
        }

        return inletOutletInitPositions;

	}

	private int[] RandomNumbers(int Min, int Max, int num){

		int[] result = new int[num];
		//0～9から選び出す場合の数字の数。
		int cnt = Max;
		List<int> numbers = new List<int>(cnt);
		for(int i = 0; i < cnt; i++)
		{
			numbers.Add(i + Min);
		}

		System.Random rnd = new System.Random();

	
		for(int i = 0; i < num; i++)
		{
			int index = rnd.Next(cnt);
			result[i] = numbers[index];
			cnt--;
			numbers.RemoveAt(index);
		}

		return result;

	}

	public RoomInformation Get_Random_OutletInlet_RoomInformation(string itemName,RoomInformation oldroomInformation ,int OutletNumber, int InletNumber){

		// 全て削除.
		/*
		for (var i = 0; i < oldroomInformation.ROOM_MAX_X; i++) {
			for (var j = 0; j < oldroomInformation.ROOM_MAX_Y; j++) {
				Destroy (oldroomInformation.physicsRooms [j] [i].gameObject);
			}
		}*/
		for (int i = 0; i < FluidMechanicsController.Instance.gameObject.transform.childCount; i++) {
			Destroy (FluidMechanicsController.Instance.gameObject.transform.GetChild(i).gameObject);
		}

		// 煙削除.
		GameObject[] smokes = GameObject.FindGameObjectsWithTag ("Smoke");
		foreach(GameObject Obj in smokes){
			Destroy (Obj);
		}

		// 生成
		TextAsset roomTextAsset = Resources.Load (itemName) as TextAsset;

		string[] All_Colmn = roomTextAsset.text.Split ('\n');
		int ROOM_MAX_Y = All_Colmn.Length;
		// csvファイルの仕様のため-1する.
		int ROOM_MAX_X = All_Colmn [0].Split (',').Length - 1;
		GameObject physicsRoomPrefab = Resources.Load ("Room") as GameObject;
		GameObject wallRoomPrefab = Resources.Load ("Wall") as GameObject;
		GameObject inLetRoomPrefab = Resources.Load ("InLet") as GameObject;
		GameObject outLetRoomPrefab = Resources.Load ("OutLet") as GameObject;
		GameObject safeRoomPrefab = Resources.Load ("SafeAria") as GameObject;
		GameObject tabaccoinPhysicsRoomPrefab = Resources.Load ("TabaccoInRoom") as GameObject;
		List<List<GameObject>> rooms = new List<List<GameObject>> ();
		RoomInformation roomInformation = new RoomInformation ();

		roomInformation.ROOM_MAX_X = ROOM_MAX_X;
		roomInformation.ROOM_MAX_Y = ROOM_MAX_Y;
		roomInformation.physicsRooms = rooms;

		int leftSideX = 0, rightSideX = roomInformation.ROOM_MAX_X - 1;
		InletOutletInitPositions inoutPositions = RandomNumbers (ROOM_MAX_X - 1, 1, (roomInformation.ROOM_MAX_Y - 2), OutletNumber, InletNumber);

		for(int Y = 0; Y < ROOM_MAX_Y; Y++){

			rooms.Add (new List<GameObject> ());

			string[] values = All_Colmn[Y].Split (',');
			for (int X = 0; X < ROOM_MAX_X; X++) {
				string value = values [X];
				GameObject roomObject = null;

				switch (value) {
				case "0":
					roomObject = Instantiate (physicsRoomPrefab);
					break;

				case "1":
					roomObject = Instantiate (wallRoomPrefab);
					break;
					/*					
				case "I":
					roomObject = Instantiate (inLetRoomPrefab);
					break;

				case "O":
					roomObject = Instantiate (outLetRoomPrefab);
					break;

				case "L":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (1.0f, 0.0f);
					break;

				case "R":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (-1.0f, 0.0f);
					break;

				case "D":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (0.0f, -1.0f);
					break;

				case "U":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (0.0f, 1.0f);
					break;
					*/

				case "S":
					roomObject = Instantiate (safeRoomPrefab);
					break;

				case "T":
					roomObject = Instantiate (tabaccoinPhysicsRoomPrefab);
					break;

				default:
					roomObject = Instantiate (wallRoomPrefab);
					break;
				}
														
				Vector3 newPosition = Vector3.zero;
				if (ROOM_MAX_X % 2 == 0)
				{

				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x - ROOM_SPRITE_LENGTH / 2,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y - ROOM_SPRITE_LENGTH / 2,
					1.0f
				);
				}
				else
				{
				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y,
					1.0f
				);
				}

				roomObject.transform.localPosition = newPosition;
				roomObject.transform.SetParent(transform);
				roomObject.name = "Room(" + X.ToString() + " , " + Y.ToString() + ")";

				rooms [Y].Add (roomObject);

			}

		}

		// Inletに置換
		for (int i = 0; i < inoutPositions.InletPos.Length; i++) {
			Vector2Int tmp = inoutPositions.InletPos [i];
			GameObject roomObject = Instantiate (inLetRoomPrefab);
			if (tmp.x == 0) {
				roomObject.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right * velocityScale;		
			} else {
				roomObject.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left * velocityScale;		
			}

			GameObject old = rooms [tmp.y] [tmp.x];
			roomObject.transform.position = old.transform.position;
			roomObject.name = old.name;
			roomObject.transform.parent = old.transform.parent;

			rooms [tmp.y].RemoveAt (tmp.x);
			rooms [tmp.y].Insert (tmp.x, roomObject);
			Destroy (old);

			roomObject.GetComponent<PhysicsRoom> ().myType = RoomType.InLet;
		}

		// Outletに置換
		for (int i = 0; i < inoutPositions.OutletPos.Length; i++) {
			Vector2Int tmp = inoutPositions.OutletPos [i];
			GameObject roomObject = Instantiate (outLetRoomPrefab);
			if (tmp.x == 0) {
				roomObject.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left * velocityScale;		
			} else {
				roomObject.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right * velocityScale;		
			}
				
			GameObject old = rooms [tmp.y] [tmp.x];

			roomObject.transform.position = old.transform.position;
			roomObject.name = old.name;
			roomObject.transform.parent = old.transform.parent;

			rooms [tmp.y].RemoveAt (tmp.x);
			rooms [tmp.y].Insert (tmp.x, roomObject);
			Destroy(old);

			roomObject.GetComponent<PhysicsRoom> ().myType = RoomType.OutLet;
		}

		roomInformation.inletPositions = inoutPositions.InletPos;
		roomInformation.outletPostions = inoutPositions.OutletPos;

		return roomInformation;
	}

	public RoomInformation GetRegularRoomInformation(string itemName)
	{

		TextAsset roomTextAsset = Resources.Load(itemName) as TextAsset;

		string[] All_Colmn = roomTextAsset.text.Split('\n');
		int ROOM_MAX_Y = All_Colmn.Length;
		// csvファイルの仕様のため-1する.
		int ROOM_MAX_X = All_Colmn[0].Split(',').Length - 1;
		GameObject physicsRoomPrefab = Resources.Load("Re_Room") as GameObject;
		GameObject wallRoomPrefab = Resources.Load("Re_Wall") as GameObject;
		GameObject inLetRoomPrefab = Resources.Load("Re_InLet") as GameObject;
		GameObject outLetRoomPrefab = Resources.Load("Re_OutLet") as GameObject;
		GameObject safeRoomPrefab = Resources.Load ("SafeAria")as GameObject;
		List<List<GameObject>> rooms = new List<List<GameObject>>();
		RoomInformation roomInformation = new RoomInformation();

		roomInformation.ROOM_MAX_X = ROOM_MAX_X;
		roomInformation.ROOM_MAX_Y = ROOM_MAX_Y;
		roomInformation.physicsRooms = rooms;

		for (int Y = 0; Y < ROOM_MAX_Y; Y++)
		{

			rooms.Add(new List<GameObject>());

			string[] values = All_Colmn[Y].Split(',');
			for (int X = 0; X < ROOM_MAX_X; X++)
			{

				string value = values[X];
				GameObject roomObject = null;

				switch (value)
				{
				case "0":
					roomObject = Instantiate(physicsRoomPrefab);
					break;

				case "1":
					roomObject = Instantiate(wallRoomPrefab);
					break;

				case "I":
					roomObject = Instantiate(inLetRoomPrefab);
					break;

				case "O":
					roomObject = Instantiate(outLetRoomPrefab);
					break;

				case "S":
					roomObject = Instantiate (safeRoomPrefab);
					break;

				default:
					roomObject = Instantiate(wallRoomPrefab);
					break;
				}

				Vector3 newPosition = Vector3.zero;
				if (ROOM_MAX_X % 2 == 0)
				{

				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x - ROOM_SPRITE_LENGTH / 2,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y - ROOM_SPRITE_LENGTH / 2,
					1.0f
				);
				}
				else
				{
				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y,
					1.0f
				);
				}

				roomObject.transform.localPosition = newPosition;
				roomObject.transform.SetParent(transform);
				roomObject.name = "Room(" + X.ToString() + " , " + Y.ToString() + ")";

				rooms[Y].Add(roomObject);


			}

		}

		return roomInformation;
	}

	public RoomInformation GetRoomInformation(string itemName){

		TextAsset roomTextAsset = Resources.Load (itemName) as TextAsset;

		string[] All_Colmn = roomTextAsset.text.Split ('\n');
		int ROOM_MAX_Y = All_Colmn.Length;
		// csvファイルの仕様のため-1する.
		int ROOM_MAX_X = All_Colmn [0].Split (',').Length - 1;
		GameObject physicsRoomPrefab = Resources.Load ("Room") as GameObject;
		GameObject wallRoomPrefab = Resources.Load ("Wall") as GameObject;
		GameObject inLetRoomPrefab = Resources.Load ("InLet") as GameObject;
		GameObject outLetRoomPrefab = Resources.Load ("OutLet") as GameObject;
		GameObject safeRoomPrefab = Resources.Load ("SafeAria") as GameObject;
		GameObject tabaccoinPhysicsRoomPrefab = Resources.Load ("TabaccoInRoom") as GameObject;
		List<List<GameObject>> rooms = new List<List<GameObject>> ();
		RoomInformation roomInformation = new RoomInformation ();

		roomInformation.ROOM_MAX_X = ROOM_MAX_X;
		roomInformation.ROOM_MAX_Y = ROOM_MAX_Y;
		roomInformation.physicsRooms = rooms;

		for (int i = 0; i < FluidMechanicsController.Instance.gameObject.transform.childCount; i++) {
			Destroy (FluidMechanicsController.Instance.gameObject.transform.GetChild(i).gameObject);
		}

		for(int Y = 0; Y < ROOM_MAX_Y; Y++){

			rooms.Add (new List<GameObject> ());

			string[] values = All_Colmn[Y].Split (',');
			for (int X = 0; X < ROOM_MAX_X; X++) {
				string value = values [X];
				GameObject roomObject = null;

				switch (value) {
				case "0":
					roomObject = Instantiate (physicsRoomPrefab);
					break;

				case "1":
					roomObject = Instantiate (wallRoomPrefab);
					break;

				case "L":
					if (X == 0) {
						roomObject = Instantiate (outLetRoomPrefab);
					} else {
						roomObject = Instantiate (inLetRoomPrefab);
					}
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (-1.0f, 0.0f);
					break;

				case "R":
					if (X == 0) {
						roomObject = Instantiate (inLetRoomPrefab);
					} else {
						roomObject = Instantiate (outLetRoomPrefab);
					}
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (1.0f, 0.0f);
					break;

				case "D":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (0.0f, -1.0f);
					break;

				case "U":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (0.0f, 1.0f);
					break;

				case "S":
					roomObject = Instantiate (safeRoomPrefab);
					break;

				case "T":
					roomObject = Instantiate (tabaccoinPhysicsRoomPrefab);
					break;

				default:
					roomObject = Instantiate (wallRoomPrefab);
					break;
				}

				Vector3 newPosition = Vector3.zero;
				if (ROOM_MAX_X % 2 == 0)
				{

				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x - ROOM_SPRITE_LENGTH / 2,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y - ROOM_SPRITE_LENGTH / 2,
					1.0f
				);
				}
				else
				{
				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y,
					1.0f
				);
				}

				roomObject.transform.localPosition = newPosition;
				roomObject.transform.SetParent(transform);
				roomObject.name = "Room(" + X.ToString() + " , " + Y.ToString() + ")";

				rooms [Y].Add (roomObject);

			}

		}

		return roomInformation;
	}

	/*
	public RoomInformation Get_Random_OutletInlet_RoomInformation(RoomInformation roomInformation, int OutletNumber, int InletNumber){

		GameObject inLetRoomPrefab = Resources.Load ("InLet") as GameObject;
		GameObject outLetRoomPrefab = Resources.Load ("OutLet") as GameObject;
		GameObject wallRoomPrefab = Resources.Load ("Wall") as GameObject;

		List<GameObject> wallsGameObjectList = new List<GameObject> ();

		int leftSideX = 0, rightSideX = roomInformation.ROOM_MAX_X - 1;
		InletOutletInitPositions inoutPositions = RandomNumbers (1, (roomInformation.ROOM_MAX_Y - 2) * 2, OutletNumber, InletNumber);

		// 壁の部分を全て初期化する.
		// 左側
		for (int Y = 1; Y < roomInformation.ROOM_MAX_Y - 1; Y++) {
		
			GameObject clone = Instantiate (wallRoomPrefab) as GameObject;
			clone.transform.position = roomInformation.physicsRooms [Y] [leftSideX].gameObject.transform.position;
			clone.name = roomInformation.physicsRooms [Y] [leftSideX].gameObject.name;
			Destroy (roomInformation.physicsRooms [Y] [leftSideX].gameObject);
			roomInformation.physicsRooms [Y].Remove(roomInformation.physicsRooms[Y][leftSideX]);
			roomInformation.physicsRooms [Y].Insert (leftSideX, clone);

			wallsGameObjectList.Add (clone);
		}

		// 右側
		for (int Y = 1; Y < roomInformation.ROOM_MAX_Y - 1; Y++) {
		
			GameObject clone = Instantiate (wallRoomPrefab) as GameObject;
			clone.transform.position = roomInformation.physicsRooms [Y] [rightSideX].gameObject.transform.position;
			clone.name = roomInformation.physicsRooms [Y] [rightSideX].gameObject.name;
			Destroy (roomInformation.physicsRooms [Y] [rightSideX].gameObject);
			roomInformation.physicsRooms [Y].Remove(roomInformation.physicsRooms[Y][rightSideX]);
			roomInformation.physicsRooms [Y].Insert (rightSideX, clone);

			wallsGameObjectList.Add (clone);

		}

		// Outlet
		for (int counter = 0; counter < OutletNumber; counter++) {

			int Y = inoutPositions.OutletPos [counter];
			GameObject oldWall = wallsGameObjectList [Y];

			Debug.Log ("In Outlet");
			Debug.Log ("Insert Position:" + Y.ToString());
			Debug.Log (oldWall.name);

			if (Y <= roomInformation.ROOM_MAX_Y - 1) {
				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right;

				//bool isRemove = roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Remove (roomInformation.physicsRooms[Y][leftSideX].gameObject);
				roomInformation.physicsRooms [Y].Insert (leftSideX, clone);
				Destroy (oldWall);

			} else {

				Y = Y - roomInformation.ROOM_MAX_Y - 1;

				Debug.Log (Y);

				if (roomInformation.physicsRooms.Count <= Y) {
					Debug.Log ("ArrayError");
				} 

				Debug.Log ("RoomMaxY:" + roomInformation.ROOM_MAX_Y);
				Debug.Log (Y);

				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;

				//bool isRemove = roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Remove (roomInformation.physicsRooms[Y][rightSideX].gameObject);
				roomInformation.physicsRooms [Y].Insert (rightSideX, clone);
				//Destroy (oldWall);
				Destroy (oldWall);

			
			}


		}

		// Inlet
		for (int counter = 0; counter < InletNumber; counter++) {

			int Y = inoutPositions.InletPos [counter];

			GameObject oldWall = wallsGameObjectList [Y];

			Debug.Log ("In Inlet");
			Debug.Log ("Insert Position:" + Y.ToString());
			Debug.Log (oldWall.name);

			if (Y <= roomInformation.ROOM_MAX_Y - 1) {
				GameObject clone = Instantiate (inLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;

				//bool isRemove = roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Remove (roomInformation.physicsRooms[Y][leftSideX].gameObject);
				roomInformation.physicsRooms [Y].Insert (leftSideX, clone);
				Destroy (oldWall);
				//Destroy (roomInformation.physicsRooms [Y] [leftSideX].gameObject);

			} else {

				Y = Y - roomInformation.ROOM_MAX_Y - 1;

				if (roomInformation.physicsRooms.Count < Y) {
					Debug.Log ("ArrayError");
				}

				Debug.Log ("RoomMaxY:" + roomInformation.ROOM_MAX_Y);
				Debug.Log (Y);

				GameObject clone = Instantiate (inLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right;

				//bool isRemove = roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Remove (roomInformation.physicsRooms[Y][rightSideX].gameObject);
				roomInformation.physicsRooms [Y].Insert (rightSideX, clone);
				Destroy (oldWall);
				//Destroy (roomInformation.physicsRooms [Y] [rightSideX].gameObject);
							
			}
		}
			
		return roomInformation;

	}*/

	public RoomInformation Get_NoOutletInlet_RoomInformation(string itemName){
		TextAsset roomTextAsset = Resources.Load (itemName) as TextAsset;

		string[] All_Colmn = roomTextAsset.text.Split ('\n');
		int ROOM_MAX_Y = All_Colmn.Length;
		// csvファイルの仕様のため-1する.
		int ROOM_MAX_X = All_Colmn [0].Split (',').Length - 1;
		GameObject physicsRoomPrefab = Resources.Load ("Room") as GameObject;
		GameObject wallRoomPrefab = Resources.Load ("Wall") as GameObject;
		GameObject inLetRoomPrefab = Resources.Load ("InLet") as GameObject;
		GameObject outLetRoomPrefab = Resources.Load ("OutLet") as GameObject;
		GameObject safeRoomPrefab = Resources.Load ("SafeAria") as GameObject;
		GameObject tabaccoinPhysicsRoomPrefab = Resources.Load ("TabaccoInRoom") as GameObject;
		List<List<GameObject>> rooms = new List<List<GameObject>> ();
		RoomInformation roomInformation = new RoomInformation ();

		roomInformation.ROOM_MAX_X = ROOM_MAX_X;
		roomInformation.ROOM_MAX_Y = ROOM_MAX_Y;
		roomInformation.physicsRooms = rooms;

		for(int Y = 0; Y < ROOM_MAX_Y; Y++){

			rooms.Add (new List<GameObject> ());

			string[] values = All_Colmn[Y].Split (',');
			for (int X = 0; X < ROOM_MAX_X; X++) {
				string value = values [X];
				GameObject roomObject = null;

				switch (value) {
				case "0":
					roomObject = Instantiate (physicsRoomPrefab);
					break;

				case "1":
					roomObject = Instantiate (wallRoomPrefab);
					break;
					/*
				case "I":
					roomObject = Instantiate (inLetRoomPrefab);
					break;

				case "O":
					roomObject = Instantiate (outLetRoomPrefab);
					break;

				case "L":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (1.0f, 0.0f);
					break;

				case "R":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (-1.0f, 0.0f);
					break;

				case "D":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (0.0f, -1.0f);
					break;

				case "U":
					roomObject = Instantiate (inLetRoomPrefab);
					roomObject.GetComponent<PhysicsRoom> ().constantVelocity = new Vector2 (0.0f, 1.0f);
					break;
					*/

				case "S":
					roomObject = Instantiate (safeRoomPrefab);
					break;

				case "T":
					roomObject = Instantiate (tabaccoinPhysicsRoomPrefab);
					break;

				default:
					roomObject = Instantiate (wallRoomPrefab);
					break;
				}

				Vector3 newPosition = Vector3.zero;
				if (ROOM_MAX_X % 2 == 0)
				{

				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x - ROOM_SPRITE_LENGTH / 2,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y - ROOM_SPRITE_LENGTH / 2,
					1.0f
				);
				}
				else
				{
				newPosition = new Vector3(
					(-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x,
					(-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y,
					1.0f
				);
				}

				roomObject.transform.localPosition = newPosition;
				roomObject.transform.SetParent(transform);
				roomObject.name = "Room(" + X.ToString() + " , " + Y.ToString() + ")";

				rooms [Y].Add (roomObject);


			}

		}

		return roomInformation;
	}

}