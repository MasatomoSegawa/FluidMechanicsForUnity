﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;

//% #out #in #p(t = a秒) #OutPosition #InPosition

[System.Serializable]
public struct RoomData{
	public int OutLetNumber;
	public int InLetNumber;
	public int ParticleNumber;
	public Vector2Int[] OutPositions;
	public Vector2Int[] InPositions;
}

[System.Serializable]
public struct DeltaVelocityData{
	public float absVelocity;
	public float timeStamp;
}

public class FluidMechanicsController : Singleton<FluidMechanicsController>
{
	#region Utility
	[Header("流体ルーム用のTextAsset")]
	public TextAsset roomLevelTextAsset;
	public TextAsset[] roomLevelTextAssetList;
	#endregion

    #region 描画用の変数
    // 1ルーム当たりのスプライトの大きさ.
    const float ROOM_SPRITE_LENGTH = 2.935f;

    [Header("流体力学ルームの部屋のX数"), Range(0, 100)]
    public int ROOM_MAX_X;

    [Header("流体力学ルームの部屋のY数"), Range(0, 100)]
    public int ROOM_MAX_Y;

    [Header("部屋のPrfab")]
	public GameObject roomPrefab;

    [Header("壁のprefab")]
	public GameObject wallPrefab;

    // Roomの2次元リスト.
    private List<List<GameObject>> rooms;

	// 時間表示用UIText.
	private Text TimeText;

	// 経過時間.
	private float currentTime;
    #endregion

    #region 数値計算用変数

    [Header("リラクゼーション数")]       
    public int iteration = 10;

    [Header("デルタX")]
    public float DX = 10.0f;

    [Header("デルタY")]
    public float DY = 10.0f;

    [Header("レイノルズ数")]
    public float Re = 500.0f;

    [Header("デルタT")]
    public float deltaT = 0.01f;

    // 圧力.
    private float[,] prs;

    // 流れ関数.
    private float[,] psi;

    private float[,] velX;
    private float[,] velY;

    // x方向速度.
    private float[,] VelX;

    // y方向速度.
    private float[,] VelY;

    // x方向微分.
    private float[,] velXgx;

    // y方向微分.
    private float[,] velXgy;

    // x方向微分.
    private float[,] velYgx;

    // y方向微分.
    private float[,] velYgy;

    // 過度.
    private float[,] omg;

    // 部屋のタイプ.
    private RoomType[,] roomTypes;

    private int NX;
    private int NY;

    public float maxPsi0 = 0.02f;
    public float minPsi0 = -0.1f;
    public float maxOmg0 = 20.0f;
    public float minOmg0 = -21.0f;
    public float maxPrs0 = 1.0f;
    public float minPrs0 = -0.4f;

	// タバコオブジェクト.
	private GameObject tabaccoObject;

	public List<DeltaVelocityData> deltaVelocityDataList;
	public List<RoomData> roomDataList;
	private float coolTime = 1.0f;
	private float nextTime;
	private bool endFlag = false;
	private float loopTimeMax = 100.0f;

	public float OneLoopTimeForSimulation = 10.0f;

    #endregion

    #region Unityライフサイクル.

    void Start()
    {

		deltaVelocityDataList = new List<DeltaVelocityData> ();

		TimeText = GameObject.Find ("TimeText").GetComponent<Text> ();

		/*
		PhysicsRoomLevelImportor physicsRoomLevelImportor = GetComponent<PhysicsRoomLevelImportor> ();
		RoomInformation roomInformation = physicsRoomLevelImportor.GetRoomInformation (roomLevelTextAsset.name);
	
		InitPhysicsRooms (roomInformation);

        InitData();

		TimeText = GameObject.Find ("TimeText").GetComponent<Text> ();

		nextTime = coolTime + Time.time;
		*/

		endFlag = false;

		StartCoroutine (StartSimulation());
    }

