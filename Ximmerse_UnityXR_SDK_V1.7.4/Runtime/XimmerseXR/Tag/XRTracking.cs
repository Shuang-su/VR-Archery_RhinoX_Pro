using UnityEngine;
using Ximmerse.XR.Utils;
using Ximmerse.XR.Internal;
using System.Collections;
using System.Linq;
using Unity.XR.CoreUtils;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// Implement tracing capabilities and handle appropriate events.
    /// </summary>
    public class XRTracking : MonoBehaviour
    {
        #region Property
        [Header("--- Marker Setting ---")]
        [SerializeField]
        protected int trackID = 65;
        [SerializeField]
        protected LostState trackingIsLost = LostState.Stay;
        [Header("--- Debug Setting ---")]
        [SerializeField]
        protected bool m_debugView = false;
        [SerializeField]
        protected float m_size = 0.17f;


        GameObject tracking_clone;
        private long predTimestampNano = 0;
        private Vector3 postrackingfix;

        private bool trackingstate;
        int index = 0;
        long timestamp = 0;
        int state = 0;
        float posX = 0;
        float posY = 0;
        float posZ = 0;
        float rotX = 0;
        float rotY = 0;
        float rotZ = 0;
        float rotW = 0;
        float confidence = 0;
        float marker_distance = 0;

        Vector3 tagaxis;
        Vector3 cameraaxis;

        private XROrigin xr;
        #endregion

        #region Unity
        private void Start()
        {
            xr = FindObjectOfType<XROrigin>();
        }

        private void Update()
        {
            TagTracking();
        }
        #endregion

        #region Method

        /// <summary>
        /// Tag Tracking function
        /// </summary>
        private void TagTracking()
        {
#if !UNITY_EDITOR
            NativePluginApi.Unity_getTagTracking2(trackID,
                ref index, ref timestamp, ref state, ref posX, ref posY, ref posZ,
                ref rotX, ref rotY, ref rotZ, ref rotW,
                ref confidence, ref marker_distance);
#endif
            if (TagProfileLoading.Instance!=null)
            {
                trackingstate = IsTracking();
                if (trackingstate)
                {
                    if (xr!=null)
                    {
                        postrackingfix = xr.CameraFloorOffsetObject.transform.TransformPoint(new Vector3(posX, posY, posZ));
                        gameObject.transform.position = postrackingfix;
                        gameObject.transform.rotation = xr.transform.rotation * new Quaternion(rotX, rotY, rotZ, rotW);
                    }
                    else
                    {
                        gameObject.transform.position = Camera.main.transform.parent.transform.TransformPoint(new Vector3(posX, posY, posZ));
                        gameObject.transform.rotation = Camera.main.transform.parent.transform.rotation * new Quaternion(rotX, rotY, rotZ, rotW);
                    }
                    GetAxis();
                    if (m_debugView)
                    {
                        DrawDebugView(m_size);
                    }
                }
                else
                {
                    if (trackingIsLost == LostState.FollowHead)
                    {
                        transform.position = xr.Camera.transform.position + xr.Camera.transform.TransformDirection(cameraaxis);
                        AxisLookAt(transform, xr.Camera.transform.position, tagaxis);
                    }
                }
            }
        }

        /// <summary>
        /// Plot the axes
        /// </summary>
        /// <param name="size"></param>
        private void DrawDebugView(float size)
        {
            RxDraw.DrawWirePlane(transform.position, transform.rotation, size, size, Color.green);
            if (xr != null)
            {
                var textRotation = Quaternion.LookRotation(transform.position - xr.Camera.transform.position);
                textRotation = textRotation.PitchNYaw();
                string debugTxt = trackID.ToString();
                RxDraw.Text3D(transform.position, textRotation, 0.02f, debugTxt, Color.green);
            }
            else
            {
                var textRotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
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


        private void GetAxis()
        {
            tagaxis = transform.InverseTransformDirection(xr.Camera.transform.position - transform.position);
            cameraaxis = xr.Camera.transform.InverseTransformDirection(transform.position - xr.Camera.transform.position);
        }

        private static void AxisLookAt(Transform target, Vector3 lookPos, Vector3 directionAxis)
        {
            Quaternion rotation = target.rotation;
            Vector3 targetDir = lookPos - target.position;
            Vector3 fromDir = target.rotation * directionAxis;
            Vector3 axis = Vector3.Cross(fromDir, targetDir);
            float angle = Vector3.Angle(fromDir, targetDir);
            target.rotation = Quaternion.AngleAxis(angle, axis) * rotation;
        }
        #endregion
    }
}

