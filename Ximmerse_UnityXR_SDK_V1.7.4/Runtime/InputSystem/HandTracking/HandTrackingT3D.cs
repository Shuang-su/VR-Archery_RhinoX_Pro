using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using System.Runtime.InteropServices;
using TouchlessA3D;
using System;
using System.Xml;
using System.Text;


namespace Ximmerse.XR.InputSystems
{

    /// <summary>
    /// Hand tracking module implementor on touchless 3D SDK.
    /// </summary>
    internal class HandTrackingT3D : I_HandleTrackingModule
    {

        private readonly static int width = 1440;

        private readonly static int height = 1080; //264

        /// <summary>
        /// Image data array.
        /// </summary>
        private Color32[] imgData = new Color32[width * height];

        private bool m_IsModuleEnabled;

        /// <summary>
        /// RGB camera texture to be passed to native plugin.
        /// </summary>
        private WebCamTexture cameraTexture;

        private GCHandle imageHandle;

        public bool IsModuleEnabled
        {
            get => m_IsModuleEnabled;
        }

        private Engine touchlessEngine;

        Transform handTrackingAnchor;

        Transform mainCamera;

        Matrix4x4 hand_local_2_world;

        /// <summary>
        /// Raw hand track info by native plugin.
        /// </summary>
        HandTrackingInfo handTrackInfo;

        /// <summary>
        /// 上一个合法帧的hand track info.
        /// </summary>
        HandTrackingInfo previousValidFrameHandTrackInfo;

        /// <summary>
        /// Gets the hand track info
        /// </summary>
        public HandTrackingInfo HandleTrackInfo
        {
            get => handTrackInfo;
        }

        public bool IsTrackings
        {
            get => handTrackInfo.IsTracking;
        }

        private readonly object lockObj = new object();
        GestureEvent locked_gestureEvent;

        public void DisableModule()
        {
#if DEVELOPMENT_BUILD
            try
            {
#endif
            if (!m_IsModuleEnabled)
            {
                return;
            }
            m_IsModuleEnabled = false;
            handTrackInfo.Dispose();
            previousValidFrameHandTrackInfo.Dispose();
                //cameraTexture.Stop();
                //cameraTexture = null;
                XimmerseXR.RequestStopRGBCamera();
            touchlessEngine = null;
            imageHandle.Free();

#if DEVELOPMENT_BUILD
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }

        /// <summary>
        /// A 3 x 3 float array to calibrate RGB camera.
        /// </summary>
        static float[,] CalibrationMatrix = null;

        /// <summary>
        /// A 8 array to identify the distortion coefficients of the RGB camera.
        /// </summary>
        static float[] DistortionCoefficients = null;

        /// <summary>
        /// XML file path contains the rgb camera parameter.
        /// </summary>
        const string kRGBCalibrationXMLPath = "/backup/rgb_vio_camera_params.xml";

        static void ParseRGBCameraParams()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
                xmlDoc.Load(kRGBCalibrationXMLPath); // Load the XML document from the specified file
                XmlNodeList RGBCamMats = xmlDoc.GetElementsByTagName("RGBCamMat");
                ParseCalibrationMatrix(RGBCamMats, out CalibrationMatrix);
                XmlNodeList RGBDistCoeff = xmlDoc.GetElementsByTagName("RGBDistCoeff");
                ParseDistortionCoefficients(RGBDistCoeff, out DistortionCoefficients);

#if DEVELOPMENT_BUILD
                StringBuilder buffer = new StringBuilder();
                buffer.AppendFormat("CalibrationMatrix : ");
                for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 3; col++)
                    {
                        buffer.AppendFormat("{0} ", CalibrationMatrix[col, row]);
                    }


