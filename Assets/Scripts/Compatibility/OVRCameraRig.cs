#if UNITY_2019_1_OR_NEWER
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Simplified OVRCameraRig implementation that drives the legacy anchor hierarchy
/// using UnityEngine.XR tracking data.
/// </summary>
public class OVRCameraRig : MonoBehaviour
{
    [Header("Legacy settings")]
    public bool usePerEyeCameras = false;
    public bool useFixedUpdateForTracking = false;

    [Header("Anchors")]
    public Transform trackingSpace;
    public Transform centerEyeAnchor;
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public Transform trackerAnchor;

    private void Awake()
    {
        EnsureAnchorSetup();
    }

    private void Start()
    {
        EnsureAnchorSetup();
    }

    private void Update()
    {
        if (!useFixedUpdateForTracking)
        {
            UpdateAnchors();
        }
    }

    private void FixedUpdate()
    {
        if (useFixedUpdateForTracking)
        {
            UpdateAnchors();
        }
    }

    private void EnsureAnchorSetup()
    {
        trackingSpace = ResolveOrCreateAnchor(trackingSpace, transform, "TrackingSpace");
        leftEyeAnchor = ResolveOrCreateAnchor(leftEyeAnchor, trackingSpace, "LeftEyeAnchor");
        centerEyeAnchor = ResolveOrCreateAnchor(centerEyeAnchor, trackingSpace, "CenterEyeAnchor");
        rightEyeAnchor = ResolveOrCreateAnchor(rightEyeAnchor, trackingSpace, "RightEyeAnchor");
        leftHandAnchor = ResolveOrCreateAnchor(leftHandAnchor, trackingSpace, "LeftHandAnchor");
        rightHandAnchor = ResolveOrCreateAnchor(rightHandAnchor, trackingSpace, "RightHandAnchor");
        trackerAnchor = ResolveOrCreateAnchor(trackerAnchor, trackingSpace, "TrackerAnchor");
    }

    private Transform ResolveOrCreateAnchor(Transform anchor, Transform parent, string name)
    {
        if (parent == null)
        {
            parent = transform;
        }

        if (anchor != null)
        {
            return anchor;
        }

        var child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private void UpdateAnchors()
    {
        UpdateAnchor(leftEyeAnchor, XRNode.LeftEye);
        UpdateAnchor(centerEyeAnchor, XRNode.CenterEye);
        UpdateAnchor(rightEyeAnchor, XRNode.RightEye);
        UpdateAnchor(leftHandAnchor, XRNode.LeftHand);
        UpdateAnchor(rightHandAnchor, XRNode.RightHand);
        UpdateAnchor(trackerAnchor, XRNode.TrackingReference);
    }

    private void UpdateAnchor(Transform anchor, XRNode node)
    {
        if (anchor == null)
        {
            return;
        }

        anchor.localPosition = InputTracking.GetLocalPosition(node);
        anchor.localRotation = InputTracking.GetLocalRotation(node);
    }
}
#endif
