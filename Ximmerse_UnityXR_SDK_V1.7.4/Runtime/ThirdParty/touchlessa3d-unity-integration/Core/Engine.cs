using System;
using System.Runtime.InteropServices;
using AOT;
namespace TouchlessA3D {
  /**
   * An engine for processing frames.
   *
   */
  public class Engine {
    private GCHandle m_handleToEventHandler;
    private delegate void CallbackFromNative (IntPtr context, IntPtr ta3d_event_t);
    private CallbackFromNative m_CallbackFromNative;
    private IntPtr m_ta3d_engine_t;
    public Engine (string unique_id, string persistent_storage_path, ICalibration calibration, EventHandler<GestureEvent> ta3dEventHandler) {
      m_handleToEventHandler = GCHandle.Alloc (ta3dEventHandler);
      IntPtr nativeCalibration = calibration == null? IntPtr.Zero : calibration.getNativeCalibration ();
      m_CallbackFromNative = new CallbackFromNative (ta3d_callback_implementation);
      m_ta3d_engine_t = NativeCalls.ta3d_engine_acquire (unique_id, persistent_storage_path, nativeCalibration,
        Marshal.GetFunctionPointerForDelegate (m_CallbackFromNative), GCHandle.ToIntPtr (m_handleToEventHandler));
    }

    ~Engine () {
      m_handleToEventHandler.Free ();
      NativeCalls.ta3d_engine_release (m_ta3d_engine_t);
    }

    /**
     * Analyzes a frame with respect to touchless interaction.
     * The ta3dEventhandler is notified if any touchless interaction is detected.
     * @param frame A frame to analyze.
     */
    [MonoPInvokeCallback (typeof (CallbackFromNative))]
    private static void ta3d_callback_implementation (IntPtr context, IntPtr ta3d_event_t) {
      GestureEvent args = new GestureEvent (ta3d_event_t);
      GCHandle newHandleToEventHandler = GCHandle.FromIntPtr (context); //same as m_handleToEventHandler, but static method
      var ta3dEventHandler = (EventHandler<GestureEvent>) (newHandleToEventHandler.Target);
      if (null != ta3dEventHandler) {
        ta3dEventHandler (ta3dEventHandler, args);
      }
    }

    public void handleFrame (IFrame frame) {
      NativeCalls.ta3d_engine_handle_frame (m_ta3d_engine_t, frame.getNativeFrame ());
    }
  }
}