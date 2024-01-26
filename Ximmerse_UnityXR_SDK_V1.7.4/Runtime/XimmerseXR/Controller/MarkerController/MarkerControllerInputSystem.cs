using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Ximmerse.XR.InputSystems
{
    public class MarkerControllerInputSystem : MonoBehaviour
    {
        public static MarkerControllerInput ximmerseMarkerControllerInputDevice
        {
            get; private set;
        }

        public void EnabelMarkerController()
        {
            if (ximmerseMarkerControllerInputDevice == null)
            {
                //Adds a virtural input device for gesture input:
                MarkerControllerInput MarkerControllerInputDevice = (MarkerControllerInput)InputSystem.AddDevice(new InputDeviceDescription
                {
                    interfaceName = "MarkerController",
                });
                ximmerseMarkerControllerInputDevice = MarkerControllerInputDevice;
            }
        }
        static bool IsHeadsetDeviceLayoutRegistered = false;
        private static void RegisterXRCameraPointLayout()
        {
            InputSystem.RegisterLayout<MarkerControllerInput>(matches: new InputDeviceMatcher()
                .WithInterface("MarkerController"));
            IsHeadsetDeviceLayoutRegistered = true;
        }

        private void Start()
        {
            RegisterXRCameraPointLayout();
            EnabelMarkerController();
        }

        private void OnDestroy()
        {
            if (ximmerseMarkerControllerInputDevice != null)
            {
                InputSystem.RemoveDevice(ximmerseMarkerControllerInputDevice);
            }
        }
    }

}