	IEnumerator StartSimulation(){

		PhysicsRoomLevelImportor physicsRoomLevelImportor = GetComponent<PhysicsRoomLevelImportor> ();

		foreach (TextAsset textAsset in roomLevelTextAssetList) {
			//RoomInformation roomInformation = physicsRoomLevelImportor.GetRoomInformation (roomLevelTextAsset.name);
			//RoomInformation initRoomInformation = physicsRoomLevelImportor.Get_NoOutletInlet_RoomInformation (roomLevelTextAsset.name);
			RoomInformation initRoomInformation = physicsRoomLevelImportor.Get_NoOutletInlet_RoomInformation (textAsset.name);

			// Data入れるリスト初期化.
			roomDataList = new List<RoomData> ();

			RoomInformation currentRoomInformation = initRoomInformation;
			// ルームのInLet/Outletの全てのパターンを試して,
			// タバコの粒子の煙が少ないパターンを得る.
			for (int InLet = 1; InLet < initRoomInformation.ROOM_MAX_Y - 1; InLet++) {
				for (int OutLet = 1; OutLet < initRoomInformation.ROOM_MAX_Y - 1; OutLet++) {
					currentRoomInformation = 
						physicsRoomLevelImportor.Get_Random_OutletInlet_RoomInformation (textAsset.name,initRoomInformation, OutLet, InLet);

					InitPhysicsRooms (currentRoomInformation);

					InitData ();

					yield return new WaitWhile (() => endFlag == false);

					endFlag = false;

					//一度終わる毎にRoomDataにデータを追加.
					RoomData newRoomData = new RoomData ();
					newRoomData.InLetNumber = InLet;
					newRoomData.OutLetNumber = OutLet;
					newRoomData.ParticleNumber = GetSmokeNumber ();
					newRoomData.InPositions = currentRoomInformation.inletPositions;
					newRoomData.OutPositions = currentRoomInformation.outletPostions;

					roomDataList.Add (newRoomData);
					//OutPutSmokeData (currentRoomInformation);

				}
			}

			OutPutSmokeData (currentRoomInformation, textAsset.name);

			Debug.Log (textAsset.name + " is End!");
		}
			
		yield break;
	}

	private int GetSmokeNumber(){
		GameObject[] safeArias = GameObject.FindGameObjectsWithTag ("SafeAria");

		int smokeCount = 0;
		foreach(GameObject safeAria in safeArias){

			BoxCollider2D boxCollider = safeAria.GetComponent<BoxCollider2D> ();
			Collider2D[] targets = Physics2D.OverlapAreaAll(boxCollider.bounds.min,boxCollider.bounds.max);
			List<GameObject> smokeObjectList = new List<GameObject> ();

			foreach (Collider2D target in targets) {
				if (target.tag == "Smoke" && smokeObjectList.Contains(target.gameObject) == false) {
					smokeObjectList.Add (target.gameObject);
					smokeCount++;
				}
			}

		}

		return smokeCount;
	}

	/// <summary>
	/// データを取得してcsvにして保存する関数.
	/// </summary>
	/// <param name="currentRoomInformation">Current room information.</param>
	private void OutPutSmokeData(RoomInformation currentRoomInformation,string Assetname){
	
		StreamWriter sw;
		FileInfo fi;
		fi = new FileInfo(Application.dataPath + "/Data/" + Assetname + "_data.csv");
		sw = fi.AppendText();

		sw.WriteLine(roomLevelTextAsset.text);
		sw.WriteLine("OutNumber , InNumber , ParticleNumber , OutPosition, InPosition");

		int max = roomDataList.Count;
		int count = 0;
		foreach (RoomData data in roomDataList) {
			string str = data.InLetNumber.ToString () + "," + data.OutLetNumber.ToString() + 
			             "," + data.ParticleNumber.ToString() + ",";

			for (int i = 0; i < data.InLetNumber; i++) {
				str += "(" + data.InPositions [i].x + "," + data.InPositions [i].y + ") - ";
			}

			str += ",";

			for (int i = 0; i < data.OutLetNumber; i++) {
				str += "(" + data.OutPositions [i].x + "," + data.OutPositions [i].y + ") - ";
			}
						
			sw.WriteLine (str);
			Debug.Log((++count).ToString() + "/" + max.ToString());
			Debug.Log(str);
		}
		Debug.Log("OutputComplete");

		sw.Flush();
		sw.Close();

	}
				
