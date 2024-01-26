using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Ximmerse.XR.InputSystems
{
    public struct MarkerController : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('X', 'M');

        /// <summary>
        /// left
        /// </summary>
        [InputControl(name = "isLeftTriggerButton", layout = "Button")]
        public bool isLeftTriggerButton;

        [InputControl(name = "isLeftTouchButton", layout = "Button")]
        public bool isLeftTouchButton;

        [InputControl(name = "isLeftAppButton", layout = "Button")]
        public bool isLeftAppButton;

        [InputControl(name = "isLeftHomeButton", layout = "Button")]
        public bool isLeftHomeButton;

        [InputControl(name = "LeftTriggerValue", layout = "Axis")]
        public float LeftTriggerValue;

        /// <summary>
        /// right
        /// </summary>
        [InputControl(name = "isRightTriggerButton", layout = "Button")]
        public bool isRightTriggerButton;

        [InputControl(name = "isRightTouchButton", layout = "Button")]
        public bool isRightTouchButton;

        [InputControl(name = "isRightAppButton", layout = "Button")]
        public bool isRightAppButton;

        [InputControl(name = "isRightHomeButton", layout = "Button")]
        public bool isRightHomeButton;

        [InputControl(name = "RightTriggerValue", layout = "Axis")]
        public float RightTriggerValue;


    }
    [InputControlLayout(stateType = typeof(MarkerController))]
    public class MarkerControllerInput : InputDevice, IInputUpdateCallbackReceiver
    {
        /// <summary>
        /// left
        /// </summary>
        [InputControl(name = "MarkerController/isLeftTriggerButton")]
        public ButtonControl isLeftTriggerButton
        {
            get; internal set;
        }

        [InputControl(name = "MarkerController/isLeftTouchButton")]
        public ButtonControl isLeftTouchButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/isLeftAppButton")]
        public ButtonControl isLeftAppButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/isLeftHomeButton")]
        public ButtonControl isLeftHomeButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/LeftTriggerValue")]
        public AxisControl LeftTriggerValue
        {
            get; internal set;
        }

        /// <summary>
        /// right
        /// </summary>
        [InputControl(name = "MarkerController/isRightTriggerButton")]
        public ButtonControl isRightTriggerButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/isRightTouchButton")]
        public ButtonControl isRightTouchButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/isRightAppButton")]
        public ButtonControl isRightAppButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/isRightHomeButton")]
        public ButtonControl isRightHomeButton
        {
            get; internal set;
        }
        [InputControl(name = "MarkerController/RightTriggerValue")]
        public AxisControl RightTriggerValue
        {
            get; internal set;
        }


        public MarkerControllerInput() : base()
        {
            displayName = "Ximmerse Marker Controller Input Device";
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            isLeftTriggerButton = GetChildControl<ButtonControl>("isLeftTriggerButton");
            isLeftTouchButton = GetChildControl<ButtonControl>("isLeftTouchButton");
            isLeftAppButton = GetChildControl<ButtonControl>("isLeftAppButton");
            isLeftHomeButton = GetChildControl<ButtonControl>("isLeftHomeButton");
            LeftTriggerValue = GetChildControl<AxisControl>("LeftTriggerValue");

            isRightTriggerButton = GetChildControl<ButtonControl>("isLeftTriggerButton");
            isRightTouchButton = GetChildControl<ButtonControl>("isLeftTriggerButton");
            isRightAppButton = GetChildControl<ButtonControl>("isLeftTriggerButton");
            isRightHomeButton = GetChildControl<ButtonControl>("isLeftTriggerButton");
            RightTriggerValue = GetChildControl<AxisControl>("isLeftTriggerButton");

        }

        public void OnUpdate()
        {
            var markerController = new MarkerController();
            GetAllButton();
            markerController.isLeftTriggerButton = isLeftTriggerButtonDown;
            markerController.isLeftTouchButton = isLeftTouchButtonDown;
            markerController.isLeftAppButton = isLeftAppButtonDown;
            markerController.isLeftHomeButton = isLeftHomeButtonDown;
            markerController.LeftTriggerValue = LeftTrigger/255;

            markerController.isRightTriggerButton = isRightTriggerButtonDown;
            markerController.isRightTouchButton = isRightTouchButtonDown;
            markerController.isRightAppButton = isRightAppButtonDown;
            markerController.isRightHomeButton = isRightHomeButtonDown;
            markerController.RightTriggerValue = RightTrigger/255;


            InputSystem.QueueStateEvent(this, markerController);
        }

        private bool isLeftTriggerButtonDown;

        private bool isLeftTouchButtonDown;

        private bool isLeftAppButtonDown;

        private bool isLeftHomeButtonDown;

        private int LeftTrigger;

        private bool isRightTriggerButtonDown;

        private bool isRightTouchButtonDown;

        private bool isRightAppButtonDown;

        private bool isRightHomeButtonDown;

        private int RightTrigger;

        public void GetAllButton()
        {
#if !UNITY_EDITOR
            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(0)) == 32)
            {
                isLeftTriggerButtonDown = true;
                Debug.Log("isTriggerButtonDown +32");
            }
            else
            {
                isLeftTriggerButtonDown = false;
            }

            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(0)) == 4)
            {
                isLeftTouchButtonDown = true;
                Debug.Log("isTouchButtonDown +4");
            }
            else
            {
                isLeftTouchButtonDown = false;
            }

            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(0)) == 16)
            {
                isLeftAppButtonDown = true;
                Debug.Log("isAppButtonDown +16");
            }
            else
            {
                isLeftAppButtonDown = false;
            }

            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(0)) == 8)
            {
                isLeftHomeButtonDown = true;
                Debug.Log("isHomeButtonDown +8");
            }
            else
            {
                isLeftHomeButtonDown = false;
            }

            LeftTrigger = XDevicePlugin.xdevc_ctrl_get_trigger(XDevicePlugin.xdevc_get_controller(0));



            //right
            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(1)) == 32)
            {
                isRightTriggerButtonDown = true;
                Debug.Log("isTriggerButtonDown +32");
            }
            else
            {
                isRightTriggerButtonDown = false;
            }

            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(1)) == 4)
            {
                isRightTouchButtonDown = true;
                Debug.Log("isTouchButtonDown +4");
            }
            else
            {
                isRightTouchButtonDown = false;
            }

            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(1)) == 16)
            {
                isRightAppButtonDown = true;
                Debug.Log("isAppButtonDown +16");
            }
            else
            {
                isRightAppButtonDown = false;
            }

            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller(1)) == 8)
            {
                isRightHomeButtonDown = true;
                Debug.Log("isHomeButtonDown +8");
            }
            else
            {
                isRightHomeButtonDown = false;
            }

            RightTrigger = XDevicePlugin.xdevc_ctrl_get_trigger(XDevicePlugin.xdevc_get_controller(1));
#endif
        }
    }

}

