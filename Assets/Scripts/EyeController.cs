using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeController : VRTrackable {


	LookAt lookAt;
	Transform originalParent;
	public Vector3 baseOffset = new Vector3(0.3f, 0, 0);




	void Awake()
	{
		lookAt = GetComponentInChildren<LookAt>();
        originalParent = transform.parent;

		Camera cam = GetComponent<Camera>();
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(cam, true);
	}

	public void UpdatePosition(Vector3 eyeCentralPosition, Quaternion eyeCentralRotation)
	{
		if (attached)
		{
            transform.localRotation = eyeCentralRotation;
            transform.localPosition = eyeCentralPosition + eyeCentralRotation * baseOffset;
        }
	}


	public void SetAttach(bool attachState)
	{
		attached = attachState;
		if (attached)
		{
            transform.parent = originalParent;
		}
		
	}

}