	private void OutputDeltaVelocityData(){

		StreamWriter sw;
		FileInfo fi;
		fi = new FileInfo(Application.dataPath + "/Data/" + roomLevelTextAsset.name + "_DeltaVelocityData.csv");
		sw = fi.AppendText();

        Debug.Log(Application.dataPath + "/DeltaVelocity.csv");

        sw.WriteLine(roomLevelTextAsset.text);
        sw.WriteLine("Time , DeltaVelocity");

        int max = deltaVelocityDataList.Count;
        int count =0;
		foreach (DeltaVelocityData data in deltaVelocityDataList) {
			string str = data.timeStamp.ToString () + "," + data.absVelocity.ToString();
			sw.WriteLine (str);
            Debug.Log((++count).ToString() + "/" + max.ToString());
            Debug.Log(str);
		}
        Debug.Log("OutputComplete");
			
		sw.Flush();
		sw.Close();

	}

    void Update()
    {

		if (endFlag == true) {
			return;
		}

        if (Input.GetKeyDown(KeyCode.A))
        {
			//DebugLog();
			OutputDeltaVelocityData ();
        }

        // step1
        // 境界条件
        Calculate_Boundarycondition();

        // step2
        // CIP
        Calculate_CIP();

        // step3
        // ポアソン
        Calculate_Poisson();

        // step4
        // 変数アップデート
        Update_Variables();

        // 描画更新.
        DrawVelocity();

		if (currentTime >= OneLoopTimeForSimulation) {
			endFlag = true;
			//OutputDeltaVelocityData ();
		}

		/*
		// グラフ生成用.
		if (currentTime >= nextTime){
			Save_DeltaVelocityData ();
			nextTime = currentTime + coolTime;
		}

		if (currentTime >= 500.0f) {
			endFlag = true;
			OutputDeltaVelocityData ();
		}
		*/

		currentTime += deltaT;
		TimeText.text = "Time:" + currentTime.ToString("F1");
    }

    #endregion

    #region 流体力学用の関数.

    private void InitPhysicsRooms(RoomInformation roomInformation)
    {

        rooms = roomInformation.physicsRooms;

        ROOM_MAX_X = roomInformation.ROOM_MAX_X;
        ROOM_MAX_Y = roomInformation.ROOM_MAX_Y;

        // 速度点は(部屋の数+1)個存在する.
        NX = ROOM_MAX_X + 1;
        NY = ROOM_MAX_Y + 1;

        // 部屋のタイプリストオブジェクトを初期化.
        roomTypes = new RoomType[NX + 1, NY + 1];

        for (int Y = 0; Y < ROOM_MAX_Y; Y++)
        {
            for (int X = 0; X < ROOM_MAX_X; X++)
            {

                PhysicsRoom currentRoom = rooms[Y][X].GetComponent<PhysicsRoom>();
                roomTypes[X, Y] = currentRoom.myType;

            }
        }

    }

    /// <summary>
    /// PhysicsRoomを構築する.
    /// </summary>
    private void InitPhysicsRooms(){

		// 部屋のリストオブジェクトを初期化.
		rooms = new List<List<GameObject>>();

        // 部屋のタイプリストオブジェクトを初期化.
        roomTypes = new RoomType[NX + 1, NY + 1];

		// 速度点は(部屋の数+1)個存在する.
		NX = ROOM_MAX_X + 1;
		NY = ROOM_MAX_Y + 1;

		// 部屋を生成.
		for (int Y = 0; Y < ROOM_MAX_Y; Y++)
		{

			// Y軸初期化.
			rooms.Add(new List<GameObject>());

			for (int X = 0; X < ROOM_MAX_X; X++)
			{

				GameObject obj = InstanceRoomObject(X, Y);

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

				obj.transform.localPosition = newPosition;
				obj.transform.SetParent(transform);
				obj.name = "Room(" + X.ToString() + " , " + Y.ToString() + ")";
				rooms[Y].Add(obj);

			}
		}

	}

