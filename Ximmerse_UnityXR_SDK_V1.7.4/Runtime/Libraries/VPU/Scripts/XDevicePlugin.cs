﻿using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

//#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_WIN || UNITY_MAC
#if true
using UnityEngine;
using UnityEngine.Events;
using XDebug = UnityEngine.Debug;
#else
using XDebug = System.Diagnostics.Debug;
#endif // UNITY_EDITOR

using NativeHandle = System.Int64;
using NativeExHandle = System.Int64;
using AOT;
using Unity.Collections;
using System.IO;
using System.Linq;
using Ximmerse.XR.Collections;
namespace Ximmerse.XR
{
    /// <summary>
    /// Tracking summary, reported per frame
    /// </summary>
    internal struct TrackingSummary : IDisposable
    {
        /// <summary>
        /// The tracked marker count
        /// </summary>
        public int TrackedCount;

        /// <summary>
        /// The untracked marker count
        /// </summary>
        public int UntrackedCount;

        /// <summary>
        /// The lost tracked marker count
        /// </summary>
        public int LostTrackedCount;

        /// <summary>
        /// The new tracked marker count
        /// </summary>
        public int NewTrackedCount;

        /// <summary>
        /// All results
        /// </summary>
        public NativeArray<TrackingResult> allResults;

        /// <summary>
        /// Currently tracked result.
        /// </summary>
        public NativeArray<TrackingResult> tracked;

        /// <summary>
        /// Untracked result.
        /// </summary>
        public NativeArray<TrackingResult> unTracked;

        /// <summary>
        /// Added result;
        /// </summary>
        public NativeArray<TrackingResult> newTracked;

        /// <summary>
        /// Lost tracking result.
        /// </summary>
        public NativeArray<TrackingResult> lostTracked;

        public void Dispose()
        {
            allResults.Dispose();
            tracked.Dispose();
            unTracked.Dispose();
            newTracked.Dispose();
            lostTracked.Dispose();
        }
    }

    public partial class XDevicePlugin
    {
        public const string pluginName = "xdevice_client";

        internal static List<MarkerDescriptor> loadedMarkers = new List<MarkerDescriptor>();

        /// <summary>
        /// Cache the previous frames tracking results.
        /// </summary>
        static Dictionary<int, TrackingResult> prevFrameTrackingResults = new Dictionary<int, TrackingResult>();

        #region api wrapper

        public static bool ConnectController(int index, string mac, bool force)
        {
            int ret = xdevc_ctrl_connect_to_address(xdevc_get_controller(index), mac, force);
            if (ret < 0)
            {
                return false;
            }
            return true;
        }
        ///////////////////////////////
        /// \brief Reset tracking marker settings.
        /// \returns
        public static XErrorCodes ResetTrackingMarkerSettings()
        {
            XErrorCodes c = (XErrorCodes)xdevc_vpu_clear_marker_calibration_settings(xdevc_get_vpu());
            if (c == XErrorCodes.kErrNoError)
            {
                loadedMarkers.Clear();
            }
            return c;
        }


        ///////////////////////////////
        /// \brief Load tracking algorithm marker settings.
        /// \param settings_file File to load.
        /// \param markerIds [out] Output int array for marker IDs defined in specified file.
        /// \returns Return count of marker IDs, or a negactive value for error. 
        [System.Obsolete("Deprecated , usee another one with NativeArray")]
        public static int LoadTrackingMarkerSettingsFile(string settings_file, out int[] markerIds, int n)
        {
            int[] tmp = new int[n];
            IntPtr buffer = Marshal.AllocCoTaskMem(Buffer.ByteLength(tmp));
            int ret = xdevc_vpu_load_marker_calibration_from_file(xdevc_get_vpu(), settings_file, n, buffer);
            Marshal.Copy(tmp, 0, buffer, n);
            Marshal.FreeCoTaskMem(buffer);
            if (ret > 0)
            {
                markerIds = new int[ret];
                Array.Copy(tmp, markerIds, ret);
            }
            else
            {
                markerIds = new int[0];
            }

            return ret;
        }

