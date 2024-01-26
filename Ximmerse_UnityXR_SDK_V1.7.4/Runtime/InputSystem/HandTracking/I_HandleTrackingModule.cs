using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.XR.InputSystems
{
    public struct InitializeHandTrackingModuleParameter
    {
        public Transform TrackingAnchor;
    }

    /// <summary>
    /// Handle tracking module interface.
    /// </summary>
    public interface I_HandleTrackingModule
    {
        /// <summary>
        /// Enables hand tracking module
        /// </summary>
        bool EnableModule(InitializeHandTrackingModuleParameter initParameter);

        /// <summary>
        /// Disable hand tracking module.
        /// </summary>
        void DisableModule();

        /// <summary>
        /// Gets the hand track info
        /// </summary>
        HandTrackingInfo HandleTrackInfo
        {
            get;
        }

        /// <summary>
        /// Call per frame to tick the hande tracking module.
        /// </summary>
        void Tick();

        /// <summary>
        /// is the module currently enabled 
        /// </summary>
        bool IsModuleEnabled
        {
            get;
        }
    }
}