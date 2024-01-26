using System;
using System.Runtime.InteropServices;

namespace TouchlessA3D {
  public static class NativeCalls {
    public enum ResultType {
      RESULT_UNAVAILABLE = 0,
      RESULT_OK = 1,
    }

    [DllImport("touchless_a3d_xr")] // ta3d_engine_t
    public static extern IntPtr ta3d_engine_acquire(System.String unique_id, System.String persistent_storage_path,
      IntPtr calibration, IntPtr callback, IntPtr callback_context);

    [DllImport("touchless_a3d_xr")]
    public static extern void ta3d_engine_release(IntPtr ta3d_engine_t);

    [DllImport("touchless_a3d_xr")]
    public static extern void ta3d_engine_handle_frame(IntPtr ta3d_engine_t, IntPtr ta3d_frame_t);

    [DllImport("touchless_a3d_xr")] //ta3d_frame_t
    public static extern IntPtr ta3d_frame_create_from_android_420(
      IntPtr src_y, int src_stride_y,
      IntPtr src_u, int src_stride_u,
      IntPtr src_v, int src_stride_v,
      int pixel_stride_uv, int width, int height,
      long timestamp_ms, FrameRotation rotation);

    [DllImport("touchless_a3d_xr")]
    public static extern IntPtr ta3d_frame_create_from_rgba(
      IntPtr src_rgba, int src_stride,
      int width, int height,
      long timestamp_ms, FrameRotation rotation,
      bool flip_vertically);

    [DllImport("touchless_a3d_xr")]
    public static extern void ta3d_frame_destroy(IntPtr ta3d_frame_t);

    [DllImport("touchless_a3d_xr")]
    public static extern GestureType ta3d_event_get_type(IntPtr ta3d_event_t);

    [DllImport("touchless_a3d_xr")]
    public static extern void ta3d_skeleton_points_3d(IntPtr ta3d_event_t, IntPtr ta3d_skeleton_3d_t);

    [DllImport("touchless_a3d_xr")]
    public static extern ResultType ta3d_get_calibration(PreconfiguredCalibrations ta3denum, IntPtr ta3d_skeleton_3d_s);

    [DllImport("touchless_a3d_xr")]
    public static extern HandednessType ta3d_get_handedness(IntPtr ta3d_event_t);

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_point_2_float_t {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public float[] coordinates;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_point_3_float_t {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public float[] coordinates;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_size_2_float_t {
      public float width;
      public float height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_matrix_3_3_float_t {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
      public float[] elements;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_matrix_1_8_float_t {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public float[] elements;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_calibration_s {
      public ta3d_size_2_float_t calibration_size;
      public ta3d_matrix_3_3_float_t camera_matrix;
      public ta3d_matrix_1_8_float_t distortion_coefficients;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ta3d_skeleton_3d_s {
      public ResultType status;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = SkeletonInfo.POINTS_END)]
      public ta3d_point_3_float_t[] points;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = SkeletonInfo.BONES_END)]
      public ta3d_matrix_3_3_float_t[] rotations;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = SkeletonInfo.BONES_END)]
      public float[] bone_lengths;
    }
  }
}