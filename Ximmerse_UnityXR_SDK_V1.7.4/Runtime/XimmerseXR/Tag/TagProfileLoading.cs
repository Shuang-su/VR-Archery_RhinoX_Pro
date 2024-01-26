using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// It is used to load the calibration file and pass in the Tag coordinates.
    /// </summary>
    public class TagProfileLoading : TagLoadingManager
    {
        /// <summary>
        /// Enable the loading of calibration parameters with a Tag type of Beacon and ID 65-67.
        /// </summary>
        [Header("Beacon")]
        [Tooltip("ID 65-67")]
        [SerializeField] private bool Beacon = true;
        /// <summary>
        /// Enable loading of the Tag type as LiBeacon.
        /// </summary>
        [Header("LiBeacon")]
        [Tooltip("ID 36, 35, 28, 32")]
        [SerializeField] private bool LiBeacon = false;
        /// <summary>
        /// Enable the calibration parameter with Topo Tag type and ID 100-170.
        /// </summary>
        [Header("TopoTag")]
        [Tooltip("ID 100-227")]
        [SerializeField] private bool TopoTag = true;
        /// <summary>
        /// Enable the loading of calibration parameters with Tag type SingleCard and ID 18, 19, 22, 23, 25, 39.
        /// </summary>
        [Header("Card")]
        [Tooltip("ID 18, 23, 25, 39, 19, 22")]
        [SerializeField] private bool SingleCard = false;
        /// <summary>
        /// Enable loading of Tag type to Gun.
        /// </summary>
        [Header("Gun")]
        [Tooltip("92ID£º2, 6, 11, 12, 13, 16, 20, 21, 24, 29, 38, 41, 43, 44, 50 ." +
            "  95ID:15, 31, 27, 28, 30, 26, 35, 42, 58, 61, 32, 53, 55, 62, 64, 36 .")]
        [SerializeField] private bool Gun = false;
        /// <summary>
        /// The id range of tags used.
        /// </summary>
        //[Header("ID-Range")]
        //[SerializeField] private int minId = 65;
        //[SerializeField] private int maxId = 227;
        #region Property
        /// <summary>
        /// Select the type of LiBeacon.
        /// </summary>
        [SerializeField] [EnumFlags] private LiBeaconType liBeaconType;
        /// <summary>
        /// Choose the size of the Topo Tag, 50cm or 38cm.
        /// </summary>
        [SerializeField] private TopoTagSize topoTagSize;
        /// <summary>
        /// Select the type of Gun, 92 or 95.
        /// </summary>
        [SerializeField] [EnumFlags] private GunType guntype;
        /// <summary>
        /// Choose the size of the Single Card, 40mm or 62mm.
        /// </summary>
        [SerializeField] private SingleCardSize singleSize;
        /// <summary>
        /// The path to the calibration file in the RhinoX Pro.
        /// </summary>
        private string CalibraFilePath = "/sdcard/vpusdk/marker_calib";
        public class EnumFlags : PropertyAttribute
        {
        }

        /// <summary>
        /// The size of the Topo Tag.
        /// </summary>
        public enum TopoTagSize
        {
            TopoTag_450mm,
            TopoTag_350mm
        }

        /// <summary>
        /// The type of gun.
        /// </summary>
        [System.Flags]
        public enum GunType
        {
            gunsight92 = 1,
            gunsight95 = 2,
        }
        /// <summary>
        /// The type and ID of LiBeacon
        /// </summary>
        [System.Flags]
        public enum LiBeaconType
        {
            LiBeacon_1ID36 = 1,
            LiBeacon_2ID35 = 2,
            LiBeacon_3ID28 = 4,
            LiBeacon_4ID32 = 8,
        }

        public enum SingleCardSize
        {
            Single_40mm,
            Single_62mm,
        }
        private static TagProfileLoading instance;
        public static TagProfileLoading Instance
        {
            get
            {
                return instance;
            }
        }

        public Vector3 OffsetPos
        {
            get => offsetPos;
            set => offsetPos = value;
        }
        public Vector3 OffsetRot
        {
            get => offsetRot;
            set => offsetRot = value;
        }

        #endregion

        #region Unity
        private void Awake()
        {
            if (instance==null)
            {
                instance = this;
            }
            ThreadTagLoading();
        }

        private void Start()
        {
            StartCoroutine(StartFusion());
        }

        private void OnDestroy()
        {
#if !UNITY_EDITOR
            XDevicePlugin.ResetTrackingMarkerSettings();
#endif
            StopAllCoroutines();
            instance = null;
        }
        #endregion

        #region Method
        /// <summary>
        /// Refresh and re-acquire the coordinate information of the large spatial positioning board.
        /// </summary>
        public void RefreshBeaconTran()
        {
            RefreshBeacon();
        }
        /// <summary>
        /// Clearing the algorithm data invalidates the large spatial positioning function.
        /// </summary>
        public void CleanBeaconData()
        {
            CleanBeacon();
        }
        /// <summary>
        /// Set targeting parameters
        /// </summary>
        public void SettingData()
        {
            SettingTagData();
        }
        /// <summary>
        /// Load calibration parameters
        /// </summary>
        private void SetCalibraFile()
        {
#if !UNITY_EDITOR
            XDevicePlugin.ResetTrackingMarkerSettings();


            if (Beacon)
            {
                int[] ids = new int[3];
                XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/BEACON-500.json", out ids, 3);
            }
            if (LiBeacon)
            {
                if ((liBeaconType & LiBeaconType.LiBeacon_1ID36) != 0)
                {
                    int[] ids = new int[1];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/LiBeacon-500-1.json", out ids, 1);
                }

                if ((liBeaconType & LiBeaconType.LiBeacon_2ID35) != 0)
                {
                    int[] ids = new int[1];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/LiBeacon-500-2.json", out ids, 1);
                }


                if ((liBeaconType & LiBeaconType.LiBeacon_3ID28) != 0)
                {
                    int[] ids = new int[1];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/LiBeacon-500-3.json", out ids, 1);
                }

                if ((liBeaconType & LiBeaconType.LiBeacon_4ID32) != 0)
                {
                    int[] ids = new int[1];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/LiBeacon-500-4.json", out ids, 1);
                }
            }

            if (TopoTag)
            {
                if (topoTagSize == TopoTagSize.TopoTag_450mm)
                {
                    int[] ids = new int[128];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/Topotag_model_100_to_227.json", out ids, 128);
                }

                if (topoTagSize == TopoTagSize.TopoTag_350mm)
                {
                    int[] ids = new int[128];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/BEACON/Topotag_model_100_to_227_350mm.json", out ids, 128);
                }
            }

            if (SingleCard)
            {
                if (singleSize == SingleCardSize.Single_40mm)
                {
                    int[] ids = new int[6];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/CARD/single_markers_500_03_40mm.json", out ids, 6);
                }

                if (singleSize == SingleCardSize.Single_62mm)
                {
                    int[] ids = new int[6];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/CARD/single_markers_500_03_62mm.json", out ids, 6);
                }
            }

            if (Gun)
            {
                if ((guntype & GunType.gunsight92) != 0)
                {
                    int[] ids = new int[16];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/GUN/92_gunsight_500_03/92_gunsight_500_03.json", out ids, 16);
                }

                if ((guntype & GunType.gunsight95) != 0)
                {
                    int[] ids = new int[16];
                    XDevicePlugin.LoadTrackingMarkerSettingsFile(CalibraFilePath + "/GUN/95_gunsight_500_03/95_gunsight_500_03.json", out ids, 16);
                }
            }
#endif
        }
        public void ThreadTagLoading()
        {
            Thread thread;
            thread = new Thread(SetCalibraFile);
            threadLoad = thread;
            threadLoad.Start();
        }
        #endregion
    }
}

