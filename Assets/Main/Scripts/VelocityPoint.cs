using UnityEngine;
using System.Collections;

/// <summary>
/// 吹き出しポイントのタイプ
/// constant   - 他の風に左右されず、一定量吹き出す.
/// noVelocity - 他の風に左右されず、風が吹き出されず常にvelocityがzero.
/// Normal     - 他の風に左右される.
/// </summary>
public enum VelocityPointType{
	constant,noVelocity, Normal,
}

public class VelocityPoint : MonoBehaviour {

	public Vector2 velocity = Vector2.zero;
	public Vector2 velocityAfter = Vector2.zero;

	// 吹き出しポイントのタイプ.
	public VelocityPointType myType;
	public Vector2 ConstantAirVector;

	public bool isCalculate = false;

	public void UpdateConstantAirVector(){

		velocity = ConstantAirVector;
        velocityAfter = ConstantAirVector;

	}

	/// <summary>
	/// velocityAfterの値をvelocityに代入する.
	/// </summary>
	public void UpdateVelocity(){

        
        if (this.isCalculate == true)
        {
            return;
        }

        this.velocity = this.velocityAfter;

        this.isCalculate = true;
	}

}