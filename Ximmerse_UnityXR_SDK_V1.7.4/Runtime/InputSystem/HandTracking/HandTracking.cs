using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Layouts;
using System.IO;
using Ximmerse.XR.Internal.Mathmatics;
using System.Xml;
using SXR;
namespace Ximmerse.XR.InputSystems
{


    /// <summary>
    /// Gesture type: open hand / fist
    /// </summary>
    public enum GestureType_Fist_OpenHand
    {
        None = 0,

        /// <summary>
        /// Five fingers opened.
        /// </summary>
        Opened = 1,

        /// <summary>
        /// Gesture of making hand as a fist
        /// </summary>
        Fist = 2,

    }

    /// <summary>
    /// Gesture type : grisp opened / closed
    /// </summary>
    public enum GestureType_Grisp
    {
        None = 0,

        /// <summary>
        /// Grisp open 
        /// </summary>
        GrispOpen,

        /// <summary>
        /// Grisp close
        /// </summary>
        GraspClosed,
    }


    /// <summary>
    /// Hand tracking API expose interface for hand tracking functionality with RhinoX in XR platform.
    /// </summary>
    public static class HandTracking
    {
        static I_HandleTrackingModule currentHandTrackModule = null;

        static Vector3 rgbCameraAnchor = new Vector3(0.02631f, 0.05096f, 0.10121f);

        static Quaternion rgbCameraQ = Quaternion.Euler(0, 0, 0);

        /// <summary>
        /// The rgb camera anchor
        /// </summary>
        public static Transform rgbCamAnchor;

        /// <summary>
        /// The virtual XimXR gesture input device.
        /// </summary>
        public static XimmerseXRGesture sGestureInputDevice
        {
            get; private set;
        }

        static bool sIsGestureDeviceLayoutRegistered = false;

        static bool isDeviceCalibrated;

        static string pathDeviceCalibration = "/backup/slam/device_calibration.xml";

        static string pathRGB2VIO = "/backup/rgb_vio_camera_params.xml";