    /// <summary>
    /// 変数などを初期化する.
    /// </summary>
    private void InitData()
    {

        prs = new float[NX + 1, NY + 1];
        psi = new float[NX + 1, NY + 1];
        omg = new float[NX + 1, NY + 1];
        VelX = new float[NX + 1, NY + 1];
        VelY = new float[NX + 1, NY + 1];
        velX = new float[NX + 1, NY + 1];
        velY = new float[NX + 1, NY + 1];
        velXgx = new float[NX + 1, NY + 1];
        velXgy = new float[NX + 1, NY + 1];
        velYgx = new float[NX + 1, NY + 1];
        velYgy = new float[NX + 1, NY + 1];

        //入口/出口は流速1
        for (int j = 0; j <= NY; j++)
            for (int i = 0; i <= NX; i++)
            {
                //圧力
                prs[i,j] = 0.0f;
                //速度
                if (roomTypes[i, j] == RoomType.Wall)
                {
                    velX[i, j] = 0.0f;
                }
                else
                {
                    //velX[i, j] = 1.0f;//その他はすべて1で初期化
                }

                velY[i,j] = 0.0f;//すべての速度ｙ成分は0
                velXgx[i,j] = 0.0f;
                velXgy[i,j] = 0.0f;
                velYgx[i,j] = 0.0f;
                velYgy[i,j] = 0.0f;
                VelX[i,j] = velX[i,j];
                VelY[i,j] = velY[i,j];
                omg[i,j] = 0.0f;//渦度
            }

        maxPrs0 = -1000.0f; minPrs0 = 1000.0f;
        maxOmg0 = -1000.0f; minOmg0 = 1000.0f;

		// タバコオブジェクトを取得.
		tabaccoObject = GameObject.FindGameObjectWithTag ("Tabacco");
        if (tabaccoObject != null)
        {
            tabaccoObject.GetComponent<Tabacco>().StartExtractSmoke();
            Debug.Log("uhun");
        }

		currentTime = 0.0f;
    }

    #endregion

    #region 計算用関数

    /// <summary>
    /// 速度の境界条件
    /// </summary>
    private void Calculate_Boundarycondition()
    {

		//Debug.Log("(" + NX + "," + NY + ")");
		//Debug.Log ("(" + rooms[0].Count + "," + rooms.Count + ")");

		/*
        // 入出力
        for (int i = 0; i < NX; i++)
        {
            for (int j = 0; j < NY; j++)
            {
                if (roomTypes[i, j] == RoomType.InLet || roomTypes[i, j] == RoomType.OutLet)
                {			

					velX[i,j] = rooms [j] [i].GetComponent<PhysicsRoom> ().constantVelocity.x;
					//velX[i, j] = 1.0f;
                    //Debug.Log("(" + i.ToString() + "," + j.ToString() + ")");
                }
            }
		}*/

		for (int i = 0; i < ROOM_MAX_X; i++) {
			for (int j = 0; j < ROOM_MAX_Y; j++) {

				if (rooms [j] [i].GetComponent<PhysicsRoom> ().myType == RoomType.InLet ||
					rooms [j] [i].GetComponent<PhysicsRoom> ().myType == RoomType.OutLet) {

					if (i - 1 >= 0) {
						velX [i - 1, j] = rooms [j] [i].GetComponent<PhysicsRoom> ().constantVelocity.x;
					}
					if (i + 1 < NX) {
						velX [i + 1, j] = rooms [j] [i].GetComponent<PhysicsRoom> ().constantVelocity.x;
					}
				}
			}
		}


        // 上下.
        for (int i = 0; i <= NX; i++)
        {

            // ↓についての境界条件
            velY[i, 0] = velY[i, 2];
            velY[i, 1] = 0.0f;
            velX[i, 0] = velX[i, 1];

            // ↑についての境界条件
            velX[i, NY - 1] = -velX[i, NY - 2];//上境界度を1とする(平均値が1となる)の速
            velY[i, NY] = -velY[i, NY - 2];
            velY[i, NY - 1] = 0.0f;

        }

        //左右
        for (int j = 0; j <= NY; j++)
        {
            if (roomTypes[0, j] == RoomType.InLet || roomTypes[0,j] == RoomType.OutLet)
            {
                velX[0, j] = 1.0f;
            }
            else
            {
                velX[0, j] = velX[2, j];
                velX[1, j] = 0.0f;
                velY[0, j] = -velY[1, j];

                velX[NX, j] = velX[NX - 2, j];
                velX[NX - 1, j] = 0.0f;
                velY[NX - 1, j] = -velY[NX - 2, j];
            }
        }

        /*
          //障害物左右
  for(j = nY1; j <= nY2; j++)
  {
    //左端
    velX[nX1+1][j] =  velX[nX1-1][j];
    velX[nX1][j]   =  0.0;
    velY[nX1][j]   = -velY[nX1-1][j];
    //右端
    velX[nX2-1][j] =  velX[nX2+1][j];
    velX[nX2][j]   =  0.0;
    velY[nX2-1][j] = -velY[nX2][j];
  }
  //障害物上下
  for(i = nX1; i <= nX2; i++)
  {
    //上端
    velX[i][nY2] = - velX[i][nY2+1];
    velY[i][nY2-1] = velY[i][nY2+1];
    velY[i][nY2]   = 0.0;
    //下端
    velX[i][nY1+1] = - velX[i][nY1];
    velY[i][nY1+1] = velY[i][nY1-1];
    velY[i][nY1]   = 0.0;
  }
        */

    }

