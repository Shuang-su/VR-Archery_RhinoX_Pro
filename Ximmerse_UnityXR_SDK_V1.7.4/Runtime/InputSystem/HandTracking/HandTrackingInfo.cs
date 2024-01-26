using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using Unity.Jobs;

namespace Ximmerse.XR.InputSystems
{
    /// <summary>
    /// State the left / right hand
    /// </summary>
    public enum HandnessType : byte
    {
        Left = 0, Right = 1,
    }

    /// <summary>
    /// hand tracking info.
    /// </summary>
    [Serializable]
    public struct HandTrackingInfo : IDisposable
    {
        /// <summary>
        /// The hand tracking frame time stamp.
        /// </summary>
        public long Timestamp;

        /// <summary>
        /// is currently valid tracking state ?
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Currently tracking left or right hand. 
        /// For T3D, one hand per frame.
        /// </summary>
        public HandnessType Handness;

        /// <summary>
        /// Palm center world position
        /// </summary>
        public Vector3 PalmPosition;

        /// <summary>
        /// The palm delta position in world space.
        /// </summary>
        public Vector3 PalmDeltaPosition;

        /// <summary>
        /// Palm velocity.
        /// </summary>
        public Vector3 PalmVelocity;

        /// <summary>
        /// Palm surface world normal ray
        /// </summary>
        public Vector3 PalmNormal;

        /// <summary>
        /// Palm scale.
        /// </summary>
        public Vector3 PalmScale;

        /// <summary>
        /// Palm world rotation
        /// </summary>
        public Quaternion PalmRotation;

        /// <summary>
        /// The palm's local position relative to main camera's parent transform.
        /// </summary>
        public Vector3 PalmLocalPosition;

        /// <summary>
        /// The palm's local rotation relative to main camera's parent transform.
        /// </summary>
        public Quaternion PalmLocalRotation;

        /// <summary>
        /// The palm's local normal relative to main camera's parent transform.
        /// </summary>
        public Vector3 PalmLocalNormal;

        /// <summary>
        /// Thumb tracking info
        /// </summary>
        public RawFingerTrackingInfo ThumbFinger;

        /// <summary>
        /// Index tracking info
        /// </summary>
        public RawFingerTrackingInfo IndexFinger;

        /// <summary>
        /// Middle tracking info
        /// </summary>
        public RawFingerTrackingInfo MiddleFinger;

        /// <summary>
        /// Ring tracking info
        /// </summary>
        public RawFingerTrackingInfo RingFinger;

        /// <summary>
        /// Little finger info
        /// </summary>
        public RawFingerTrackingInfo LittleFinger;

        /// <summary>
        /// The current gesture type of open hand/fist.
        /// </summary>
        public GestureType_Fist_OpenHand gestureFistOpenHand;

        /// <summary>
        /// The current gesture type of grisp
        /// </summary>
        public GestureType_Grisp gestureGrisp;

        /// <summary>
        /// The gesture type from native plugin.
        /// -1 for invalid status.
        /// </summary>
        public int NativeGestureType;

        public bool IsTracking;


        internal void UpdateProperties()
        {
            ThumbFinger.UpdateInternalProperties();
            IndexFinger.UpdateInternalProperties();
            MiddleFinger.UpdateInternalProperties();
            RingFinger.UpdateInternalProperties();
            LittleFinger.UpdateInternalProperties();
        }

        public void Dispose()
        {
            ThumbFinger.Dispose();
            IndexFinger.Dispose();
            MiddleFinger.Dispose();
            RingFinger.Dispose();
            LittleFinger.Dispose();
        }

        internal void CopyFrom (HandTrackingInfo other)
        {
            Timestamp = other.Timestamp;
            IsValid = other.IsValid;
            Handness = other.Handness;
            PalmPosition = other.PalmPosition;
            PalmDeltaPosition = other.PalmDeltaPosition;
            PalmNormal = other.PalmNormal;
            PalmScale = other.PalmScale;
            PalmRotation = other.PalmRotation;
            PalmLocalPosition = other.PalmLocalPosition;
            PalmLocalRotation = other.PalmLocalRotation;
            PalmLocalNormal = other.PalmLocalNormal;
            ThumbFinger.CopyFrom(other.ThumbFinger);
            IndexFinger.CopyFrom(other.IndexFinger);
            MiddleFinger.CopyFrom(other.MiddleFinger);
            RingFinger.CopyFrom(other.RingFinger);
            LittleFinger.CopyFrom(other.LittleFinger);
            gestureFistOpenHand = other.gestureFistOpenHand;
            gestureGrisp = other.gestureGrisp;
            NativeGestureType = other.NativeGestureType;

        }
    }

    /// <summary>
    /// The raw finger data.
    /// </summary>
    [Serializable]
    public struct RawFingerTrackingInfo : IDisposable
    {
        /// <summary>
        /// Position nodes of each finger joint.
        /// </summary>
        public NativeArray<Vector3> Positions;

        /// <summary>
        /// Local position of the each finger joint.
        /// </summary>
        public NativeArray<Vector3> LocalPositions;

        /// <summary>
        /// Finger straightness factor.
        /// 
        /// 1 means straight, 0 means bended.
        /// 
        /// </summary>
        public float straightness;

        /// <summary>
        /// finger bendess factor.
        /// </summary>
        public float bendness;

        internal float bendnessRangeMin, bendnessRangeMax;

        internal void UpdateInternalProperties()
        {
            if (Positions.Length == 3)
            {
                Vector3 dir21 = (Positions[2] - Positions[1]).normalized;
                Vector3 dir10 = (Positions[1] - Positions[0]).normalized;
                this.straightness = Mathf.Abs(Vector3.Dot(dir21, dir10));
            }
            else if (Positions.Length == 4)
            {
                Vector3 dir32 = (Positions[3] - Positions[2]).normalized;
                Vector3 dir21 = (Positions[2] - Positions[1]).normalized;
                Vector3 dir10 = (Positions[1] - Positions[0]).normalized;
                this.straightness = Mathf.Abs(Vector3.Dot(dir32, dir21)) * Mathf.Abs(Vector3.Dot(dir21, dir10));
            }
            this.bendness = Mathf.Clamp01((1 - Mathf.InverseLerp(bendnessRangeMin, bendnessRangeMax, straightness) - bendnessRangeMin) / (bendnessRangeMax - bendnessRangeMin));
        }

        internal RawFingerTrackingInfo(int positionCapacity)
        {
            bendnessRangeMin = bendnessRangeMax = 0;
            straightness = 0;
            bendness = 0;
            Positions = new NativeArray<Vector3>(positionCapacity, Allocator.Persistent);
            LocalPositions = new NativeArray<Vector3>(positionCapacity, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (Positions.IsCreated)
            {
                Positions.Dispose();
            }
            if(LocalPositions.IsCreated)
            {
                LocalPositions.Dispose();
            }
        }

        public void CopyFrom (RawFingerTrackingInfo other)
        {
            Positions.CopyFrom(other.Positions);
            LocalPositions.CopyFrom(other.LocalPositions);
            straightness = other.straightness;
            bendness = other.bendness;
            bendnessRangeMin = other.bendnessRangeMin;
            bendnessRangeMax = other.bendnessRangeMax;
        }
    }
}