        ///////////////////////////////
        /// \brief Load tracking algorithm marker settings.
        /// \param settings_file File to load.
        /// \param markerIds [out] Output int array for marker IDs defined in specified file.
        /// \returns Return count of marker IDs, or a negactive value for error. 
        public static int LoadTrackingMarkerSettingsFile(string trackingJsonFile, ref NativeArray<int> markerIds, int n)
        {
            int resultCode = -1;
            int[] tmp = new int[n];
            IntPtr buffer = Marshal.AllocCoTaskMem(tmp.Length);
            string fullPath_External = Path.Combine(SDKVariants.kTrackingDataDir_External, trackingJsonFile);
            string fullPath_Internal = Path.Combine(SDKVariants.kTrackingDataDir_Internal, trackingJsonFile);

            //try to load from external, then internal:
            string _path = File.Exists(fullPath_External) ? fullPath_External : (File.Exists(fullPath_Internal) ? fullPath_Internal : string.Empty);
            if (string.IsNullOrEmpty(_path))
            {
                Debug.LogErrorFormat("Tracking json : {0} not found", trackingJsonFile);
                return resultCode;
            }

            resultCode = xdevc_vpu_load_marker_calibration_from_file(xdevc_get_vpu(), _path, n, buffer);
            Marshal.Copy(tmp, 0, buffer, tmp.Length);
            Marshal.FreeCoTaskMem(buffer);
            if (resultCode > 0)
            {
                for (int i = 0; i < tmp.Length; i++)
                {
                    markerIds[i] = tmp[i];
                }

                //Add to trace list:
                TrackedObjectJson trackObj = (TrackedObjectJson)JsonUtility.FromJson(File.ReadAllText(_path), typeof(TrackedObjectJson));
                if (trackObj.IsCardGroup)
                {
                    if (loadedMarkers.Count(x => x.id == trackObj.CARD_GROUP.GroupID) == 0)
                    {
                        loadedMarkers.Add(new MarkerDescriptor()
                        {
                            id = trackObj.CARD_GROUP.GroupID,
                            isGroup = true,
                            size = trackObj.CARD_GROUP.MarkersSize[0],
                        });
                    }
                }
                else
                {
                    for (int i = 0; i < trackObj.CARD_SINGLE.Markers.Length; i++)
                    {
                        int cardID = trackObj.CARD_SINGLE.Markers[i];
                        float _size = trackObj.CARD_SINGLE.MarkersSize[i];
                        if (loadedMarkers.Count(x => x.id == cardID) == 0)
                        {
                            loadedMarkers.Add(new MarkerDescriptor()
                            {
                                id = cardID,
                                isGroup = false,
                                size = _size,
                            });
                        }
                    }
                }
            }
            Debug.LogFormat("Tracking json profile : {0} loaded.", _path);
            return resultCode;
        }

