using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ximmerse.XR.Internal;
using static Ximmerse.XR.PluginVioFusion;

namespace Ximmerse.XR.Tag
{
    public class TagLoadingManager : MonoBehaviour
    {
        #region Property
        protected Thread threadLoad;
        //private List<GameObject> _groundplanelist = new List<GameObject>();
        protected List<TagGroundPlane> _tagGroundbyJson = new List<TagGroundPlane>();
        private List<int> _trackingtaglist = new List<int>();
        [Header("Offset")]
        [SerializeField] protected Vector3 offsetPos = new Vector3();
        [SerializeField] protected Vector3 offsetRot = new Vector3();
        

        int beacon_id = -1;
        long beacon_timestamp;
        float beacon_pos0;
        float beacon_pos1;
        float beacon_pos2;
        float beacon_rot0;
        float beacon_rot1;
        float beacon_rot2;
        float beacon_rot3;
        float beacon_tracking_confidence;
        float beacon_min_distance;
        float beacon_correct_weight;

        private int _tagfusion;
        private long predTimestampNano = 0;
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
        bool trakingstate;

        private XROrigin xr;
        //private XDevicePlugin.XAttrTrackingInfo trackingInfo;

        public List<int> TrackingTagList
        {
            get => _trackingtaglist;
        }
        public int TagFusion
        {
            get => _tagfusion;
        }
        public Thread ThreadLoad
        {
            get => threadLoad;
        }
        public List<TagGroundPlane> TagGroundbyJson
        {
            get => _tagGroundbyJson;
            set => _tagGroundbyJson = value;
        }
        #endregion

        #region Unity

        private void Update()
        {
            GetTrackingStateAndData();
            CorrectCameraOffset();
        }
#endregion

        #region Method
        /// <summary>
        /// Get the coordinates of the Tag ground plane in the scene and enable large space positioning
        /// </summary>
        private void GroundPlaneSetting()
        {
#if !UNITY_EDITOR
            PluginVioFusion.plugin_vio_fusion_reset(0);
            //TagGroundPlane[] enemies = UnityEngine.Object.FindObjectsOfType<TagGroundPlane>();
            if (TagGroundPlane.tagGroundPlaneList != null)
            {
                foreach (var item in TagGroundPlane.tagGroundPlaneList)
                {
                    PluginVioFusion.XAttrBeaconInWorldInfo beacon_in_world_info = item.beacon_info;
                    Debug.Log("id£º" + beacon_in_world_info.beacon_id);
                    PluginVioFusion.plugin_vio_fusion_set_param(ref beacon_in_world_info);
                }
                PluginVioFusion.plugin_vio_fusion_run(0);
            }
#endif
        }

        /// <summary>
        /// Start enabling the Large Space component
        /// </summary>
        protected IEnumerator StartFusion()
        {
            GetXRComponent();

            while (threadLoad ==null || threadLoad.ThreadState==ThreadState.Running)
            {
                yield return null;
            }
            if (CreatesGroundPlaneByJson.Instance == null)
            {
                SettingTagData();
            }
            else
            {
                if (!CreatesGroundPlaneByJson.Instance.autoCreates)
                {
                    SettingTagData();
                }
            }
        }

        /// <summary>
        /// <summary>
        /// Refresh offsets and get coordinate information for large spatial positioning boards.
        /// </summary>
        protected void RefreshBeacon()
        {
            OffsetXr();
            UpdateTagGroundPlane();
        }

        /// <summary>
        /// Clearing the algorithm data invalidates the large spatial positioning function.
        /// </summary>
        protected void CleanBeacon()
        {
#if !UNITY_EDITOR
            PluginVioFusion.plugin_vio_fusion_reset(0);
#endif
        }

        /// <summary>
        /// Setting Tag Data
        /// </summary>
        protected void SettingTagData()
        {
            GroundPlaneSetting();
        }

        private void UpdateTagGroundPlane()
        {
            if (CreatesGroundPlaneByJson.Instance == null)
            {
                SettingTagData();
            }
            else
            {
                if (!CreatesGroundPlaneByJson.Instance.autoCreates)
                {
                    SettingTagData();
                }
                else
                {
                    CreatesGroundPlaneByJson.Instance.CreateGroundPlanesFromConfig();
                }
            }
        }

        private void GetXRComponent()
        {
            if (xr==null)
            {
                xr = FindObjectOfType<XROrigin>();
            }
        }

        /// <summary>
        /// When the positioning function takes effect, correct the camera offset
        /// </summary>
        private void CorrectCameraOffset()
        {
            if (_tagfusion!=-1&& xr!=null)
            {
                if (xr.CameraFloorOffsetObject.transform.position!=offsetPos || xr.CameraFloorOffsetObject.transform.rotation!= Quaternion.Euler(offsetRot))
                {
                    OffsetXr();
                    Debug.Log("fusion && offset");
                }
            }
        }

        /// <summary>
        /// Set the offset
        /// </summary>
        protected void OffsetXr()
        {
            if (xr!=null)
            {
                xr.CameraFloorOffsetObject.transform.position = offsetPos;
                xr.CameraFloorOffsetObject.transform.rotation = Quaternion.Euler(offsetRot);
            }
        }

        /// <summary>
        /// Get the tracking status and the ID of the fusion
        /// </summary>
        protected void GetTrackingStateAndData()
        {
#if !UNITY_EDITOR
            int ret = NativePluginApi.Unity_TagPredict(0);
            _trackingtaglist.Clear();
            if (ret >= 0)
            {
                for (int i = 0; i <= 227; i++)
                {
                    bool ret2 = NativePluginApi.Unity_getTagTracking2(i,
                          ref index, ref timestamp, ref state,
                          ref posX, ref posY, ref posZ,
                          ref rotX, ref rotY, ref rotZ, ref rotW,
                          ref confidence, ref marker_distance);

                    if (ret2 && state > 0)
                    {
                        _trackingtaglist.Add(i);
                    }
                }
            }
#endif
            _tagfusion = GetTagFusionState();
        }

        /// <summary>
        /// Gets a valid Tag ID
        /// </summary>
        /// <returns></returns>
        protected int GetTagFusionState()
        {
#if !UNITY_EDITOR
            NativePluginApi.Unity_getFusionResult(predTimestampNano, ref beacon_id,
                            ref beacon_timestamp,
                            ref beacon_pos0,
                            ref beacon_pos1,
                            ref  beacon_pos2,
                            ref  beacon_rot0,
                            ref  beacon_rot1,
                            ref  beacon_rot2,
                            ref  beacon_rot3,
                            ref  beacon_tracking_confidence,
                            ref  beacon_min_distance,
                            ref  beacon_correct_weight);
#endif
            return beacon_id;
        }
#endregion
    }
}
