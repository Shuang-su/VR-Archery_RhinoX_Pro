namespace Ximmerse.XR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.XR;
    using UnityEngine.XR.Management;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Reflection;
    using UnityEngine.XR.ARSubsystems;
    using System.IO;
    using Object = UnityEngine.Object;
    using UnityEngine.SceneManagement;
    using UnityEditor;
    using Ximmerse.XR.Tag;
    using System.Collections;


#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class XRloaderSetting
    {
        static XRloaderSetting()
        {
            GetSettings();
        }
        public static void GetSettings()
        {
            XimmerseXRSettings settings = null;
#if UNITY_EDITOR
            var exampleAsset = AssetDatabase.LoadAssetAtPath<XimmerseXRLoader>("Assets/XR/Loaders/Ximmerse XR Loader.asset");
            if (exampleAsset == null)
            {
                return;
            }
            if (exampleAsset.settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<XimmerseXRSettings>("Assets/XR/Settings/Ximmerse XR Settings.asset");
                exampleAsset.settings = settings;
                Debug.Log("Ximmerse XR Setting");
            }
            if (exampleAsset.settings == null)
            {
                Debug.LogError("Ximmerse XR loaderSetting is null");
            }
#endif
        }
    }


    /// <summary>
    /// XR Loader for Cardboard XR Plugin.
    /// Loads Display and Input Subsystems.
    /// </summary>
    [DefaultExecutionOrder(-20000)]
    public class XimmerseXRLoader : XRLoaderHelper
    {
        static NativePluginApi.DeviceType sDeviceType = NativePluginApi.DeviceType.Device_AUTO;

        private static List<XRDisplaySubsystemDescriptor> _displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();

        private static List<XRInputSubsystemDescriptor> _inputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

        public XimmerseXRSettings settings;



        internal static bool _isInitialized { get; private set; }

        internal static bool _isStarted { get; private set; }

        private const string kXimHMD = "Xim_HMD";
        private const string kLeftController = "LeftHand";
        private const string kRightController = "RightHand";

        private void Awake()
        {
            SetSDKVars();//set sdk vars at main thread
            if (!SDKVariants.IsSupported)
            {
                return;
            }
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            int _reticleTextureId = settings.displayReticle && settings.reticleTexture ? settings.reticleTexture.GetNativeTexturePtr().ToInt32() : -1;
            int _reticleTexW = settings.displayReticle && settings.reticleTexture ? settings.reticleTexture.width : -1;
            int _reticleTexH = settings.displayReticle && settings.reticleTexture ? settings.reticleTexture.height : -1;


            //Async op at daemon thread. 
            Task.Run(async () =>
            {
                Debug.Log("XR initialization thread starts...");
                try
                {
                    while (!SvrPluginAndroid.SvrIsRunning())
                    {
                        Thread.Sleep(10);
                    }
                    //Set reticle texture id:
                    Debug.Log("Daemon threads detect SVR system is started.");
                    if (settings.reticleTexture && settings.displayReticle)
                    {
                        XimmerseXR.DisplayReticle = true;
                        XimmerseXR.SetReticleTexture(_reticleTextureId, _reticleTexW, _reticleTexH);
                    }
                    else
                    {
                        XimmerseXR.DisplayReticle = false;
                    }
                    //Loads default tracking profile:
                    try
                    {
                        if (!ReferenceEquals(null, (System.Object)settings.defaultTrackingProfile))
                        {
                            //Load tracking profile : 
                            XimmerseXR.LoadTrackingProfile(this.settings.defaultTrackingProfile.trackingItems);
                        }
                    }
                    catch (System.Exception exc)
                    {
                        Debug.LogError("Default tracking profile not loaded.");
                        Debug.LogException(exc);
                    }
                    Debug.Log("Reticle set successfully.");
                    await Task.Delay(10);//delay a bit to start fusion:
                    if (SDKVariants.groundPlaneLayout != null && SDKVariants.groundPlaneLayout.IsValid())
                    {
                        XimmerseXR.LoadGroundPlaneLayout(SDKVariants.groundPlaneLayout);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("XR initialization exception: {0}, {1}", e.Message, e.StackTrace, e);
                    Debug.LogException(e);
                }
            });
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //Create XR manager:
            var xrMgr = new GameObject("XR Manager", typeof(XRManager));
            Object.DontDestroyOnLoad(xrMgr.gameObject);

            xrMgr.GetComponent<XRManager>().StartCoroutine(initializeDeivces());
            xrMgr.GetComponent<XRManager>().StartCoroutine(checkSRGBSupportIsNeeded());
            if (this.settings.HandTracking)
            {
                Ximmerse.XR.InputSystems.HandTracking.EnableHandTracking();
            }

            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

        }

        /// <summary>
        /// Check sRGB supports needs one frame delay
        /// </summary>
        /// <returns></returns>
        IEnumerator checkSRGBSupportIsNeeded ()
        {
            yield return null;//waits one frame
            bool requiredSRGB = GraphicsSettings.currentRenderPipeline != null;
            if (requiredSRGB)
            {
                NativePluginApi.Unity_requiresRGBEyeBuffer(true); //require sRGB supports in URP pipeline.
            }
        }

        /// <summary>
        /// Init XR devices : HMD , left and right controller.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator initializeDeivces()
        {
            bool hmdDeviceEnabled = false;
            bool leftHandEnabled = false;
            bool rightHandEnabled = false;
            UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDevicesChanged;
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDevicesChanged;
            while (true)
            {
                if (hmdDeviceEnabled && leftHandEnabled && rightHandEnabled)
                {
                    yield break;
                }
                for (int i = 0; i < UnityEngine.InputSystem.InputSystem.devices.Count; i++)
                {
                    UnityEngine.InputSystem.InputDevice device = UnityEngine.InputSystem.InputSystem.devices[i];
                    if (!hmdDeviceEnabled && device.name == kXimHMD && !device.enabled)
                    {
                        UnityEngine.InputSystem.InputSystem.EnableDevice(device);
                        hmdDeviceEnabled = true;
                        Debug.Log("Ximmerse XR Loader : Enable Xim_HMD.");
                    }
                    if (!leftHandEnabled && device.name == kLeftController && !device.enabled)
                    {
                        UnityEngine.InputSystem.InputSystem.EnableDevice(device);
                        leftHandEnabled = true;
                        Debug.Log("Ximmerse XR Loader : Enable Left Controller.");
                    }
                    if (!rightHandEnabled && device.name == kRightController && !device.enabled)
                    {
                        UnityEngine.InputSystem.InputSystem.EnableDevice(device);
                        rightHandEnabled = true;
                        Debug.Log("Ximmerse XR Loader : Enable Right Controller.");
                    }
                }

                yield return null;
            }


#if DEVELOPMENT_BUILD
            for (int i = 0; i < UnityEngine.InputSystem.InputSystem.devices.Count; i++)
            {
                UnityEngine.InputSystem.InputDevice device = UnityEngine.InputSystem.InputSystem.devices[i];
                System.Text.StringBuilder buffer = new System.Text.StringBuilder();
                if (device.name == kLeftController || device.name == kRightController || device.name == kXimHMD)
                {
                    var controls = device.allControls;
                    for (int j = 0; j < controls.Count; j++)
                    {
                        buffer.Clear();
                        UnityEngine.InputSystem.InputControl ctrl = controls[j];
                        foreach (var usage in ctrl.usages)
                        {
                            buffer.Append(usage.ToString() + ' ');
                        }
                        Debug.LogFormat("Xim device layout, name = {0}, path = {1}, value type = {2}, usage = {3}, device type = {4}", ctrl.name, ctrl.path, ctrl.valueType, buffer.ToString(), device.GetType());
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Enables devices after reconnection.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnDevicesChanged(UnityEngine.InputSystem.InputDevice arg1, UnityEngine.InputSystem.InputDeviceChange arg2)
        {
            if (arg2 == UnityEngine.InputSystem.InputDeviceChange.Reconnected && arg1.name == kXimHMD && !arg1.enabled)
            {
                UnityEngine.InputSystem.InputSystem.EnableDevice(arg1);
            }
            if (arg2 == UnityEngine.InputSystem.InputDeviceChange.Reconnected && arg1.name == kLeftController && !arg1.enabled)
            {
                UnityEngine.InputSystem.InputSystem.EnableDevice(arg1);
            }
            if (arg2 == UnityEngine.InputSystem.InputDeviceChange.Reconnected && arg1.name == kRightController && !arg1.enabled)
            {
                UnityEngine.InputSystem.InputSystem.EnableDevice(arg1);
            }

        }

        public override bool Initialize()
        {
            SDKInitialize();
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(_displaySubsystemDescriptors, "XmsXRDisplay");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(_inputSubsystemDescriptors, "XmsXRInput");
            Debug.LogFormat("XRLoader.Initialize, settings: {0}, SubSystems has been created.", settings.name);
#if UNITY_INPUT_SYSTEM
            Ximmerse.XR.InputSystems.InputLayout.RegisterInputLayouts();
#endif

            UnityEngine.InputSystem.InputSystem.onDeviceChange += (device, changeEvent) =>
            {
                Debug.LogFormat("Control inputSystem device event : {0}, device : {1}, {2}, {3}, {4}", changeEvent, device.name, device.displayName, device.deviceId, device);
            };
            _isInitialized = true;
            return true;
        }

        public override bool Start()
        {
            Debug.Log("[XRLoader] ==>Start");
            if (Application.platform == RuntimePlatform.Android)
            {
                if (sDeviceType == NativePluginApi.DeviceType.Device_AUTO)
                {
                    SvrPlugin.deviceModel = SvrPlugin.DeviceModel.Default;
                    int trackingMode = (int)SvrPlugin.TrackingMode.kTrackingOrientation | (int)SvrPlugin.TrackingMode.kTrackingPosition;
                    NativePluginApi.Unity_setTrackingMode(trackingMode);
                    NativePluginApi.Unity_setVsync(1);
                    //SvrPlugin.Instance.SetTrackingMode(trackingMode);
                    //SvrPlugin.Instance.SetVSyncCount(1);
                    SvrPlugin.Instance.BeginVr(3, 3, 0x0);

                }
            }

            StartSubsystem<XRDisplaySubsystem>();
            StartSubsystem<XRInputSubsystem>();
            // StartSubsystem<RhinoXGroundPlaneSubSystem>();

            _isStarted = true;
            return true;
        }

        public override bool Stop()
        {
            Debug.Log("[XRLoader] ==>Stop");
            StopSubsystem<XRDisplaySubsystem>();
            StopSubsystem<XRInputSubsystem>();
            //  StopSubsystem<RhinoXGroundPlaneSubSystem>();

            _isStarted = false;
            return true;
        }

        public override bool Deinitialize()
        {
            Debug.Log("[XRLoader] ==>Deinitialize");
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRInputSubsystem>();
            // DestroySubsystem<RhinoXGroundPlaneSubSystem>();

            _isInitialized = false;
            return true;
        }

        private void SDKInitialize()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                NativePluginApi.Unity_setDeviceType((int)sDeviceType);

                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var context = activity.Call<AndroidJavaObject>("getApplicationContext");
               //NativePluginApi.Unity_setRenderResolution(2160, 2160);
                NativePluginApi.Unity_initializeAndroid(activity.GetRawObject());

                switch (SystemInfo.graphicsDeviceType)
                {
                    case GraphicsDeviceType.OpenGLES2:
                        NativePluginApi.Unity_setGraphicsApi(NativePluginApi.GraphicsApi.kOpenGlEs2);
                        break;
                    case GraphicsDeviceType.OpenGLES3:
                        NativePluginApi.Unity_setGraphicsApi(NativePluginApi.GraphicsApi.kOpenGlEs3);
                        break;
                    default:
                        Debug.LogErrorFormat(
                          "The Ximmerse XR Plugin cannot be initialized given that the selected " +
                          "Graphics API ({0}) is not supported. Please use OpenGL ES 2.0, " +
                          "OpenGL ES 3.0 or Metal.", SystemInfo.graphicsDeviceType);
                        break;
                }

                NativePluginApi.Unity_setScreenParams((int)Screen.width, (int)Screen.height, (int)Screen.safeArea.x, (int)Screen.safeArea.y, (int)Screen.safeArea.width, (int)Screen.safeArea.height);
                return;
            }
        }


        private void SetSDKVars()
        {
            SDKVariants.kTrackingDataDir_Internal = Application.persistentDataPath;

            SDKVariants.IsSupported = Application.platform == RuntimePlatform.Android; //todo : verify on android phone

            SDKVariants.groundPlaneLayout = settings.defaultGroundPlaneLayoutConfig ? settings.defaultGroundPlaneLayoutConfig.layout : default(GroundPlaneLayout);

            SDKVariants.TrackingAnchor = Matrix4x4.TRS(SDKVariants.kVPU_Shift, Quaternion.Euler(SDKVariants.kVPU_TiltEuler), Vector3.one);

            SDKVariants.DrawTrackedMarkerGizmos = settings.DrawTrackedMarkerGizmos;

            SDKVariants.DrawDetailTrackedInfo = settings.DrawDetailTrackedInfo;
        }
    }
}
