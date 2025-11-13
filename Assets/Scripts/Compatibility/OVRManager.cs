#if UNITY_2019_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/// <summary>
/// Minimal replacement for Oculus Integration's OVRManager that maps the legacy fields
/// onto Unity's XR Management / OpenXR stack so existing prefabs keep working.
/// </summary>
public class OVRManager : MonoBehaviour
{
    public static OVRManager instance { get; private set; }

    [Header("Rendering")]
    public int queueAhead = 1;
    public bool useRecommendedMSAALevel = true;
    public bool enableAdaptiveResolution = false;
    public float minRenderScale = 0.7f;
    public float maxRenderScale = 1.0f;

    [Header("Mixed Reality Capture")]
    public bool expandMixedRealityCapturePropertySheet = false;
    public bool enableMixedReality = false;
    public CompositionMethod compositionMethod = CompositionMethod.External;
    public LayerMask extraHiddenLayers = 0;
    public CameraDevice capturingCameraDevice = CameraDevice.WebCamera0;
    public bool flipCameraFrameHorizontally = false;
    public bool flipCameraFrameVertically = false;
    public float handPoseStateLatency = 0f;
    public float sandwichCompositionRenderLatency = 0f;
    public int sandwichCompositionBufferedFrames = 8;
    public Color chromaKeyColor = Color.green;
    public float chromaKeySimilarity = 0.6f;
    public float chromaKeySmoothRange = 0.03f;
    public float chromaKeySpillRange = 0.06f;
    public bool useDynamicLighting = false;
    public DepthQuality depthQuality = DepthQuality.Medium;
    public float dynamicLightingSmoothFactor = 8f;
    public float dynamicLightingDepthVariationClampingValue = 0.001f;
    public VirtualGreenScreenType virtualGreenScreenType = VirtualGreenScreenType.OuterBoundary;
    public float virtualGreenScreenTopY = 10f;
    public float virtualGreenScreenBottomY = -10f;
    public bool virtualGreenScreenApplyDepthCulling = false;
    public float virtualGreenScreenDepthTolerance = 0.2f;

    [Header("Tracking")]
    [SerializeField]
    private TrackingOrigin _trackingOriginType = TrackingOrigin.EyeLevel;
    public bool usePositionTracking = true;
    public bool useRotationTracking = true;
    public bool useIPDInPositionTracking = true;
    public bool resetTrackerOnLoad = false;
    public bool AllowRecenter = true;

    public TrackingOrigin trackingOriginType
    {
        get => _trackingOriginType;
        set
        {
            _trackingOriginType = value;
            ApplyTrackingOrigin();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        InitializeXRLoader();
    }

    private void OnEnable()
    {
        ApplyTrackingOrigin();

        if (resetTrackerOnLoad && AllowRecenter)
        {
            TryRecenter();
        }
    }

    private void InitializeXRLoader()
    {
        if (XRGeneralSettings.Instance == null)
        {
            return;
        }

        var xrManager = XRGeneralSettings.Instance.Manager;
        if (xrManager == null)
        {
            return;
        }

        if (xrManager.activeLoader == null)
        {
            xrManager.InitializeLoaderSync();
        }

        if (xrManager.activeLoader != null)
        {
            xrManager.StartSubsystems();
        }
    }

    private void ApplyTrackingOrigin()
    {
        TrackingOriginModeFlags desiredMode = TrackingOriginModeFlags.Device;

        switch (_trackingOriginType)
        {
            case TrackingOrigin.FloorLevel:
                desiredMode = TrackingOriginModeFlags.Floor;
                break;
            case TrackingOrigin.EyeLevel:
                desiredMode = TrackingOriginModeFlags.Device;
                break;
        }

        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(subsystems);

        foreach (var xrInput in subsystems)
        {
            if (!xrInput.running)
            {
                xrInput.Start();
            }

            var supported = xrInput.GetSupportedTrackingOriginModes();
            if ((supported & desiredMode) == 0)
            {
                continue;
            }

            xrInput.TrySetTrackingOriginMode(desiredMode);
        }
    }

    public void TryRecenter()
    {
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(subsystems);
        foreach (var xrInput in subsystems)
        {
            xrInput.TryRecenter();
        }
    }

    #region Legacy enums

    public enum CompositionMethod
    {
        External = 0,
        Direct = 1,
        Sandwich = 2
    }

    public enum CameraDevice
    {
        WebCamera0,
        WebCamera1
    }

    public enum DepthQuality
    {
        Low,
        Medium,
        High
    }

    public enum VirtualGreenScreenType
    {
        OuterBoundary,
        PlayArea
    }

    public enum TrackingOrigin
    {
        EyeLevel = 0,
        FloorLevel = 1
    }

    #endregion
}
#endif