        /// <summary>
        /// Read RGB calibration data.
        /// </summary>
        static void ReadRGBCalibration()
        {
            try
            {
                //RGB to VIO R:
                XmlDocument xmlDoc_rgb = new XmlDocument(); // Create an XML document object
                xmlDoc_rgb.Load(pathRGB2VIO); // Load the XML document from the specified file
                XmlNode RotMat = xmlDoc_rgb.GetElementsByTagName("RotMat").Item(0);
                XmlNode TransVec = xmlDoc_rgb.GetElementsByTagName("TransVec").Item(0);
                ParseCalibrationMatrixFromText(RotMat["data"].InnerText, TransVec["data"].InnerText, out Matrix4x4 rgb2vioR);

                //VIO L to VIO R:
                XmlDocument xmlDoc_deviceCali = new XmlDocument(); // Create an XML document object
                xmlDoc_deviceCali.Load(pathDeviceCalibration); // Load the XML document from the specified file
                XmlNode camera_TrackingB = xmlDoc_deviceCali.GetElementsByTagName("Camera").Item(1);
                XmlNode rig = camera_TrackingB["Rig"];
                var rigAttri = rig.Attributes;
                ParseCalibrationMatrixFromText(rigAttri["rowMajorRotationMat"].InnerText, rigAttri["translation"].InnerText, out Matrix4x4 vioL2R);

                //VIO L to IMU:
                XmlNode SFConfig = xmlDoc_deviceCali.GetElementsByTagName("SFConfig").Item(0);
                XmlNode Stateinit = SFConfig["Stateinit"];
                var StateinitAttr = Stateinit.Attributes;
                ParseRotationVectorMatrixFromText(StateinitAttr["ombc"].InnerText, StateinitAttr["tbc"].InnerText, out Matrix4x4 vioL2Imu);

                Debug.LogFormat("Rgb to VIO L: {0}", rgb2vioR);
                Debug.LogFormat("VIO L to R: {0}", vioL2R);
                Debug.LogFormat("VIO L to IMU: {0}", vioL2Imu);

                Matrix4x4 rgb_to_IMU_space = vioL2Imu * vioL2R.inverse * rgb2vioR;
                Matrix4x4 righthand_rgb_2_imu = IMUSpaceToRightHandSpace(rgb_to_IMU_space);
                var eye2imu = ParamLoaderFloat16ToMatrix((int)ParamType.Design_Eye2IMU_TransMat_OpenGL_ARRAY16);
                Matrix4x4 righthand_rgb_2_eyecenter = eye2imu.inverse * righthand_rgb_2_imu; //imu2eye * rgb2imu
                Matrix4x4 unity_rgb_2_eyecenter = UnityRightHandSpaceToLeftHandSpace(righthand_rgb_2_eyecenter);
                Debug.LogFormat("(Unity space) RGB 2 EyeCenter : {0}, {1}", ((Vector3)unity_rgb_2_eyecenter.GetColumn(3)).ToString("F5"), unity_rgb_2_eyecenter.rotation.eulerAngles.ToString("F5"));
                rgbCameraAnchor = (Vector3)unity_rgb_2_eyecenter.GetColumn(3);
                rgbCameraQ = unity_rgb_2_eyecenter.rotation;
                isDeviceCalibrated = true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        static Matrix4x4 ParamLoaderFloat16ToMatrix(int type)
        {
            float[] float16 = new float[16];
            ParamLoader.ParamLoaderGetFloatArray(type, float16, System.Runtime.InteropServices.Marshal.SizeOf<float>() * 16);
            return new Matrix4x4()
            {
                m00 = float16[0],
                m01 = float16[1],
                m02 = float16[2],
                m03 = float16[3],
                m10 = float16[4],
                m11 = float16[5],
                m12 = float16[6],
                m13 = float16[7],
                m20 = float16[8],
                m21 = float16[9],
                m22 = float16[10],
                m23 = float16[11],
                m30 = float16[12],
                m31 = float16[13],
                m32 = float16[14],
                m33 = float16[15],
            };
        }

        static Matrix4x4 UnityRightHandSpaceToLeftHandSpace(Matrix4x4 rightHandSpace)
        {
            var rightHandT = (Vector3)rightHandSpace.GetColumn(3);
            var rightHandQ = rightHandSpace.rotation.eulerAngles;
            return Matrix4x4.TRS(new Vector3(-rightHandT.x, -rightHandT.y, rightHandT.z), Quaternion.Euler(rightHandQ.x, -rightHandQ.y, -rightHandQ.z), Vector3.one);
        }

        static Matrix4x4 IMUSpaceToRightHandSpace(Matrix4x4 imuSpace)
        {
            // P: (-0.00092, -0.06631, -0.01755), (-0.37781, -169.53320, -89.64063)
            var imu_t = (Vector3)imuSpace.GetColumn(3);
            Vector3 rightHand_t = new Vector3(imu_t.y, imu_t.x, -imu_t.z);
            Quaternion righthand_q = Quaternion.Euler(0, 180, 90) * imuSpace.rotation * Quaternion.Euler(0, 0, 180);
            return Matrix4x4.TRS(rightHand_t, righthand_q, Vector3.one);
        }


        private static void ParseRotationVectorMatrixFromText(string RotationVector, string TextPosition, out Matrix4x4 translate)
        {
            translate = Matrix4x4.identity;
            int row = 0, col = 0;
            try
            {
                var textInArray = RotationVector.Trim().Split(' ');
                Vector3 rotationVector = Vector3.zero;

                for (int i = 0; i < textInArray.Length; i++)
                {
                    string t = textInArray[i];
                    if (float.TryParse(t, out float value))
                    {
                        rotationVector[col] = value;
                        col++;
                    }
                }

                Matrix3x3 qMatrix = RotationVectorToMatrix(rotationVector);

                textInArray = TextPosition.Trim().Split(' ');
                Vector4 column = new Vector4(0, 0, 0, 1);
                col = 0;
                for (int i = 0; i < textInArray.Length; i++)
                {
                    string e = textInArray[i];
                    if (string.IsNullOrEmpty(e.Trim()))
                    {
                        continue;
                    }
                    column[col++] = float.Parse(e.Trim());
                }

                translate = Matrix3x3.ToTRS(qMatrix, (Vector3)column);
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
            }
        }



        private static void ParseCalibrationMatrixFromText(string textQuaternion, string textPosition, out Matrix4x4 translate)
        {
            translate = Matrix4x4.identity;
            int row = 0, col = 0;
            try
            {
                var textInArray = textQuaternion.Trim().Split(' ');
                for (int i = 0; i < textInArray.Length; i++)
                {
                    string t = textInArray[i];
                    if (float.TryParse(t, out float value))
                    {
                        translate[row, col] = value;
                        col++;
                        if (col >= 3)
                        {
                            col = 0;
                            row++;
                        }
                    }
                }

                textInArray = textPosition.Trim().Split(' ');
                Vector4 column = new Vector4(0, 0, 0, 1);
                col = 0;
                for (int i = 0; i < textInArray.Length; i++)
                {
                    string e = textInArray[i];
                    if (string.IsNullOrEmpty(e.Trim()))
                    {
                        continue;
                    }
                    column[col++] = float.Parse(e.Trim());
                }
                translate.SetColumn(3, column);
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
            }
        }

        /// <summary>
        /// Convert rotation vector in radians to a rotation matrix 3x3, using Rodrigues fomular.
        /// </summary>
        /// <param name="RotationVectorInRadians"></param>
        /// <returns>A 3x3 matrix represents a rotation quaternion.</returns>
        static Matrix3x3 RotationVectorToMatrix(Vector3 RotationVectorInRadians)
        {
            Vector3 r = RotationVectorInRadians.normalized;
            float theta = RotationVectorInRadians.magnitude;
            return Mathf.Cos(theta) * Matrix3x3.identity + (1 - Mathf.Cos(theta)) * new Matrix3x3(r, r) + Mathf.Sin(theta) * new Matrix3x3(0, -r.z, r.y, r.z, 0, -r.x, -r.y, r.x, 0);
        }

        /// <summary>
        /// Enable handle tracking
        /// </summary>
        public static void EnableHandTracking()
        {
            if(!isDeviceCalibrated)
            {
                ReadRGBCalibration();
            }
            if (!rgbCamAnchor)
            {
                rgbCamAnchor = new GameObject("RGB Camera Anchor").transform;
                rgbCamAnchor.localPosition = rgbCameraAnchor;
                rgbCamAnchor.localRotation = rgbCameraQ;
                rgbCamAnchor.SetParent(Camera.main.transform, false);
            }
            if (!sIsGestureDeviceLayoutRegistered)
            {
                RegisterXRGestureLayout();
            }
            if (currentHandTrackModule == null)
            {
                currentHandTrackModule = new HandTrackingT3D();
                if (currentHandTrackModule.EnableModule(new InitializeHandTrackingModuleParameter()
                {
                    TrackingAnchor = rgbCamAnchor,
                }))
                {
                    if (sGestureInputDevice == null)
                    {
                        //Adds a virtural input device for gesture input:
                        XimmerseXRGesture gestureInputDevice = (XimmerseXRGesture)InputSystem.AddDevice(new InputDeviceDescription
                        {
                            interfaceName = "HandState",
                            product = "GestureInputDevice",
                        });
                        sGestureInputDevice = gestureInputDevice;
                    }
                }
                XRManager.OnPostTrackUpdate += currentHandTrackModule.Tick;
            }

        }


        private static void RegisterXRGestureLayout()
        {
            InputSystem.RegisterLayout<XimmerseXRGesture>(matches: new InputDeviceMatcher()
                .WithInterface("HandState"));
            sIsGestureDeviceLayoutRegistered = true;
            Debug.LogFormat("Gesture input device layout has been registered.");
        }

        /// <summary>
        /// 
        /// </summary>
        public static I_HandleTrackingModule handTrackModule
        {
            get => currentHandTrackModule;
        }

        /// <summary>
        /// Disable hand tracking.
        /// </summary>
        public static void DisableHandTracking()
        {
            if (currentHandTrackModule != null)
            {
                currentHandTrackModule.DisableModule();
                XRManager.OnPostTrackUpdate -= currentHandTrackModule.Tick;
                currentHandTrackModule = null;

                if (sGestureInputDevice != null)
                {
                    InputSystem.RemoveDevice(sGestureInputDevice);
                }
            }
        }

        /// <summary>
        /// Is hand tracking module currently enabled and running ?
        /// </summary>
        public static bool IsHandTrackingEnable
        {
            get => currentHandTrackModule != null && currentHandTrackModule.IsModuleEnabled;
        }

        /// <summary>
        /// If IsHandTrackingEnable is true, this is the hand track info.
        /// </summary>
        public static HandTrackingInfo HandTrackingInfo
        {
            get
            {
                return currentHandTrackModule != null ? currentHandTrackModule.HandleTrackInfo : default(HandTrackingInfo);
            }
        }

#if UNITY_EDITOR

        //[UnityEditor.MenuItem("Ximmerse/Create Gesture Device")]
        private static void TestAddGestureDevice()
        {
            if (!sIsGestureDeviceLayoutRegistered)
            {
                InputSystem.RegisterLayout<XimmerseXRGesture>(matches: new InputDeviceMatcher()
    .WithInterface("HandState"));
                sIsGestureDeviceLayoutRegistered = true;
            }

            XimmerseXRGesture gestureID = (XimmerseXRGesture)InputSystem.AddDevice(new InputDeviceDescription
            {
                interfaceName = "HandState",
                product = "GestureInputDevice",
            });
            Debug.Log("Gesture device is added.");
        }

        //[UnityEditor.MenuItem("Ximmerse/Remove Gesture Device")]
        private static void RemoveDevice()
        {
            var gestureDevice = InputSystem.devices.FirstOrDefault(x => x is XimmerseXRGesture);
            if (gestureDevice != null)
            {
                InputSystem.RemoveDevice(gestureDevice);
                Debug.Log("Gesture device is removed.");
            }
        }
#endif

    }
}