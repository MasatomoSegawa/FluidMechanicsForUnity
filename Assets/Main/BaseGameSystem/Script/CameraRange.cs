using UnityEngine;
using System.Collections;

public class CameraRange : MonoBehaviour {
	
	#if UNITY_EDITOR
	
	float depth = 10;
	public bool drawSubline = true;
	
	Vector3 leftTop, rightDown, rightTop, leftDown, forwardPosition;
	
	void PositionUpdate(Camera targetCamera, float z)
	{
		leftTop = targetCamera.ViewportToWorldPoint(new Vector3(1, 0, z));
		rightDown = targetCamera.ViewportToWorldPoint(new Vector3(0, 1, z));
		rightTop = targetCamera.ViewportToWorldPoint(new Vector3(0, 0, z));
		leftDown = targetCamera.ViewportToWorldPoint(new Vector3(1, 1, z));
	}
	
	void OnDrawGizmos()
	{
		Camera targetCamera = GetComponent<Camera>();
		depth = -targetCamera.transform.localPosition.z;
		
		Vector3 forwardPosition = targetCamera.transform.forward * depth;
		PositionUpdate(targetCamera, forwardPosition.z);
		
		Vector3 cameraPosition = targetCamera.transform.position;
		
		Gizmos.DrawLine(leftTop, rightTop);
		Gizmos.DrawLine(leftTop, leftDown);
		Gizmos.DrawLine(rightDown, leftDown);
		Gizmos.DrawLine(rightDown, rightTop);
		
		Gizmos.color = new Color(1, 1, 1, 0.4f);
		
		if(! drawSubline )
			return;
		
		if( !targetCamera.orthographic )
		{
			Gizmos.DrawLine(cameraPosition, (leftTop - cameraPosition) * 10+ cameraPosition);
			Gizmos.DrawLine(cameraPosition, (leftDown - cameraPosition) * 10+ cameraPosition);
			Gizmos.DrawLine(cameraPosition, (rightTop - cameraPosition) * 10+ cameraPosition);
			Gizmos.DrawLine(cameraPosition, (rightDown - cameraPosition) * 10+ cameraPosition);
		}else{
			Vector3 forward = targetCamera.transform.forward * 10;
			Vector3 back = targetCamera.transform.forward * -10;
			Gizmos.DrawLine(leftTop + forward, leftTop + back);
			Gizmos.DrawLine(leftDown + forward, leftDown + back);
			Gizmos.DrawLine(rightTop + forward, rightTop + back);
			Gizmos.DrawLine(rightDown + forward, rightDown + back);
		}
	}
	
	void OnDrawGizmosSelected()
	{
		Camera targetCamera = GetComponent<Camera>();
		depth = -targetCamera.transform.localPosition.z;
		
		Vector3 leftcenter = targetCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, forwardPosition.z));
		Vector3 centerTop = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, forwardPosition.z));
		
		UnityEditor.Handles.Label(leftTop, leftTop.ToString());
		UnityEditor.Handles.Label(rightDown, rightDown.ToString());
		
		UnityEditor.Handles.Label(leftcenter, Vector3.Distance(leftTop, leftDown).ToString("0.0"));
		UnityEditor.Handles.Label(centerTop, Vector3.Distance(leftTop, rightTop).ToString("0.0"));
		
	}
	
	void OnValidate()
	{
		depth = Mathf.Max(0, depth);
	}
	#endif
}
