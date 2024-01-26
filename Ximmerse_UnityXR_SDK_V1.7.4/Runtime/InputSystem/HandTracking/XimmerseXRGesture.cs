using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using Ximmerse.XR.InputSystems.GazeAndGestureInteraction;

namespace Ximmerse.XR.InputSystems
{
    /// <summary>
    /// gesture state for input system to couple with input action properties system.
    /// </summary>
    public struct HandState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('X', 'M', 'G', 'T');

        #region Properties represents any hand

        [InputControl(name = "anyHandIsTracked", layout = "Button")]
        public bool anyHandIsTracked;

        /// <summary>
        /// 0 = left, 1 = right
        /// </summary>
        [InputControl(name = "handness", layout = "Integer")]
        public int handness;

        /// <summary>
        /// 0 = none, 3 (1+2) = position + rotation
        /// </summary>
        [InputControl(name = "trackingState", layout = "Integer")]
        public uint trackingState;

        /// <summary>
        /// Palm position
        /// </summary>
        [InputControl(name = "palmPosition", layout = "Vector3")]
        public Vector3 palmPosition;

        [InputControl(name = "gazeRayState", layout = "Integer")]
        public uint gazeRayState;

        [InputControl(name = "gazeRayOrigin", layout = "Vector3")]
        public Vector3 gazeRayOrigin;

        [InputControl(name = "gazeRayRotation", layout = "Quaternion")]
        public Quaternion gazeRayRotation;

        /// <summary>
        /// 手掌沿手指前向的法线
        /// </summary>
        [InputControl(name = "palmRotation", layout = "Quaternion")]
        public Quaternion palmRotation;

        [InputControl(name = "palmScale", layout = "Vector3")]
        public Vector3 palmScale;

        /// <summary>
        /// 掌心向上法线
        /// </summary>
        [InputControl(name = "palmNormal", layout = "Vector3")]
        public Vector3 palmNormal;

        /// <summary>
        /// 手势类型 : 抓取
        /// </summary>
        [InputControl(name = "isGrasp", layout = "Button")]
        public bool isGrasp;

        /// <summary>
        /// 手势类型 : 张开手掌
        /// </summary>
        [InputControl(name = "isOpenHand", layout = "Button")]
        public bool isOpenHand;

        /// <summary>
        /// 手势类型 : 握拳
        /// </summary>
        [InputControl(name = "isClosedHand", layout = "Button")]
        public bool isClosedHand;


        /// <summary>
        /// 手势类型 : 握拳
        /// </summary>
        [InputControl(name = "gripValue", layout = "Axis")]
        public float gripValue;

