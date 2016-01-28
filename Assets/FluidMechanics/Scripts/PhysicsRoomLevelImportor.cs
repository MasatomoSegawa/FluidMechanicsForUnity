	using UnityEngine;
using System.Collections.Generic;

public struct RoomInformation{

	public int ROOM_MAX_X;
	public int ROOM_MAX_Y;
	public List<List<GameObject>> physicsRooms;

}

public struct InletOutletInitPositions{

	public int[] InletPos;
	public int[] OutletPos;

}

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

	private InletOutletInitPositions RandomNumbers(int Min, int Max, int OutletNumber, int InletNumber){

		InletOutletInitPositions inletOutletInitPositions = new InletOutletInitPositions ();
		inletOutletInitPositions.InletPos = new int[InletNumber];
		inletOutletInitPositions.OutletPos = new int[OutletNumber];

		int cnt = Max;
		List<int> numbers = new List<int>(cnt);
		for(int i = 0; i < cnt; i++)
		{
			numbers.Add(i + Min);
		}

		System.Random rnd = new System.Random();

		for(int i = 0; i < OutletNumber; i++)
		{
			int index = rnd.Next(cnt);
			inletOutletInitPositions.OutletPos[i] = numbers[index];
			cnt--;
			numbers.RemoveAt(index);
		}

		for(int i = 0; i < InletNumber; i++)
		{
			int index = rnd.Next(cnt);
			inletOutletInitPositions.InletPos[i] = numbers[index];
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

	public RoomInformation Get_Random_OutletInlet_RoomInformation(RoomInformation roomInformation, int OutletNumber, int InletNumber){

		GameObject inLetRoomPrefab = Resources.Load ("InLet") as GameObject;
		GameObject outLetRoomPrefab = Resources.Load ("OutLet") as GameObject;
		GameObject wallRoomPrefab = Resources.Load ("Wall") as GameObject;

		List<GameObject> wallsGameObjectList = new List<GameObject> ();

		int leftSideX = 0, rightSideX = roomInformation.ROOM_MAX_X - 1;
		int[] Ys = RandomNumbers (1, roomInformation.ROOM_MAX_Y - 2, OutletNumber);

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
			
		for (int i = 0; i < InletNumber; i++) {
			print (inoutPositions.InletPos [i]);
		}

		for (int i = 0; i < OutletNumber; i++) {
			print (inoutPositions.OutletPos [i]);
		}

		foreach (GameObject obj in wallsGameObjectList) {
			print (obj.name);
		}

		// Outlet
		for (int counter = 0; counter < OutletNumber; counter++) {

			int Y = inoutPositions.OutletPos [counter];
			if (wallsGameObjectList.Count < Y) {
				Debug.Log ("kokodesu!");
			}
			GameObject oldWall = wallsGameObjectList [Y];

			if (Y <= roomInformation.ROOM_MAX_Y - 1) {
				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right;

				roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Insert (leftSideX, clone);
				Destroy (oldWall);

			} else {

				Y = Y - roomInformation.ROOM_MAX_Y - 1;

				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;

				roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Insert (rightSideX, clone);
				Destroy (oldWall);

			}

			/*
			// 左側に位置するなら、
			if (Y < roomInformation.ROOM_MAX_Y - 1) {
				Debug.Log (Y + "Y < " + roomInformation.ROOM_MAX_Y +" - 1");

				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = roomInformation.physicsRooms [Y] [leftSideX].gameObject.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right;

				Destroy (roomInformation.physicsRooms [Y] [leftSideX].gameObject);
				roomInformation.physicsRooms [Y].Remove(roomInformation.physicsRooms[Y][leftSideX]);
				roomInformation.physicsRooms [Y].Insert (leftSideX, clone);
				Debug.Log ("OutLet leftSide");
			} else {

				Y = Y - roomInformation.ROOM_MAX_Y;

				Debug.Log (Y + "Y > " + roomInformation.ROOM_MAX_Y +" - 1");

				/*
				outLetRoomPrefab.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;
				Destroy (roomInformation.physicsRooms [Y] [rightSideX].gameObject);
				roomInformation.physicsRooms [Y] [rightSideX] = outLetRoomPrefab;

				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;
				clone.transform.position = roomInformation.physicsRooms [Y] [rightSideX].gameObject.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;

				Destroy (roomInformation.physicsRooms [Y] [rightSideX].gameObject);
				roomInformation.physicsRooms [Y].Remove(roomInformation.physicsRooms[Y][rightSideX]);
				roomInformation.physicsRooms [Y].Insert (rightSideX, clone);

				Debug.Log ("OutLet rightSide");
			}*/

		}

		//Ys = RandomNumbers (1, roomInformation.ROOM_MAX_Y - 2, InletNumber);
		// Inlet
		for (int counter = 0; counter < InletNumber; counter++) {
			//Y = Random.Range (1, roomInformation.ROOM_MAX_Y - 1);

			//int Y = Ys [counter];
			int Y = inoutPositions.InletPos [counter];

			GameObject oldWall = wallsGameObjectList [Y];

			if (Y <= roomInformation.ROOM_MAX_Y - 1) {
				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;

				roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Insert (leftSideX, clone);
				Destroy (oldWall);
	
			} else {

					Y = Y - roomInformation.ROOM_MAX_Y - 1;


				GameObject clone = Instantiate (outLetRoomPrefab) as GameObject;

				clone.transform.position = oldWall.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right;

				roomInformation.physicsRooms [Y].Remove (oldWall);
				roomInformation.physicsRooms [Y].Insert (rightSideX, clone);
				Destroy (oldWall);
			
			}
				/*
			// 左側に位置するなら、
			if (Y < roomInformation.ROOM_MAX_Y - 1) {
				Debug.Log (Y + "Y < " + roomInformation.ROOM_MAX_Y +" - 1");

				GameObject clone = Instantiate (inLetRoomPrefab) as GameObject;
				clone.transform.position = roomInformation.physicsRooms [Y] [leftSideX].gameObject.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.left;

				Destroy (roomInformation.physicsRooms [Y] [leftSideX].gameObject);
				roomInformation.physicsRooms [Y].Remove(roomInformation.physicsRooms[Y][leftSideX]);
				roomInformation.physicsRooms [Y].Insert (leftSideX, clone);
				Debug.Log ("InLet LeftSide");
			} else {

				Y = Y - roomInformation.ROOM_MAX_Y;

				Debug.Log (Y + "Y > " + roomInformation.ROOM_MAX_Y +" - 1");

				GameObject clone = Instantiate (inLetRoomPrefab) as GameObject;
				clone.transform.position = roomInformation.physicsRooms [Y] [rightSideX].gameObject.transform.position;
				clone.GetComponent<PhysicsRoom> ().constantVelocity = Vector2.right;

				Destroy (roomInformation.physicsRooms [Y] [rightSideX].gameObject);
				roomInformation.physicsRooms [Y].Remove(roomInformation.physicsRooms[Y][rightSideX]);
				roomInformation.physicsRooms [Y].Insert (rightSideX, clone);
				Debug.Log ("InLet RightSide");
				}
				*/

		}

		return roomInformation;

	}

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