    private void Update_Variables()
    {

        //step5(スタガード格子点の速度ベクトルの更新）
        for (int j = 1; j < NY - 1; j++)
            for (int i = 2; i < NX - 1; i++)
            {                
                velX[i, j] += -deltaT * (prs[i, j] - prs[i - 1, j]) / DX;
            }
        for (int j = 2; j < NY - 1; j++)
            for (int i = 1; i < NX - 1; i++)
            {
                velY[i, j] += -deltaT * (prs[i, j] - prs[i, j - 1]) / DY;
            }

        //表示のための速度は圧力と同じ位置で
        for (int j = 1; j <= NY - 2; j++)
            for (int i = 1; i <= NX - 2; i++)
            {
                VelX[i, j] = (velX[i, j] + velX[i + 1, j]) / 2.0f;
                VelY[i, j] = (velY[i, j] + velY[i, j + 1]) / 2.0f;
            }

        //Psi
        for (int j = 0; j < NY - 1; j++)
        {
            psi[0, j] = 0.0f;
            for (int i = 1; i < NX - 1; i++)
                psi[i, j] = psi[i - 1, j] - DX * (velY[i - 1, j] + velY[i, j]) / 2.0f;
        }
        //Omega
        for (int i = 1; i <= NX - 1; i++)
            for (int j = 1; j <= NY - 1; j++)
            {
                omg[i, j] = 0.5f * ((VelY[i + 1, j] - VelY[i - 1, j]) / DX - (VelX[i, j + 1] - VelX[i, j - 1]) / DY);
            }

        //流れ関数，圧力、渦度の最小値，最大値
        for (int i = 1; i < NX; i++)
        {
            for (int j = 1; j < NY; j++)
            {
                if (prs[i, j] > maxPrs0) maxPrs0 = prs[i, j];
                if (prs[i, j] < minPrs0) minPrs0 = prs[i, j];
                if (psi[i, j] > maxPsi0) maxPsi0 = psi[i, j];
                if (psi[i, j] < minPsi0) minPsi0 = psi[i, j];
                if (omg[i, j] > maxOmg0) maxOmg0 = omg[i, j];
                if (omg[i, j] < minOmg0) minOmg0 = omg[i, j];
            }
        }



    }