        #endregion

    }

    /// <summary>
    /// Hand gesture input device
    /// </summary>
    [InputControlLayout(stateType = typeof(HandState))]
    public class XimmerseXRGesture : InputDevice, IInputUpdateCallbackReceiver
    {
        [InputControl(name = "HandState/anyHandIsTracked")]
        public ButtonControl anyHandIsTracked
        {
            get; internal set;
        }

        /// <summary>
        /// Tracking state, the value mapped to UnityEngine.XR.InputTrackingState:
        /// None = 0, Position = 1, Rotation = 2,Velocity = 4,AngularVelocity = 8,
        /// Acceleration = 16,AngularAcceleration = 32, All = 63
        /// </summary>
        [InputControl(name = "HandState/trackingState")]
        public IntegerControl trackingState
        {
            get; internal set;
        }


        /// <summary>
        /// 0 = left, 1 = right
        /// </summary>
        [InputControl(name = "HandState/handness")]
        public IntegerControl handness
        {
            get; internal set;
        }

        [InputControl(name = "HandState/palmPosition")]
        public Vector3Control palmPosition
        {
            get; internal set;
        }

        [InputControl(name = "HandState/palmRotation")]
        public QuaternionControl palmRotation
        {
            get; internal set;
        }

        [InputControl(name = "HandState/palmScale")]
        public Vector3Control palmScale
        {
            get; internal set;
        }

        [InputControl(name = "HandState/gazeRayState")]
        public IntegerControl gazeRayState
        {
            get; internal set;
        }

        [InputControl(name = "HandState/gazeRayOrigin")]
        public Vector3Control gazeRayOrigin
        {
            get; internal set;
        }

        [InputControl(name = "HandState/gazeRayRotation")]
        public QuaternionControl gazeRayRotation
        {
            get; internal set;
        }

        [InputControl(name = "HandState/palmNormal")]
        public Vector3Control palmNormal
        {
            get; internal set;
        }

        [InputControl(name = "HandState/isGrasp")]
        public ButtonControl isGrasp
        {
            get; internal set;
        }

        [InputControl(name = "HandState/isOpenHand")]
        public ButtonControl isOpenHand
        {
            get; internal set;
        }

        [InputControl(name = "HandState/isClosedHand")]
        public ButtonControl isClosedHand
        {
            get; internal set;
        }

        [InputControl(name = "HandState/gripValue")]
        public AxisControl gripValue
        {
            get; internal set;
        }

        /// <summary>
        /// The native hand track info data.
        /// </summary>
        public HandTrackingInfo handTrackInfo
        {
            get => HandTracking.HandTrackingInfo;
        }

        public XimmerseXRGesture() : base()
        {
            displayName = "Ximmerse Gesture Input Device";
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();
            anyHandIsTracked = GetChildControl<ButtonControl>("anyHandIsTracked");
            handness = GetChildControl<IntegerControl>("handness");
            trackingState = GetChildControl<IntegerControl>("trackingState");
            palmPosition = GetChildControl<Vector3Control>("palmPosition");
            palmRotation = GetChildControl<QuaternionControl>("palmRotation");

            gazeRayState = GetChildControl<IntegerControl>("gazeRayState");
            gazeRayOrigin = GetChildControl<Vector3Control>("gazeRayOrigin");
            gazeRayRotation = GetChildControl<QuaternionControl>("gazeRayRotation");

            palmScale = GetChildControl<Vector3Control>("palmScale");
            palmNormal = GetChildControl<Vector3Control>("palmNormal");
            isGrasp = GetChildControl<ButtonControl>("isGrasp");
            isOpenHand = GetChildControl<ButtonControl>("isOpenHand");
            isClosedHand = GetChildControl<ButtonControl>("isClosedHand");
            gripValue = GetChildControl<AxisControl>("gripValue");
        }

        public void OnUpdate()
        {
            var _state = new HandState();
            GazeAndHandInteractionSystem gazeInterSys = GazeAndHandInteractionSystem.instance;
            var _handTrackInfo = this.handTrackInfo;

            _state.anyHandIsTracked = _handTrackInfo.IsValid;
            _state.handness = (int)_handTrackInfo.Handness;
            _state.trackingState = _handTrackInfo.IsValid ? 3u : 0u;
            _state.palmPosition = _handTrackInfo.PalmLocalPosition;
            _state.palmRotation = _handTrackInfo.PalmLocalRotation;

            GazeAndHandInteractionSystem.GetEyeReticleLocalPose(out Pose eyeReticlePose);
            _state.gazeRayState = 3u;
            _state.gazeRayOrigin = eyeReticlePose.position;
            _state.gazeRayRotation = eyeReticlePose.rotation;

            _state.palmNormal = _handTrackInfo.PalmLocalNormal;
            _state.palmScale = _handTrackInfo.PalmScale;

            //如果 gaze interaction system 存在， 采用其抛出的优化值:
            bool isclosepinch = _handTrackInfo.NativeGestureType == (int)TouchlessA3D.GestureType.CLOSED_PINCH;
            bool isclosehand = _handTrackInfo.gestureFistOpenHand == GestureType_Fist_OpenHand.Fist;
            if (_handTrackInfo.IsTracking)
            {
                if (isclosepinch || isclosehand)
                {
                    _state.isGrasp = true;
                }
                else
                {
                    _state.isGrasp = false;
                }
                _state.isOpenHand = _handTrackInfo.gestureFistOpenHand == GestureType_Fist_OpenHand.Opened;
                _state.isClosedHand = _handTrackInfo.gestureFistOpenHand == GestureType_Fist_OpenHand.Fist;
                _state.gripValue = _state.isGrasp ? 1 : 0;
            }
            else
            {
                _state.isGrasp = false;
                _state.isOpenHand = true;
                _state.isClosedHand = false;
                _state.gripValue = 0;
            }

            InputSystem.QueueStateEvent(this, _state);
        }
    }
}