using System;
using System.Collections;
using System.Collections.Generic;
using Ximmerse.Wrapper.XDeviceService;
using System.Runtime.InteropServices;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ximmerse.Wrapper.XDeviceService.Client;
using static Ximmerse.XR.XDevicePlugin;

namespace Ximmerse.XR
{
    public class XRController3Dof : MonoBehaviour
    {
        #region Property
        enum ControllerIndex
        {
            Left = 0,
            Right = 1
        }
        [SerializeField]
        private ControllerIndex controller3Dof;
        private ClientController clientController;
        private XConnectionStates xrControllerState;
        private XRBaseController xrController;
        private XAttrControllerState _ControllerSate;
        private XRRayInteractor xrRay;
        private Quaternion imuQ;

        [SerializeField] private Vector3 _recentPosition;

        public ClientController XRController
        {
            get => clientController;
        }
        public Vector3 RecentPosition
        {
            get => _recentPosition;
            set => _recentPosition = value;
        }
        #endregion

        #region Unity
        void Start()
        {
#if !UNITY_EDITOR
            clientController = new ClientController((int)controller3Dof);
#endif
            xrController = GetComponent<XRBaseController>();
            xrRay = GetComponent<XRRayInteractor>();
        }

        void Update()
        {
#if !UNITY_EDITOR
            UpdateController();
#endif
        }
        #endregion

        #region Method

        private void UpdateController()
        {
            xrControllerState = clientController.GetConnectState();
            if (xrControllerState == XConnectionStates.kXConnSt_Connected)
            {
                _ControllerSate = clientController.GetControllerState();
                if (controller3Dof==ControllerIndex.Left)
                {
                    imuQ = Quaternion.Euler(new Vector3(-_ControllerSate.euler[0], -_ControllerSate.euler[1], _ControllerSate.euler[2]));
                }
                else
                {
                    imuQ = Quaternion.Euler(new Vector3(-_ControllerSate.euler[0], -_ControllerSate.euler[1], _ControllerSate.euler[2]));
                }
                
                if (xrController != null)
                {
                    xrController.transform.localRotation = imuQ;
                    Debug.Log("Controller is 3dof");
                }
                else
                {
                    throw new Exception("xrController is null");
                }
            }
        }

        public void RecenterController()
        {
            xrRay.rayOriginTransform.rotation = Quaternion.LookRotation(_recentPosition - xrRay.rayOriginTransform.position);
        }
        #endregion
    }
}

