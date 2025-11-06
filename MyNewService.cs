using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;

namespace MyNewService
{
    public partial class MyNewService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private BackgroundWorker backgroundWorker;
        private volatile bool isStopping = false;
        private int eventId = 1;
        // 声明为类成员变量，避免被垃圾回收
        private Timer timer;

        public MyNewService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                // 创建事件源（需管理员权限，仅首次运行需要）
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
            // 1. 记录启动日志，验证日志功能
            eventLog1.WriteEntry("服务启动中...", EventLogEntryType.Information, eventId++);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                // 2. 初始化定时器（类成员变量）
                timer = new Timer();
                timer.Interval = 3000; // 3秒触发一次
                timer.Elapsed += OnTimer;
                timer.Start();
            };


          
            // 3. 通知服务控制管理器：服务已启动（规范操作）
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("服务启动成功", EventLogEntryType.Information, eventId++);
            worker.RunWorkerAsync();
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            // 定时器触发时写入日志
            eventLog1.WriteEntry("监控系统中...", EventLogEntryType.Information, eventId++);
        }

        protected override void OnStop()
        {
            // 停止时释放定时器
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            eventLog1.WriteEntry("服务已停止", EventLogEntryType.Information, eventId++);
        }
    }

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }
}