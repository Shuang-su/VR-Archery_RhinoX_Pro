using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Ximmerse.XR.Tag;

namespace Ximmerse.XR
{
    /// <summary>
    /// XR Settings for Ximmerse XR Plugin.
    /// Required by XR Management package.
    /// </summary>
    [System.Serializable]
    public class XimmerseXRSettings : ScriptableObject
    {
        [Tooltip("Display eye reticle texture or not.")]
        public bool displayReticle = true;

        [Tooltip("Texture of eye reticle.")]
        public Texture2D reticleTexture;

        [Tooltip("The default tracking profile to be loaded, when SDK starts.")]
        public ObjectTrackingProfile defaultTrackingProfile;

        [Tooltip("The default ground plane layout to be loaded, when SDK starts. \r\n If none, there will be no ground plane by default.")]
        public GroundPlaneLayoutConfiguration defaultGroundPlaneLayoutConfig;

        [Tooltip("Draw tracked marker gizmos if true.")]
        public bool DrawTrackedMarkerGizmos = false;

        [Tooltip("Print detail tracked info.")]
        public bool DrawDetailTrackedInfo = false;


        [Tooltip("if true, hand tracking is activated when application starts.")]
        public bool HandTracking = false;
    }
}