                buffer.AppendFormat("\r\nDistortionCoefficients : ");
                foreach (var coefficient in DistortionCoefficients)
                {
                    buffer.AppendFormat("{0} ", coefficient);
                }
#endif
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("ParseRGBCameraParams error : {0}", exc.Message);
                Debug.LogException(exc);
            }
        }


        /// <summary>
        /// Reads XML element and output a calibrationMatrix (3x3) array
        /// </summary>
        /// <param name="RGBCamMats"></param>
        /// <param name="calibrationMatrix"></param>
        private static void ParseCalibrationMatrix(XmlNodeList RGBCamMats, out float[,] calibrationMatrix)
        {
            calibrationMatrix = new float[3, 3];
            int row = 0, col = 0;
            try
            {
                XmlElement dataElement = RGBCamMats.Item(0)["data"];
                var textInArray = dataElement.InnerText.Trim().Split(' ');
                for (int i = 0; i < textInArray.Length; i++)
                {
                    string t = textInArray[i];
                    if (float.TryParse(t, out float value))
                    {
                        calibrationMatrix[row, col] = value;
                        col++;
                        if (col >= 3)
                        {
                            col = 0;
                            row++;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
        }

        /// <summary>
        /// Reads XML element and output a distortion cofficient (8) array
        /// </summary>
        /// <param name="RGBCamMats"></param>
        /// <param name="DistortionCofficients"></param>
        private static void ParseDistortionCoefficients(XmlNodeList RGBDistCoeff, out float[] DistortionCofficients)
        {
            DistortionCofficients = new float[8];
            int col = 0;
            try
            {
                XmlElement dataElement = RGBDistCoeff.Item(0)["data"];
                var textInArray = dataElement.InnerText.Trim().Split(' ');
                for (int i = 0; i < textInArray.Length; i++)
                {
                    string t = textInArray[i];
                    if (float.TryParse(t, out float value))
                    {
                        DistortionCofficients[col] = value;
                        col++;
                        if (col >= 8)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
        }

        public bool EnableModule(InitializeHandTrackingModuleParameter initParameter)
        {
            if (DistortionCoefficients == null || CalibrationMatrix == null)
            {
                ParseRGBCameraParams();
            }
#if DEVELOPMENT_BUILD
            try
            {
#endif
            if (m_IsModuleEnabled)
            {
                Debug.Log("HandTrackingT3D module already activate.");
                return false;
            }
            handTrackingAnchor = initParameter.TrackingAnchor;
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }

                //WebCamDevice[] devices = WebCamTexture.devices;
                //if (devices.Length == 0)
                //    return false;
                //for (int i = 0; i < devices.Length; i++)
                //{
                //    var curr = devices[i];
                //    if (curr.isFrontFacing == true)
                //    {
                //        //RhinoX using front facing camera
                //        cameraTexture = new WebCamTexture(curr.name, width, height, 60);
                //        break;
                //    }
                //}

                //cameraTexture.Play();
                XimmerseXR.RequestOpenRGBCamera(width, height);
                cameraTexture = XimmerseXR.RGBCameraTexture;

            imageHandle = GCHandle.Alloc(imgData, GCHandleType.Pinned);
            string uniqueID = SystemInfo.deviceUniqueIdentifier;
            string storageLocation = Application.persistentDataPath;
            //var calibration = new NativeCalibration();
            //var calibration = new Calibration(cameraTexture.width, cameraTexture.height,
            //    CalibrationMatrix, DistortionCoefficients);
            var calibration = new Calibration(1440, 1080, CalibrationMatrix, DistortionCoefficients);
            //var calibration = new Calibration(width * 4f, height * 4f, CalibrationMatrix, DistortionCoefficients);
            touchlessEngine = new Engine(uniqueID, storageLocation, calibration, OnTouchlessEvent);

            handTrackInfo = new HandTrackingInfo()
            {
                ThumbFinger = new RawFingerTrackingInfo(3)
                {
                    bendnessRangeMin = 0.7f,
                    bendnessRangeMax = 0.9f,
                },
                IndexFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
                MiddleFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
                RingFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
                LittleFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
            };

            previousValidFrameHandTrackInfo = new HandTrackingInfo()
            {
                ThumbFinger = new RawFingerTrackingInfo(3)
                {
                    bendnessRangeMin = 0.7f,
                    bendnessRangeMax = 0.9f,
                },
                IndexFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
                MiddleFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
                RingFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
                LittleFinger = new RawFingerTrackingInfo(4)
                {
                    bendnessRangeMin = 0.1f,
                    bendnessRangeMax = 0.9f,
                },
            };

            m_IsModuleEnabled = true;

            Debug.Log("T3D hand tracking is now active.");

            return true;

#if DEVELOPMENT_BUILD
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
#endif
        }

        private bool isvalid;
        private bool isTracking;
        int frame = 0;
        public void Optimize()
        {
            isvalid = handTrackInfo.IsValid;
            if (isvalid)
            {
                frame = 0;
                handTrackInfo.IsTracking = isvalid;
            }
            else
            {
                frame++;
                if (frame > 5)
                {
                    handTrackInfo.IsTracking = isvalid;
                }
            }
        }

        /// <summary>
        /// Call per frame , in main thread.
        /// </summary>
        public void Tick()
        {
#if DEVELOPMENT_BUILD
            try
            {
#endif
            //备份当前帧
            if (handTrackInfo.IsValid)
            {
                previousValidFrameHandTrackInfo.CopyFrom(handTrackInfo);
            }
            //对于50ms之前的合法帧，由于时间过长， 需要抛弃
            else if (previousValidFrameHandTrackInfo.IsValid)
            {
                if (new TimeSpan(handTrackInfo.Timestamp - previousValidFrameHandTrackInfo.Timestamp).TotalMilliseconds > 50d)
                {
                    previousValidFrameHandTrackInfo.IsValid = false;
                }
            }

            handTrackInfo.IsValid = false;//reset hand track info before native plugin feedback

            if (!m_IsModuleEnabled || null == cameraTexture || !cameraTexture.didUpdateThisFrame)
            {
                return;
            }

            //GestureEvent syncedEvent = null;
            lock (lockObj)
            {
                if (locked_gestureEvent != null)
                {
                    ParseGestureEvent(locked_gestureEvent);
                    //syncedEvent = new GestureEvent(locked_gestureEvent);
                    //locked_gestureEvent = null;
                }
            }
            Optimize();
            ////Debug.LogFormat("HandTrackingT3D.Tick : {0}, {1}", syncedEvent != null, syncedEvent != null && syncedEvent.skeletonValid);
            //if (null != syncedEvent)
            //{
            //    ParseGestureEvent(locked_gestureEvent);
            //}

            //update the tracking anchor local to world matrix
            hand_local_2_world = handTrackingAnchor.localToWorldMatrix;
            cameraTexture.GetPixels32(imgData);
            using (var frame = new Frame(imageHandle.AddrOfPinnedObject(), cameraTexture.width * 4, cameraTexture.width, cameraTexture.height, System.DateTimeOffset.Now.ToUnixTimeMilliseconds(), FrameRotation.ROTATION_NONE, true))
            {
                touchlessEngine.handleFrame(frame);
            }
#if DEVELOPMENT_BUILD
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }

        /// <summary>
        /// Convert T3D skeleton to hand track info : 
        /// </summary>
        /// <param name="args"></param>
        private void ParseGestureEvent(GestureEvent args)
        {
            Skeleton t3dSkel = args.skeleton;

            if (handTrackInfo.IsValid && (!args.skeletonValid || t3dSkel == null))
            {
                Debug.Log("Hand tracking become invalid !");
            }

            if (t3dSkel == null)
            {
                return;
            }

            handTrackInfo.IsValid = args.skeletonValid;


            handTrackInfo.Timestamp = System.DateTime.Now.Ticks;
            //update raw hand tracking info data
            handTrackInfo.Handness = args.handedness == HandednessType.LEFT_HAND ? HandnessType.Left : HandnessType.Right;


            handTrackInfo.ThumbFinger.Positions[0] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.THUMB2]);
            handTrackInfo.ThumbFinger.Positions[1] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.THUMB3]);
            handTrackInfo.ThumbFinger.Positions[2] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.THUMB4]);


            handTrackInfo.IndexFinger.Positions[0] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.INDEX1]);
            handTrackInfo.IndexFinger.Positions[1] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.INDEX2]);
            handTrackInfo.IndexFinger.Positions[2] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.INDEX3]);
            handTrackInfo.IndexFinger.Positions[3] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.INDEX4]);


            handTrackInfo.MiddleFinger.Positions[0] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.MIDDLE1]);
            handTrackInfo.MiddleFinger.Positions[1] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.MIDDLE2]);
            handTrackInfo.MiddleFinger.Positions[2] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.MIDDLE3]);
            handTrackInfo.MiddleFinger.Positions[3] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.MIDDLE4]);


            handTrackInfo.RingFinger.Positions[0] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.RING1]);
            handTrackInfo.RingFinger.Positions[1] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.RING2]);
            handTrackInfo.RingFinger.Positions[2] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.RING3]);
            handTrackInfo.RingFinger.Positions[3] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.RING4]);


            handTrackInfo.LittleFinger.Positions[0] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.PINKY1]);
            handTrackInfo.LittleFinger.Positions[1] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.PINKY2]);
            handTrackInfo.LittleFinger.Positions[2] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.PINKY3]);
            handTrackInfo.LittleFinger.Positions[3] = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.PINKY4]);
            handTrackInfo.UpdateProperties();
            //Debug.LogFormat("bendness thumb = {0}, index = {1}, middle = {2}, ring = {3}, little = {4}", handTrackInfo.ThumbFinger.bendness, handTrackInfo.IndexFinger.bendness,
            //handTrackInfo.MiddleFinger.bendness, handTrackInfo.RingFinger.bendness, handTrackInfo.LittleFinger.bendness);

            Vector3 wristPos = hand_local_2_world.MultiplyPoint3x4(t3dSkel.points[SkeletonPointsID.WRIST]);
            Vector3 wristToRing = handTrackInfo.RingFinger.Positions[0] - wristPos;
            Vector3 wristToMiddle = handTrackInfo.MiddleFinger.Positions[0] - wristPos;
            Vector3 ringToMiddle = handTrackInfo.MiddleFinger.Positions[0] - handTrackInfo.RingFinger.Positions[0];

            handTrackInfo.PalmPosition = wristPos + wristToRing * 0.45f + ringToMiddle * 0.43f;
            handTrackInfo.PalmScale = Vector3.one * wristToMiddle.magnitude * 0.86f;
            if (previousValidFrameHandTrackInfo.IsValid)
            {
                handTrackInfo.PalmDeltaPosition = handTrackInfo.PalmPosition - previousValidFrameHandTrackInfo.PalmPosition;
                handTrackInfo.PalmVelocity = handTrackInfo.PalmDeltaPosition / (float)new TimeSpan(handTrackInfo.Timestamp - previousValidFrameHandTrackInfo.Timestamp).TotalSeconds;
            }

            Vector3 crs = Vector3.Cross(wristToRing, wristToMiddle);
            //Make palm normal always facing UP upon the palm surface:
            if (args.handedness == HandednessType.LEFT_HAND)
            {
                crs = -crs;
            }
            handTrackInfo.PalmRotation = Quaternion.LookRotation(wristToRing, crs);
            handTrackInfo.PalmNormal = crs.normalized;

            //Calculate  local position to main camera:
            if (!mainCamera)
            {
                mainCamera = Camera.main.transform;
            }

            if (mainCamera && mainCamera.parent)
            {
                if (mainCamera.parent)
                {
                    handTrackInfo.PalmLocalPosition = mainCamera.parent.InverseTransformPoint(handTrackInfo.PalmPosition);
                    handTrackInfo.PalmLocalRotation = Quaternion.Inverse(mainCamera.parent.rotation) * handTrackInfo.PalmRotation;
                    handTrackInfo.PalmLocalNormal = mainCamera.parent.InverseTransformVector(handTrackInfo.PalmNormal);
                }
                else
                {
                    handTrackInfo.PalmLocalPosition = mainCamera.InverseTransformPoint(handTrackInfo.PalmPosition);
                    handTrackInfo.PalmLocalRotation = Quaternion.Inverse(mainCamera.rotation) * handTrackInfo.PalmRotation;
                    handTrackInfo.PalmLocalNormal = mainCamera.InverseTransformVector(handTrackInfo.PalmNormal);
                }
            }

            //Debug.LogFormat("Get valid hand track frame at time: {0}, is valid: {3} palm point: {1}, thumb point: {2}",
            //    handTrackInfo.Timestamp, handTrackInfo.PalmPosition, handTrackInfo.ThumbFinger.Positions[0], handTrackInfo.IsValid);

            handTrackInfo.NativeGestureType = (int)args.type;

            //Update gesture enum:
            if (handTrackInfo.IsValid == false)
            {
                handTrackInfo.gestureFistOpenHand = GestureType_Fist_OpenHand.None;
                handTrackInfo.gestureGrisp = GestureType_Grisp.None;
                handTrackInfo.NativeGestureType = -1;
            }
            else
            {
                //gesture type of open hand / grisp : 
                if (handTrackInfo.NativeGestureType == (int)TouchlessA3D.GestureType.HAND || handTrackInfo.NativeGestureType == (int)TouchlessA3D.GestureType.OPEN_HAND)
                {
                    handTrackInfo.gestureFistOpenHand = GestureType_Fist_OpenHand.Opened;
                    handTrackInfo.gestureGrisp = GestureType_Grisp.GrispOpen;
                }

                //Fist - close hand and grasp clsoed
                if (handTrackInfo.NativeGestureType == (int)TouchlessA3D.GestureType.CLOSED_HAND)
                {
                    handTrackInfo.gestureFistOpenHand = GestureType_Fist_OpenHand.Fist;
                    handTrackInfo.gestureGrisp = GestureType_Grisp.GraspClosed;
                }

                //Grisp pinch:
                if (handTrackInfo.NativeGestureType == (int)TouchlessA3D.GestureType.CLOSED_PINCH)
                {
                    handTrackInfo.gestureGrisp = GestureType_Grisp.GraspClosed;
                }
            }

        }

        /// <summary>
        /// Callback on native event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTouchlessEvent(object sender, GestureEvent args)
        {
            //Debug.LogFormat("OnTouchlessEvent : {0}", args.skeletonValid);
            lock (lockObj)
            {
                locked_gestureEvent = args;
            }
        }
    }
}