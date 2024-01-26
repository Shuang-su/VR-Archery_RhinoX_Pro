using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.XR.InputSystems.GazeAndGestureInteraction
{
    /// <summary>
    /// Gaze and hand interaction system manages how XR user interacts world objects withe eye reticle and hand gesture.
    /// </summary>
    public partial class GazeAndHandInteractionSystem : MonoBehaviour
    {
        private EyeReticle _eyeReticle;

        public static GazeAndHandInteractionSystem instance
        {
            get; private set;
        }

        private void Awake()
        {
            instance = this;
        }
        public EyeReticle eyeReticle
        {
            get => _eyeRay.GetComponent<EyeReticle>();
        }
        static Camera sMainCamera;

        TrackState m_TrackingState = new TrackState();

        UIObjectsInteractionState m_UIInteractionState = new UIObjectsInteractionState();
        /// <summary>
        /// Hand tracking state.
        /// </summary>
        public I_HandTrackingState TrackingState
        {
            get => m_TrackingState;
        }

        public enum CursorStateImage
        {
            Default,
            Custom,
        }
        public enum EyeRayGameobject
        {
            Default,
            Custom,
        }
        public CursorStateImage _cursorStateImage = CursorStateImage.Default;

        public EyeRayGameobject EyeRayGO = EyeRayGameobject.Default;

        public Sprite normal;

        public Sprite tracking;

        public Sprite select;

        public GameObject _eyeRay;

        private GestureXRInteractionManager _gestureXRInteractionManager;
        private void Start()
        {
            _gestureXRInteractionManager = new GameObject("Gesture XR InteractionManager").AddComponent<GestureXRInteractionManager>();
            _gestureXRInteractionManager.CursorManagergo.Normal = normal;
            _gestureXRInteractionManager.CursorManagergo.Tracking = tracking;
            _gestureXRInteractionManager.CursorManagergo.None = select;
        }
        // Update is called once per frame
        void Update()
        {
            if (HandTracking.IsHandTrackingEnable)
            {
                if (!m_TrackingState.IsEnabled)
                {
                    m_TrackingState.OnEnable();
                }

                m_TrackingState.Tick();

                if (!m_UIInteractionState.IsEnabled)
                {
                    m_UIInteractionState.OnEnable();
                }
                m_UIInteractionState.Tick();
            }
            else
            {
                if (m_TrackingState.IsEnabled)
                    m_TrackingState.OnDisable();

                if (m_UIInteractionState.IsEnabled)
                    m_UIInteractionState.OnDisable();
            }
        }

        /// <summary>
        /// Get eye reticle local pose to main camera
        /// </summary>
        /// <returns></returns>
        public static bool GetEyeReticleLocalPose(out Pose pose)
        {
            if (!sMainCamera)
            {
                sMainCamera = Camera.main;
                if (!sMainCamera)
                {
                    pose = default(Pose);
                    return false;
                }
            }
            pose = new Pose(Vector3.zero, Quaternion.identity);
            if (sMainCamera.transform.parent)
            {
                pose.position = sMainCamera.transform.parent.InverseTransformPoint(sMainCamera.transform.position);
                pose.rotation = Quaternion.Inverse(sMainCamera.transform.parent.rotation) * (sMainCamera.transform.rotation);
            }
            else
            {
                pose.position = sMainCamera.transform.position;
                pose.rotation = sMainCamera.transform.rotation;
            }
            return true;
        }
    }
}