        /// <summary>
        /// Called per frame to get hardware tracking result.
        /// </summary>
        /// <param name="centerEyePose"></param>
        /// <param name="trackingSummary"></param>
        internal static void UpdateTracking(
            Matrix4x4 centerEyePose, out TrackingSummary trackingSummary)
        {
            trackingSummary = new TrackingSummary();
            NativeArray<TrackingResult> _allResults = new NativeArray<TrackingResult>(loadedMarkers.Count, Allocator.TempJob);

            xNativeList<TrackingResult> _tracked = xNativeList<TrackingResult>.Create(32);
            xNativeList<TrackingResult> _unTracked = xNativeList<TrackingResult>.Create(32);
            xNativeList<TrackingResult> _newTracked = xNativeList<TrackingResult>.Create(32);
            xNativeList<TrackingResult> _lostTracked = xNativeList<TrackingResult>.Create(32);

            for (int i = 0; i < loadedMarkers.Count; i++)
            {
                MarkerDescriptor loadedMarker = loadedMarkers[i];
                TrackingResult r = new TrackingResult()
                {
                    id = loadedMarker.id,
                    isGroup = loadedMarker.isGroup,
                    tracked = false,
                    size = loadedMarker.size,
                };

                int id = loadedMarker.id;
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
                long predTimestampNano = 0;
                if (NativePluginApi.Unity_getTagTracking(id, predTimestampNano,
    ref index, ref timestamp, ref state, ref posX, ref posY, ref posZ,
    ref rotX, ref rotY, ref rotZ, ref rotW,
    ref confidence, ref marker_distance))
                {
                    if (state != 0)
                    {
                        r.tracked = true;
                        r.trackedConfidence = confidence;
                        r.trackedDistance = marker_distance;
                        
                        var world = centerEyePose * Matrix4x4.TRS(new Vector3(posX, posY, posZ), new Quaternion(rotX, rotY, rotZ, rotW), Vector3.one);
                        r.worldPose = new Pose(world.GetColumn(3),
                            world.rotation);

                        trackingSummary.TrackedCount++;
                        _tracked.Add(r);
                        //compare to prev frame:
                        bool prevFrameTracked = prevFrameTrackingResults.ContainsKey(id) && prevFrameTrackingResults[id].tracked;
                        if (!prevFrameTracked)//new tracked
                        {
                            trackingSummary.NewTrackedCount++;
                            _newTracked.Add(r);
                        }

                        //Debug.LogFormat("ID: {0} is tracked, world position = {1}, rotation = {2}", id, r.worldPose.position.ToString("F3"), r.worldPose.rotation.ToString("F3"));
                    }
                    //not tracked
                    else
                    {
                        trackingSummary.UntrackedCount++;
                        _unTracked.Add(r);
                        //compare to prev frame:
                        bool prevFrameTracked = prevFrameTrackingResults.ContainsKey(id) && prevFrameTrackingResults[id].tracked;
                        if (prevFrameTracked)//lost track
                        {
                            trackingSummary.LostTrackedCount++;
                            _lostTracked.Add(r);
                        }
                    }
                }
                _allResults[i] = r;
            }


            //Cache this frame's result:
            prevFrameTrackingResults.Clear();
            foreach (var ret in _allResults)
            {
                prevFrameTrackingResults.Add(ret.id, ret);
            }

            trackingSummary.allResults = _allResults;
            trackingSummary.tracked = _tracked.ToArray(Allocator.TempJob);
            trackingSummary.newTracked = _newTracked.ToArray(Allocator.TempJob);
            trackingSummary.unTracked = _unTracked.ToArray(Allocator.TempJob);
            trackingSummary.lostTracked = _lostTracked.ToArray(Allocator.TempJob);

            _tracked.Dispose();
            _newTracked.Dispose();
            _unTracked.Dispose();
            _lostTracked.Dispose();
        }


        public static bool GetControllerState(int index, Int64 predTimestampNano, ref XAttrTrackingInfo info)
        {

            System.IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(XAttrTrackingInfo)));
            Marshal.StructureToPtr(info, ptr, false);

            int ret = xdevc_ctrl_get_fusion(xdevc_get_controller(index), predTimestampNano, ptr);

            info = (XAttrTrackingInfo)Marshal.PtrToStructure(ptr, typeof(XAttrTrackingInfo));
            Marshal.FreeHGlobal(ptr);

            if (ret < 0)
            {
                return false;
            }

