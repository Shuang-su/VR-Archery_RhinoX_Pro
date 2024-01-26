using Unity.XR.CoreUtils;
using UnityEngine;
using Ximmerse.XR.Utils;
using Ximmerse.XR.Internal;
using System.Collections;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// Implement large spatial positioning capabilities and handle appropriate events.
    /// </summary>
    public class GroundSpace : MonoBehaviour
    {
        #region Property

        private XROrigin xr;
        private GameObject tagground_clone;
        private float px, py, pz, rx, ry, rz, rw;
        private long predTimestampNano = 0;
        private bool first = false;
        private bool exit = false;
        private bool onfirstTrackingEnter = false;
        private bool onTrackingEnter = false;
        private bool onTrackingStay = false;
        private bool onTrackingExit = false;
        private bool isTrakingState;
        private bool isvalid;

        private GameObject debugaxis;
        protected TrackingEvent trackingEvent;
        [Header("--- Marker Setting ---")]
        [SerializeField]
        protected int trackID = 65;
        [SerializeField]
        protected float m_Confidence = 0.9f;
        [Header("--- Vio Drift Threshold ---")]
        [SerializeField]
        protected float m_distance = 1.0f;
        [SerializeField]
        protected float m_angle = 1.0f;
        [Header("--- Tracking distance ---")]
        [SerializeField]
        protected float m_minDistance = 0.1f;
        [SerializeField]
        protected float m_maxDistance = 1.8f;
        [Header("--- Debug Setting ---")]
        [SerializeField]
        protected bool m_debugView;
        [SerializeField]
        protected float m_size = 0.17f;
        //private XDevicePlugin.XAttrTrackingInfo trackingInfo;
        #endregion

        #region Unity

        protected virtual void Awake()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        private void Start()
        {
            GetXRComponent();
        }

        private void Update()
        {
            if (TagProfileLoading.Instance != null)
            {
                isTrakingState = IsTracking();
                IsValid();
                if (m_debugView)
                {
                    if (m_debugView && isvalid)
                    {
                        DrawDebugView(m_size);
                    }
                }
            }
            
        }
        #endregion

        #region Method
        
        private void GetXRComponent()
        {
            if (xr == null)
            {
                xr = FindObjectOfType<XROrigin>();
            }
        }

        /// <summary>
        /// Plot the axes
        /// </summary>
        /// <param name="size"></param>
        protected void DrawDebugView(float size)
        {

            RxDraw.DrawWirePlane(transform.position, transform.rotation, size, size, Color.green);
            if (xr != null)
            {
                Quaternion textRotation = Quaternion.LookRotation(transform.position - xr.Camera.transform.position);
                textRotation = textRotation.PitchNYaw();

                string debugTxt = trackID.ToString();

                RxDraw.Text3D(transform.position, textRotation, 0.02f, debugTxt, Color.green);
            }
            else
            {
                Quaternion textRotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
                textRotation = textRotation.PitchNYaw();

                string debugTxt = trackID.ToString();

                RxDraw.Text3D(transform.position, textRotation, 0.02f, debugTxt, Color.green);
            }

            RxDraw.DrawTranslateGizmos(transform.position, transform.rotation, size * 0.85f);
        }
        /// <summary>
        /// Get the Tag tracking status
        /// </summary>
        /// <returns></returns>
        protected bool IsTracking()
        {
            if (TagProfileLoading.Instance.TrackingTagList.Contains(trackID))
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Whether the tag is valid
        /// </summary>
        /// <returns></returns>
        protected bool IsValid()
        {
            if (trackID ==TagProfileLoading.Instance.TagFusion)
            {
                isvalid = true;
            }
            else
            {
                isvalid = false;
            }
            return isvalid;
        }
        /// <summary>
        /// Displays the axis model
        /// </summary>
        protected void DisplaysAxisModel()
        {
            if (m_debugView && IsValid())
            {
                if (debugaxis == null)
                {
                    debugaxis = GameObject.Instantiate(Resources.Load("Tag/Prefabs/Debug Axis")) as GameObject;
                    debugaxis.transform.parent = gameObject.transform;
                    debugaxis.transform.localPosition = new Vector3(0, 0, 0);
                    debugaxis.transform.localEulerAngles = new Vector3(0, 0, 0);
                    debugaxis.transform.localScale = new Vector3(m_size, m_size, m_size);
                }
                debugaxis.SetActive(true);
            }
            else
            {
                if (debugaxis != null)
                {
                    debugaxis.SetActive(false);
                }
            }
        }

        #endregion
    }
}