    /// <summary>
    /// ポアソン方程式の計算.
    /// </summary>
    private void Calculate_Poisson()
    {
        float tolerance = 0.00001f;//許容誤差
        float maxError = 0.0f;

        float A1 = 0.5f * (DY * DY) / ((DX * DX) + (DY * DY));
        float A2 = 0.5f * (DX * DX) / ((DX * DX) + (DY * DY));
        float A3 = 0.25f * (DX * DX) * (DY * DY) / ((DX * DX) + (DY * DY));

        // ポアソン方程式の右辺.
        float[,] D = new float[NX + 1, NY + 1];
        for (int j = 1; j < NY - 1; j++)
            for (int i = 1; i < NX - 1; i++)
            {
                if (roomTypes[i, j] != RoomType.Wall)
                {
                    float a = (velX[i + 1, j] - velX[i, j]) / DX;
                    float b = (velY[i, j + 1] - velY[i, j]) / DY;
                    D[i, j] = A3 * (a + b) / deltaT;
                }
            }

        //反復法
        int cnt = 0;
        while (cnt < iteration)
        {
            maxError = 0.0f;

            //圧力境界値
            for (int j = 1; j < NY; j++)
            {
                prs[0, j] = prs[1, j] - 2.0f * velX[0, j] / (DX * Re);//左端
                prs[NX - 1, j] = prs[NX - 2, j] + 2.0f * velX[NX, j] / (DX * Re);//右端
            }
            for (int i = 1; i < NX; i++)
            {
                prs[i, 0] = prs[i, 1] - 2.0f * velY[i, 0] / (DY * Re);//下端
                prs[i, NY - 1] = prs[i, NY - 2] + 2.0f * velY[i, NY] / (DY * Re);//上端
            }

            // 障害物.
            for(int j = 0; j < NY; j++)
            {
                for(int i = 0; i < NX; i++)
                {
                    if(roomTypes[i,j] == RoomType.Wall)
                    {
                        prs[i, j] = maxPrs0;
                    }
                }
            }

            for (int j = 1; j < NY - 1; j++)
                for (int i = 1; i < NX - 1; i++)
                {
                    if (roomTypes[i, j] != RoomType.Wall)
                    {
                        float pp = A1 * (prs[i + 1, j] + prs[i - 1, j]) + A2 * (prs[i, j + 1] + prs[i, j - 1]) - D[i, j];
                        float error = Mathf.Abs(pp - prs[i, j]);
                        if (error > maxError) maxError = error;
                        prs[i, j] = pp;//更新 
                    }
                }
            if (maxError < tolerance) break;

            cnt++;
        }


    }

    private void Calculate_CIP()
    {

        float[,] vel = new float[NX + 1, NY + 1];

        //x方向速度定義点における速度
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
                vel[i, j] = (velY[i - 1, j] + velY[i, j] + velY[i - 1, j + 1] + velY[i, j + 1]) / 4.0f;

        methodCIP(velX, velXgx, velXgy, velX, vel);

        //y成分
        //y方向速度定義点における速度
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
                vel[i, j] = (velX[i, j] + velX[i, j - 1] + velX[i + 1, j - 1] + velX[i + 1, j]) / 4.0f;

        methodCIP(velY, velYgx, velYgy, vel, velY);
    }

