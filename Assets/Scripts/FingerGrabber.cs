using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerGrabber : MonoBehaviour {

	public Transform attachPoint;
	public Transform finger1, finger2;
	public float maxAngleDiff = 70;
    public float minAngleDiff = 15;


    public OVRInput.Controller controller;
	
	bool grabbed, released;

	EyeController grabbedEye;


	// Update is called once per frame
	void Update ()
	{
        float v = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        SetFingers(1-v);

		grabbed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);
        released = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller);

        if (released)
        {
            released = false;

            if (grabbedEye != null)
            {
                grabbedEye.SetAttach(true);
                grabbedEye = null;
            }

        }

	}


	void SetFingers(float v)
	{
		float angle = (minAngleDiff + v * (maxAngleDiff - minAngleDiff))  / 2;

        Vector3 f1Angs = finger1.localEulerAngles;
		f1Angs.y = angle;
        finger1.localEulerAngles = f1Angs;

        Vector3 f2Angs = finger2.localEulerAngles;
        f2Angs.y = -angle;
        finger2.localEulerAngles = f2Angs;
	}

	void OnTriggerStay(Collider other)
	{
		Debug.Log(other.name);
        if (grabbed)
		{
			grabbed = false;

            EyeController eye = other.GetComponent<EyeController>();
			
            if (eye != null)
            {
                // move to new position
                eye.transform.position = attachPoint.position;
                eye.transform.eulerAngles = attachPoint.eulerAngles;
                eye.transform.parent = attachPoint.transform;


                grabbedEye = eye;
                eye.SetAttach(false);
            }
		}


	}


}
