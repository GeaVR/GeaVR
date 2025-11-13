#if UNITY_2019_1_OR_NEWER
using UnityEngine;

namespace UnityEngine.VR
{
    // Provides the legacy UnityEngine.VR API surface on top of UnityEngine.XR to keep older scripts compiling in Unity 6.
    public enum VRNode
    {
        LeftEye = UnityEngine.XR.XRNode.LeftEye,
        RightEye = UnityEngine.XR.XRNode.RightEye,
        CenterEye = UnityEngine.XR.XRNode.CenterEye,
        Head = UnityEngine.XR.XRNode.Head,
        LeftHand = UnityEngine.XR.XRNode.LeftHand,
        RightHand = UnityEngine.XR.XRNode.RightHand,
        GameController = UnityEngine.XR.XRNode.GameController,
        TrackingReference = UnityEngine.XR.XRNode.TrackingReference,
        HardwareTracker = UnityEngine.XR.XRNode.HardwareTracker
    }

    public static class InputTracking
    {
        public static Vector3 GetLocalPosition(VRNode node)
        {
            return UnityEngine.XR.InputTracking.GetLocalPosition(ToXRNode(node));
        }

        public static Quaternion GetLocalRotation(VRNode node)
        {
            return UnityEngine.XR.InputTracking.GetLocalRotation(ToXRNode(node));
        }

        public static void Recenter()
        {
            UnityEngine.XR.InputTracking.Recenter();
        }

        private static UnityEngine.XR.XRNode ToXRNode(VRNode node)
        {
            return (UnityEngine.XR.XRNode)node;
        }
    }

    public static class VRDevice
    {
        public static bool isPresent => UnityEngine.XR.XRSettings.isDeviceActive;
    }

    public static class VRSettings
    {
        public static string loadedDeviceName => UnityEngine.XR.XRSettings.loadedDeviceName;
        public static string[] supportedDevices => UnityEngine.XR.XRSettings.supportedDevices;

        public static float renderScale
        {
            get => UnityEngine.XR.XRSettings.eyeTextureResolutionScale;
            set => UnityEngine.XR.XRSettings.eyeTextureResolutionScale = Mathf.Max(0.1f, value);
        }

        public static float renderViewportScale
        {
            get => UnityEngine.XR.XRSettings.renderViewportScale;
            set => UnityEngine.XR.XRSettings.renderViewportScale = Mathf.Clamp(value, 0.1f, 1.0f);
        }

        public static bool showDeviceView
        {
            get => UnityEngine.XR.XRSettings.showDeviceView;
            set => UnityEngine.XR.XRSettings.showDeviceView = value;
        }
    }
}
#endif
