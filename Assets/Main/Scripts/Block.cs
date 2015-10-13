using UnityEngine;
using System.Collections;

public enum BlockType
{
	AirBlock, Wall, AirSource,
}

public class Block : MonoBehaviour
{
	public BlockType blockType;
	// p - 空気の密度.
    public float airDensity = 1.0f;
    public float airDensityAfter;

	// R
	public float R = 1.0f;
	// m
	public float m = 1.0f;
	// T
	public float T = 20.0f;

    // diver
    public float s;

	// 空気の流れ.
	[SerializeField]
	public Vector2 BlockAirVector = Vector2.zero;
	[SerializeField]
	public Vector2 afterBlockAirVector = Vector2.zero;
	// 部屋のX速度の平均.
	public float dotVelocityBlock_X {
		get {
			return (LeftVelocityPoint.velocity.x + RightVelocityPoint.velocity.x + UpVelocityPoint.velocity.x + DownVelocityPoint.velocity.x) / 4;
		}
	}
	// 部屋のY速度の平均.
	public float dotVelocityBlock_y {
		get {
			return (LeftVelocityPoint.velocity.y + RightVelocityPoint.velocity.y + UpVelocityPoint.velocity.y + DownVelocityPoint.velocity.y) / 4; 
		}
	}
	// Blockに対応しているVelocityPointのポジション.
	public GameObject LeftVelocityPointPosition;
	public GameObject RightVelocityPointPosition;
	public GameObject DownVelocityPointPosition;
	public GameObject UpVelocityPointPosition;
	// Blockに対応しているVelocityPoint.
	//[HideInInspector]
	public VelocityPoint UpVelocityPoint;
	//[HideInInspector]
	public VelocityPoint DownVelocityPoint;
	//[HideInInspector]
	public VelocityPoint LeftVelocityPoint;
	//[HideInInspector]
	public VelocityPoint RightVelocityPoint;
	// 矢印のゲームオブジェクト.
	public GameObject VectorGameObject;

	void Start ()
	{

		// 壁じゃなかったら.
		if (blockType == BlockType.AirBlock && this.name != "Wall") {

			//Debug.Log (this.name);

			// 各変数を初期化.
			//this.UpdateAirVector (Vector2.up);

            VectorGameObject.transform.localScale = Vector2.zero;
		
		}

	}

    public void UpdateBlockAirDensity()
    {

        this.airDensity = this.airDensityAfter;

    }

	public void UpdateBlockVelocity ()
	{

        
		this.BlockAirVector = 
			(this.UpVelocityPoint.velocity
		+ this.DownVelocityPoint.velocity
		+ this.LeftVelocityPoint.velocity
		+ this.RightVelocityPoint.velocity
		) / 2;
        
        /*
        this.BlockAirVector = new Vector2(
            (this.LeftVelocityPoint.velocity.x + this.RightVelocityPoint.velocity.x)/2.0f ,
            (this.UpVelocityPoint.velocity.y + this.DownVelocityPoint.velocity.y)/2.0f
            );*/

		this.UpdateAirVector (this.BlockAirVector);

	}

	/// <summary>
	/// airVectorの値を更新して、ローテションとを変更させる.
	/// </summary>
	public void UpdateAirVector (Vector2 newAirVector)
	{

        Vector3 from = Vector2.right;
        Vector3 to = newAirVector;

        float angle = Vector3.Angle(from, to);

        //if (Mathf.Abs(angle) > 0.1f)
        //{
            //Debug.Log(angle);
            //angle %= 360.0f;
            //angle = Mathf.Clamp(angle, 0.0f, 360.0f);

            Quaternion q = Quaternion.AngleAxis(angle, new Vector3(0.0f, 0.0f, 1.0f));

            //VectorGameObject.transform.Rotate(Vector3.forward, angle);
            VectorGameObject.transform.rotation = q;
        //}

        if (newAirVector.magnitude > 0.1f)
        {
            Vector2 scale = Vector2.ClampMagnitude(new Vector2(BlockAirVector.magnitude, 1.0f), 3.5f);
            scale.y = 2.0f;
            scale.x = Mathf.Clamp(scale.x, 0.0f, 3.5f);
            VectorGameObject.transform.localScale = scale;
        }
	
        /*
		// 角度の計算.
		float dot = Vector2.Dot (BlockAirVector, newAirVector);
		float magnitude = BlockAirVector.magnitude * newAirVector.magnitude;
		float theta = Mathf.Acos (dot / magnitude) * Mathf.Rad2Deg;

        if (float.IsNaN(BlockAirVector.magnitude))
        {
            Debug.Log(BlockAirVector.magnitude);
            return;
        }

		if (magnitude > 0.0f) {

			if (Mathf.Sqrt(magnitude) > 0.0001f) {

                Debug.Log("Rotate");
				// z軸上で回転させる.

                if (Mathf.Sqrt(theta) > 0.0001f)
                {
                    VectorGameObject.transform.Rotate(0.0f, 0.0f, theta);
                    VectorGameObject.transform.localScale = new Vector2(BlockAirVector.magnitude, 1.0f);
                }

			}
		}*/
		
	}
}