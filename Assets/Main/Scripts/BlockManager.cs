using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour
{
	// 管理するBlock群.
	public Block[] ManagedBlocks;
	public int MAX_X = 5;
	public int MAX_Y = 5;
	// 計算の都合上2次元配列にする.
	private Block[,] ManagedBlocks_2DArray;
	public float DeltaTime = 0.01f;
	public GameObject PrefabVeloityPointGameObject;
	
    // 格子定数
	public float a = 0.1f;
	// Δt
	public float deltaT = 0.1f;

	void Start ()
	{

		#region Blockの初期化.
		ManagedBlocks_2DArray = new Block [MAX_Y, MAX_X];

		for (int y = 0; y < MAX_Y; y++) {
			for (int x = 0; x < MAX_X; x++) {
				ManagedBlocks_2DArray [y, x] = ManagedBlocks [y * MAX_Y + x];
			}
		}
		#endregion

		#region AirVelocityPointの初期化.
		for (int y = 0; y < MAX_Y; y++) {
			for (int x = 0; x < MAX_X; x++) {

				if (ManagedBlocks_2DArray [y, x].blockType != BlockType.Wall) {

					// UP
					if (ManagedBlocks_2DArray [y - 1, x].blockType != BlockType.Wall) {
						ManagedBlocks_2DArray [y, x].UpVelocityPoint = ManagedBlocks_2DArray [y - 1, x].DownVelocityPoint;
					} else {
						GameObject velocityPointObject = Instantiate (PrefabVeloityPointGameObject);
						velocityPointObject.transform.position = ManagedBlocks_2DArray [y, x].UpVelocityPointPosition.transform.position;
						velocityPointObject.GetComponent<VelocityPoint> ().myType = VelocityPointType.Normal;
						ManagedBlocks_2DArray [y, x].UpVelocityPoint = velocityPointObject.GetComponent<VelocityPoint> ();
					}

					// Down
					if (ManagedBlocks_2DArray [y + 1, x].blockType != BlockType.Wall) {
						GameObject velocityPointObject = Instantiate (PrefabVeloityPointGameObject);
						velocityPointObject.transform.position = ManagedBlocks_2DArray [y, x].DownVelocityPointPosition.transform.position;
						velocityPointObject.GetComponent<VelocityPoint> ().myType = VelocityPointType.Normal;
						ManagedBlocks_2DArray [y, x].DownVelocityPoint = velocityPointObject.GetComponent<VelocityPoint> ();
					} else {
						GameObject velocityPointObject = Instantiate (PrefabVeloityPointGameObject);
						velocityPointObject.transform.position = ManagedBlocks_2DArray [y, x].DownVelocityPointPosition.transform.position;
						velocityPointObject.GetComponent<VelocityPoint> ().myType = VelocityPointType.Normal;
						ManagedBlocks_2DArray [y, x].DownVelocityPoint = velocityPointObject.GetComponent<VelocityPoint> ();
					}

					// Right
					if (ManagedBlocks_2DArray [y, x + 1].blockType != BlockType.Wall) {
						GameObject velocityPointObject = Instantiate (PrefabVeloityPointGameObject);
						velocityPointObject.transform.position = ManagedBlocks_2DArray [y, x].RightVelocityPointPosition.transform.position;
						velocityPointObject.GetComponent<VelocityPoint> ().myType = VelocityPointType.Normal;
						ManagedBlocks_2DArray [y, x].RightVelocityPoint = velocityPointObject.GetComponent<VelocityPoint> ();
					} else {
						GameObject velocityPointObject = Instantiate (PrefabVeloityPointGameObject);
						velocityPointObject.transform.position = ManagedBlocks_2DArray [y, x].RightVelocityPointPosition.transform.position;
						velocityPointObject.GetComponent<VelocityPoint> ().myType = VelocityPointType.Normal;
						ManagedBlocks_2DArray [y, x].RightVelocityPoint = velocityPointObject.GetComponent<VelocityPoint> ();
					}

					// Left
					if (ManagedBlocks_2DArray [y, x - 1].blockType != BlockType.Wall) {
						ManagedBlocks_2DArray [y, x].LeftVelocityPoint = ManagedBlocks_2DArray [y, x - 1].RightVelocityPoint;
					} else {
						GameObject velocityPointObject = Instantiate (PrefabVeloityPointGameObject);
						velocityPointObject.transform.position = ManagedBlocks_2DArray [y, x].LeftVelocityPointPosition.transform.position;
						velocityPointObject.GetComponent<VelocityPoint> ().myType = VelocityPointType.noVelocity;
						ManagedBlocks_2DArray [y, x].LeftVelocityPoint = velocityPointObject.GetComponent<VelocityPoint> ();
					}
						
				}
			}
		}
		#endregion

	}

	void Update ()
	{

		// 四点のアップデート.
		Update_AfterVelocity ();

		// 全てのブロックの４点をアップデートさせる.
		Update_Velocity ();

		// 湧き出しポイントからの風をアップデートさせる.
		Update_AddAirVelocity ();

        // 全てのAfterAirDensityをアップデートさせる.
        Update_AfterAirDensity();

        // 全てのブロックのAirDensityをアップデートさせる.
        Update_AirDensity();

		// 壁沿いの成分調整.
		//Update_BlockVelocity ();

	}

    void Update_AirDensity()
    {


        for (var y = 0; y < MAX_Y; y++) {
            for (var x = 0; x < MAX_X; x++){

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall){

                    ManagedBlocks_2DArray[y, x].UpdateBlockAirDensity();

                }

            }
        
        }

    }

	/// <summary>
	/// 風を吹かせるポイントから風を吹かせる.
	/// </summary>
	void Update_AddAirVelocity(){

		for (var y = 0; y < MAX_Y; y++) {
			for (var x = 0; x < MAX_X; x++) {

				// 壁じゃなければ
				if (ManagedBlocks_2DArray [y, x].blockType != BlockType.Wall) {

					Block myBlock = ManagedBlocks_2DArray [y, x];

					if (myBlock.UpVelocityPoint.myType == VelocityPointType.constant) {

						myBlock.UpVelocityPoint.UpdateConstantAirVector ();

					}

					if (myBlock.DownVelocityPoint.myType == VelocityPointType.constant) {

						myBlock.DownVelocityPoint.UpdateConstantAirVector ();

					}


					if (myBlock.LeftVelocityPoint.myType == VelocityPointType.constant) {

						myBlock.LeftVelocityPoint.UpdateConstantAirVector ();

					}


					if (myBlock.RightVelocityPoint.myType == VelocityPointType.constant) {

						myBlock.RightVelocityPoint.UpdateConstantAirVector ();

					}
						
				}

			}
		}

	}

	/// <summary>
	/// Blockの境界に沿ってアップデートさせる(境界条件).
	/// </summary>
	void Update_BlockVelocity ()
	{

		for (var y = 0; y < MAX_Y; y++) {
			for (var x = 0; x < MAX_X; x++) {

				// 壁じゃなければ
				if (ManagedBlocks_2DArray [y, x].blockType != BlockType.Wall) {

					Block myBlock = ManagedBlocks_2DArray [y, x];

					myBlock.UpVelocityPoint.UpdateVelocity ();
					myBlock.DownVelocityPoint.UpdateVelocity ();
					myBlock.LeftVelocityPoint.UpdateVelocity ();
					myBlock.RightVelocityPoint.UpdateVelocity ();

				}

			}
		}

	}

    /// <summary>
    /// 空気の密度のアップデート.
    /// </summary>
    void Update_AfterAirDensity()
    {

        // 全てのブロックのAirdensityAfterの計算をする.
        for (var y = 0; y < MAX_Y; y++){
            for (var x = 0; x < MAX_X; x++){
                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall){               

                    Block myBlock = ManagedBlocks_2DArray[y, x];

                    myBlock.airDensityAfter = - (
                        myBlock.RightVelocityPoint.velocity.x - myBlock.LeftVelocityPoint.velocity.x
                        + myBlock.UpVelocityPoint.velocity.y - myBlock.DownVelocityPoint.velocity.y
                        ) * myBlock.airDensity * deltaT / a + myBlock.airDensity;
                        
                }
            }
        }

    }

	void Update_Velocity ()
	{

		for (var y = 0; y < MAX_Y; y++) {
			for (var x = 0; x < MAX_X; x++) {

				// 壁じゃなければ
				if (ManagedBlocks_2DArray [y, x].blockType != BlockType.Wall) {

					Block myBlock = ManagedBlocks_2DArray [y, x];

					myBlock.UpVelocityPoint.UpdateVelocity ();
					myBlock.DownVelocityPoint.UpdateVelocity ();
					myBlock.LeftVelocityPoint.UpdateVelocity ();
					myBlock.RightVelocityPoint.UpdateVelocity ();

					myBlock.UpdateBlockVelocity ();
				}

			}
		}

	}

	/// <summary>
	/// Blockの部屋の流れのアップデート.
	/// </summary>
	void Update_AfterVelocity ()
	{

		for (var y = 0; y < MAX_Y; y++) {
			for (var x = 0; x < MAX_X; x++) {

				// 壁じゃなければ.
				if (ManagedBlocks_2DArray [y, x].blockType != BlockType.Wall) {


					// Block[y , x]についてのプロパティ
					Block myBlock = ManagedBlocks_2DArray [y, x];
					float m = myBlock.m;
					float R = myBlock.R;
					float airDensity = myBlock.airDensity;
					float T = myBlock.T;
					float dotX = myBlock.dotVelocityBlock_X / 4;
					float dotY = myBlock.dotVelocityBlock_y / 4;

					// Block[y + 1, x]についてのプロパティ.
					Block downBlock = ManagedBlocks_2DArray [y + 1, x];

					// Block[y , x + 1]についてのプロパティ.
					Block rightBlock = ManagedBlocks_2DArray [y, x + 1];

					#region upVelocityPointについて計算.
					// x方向について計算.
					// 参照する下の壁のUpVelocityPointが存在するならば.
					if (rightBlock.RightVelocityPoint != null) {
						myBlock.UpVelocityPoint.velocityAfter.x = 
						(-(1 / airDensity) * (R / m) * ((rightBlock.airDensity * rightBlock.T - airDensity * T) / a)
						- (dotX
						* (rightBlock.UpVelocityPoint.velocity.x - myBlock.UpVelocityPoint.velocity.x) / a
						+ dotY
						* (myBlock.DownVelocityPoint.velocity.x - myBlock.UpVelocityPoint.velocity.x) / a
						) 
						) * deltaT + myBlock.UpVelocityPoint.velocity.x;
					}

					// y方向について計算.
					// 参照する右の壁のUpVelocityPointが存在するならば.
					if (rightBlock.RightVelocityPoint != null) {
						myBlock.UpVelocityPoint.velocityAfter.y = 
						(-(1 / airDensity) * (R / m) * ((downBlock.airDensity * downBlock.T - airDensity * T) / a)
						- (dotX
						* (rightBlock.UpVelocityPoint.velocity.y - myBlock.UpVelocityPoint.velocity.y) / a
						+	dotY
						* (myBlock.DownVelocityPoint.velocity.y - myBlock.UpVelocityPoint.velocity.y) / a
						)
                        ) * deltaT + myBlock.UpVelocityPoint.velocity.y;
					}	
					#endregion

					#region downVelocityPointについて計算.
					// x方向について計算.
					// 参照する下の壁のDownVelocityPointが存在するならば.
					if (downBlock.DownVelocityPoint != null) {
						myBlock.DownVelocityPoint.velocityAfter.x = 
						(-(1 / airDensity) * (R / m) * ((rightBlock.airDensity * rightBlock.T - airDensity * T) / a)
						- (dotX
						* (downBlock.DownVelocityPoint.velocity.x - myBlock.DownVelocityPoint.velocity.x) / a
						+ dotY
						* (myBlock.DownVelocityPoint.velocity.x - myBlock.UpVelocityPoint.velocity.x) / a
						)
                        ) * deltaT + myBlock.DownVelocityPoint.velocity.x;
					}

					// y方向について計算.
					// 参照する右の壁のDownVelocityPointと下の壁のUpVelocityPointが存在するならば.
					if (rightBlock.DownVelocityPoint != null && downBlock.UpVelocityPoint != null) {
						myBlock.DownVelocityPoint.velocityAfter.y = 
						(-(1 / airDensity) * (R / m) * ((downBlock.airDensity * downBlock.T - airDensity * T) / a)
						- (dotX
						* (rightBlock.DownVelocityPoint.velocity.y - myBlock.UpVelocityPoint.velocity.y) / a
						+ dotY
						* (myBlock.DownVelocityPoint.velocity.y - downBlock.UpVelocityPoint.velocity.y) / a
                        )
                        ) * deltaT + myBlock.DownVelocityPoint.velocity.y;
					}
					#endregion

					#region leftVelocityPointについて計算.
					// x方向について計算.
					// 参照する下の壁のLeftVelocityPointが存在するならば.
					if (downBlock.LeftVelocityPoint != null) {
						myBlock.LeftVelocityPoint.velocityAfter.x = 
						(-(1 / airDensity) * (R / m) * ((rightBlock.airDensity * rightBlock.T - airDensity * T) / a)
						- (dotX
						* (myBlock.LeftVelocityPoint.velocity.x - myBlock.RightVelocityPoint.velocity.x) / a
						+ dotY
						* (downBlock.LeftVelocityPoint.velocity.x - myBlock.LeftVelocityPoint.velocity.x) / a
						)
                        ) * deltaT + myBlock.LeftVelocityPoint.velocity.x;
					}

					// y方向について計算.
					// 参照する下の壁のLeftVelocityPointが存在するならば.
					if (downBlock.LeftVelocityPoint != null) {
						myBlock.LeftVelocityPoint.velocityAfter.y = 
						(-(1 / airDensity) * (R / m) * ((downBlock.airDensity * downBlock.T - airDensity * T) / a)
						- (dotX
						* (myBlock.LeftVelocityPoint.velocity.y - myBlock.RightVelocityPoint.velocity.y) / a
						+ dotY
						* (downBlock.LeftVelocityPoint.velocity.y - myBlock.LeftVelocityPoint.velocity.y) / a
						)
                        ) * deltaT + myBlock.LeftVelocityPoint.velocity.y;
					}
					#endregion

					#region rightVelocityPointについて計算.
					// x方向について計算.
					// 参照する下の壁のRightVelocityPointと右の壁のRightVelocityPointが存在するならば.
					if (rightBlock.RightVelocityPoint != null && downBlock.RightVelocityPoint != null) {
						myBlock.RightVelocityPoint.velocityAfter.x = 
						(-(1 / airDensity) * (R / m) * ((rightBlock.airDensity * rightBlock.T - airDensity * T) / a)
						- (dotX
						* (rightBlock.RightVelocityPoint.velocity.x - myBlock.RightVelocityPoint.velocity.x) / a
						+ dotY
						* (downBlock.RightVelocityPoint.velocity.x - myBlock.RightVelocityPoint.velocity.x) / a
						)
                        ) * deltaT + myBlock.RightVelocityPoint.velocity.x;
					}

					// y方向について計算.
					// 参照する下の壁のRightVelocityPointと右の壁のRightVelocityPointが存在するならば.
					if (rightBlock.RightVelocityPoint != null && downBlock.RightVelocityPoint != null) {
                        myBlock.RightVelocityPoint.velocityAfter.y =
                        (-(1 / airDensity) * (R / m) * ((downBlock.airDensity * downBlock.T - airDensity * T) / a)
                        - (dotX
                        * (rightBlock.RightVelocityPoint.velocity.y - myBlock.RightVelocityPoint.velocity.y) / a
                        + dotY
                        * (downBlock.RightVelocityPoint.velocity.y - myBlock.RightVelocityPoint.velocity.y) / a
                        )
                        ) * deltaT + myBlock.RightVelocityPoint.velocity.y;
					}
					#endregion
				}

			}


		}

	}
}