            return true;

        }

        ///////////////////////////
        /// \brief Get pose data of tracking object.
        /// \param track_id Tracking ID.
        /// \returns Tracking pose data. \see XAttrTrackingInfo.
        public static bool GetTracking(int track_id, Int64 predTimestampNano, ref XAttrTrackingInfo trackingInfo)
        {
            System.IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(XAttrTrackingInfo)));
            Marshal.StructureToPtr(trackingInfo, ptr, false);
            int ret = xdevc_vpu_get_predict_tracking(xdevc_get_vpu(), track_id, predTimestampNano, ptr);
            trackingInfo = (XAttrTrackingInfo)Marshal.PtrToStructure(ptr, typeof(XAttrTrackingInfo));
            Marshal.FreeHGlobal(ptr);

            if (ret < 0)
            {
                return false;
            }

            return true;
        }

        #endregion api wrapper

        public enum XEvent
        {
            kXEvtNormalEventsBegin = 1000,
            kXEvtInitSuccess = kXEvtNormalEventsBegin,
            kXEvtInitFailed,

            kXEvtConnectionStateChange,

            kXEvtDeviceBatteryStateChange,

            kXEvtVpuInfoUpdated,
            kXEvtVpuConnectionStateChange,

            kXEvtControllerInfoUpdate,
            kXEvtControllerStateChange,
            kXEvtControllerConnectionStateChange,

            kXEvtButtonStateChange,
            kXEvtControllerPaired,
            kXEvtControllerUnpaired,

            kXEvtMax,
        };


        #region Common
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_init();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xdevc_exit();

        // private delegate void xdevc_events_handle_func_t(XEvent evt, IntPtr arg, IntPtr ud);
        public delegate void XDeviceClientEventDelegate(XEvent evt, IntPtr handle, IntPtr ud);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xdevc_set_events_cb(XDeviceClientEventDelegate cb);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_event_name(XEvent evt);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_error_name(int err);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern NativeHandle xdevc_get_xcontext();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern NativeHandle xdevc_get_vpu();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern NativeHandle xdevc_get_controller(int index);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern NativeHandle xdevc_get_number_of_controllers();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_get_property_memory_id(Int64 object_id, int prop_id);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_do_test();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_client_version();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_get_client_build_number();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_client_build_information();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_get_client_alg_version(int algType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_server_version();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_get_server_build_number();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_server_build_information();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_get_server_alg_version(int algType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_sdk_autorize();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_get_authorization_available_time();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_tx_message(NativeExHandle device_handl, int size, string data);
        #endregion Common

        #region VPU API
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xdevc_vpu_is_connected(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_start_pairing_by_type(NativeExHandle vpu_handle, XControllerTypes type);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_stop_pairing(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_unpair_and_disconnect_all_controllers(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_firmware_version(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_hardware_version(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_sn(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_device_name(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_model_name(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_fpga_version(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_ble_firmware_version(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xdevc_vpu_get_imu_alg_version(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_fpga_model_number(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_mcu_model_number(NativeExHandle vpu_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_ble_model_number(NativeExHandle vpu_handle);

        public delegate void XDeviceFirmwareUpgradeEventsDelegateFn_t(int evt, int int_arg, IntPtr ud);
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_start_upgrading(
            NativeExHandle vpu_handle,
            int proj_type,
            int fw_type,
            string fw_path,
            XDeviceFirmwareUpgradeEventsDelegateFn_t event_delegate,
            IntPtr ud,
            int wait_ms);

        #region VPU Tracking Alg
        /**
         * @brief load marker calibration configure data from file.
         * 
         * @param vpu_handle 
         * @param file the path of file which must can be read by xdeviceservice application.
         * @param nof_output_elements size of output IDs buffer, how many id can be write in output_ids parameter.
         * @param output_ids
         * @return negative value for error, positive value for number of output tracking IDs.
         */
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_load_marker_calibration_from_file(NativeExHandle vpu_handle, string file, int nof_output_elements, IntPtr output_ids);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_load_marker_calibration_from_file(NativeExHandle vpu_handle, string file, int nof_output_elements, ref int[] outputIds);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_clear_marker_calibration_settings(NativeExHandle vpu_handle);

        /**
        * @brief 
        * 
        * @param track_id 
        * @param timestamp positive value for boot time in nanosecond to predict.
        *      0 for no predition, -1 for now timestamp,
        * @param out 
        * @return XIM_API 
        */
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_predict_tracking(NativeExHandle vpu_handle, int track_id, Int64 timestamp, IntPtr out_tracking_info);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_predict_tracking(NativeExHandle vpu_handle, int track_id, Int64 timestamp, ref XAttrTrackingInfo trackingInfo);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_tracking(NativeExHandle vpu_handle, int track_id, IntPtr out_tracking_info);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_tracking(long vpuHandle, int trackId, ref XAttrTrackingInfo trackingInfo);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_set_power_mode(long vpuHandle, int power_mode);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_set_glass_perspective(long vpuHandle, int what, int val);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_tracked(NativeExHandle vpu_handle, Int64 timestamp, int nof_buf, IntPtr out_tracking_info);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_get_multiple_trackings(NativeExHandle vpu_handle, Int64 timestamp, int nof_ids, IntPtr ids, int nof_buf, IntPtr[] out_tracking_info);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_tracking_predict(Int64 timestamp_ns);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_tracking_get_predicted(int tag_id, IntPtr out_tracking_info);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_vpu_tracking_get_predicted2(
            int tag_id,
            ref float px, ref float py, ref float pz,
            ref float qx, ref float qy, ref float qz, ref float qw,
            ref Int64 timestamp,
            ref double confidence,
            ref double marker_distance);

        #endregion VPU Tracking Alg
        #endregion VPU API

        #region Controller API
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_connect_state(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xdevc_ctrl_is_paired(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_pairing_state(long controllerHandle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_imu_fps(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_battery_level(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_button_state_bitmask(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_trigger(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_imu(NativeExHandle controller_handle, IntPtr imu);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_imu(long controllerHandle, ref XAttrImuInfo imuInfo);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_fusion(NativeExHandle controller_handle, Int64 timestamp_ns, IntPtr track_info);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_fusion(long controllerHandle, long timestampNs, ref XAttrTrackingInfo trackingInfo);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_touchpad_state(NativeExHandle controller_handle, IntPtr state);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_touchpad_state(long controllerHandle, ref XAttrTouchPadState touchPadState);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_controller_state(NativeExHandle controller_handle, IntPtr state);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_controller_state(long controllerHandle, ref XAttrControllerState controllerState);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_track_id(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_get_device_type(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_device_address(NativeExHandle controller_handle);

        //[DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr xdevc_ctrl_get_device_address(long controllerHandle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_device_name(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_software_revision(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_hardware_revision(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_serial_number(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_model_name(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string xdevc_ctrl_get_manufacture_name(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_connect(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_connect_to_type(NativeExHandle controller_handle, int type, bool force);

        /**
         * @brief 
         * 
         * @param controller_handle 
         * @param mac MAC string of controller, e.g. 01:09:0B:09:0E:39
         * @param force 
         * @return XIM_API 
         */

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_connect_to_address(NativeExHandle controller_handle, string mac, bool force);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_connect_to_rfid(NativeExHandle controller_handle, int rfid, bool force);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_connect_to_rfid_pattern(NativeExHandle controller_handle, int rfid_pattern, bool force);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_disconnect(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_confirm_pair(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_hold_connection(NativeExHandle controller_handle, int hold_time_sec);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_unbind(NativeExHandle controller_handle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_vibrate(NativeExHandle controller_handle, int strengh_percentage, int dur_ms);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_ctrl_tx_message(NativeExHandle controller_handle, int uuid, int msg_size, IntPtr message);

        #endregion Controller API

        #region Others
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_start_vio_service();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int xdevc_stop_vio_service();
        #endregion Others

        public static XDeviceClientEventDelegate _xDeviceClientEventDelegate = null;
        public void RegisterXEventCallbackDelegate(XDeviceClientEventDelegate dlg)
        {
            _xDeviceClientEventDelegate = dlg;
            xdevc_set_events_cb(dlg);
        }

        public void UnregisterXEventCallbackDelegate()
        {
            xdevc_set_events_cb(null);
            _xDeviceClientEventDelegate = null;
        }
    }
}
