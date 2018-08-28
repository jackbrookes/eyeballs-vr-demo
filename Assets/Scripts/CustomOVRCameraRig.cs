/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A head-tracked stereoscopic virtual reality camera rig.
/// </summary>
[ExecuteInEditMode]
public class CustomOVRCameraRig : MonoBehaviour
{
    /// <summary>
    /// The left eye camera.
    /// </summary>
    public Camera leftEyeCamera { get { return (usePerEyeCameras) ? _leftEyeCamera : _centerEyeCamera; } }
    /// <summary>
    /// The right eye camera.
    /// </summary>
    public Camera rightEyeCamera { get { return (usePerEyeCameras) ? _rightEyeCamera : _centerEyeCamera; } }
    /// <summary>
    /// Provides a root transform for all anchors in tracking space.
    /// </summary>
    public Transform trackingSpace;

    public EyeController leftEye;
    /// <summary>
    /// Always coincides with average of the left and right eye poses.
    /// </summary>
    public Transform centerEyeAnchor;

    public EyeController rightEye;
    /// <summary>
    /// Always coincides with the pose of the left hand.
    /// </summary>
    public Transform leftHandAnchor;
    /// <summary>
    /// Always coincides with the pose of the right hand.
    /// </summary>
    public Transform rightHandAnchor;
    /// <summary>
    /// Always coincides with the pose of the sensor.
    /// </summary>
    public Transform trackerAnchor;
    /// <summary>
    /// Occurs when the eye pose anchors have been set.
    /// </summary>
    public event System.Action<OVRCameraRig> UpdatedAnchors;
    /// <summary>
    /// If true, separate cameras will be used for the left and right eyes.
    /// </summary>
    public bool usePerEyeCameras = false;
    /// <summary>
    /// If true, all tracked anchors are updated in FixedUpdate instead of Update to favor physics fidelity.
    /// \note: If the fixed update rate doesn't match the rendering framerate (OVRManager.display.appFramerate), the anchors will visibly judder.
    /// </summary>
    public bool useFixedUpdateForTracking = false;

    protected bool _skipUpdate = false;
    protected readonly string trackingSpaceName = "TrackingSpace";
    protected readonly string trackerAnchorName = "TrackerAnchor";
    protected readonly string leftEyeAnchorName = "LeftEyeAnchor";
    protected readonly string centerEyeAnchorName = "CenterEyeAnchor";
    protected readonly string rightEyeAnchorName = "RightEyeAnchor";
    protected readonly string leftHandAnchorName = "LeftHandAnchor";
    protected readonly string rightHandAnchorName = "RightHandAnchor";
    public Camera _centerEyeCamera;
    public Camera _leftEyeCamera;
    public Camera _rightEyeCamera;

    #region Unity Messages
    protected virtual void Awake()
    {
        _skipUpdate = true;
        EnsureGameObjectIntegrity();
    }

    protected virtual void Start()
    {
        UpdateAnchors();
    }

    protected virtual void FixedUpdate()
    {
        if (useFixedUpdateForTracking)
            UpdateAnchors();
    }

    protected virtual void Update()
    {
        _skipUpdate = false;

        if (!useFixedUpdateForTracking)
            UpdateAnchors();
    }
    #endregion

    protected virtual void UpdateAnchors()
    {
        EnsureGameObjectIntegrity();

        if (!Application.isPlaying)
            return;


        OVRPose tracker = OVRManager.tracker.GetPose();
        trackerAnchor.localPosition = tracker.position;
        trackerAnchor.localRotation = tracker.orientation;

        centerEyeAnchor.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
        centerEyeAnchor.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);


		// custom updates of eyes

        leftEye.UpdatePosition(
            UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye),
			UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftEye)
		);

        rightEye.UpdatePosition(
            UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye),
            UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightEye)
        );

		// controllers

        leftHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
        rightHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);

        leftHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        rightHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

    }

    public virtual void EnsureGameObjectIntegrity()
    {
		return;
    }

    protected virtual Transform ConfigureAnchor(Transform root, string name)
    {
        Transform anchor = (root != null) ? transform.Find(root.name + "/" + name) : null;

        if (anchor == null)
        {
            anchor = transform.Find(name);
        }

        if (anchor == null)
        {
            anchor = new GameObject(name).transform;
        }

        anchor.name = name;
        anchor.parent = (root != null) ? root : transform;
        anchor.localScale = Vector3.one;
        anchor.localPosition = Vector3.zero;
        anchor.localRotation = Quaternion.identity;

        return anchor;
    }

    public virtual Matrix4x4 ComputeTrackReferenceMatrix()
    {
        if (centerEyeAnchor == null)
        {
            Debug.LogError("centerEyeAnchor is required");
            return Matrix4x4.identity;
        }

        // The ideal approach would be using UnityEngine.VR.VRNode.TrackingReference, then we would not have to depend on the OVRCameraRig. Unfortunately, it is not available in Unity 5.4.3

        OVRPose headPose;
#if UNITY_2017_2_OR_NEWER
        headPose.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
        headPose.orientation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
#else
		headPose.position = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head);
		headPose.orientation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head);
#endif

        OVRPose invHeadPose = headPose.Inverse();
        Matrix4x4 invHeadMatrix = Matrix4x4.TRS(invHeadPose.position, invHeadPose.orientation, Vector3.one);

        Matrix4x4 ret = centerEyeAnchor.localToWorldMatrix * invHeadMatrix;

        return ret;
    }
}