    void methodCIP(float[,] f, float[,] gx, float[,] gy, float[,] vx, float[,] vy)
    {
        float[,] newF = new float[NX + 1, NY + 1];//関数
        float[,] newGx = new float[NX + 1, NY + 1];//x方向微分
        float[,] newGy = new float[NX + 1, NY + 1];//y方向微分

        float c11, c12, c21, c02, c30, c20, c03, a, b, sx, sy, x, y, dx, dy, dx2, dy2, dx3, dy3;

        int ip, jp;
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
            {
                if (vx[i, j] >= 0.0) sx = 1.0f; else sx = -1.0f;
                if (vy[i, j] >= 0.0) sy = 1.0f; else sy = -1.0f;

                x = -vx[i, j] * deltaT;
                y = -vy[i, j] * deltaT;
                ip = (int)(i - sx);//上流点
                jp = (int)(j - sy);
                dx = sx * DX;
                dy = sy * DY;
                dx2 = dx * dx;
                dy2 = dy * dy;
                dx3 = dx2 * dx;
                dy3 = dy2 * dy;

                c30 = ((gx[ip, j] + gx[i, j]) * dx - 2.0f * (f[i, j] - f[ip, j])) / dx3;
                c20 = (3.0f * (f[ip, j] - f[i, j]) + (gx[i, j] + 2.0f * gx[i, j]) * dx) / dx2;
                c03 = ((gy[i, jp] + gy[i, j]) * dy - 2.0f * (f[i, j] - f[i, jp])) / dy3;
                c02 = (3.0f * (f[i, jp] - f[i, j]) + (gy[i, jp] + 2.0f * gy[i, j]) * dy) / dy2;
                a = f[i, j] - f[i, jp] - f[ip, j] + f[ip, jp];
                b = gy[ip, j] - gy[i, j];
                c12 = (-a - b * dy) / (dx * dy2);
                c21 = (-a - (gx[i, jp] - gx[i, j]) * dx) / (dx2 * dy);
                c11 = -b / dx + c21 * dx;

                newF[i, j] = f[i, j] + ((c30 * x + c21 * y + c20) * x + c11 * y + gx[i, j]) * x
                           + ((c03 * y + c12 * x + c02) * y + gy[i, j]) * y;

                newGx[i, j] = gx[i, j] + (3.0f * c30 * x + 2.0f * (c21 * y + c20)) * x + (c12 * y + c11) * y;
                newGy[i, j] = gy[i, j] + (3.0f * c03 * y + 2.0f * (c12 * x + c02)) * y + (c21 * x + c11) * x;

                //粘性項に中央差分
                newF[i, j] += deltaT * ((f[i - 1, j] + f[i + 1, j] - 2.0f * f[i, j]) / dx2
                           + (f[i, j - 1] + f[i, j + 1] - 2.0f * f[i, j]) / dy2) / Re;
            }

        //更新
        for (int j = 1; j < NY; j++)
            for (int i = 1; i < NX; i++)
            {
                f[i, j] = newF[i, j];
                gx[i, j] = newGx[i, j];
                gy[i, j] = newGy[i, j];
            }


    }

    #endregion

    #region 描画用の関数
    private GameObject InstanceRoomObject(int X, int Y)
    {

        // Debug.Log(X.ToString() + "," + Y.ToString());

        // 壁なら.
        if (Y == 0 || X == 0 || Y == ROOM_MAX_Y - 1 || X == ROOM_MAX_X - 1)
        {
            //Debug.Log("Wall");

            return Instantiate(wallPrefab);
        }

        //Debug.Log("Room");
        return Instantiate(roomPrefab);
    }

	public float oldSumVelocity;

	/// <summary>
	/// 各ポイントの速度の総和の絶対値保存する
	/// </summary>
	private void Save_DeltaVelocityData(){

		float currentSumVelocity = 0.0f;
		float deltaVelocity;

		for (int Y = 0; Y < ROOM_MAX_Y; Y++)
		{
			for (int X = 0; X < ROOM_MAX_X; X++)
			{
				PhysicsRoom currentRoom = rooms[Y][X].GetComponent<PhysicsRoom>();

				currentSumVelocity += currentRoom.velocity.magnitude;
			}
		}

		deltaVelocity = Mathf.Abs(oldSumVelocity - currentSumVelocity);

		oldSumVelocity = currentSumVelocity;

		DeltaVelocityData currentDeltaVelocityData = new DeltaVelocityData ();
		currentDeltaVelocityData.absVelocity = deltaVelocity;
		currentDeltaVelocityData.timeStamp = (int)currentTime;

		deltaVelocityDataList.Add (currentDeltaVelocityData);
	}
		
    private void DrawVelocity()
    {

        for (int Y = 0; Y < ROOM_MAX_Y; Y++)
        {
            for (int X = 0; X < ROOM_MAX_X; X++)
            {
				PhysicsRoom currentRoom = rooms[Y][X].GetComponent<PhysicsRoom>();

				currentRoom.UpdateVelocity (VelX [X,Y], VelY [X,Y]);
            }
        }

    }
    #endregion

    void DebugLog()
    {
        string str = "";
        for (int j = 0; j < NY; j++)
        {
            for (int i = 0; i < NX; i++)
            {
                str += "(" + VelX[i, j].ToString("F4") + " , " + VelY[i, j].ToString("F4") + ")\t";
            }
            str += "\n";
        }
        Debug.Log(str);
    }


}
