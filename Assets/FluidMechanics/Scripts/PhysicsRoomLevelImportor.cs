using UnityEngine;
using System.Collections.Generic;

public struct RoomInformation{

	public int ROOM_MAX_X;
	public int ROOM_MAX_Y;
	public List<List<GameObject>> physicsRooms;

}

public enum RoomType{
	Normal = 0,Wall = 1,
}

public class PhysicsRoomLevelImportor : MonoBehaviour {

	const float ROOM_SPRITE_LENGTH = 2.935f;

	public RoomInformation GetRoomInformation(string itemName){

		TextAsset roomTextAsset = Resources.Load (itemName) as TextAsset;

		string[] All_Colmn = roomTextAsset.text.Split ('\n');
		int ROOM_MAX_Y = All_Colmn.Length;
		// csvファイルの仕様のため-1する.
		int ROOM_MAX_X = All_Colmn [0].Split (',').Length - 1;
		GameObject physicsRoomPrefab = Resources.Load ("Room") as GameObject;
		GameObject wallRoomPrefab = Resources.Load ("Wall") as GameObject;
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
				GameObject roomObject;

				if (value == "0") {
					roomObject = Instantiate (physicsRoomPrefab);
				} else {
					roomObject = Instantiate (wallRoomPrefab);
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