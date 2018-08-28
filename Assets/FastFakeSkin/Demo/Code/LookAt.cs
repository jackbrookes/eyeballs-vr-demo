using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {

	public Vector3 lookDir = Vector3.right;
	void Update()
	{
		transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
	}
}
