using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ximmerse.XR
{
    public class NativePluginApi
    {   
  
      private const string LibraryName = "unity_native_api";

        public enum GraphicsApi
        {
            kOpenGlEs2 = 1,
            kOpenGlEs3 = 2,
            kMetal = 3,
            kNone = -1,
        }

        public enum DeviceType
        {
            Device_CardBoard = 0,
            Device_RhinoXPro = 1,
            Devicc_HMD20 = 2,
            Device_AUTO = 0xef
        }

        [DllImport(LibraryName)]
        public static extern void Unity_setScreenParams(int screen_width, int screen_height, int viewport_x, int viewport_y, int viewport_width, int viewport_height);

        [DllImport(LibraryName)]
        public static extern void Unity_setGraphicsApi(GraphicsApi graphics_api);

        [DllImport(LibraryName)]
        public static extern void Unity_initializeAndroid(IntPtr context);

        [DllImport(LibraryName)]
        public static extern void Unity_setDeviceType(int device);

        [DllImport(LibraryName)]
        public static extern int Unity_TagPredict(long predTimestampNano);

        [DllImport(LibraryName)]
        public static extern bool Unity_getTagTracking2(int track_id,
                      ref int index, ref long timestamp, ref int state,
                      ref float posX, ref float posY, ref float posZ,
                      ref float rotX, ref float rotY, ref float rotZ, ref float rotW,
                      ref float confidence, ref float marker_distance);

        [DllImport(LibraryName)]
        public static extern bool Unity_getTagTracking(int track_id, long predTimestampNano,
                      ref int index, ref long timestamp, ref int state,
                      ref float posX, ref float posY, ref float posZ,
                      ref float rotX, ref float rotY, ref float rotZ, ref float rotW,
                      ref float confidence, ref float marker_distance);

        [DllImport(LibraryName)]
        public static extern void Unity_setTrackingMode(int mode);

        [DllImport(LibraryName)]
        public static extern void Unity_setVsync(int vsync);

        [DllImport(LibraryName)]
        public static extern void Unity_setRenderResolution(int width, int height);

        [DllImport(LibraryName)]
        public static extern void Unity_setCustomViewFrustum(float lLeft, float lRight, float lTop, float lBottom,
    float rLeft, float rRight, float rTop, float rBottom);


        [DllImport(LibraryName)]
        public static extern void Unity_resetViewFrustum();

        [DllImport(LibraryName)]
        public static extern void Unity_Vibration(int index, float amplitude, float duration);


        [DllImport(LibraryName)]
        public static extern bool Unity_getFusionResult(long predictedTimeNs,
                            ref int beacon_id,
                            ref long beacon_timestamp,
                            ref float beacon_pos0,
                            ref float beacon_pos1, 
                            ref float beacon_pos2,
                            ref float beacon_rot0,
                            ref float beacon_rot1,
                            ref float beacon_rot2,
                            ref float beacon_rot3,
                            ref float beacon_tracking_confidence,
                            ref float beacon_min_distance,
                            ref float beacon_correct_weight);

        [DllImport(LibraryName)]
        public static extern void Unity_setMaxPredictTime(float time);

        [DllImport(LibraryName)]
        public static extern void Unity_setHalfExposureTime(float time);

        /// <summary>
        /// 设置 sRGB 支持标志，如果为true，则eye buffer会以sRGB的形式创建Render texture。
        /// </summary>
        /// <param name="flag"></param>
        [DllImport(LibraryName)]
        public static extern void Unity_requiresRGBEyeBuffer(bool flag);

    }
}
