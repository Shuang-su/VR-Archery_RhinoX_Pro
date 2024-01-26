using UnityEngine;

namespace Ximmerse.Wrapper.XDeviceService.Client
{
    public static class XDeviceClientWrapper
    {

        private static bool _isInit = false;

        public static bool IsInit
        {
            get { return _isInit; }
        }

        public static int Init()
        {
            if (_isInit)
            {
                Debug.Log("inited");
                return 0;
            }

            int ret = XDeviceClientApi.Init();
            Debug.Log("init: " + ret);

            if (ret == 0)
            {
                _isInit = true;
                int numberOfControllers = XDeviceClientApi.GetNumberOfControllers();
                if (numberOfControllers > 0)
                {
                    for (int i = 0; i < numberOfControllers; i++)
                    {
                        XDeviceClientApi.GetController(i);
                    }
                }
                XDeviceClientApi.StartEventCallback();
            }
            else
            {
                Debug.LogError("init failed: " + ret);
            }
            return ret;
        }

        public static int Exit()
        {
            if (!_isInit)
            {
                Debug.Log("Not inited");
                return 0;
            }

            Debug.Log("exit");
            XDeviceClientApi.StopEventCallback();
            XDeviceClientApi.Exit();
            _isInit = false;
            return 0;
        }
    }
}