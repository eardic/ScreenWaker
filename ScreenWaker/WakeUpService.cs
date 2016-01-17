using System;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Util;
using Java.Lang;
using Exception = Java.Lang.Exception;
using Math = Java.Lang.Math;
using Process = Android.OS.Process;

namespace KnockKnockScreen
{
    [Service(Enabled = true)]
    public class WakeUpService : Service, ISensorEventListener
    {
        private readonly string _tag = typeof(WakeUpService).Name;
        private long _lastKnockTime = 0/*,
                     _lastDistTime = 0*/;
        private PowerManager _powerManager;
        private float? _prevZ;
        private BroadcastReceiver _screenOffReceiver;
        private SensorManager _sensorManager;
        private PowerManager.WakeLock _wakeLockPartial;
        //private Logger _logger;
        public static ServiceStatus Status { get; private set; } = ServiceStatus.READY;

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            try
            {
                if (e.Sensor.Type == SensorType.Accelerometer)
                {
                    var x = e.Values[0];
                    var y = e.Values[1];
                    var z = e.Values[2];

                    if (_prevZ.HasValue && (Math.Abs(x) < 3 && Math.Abs(y) < 3))
                    {
                        var diffZ = (Math.Abs(z - _prevZ.Value) / z) * 100;

                        if (diffZ > 15 && diffZ < 30)
                        {
                            var timeDiff = (JavaSystem.NanoTime() - _lastKnockTime) / 1000000;
                            if (timeDiff > 100 && timeDiff < 800)
                            {
                                TurnOnScreen();
                                Log.Debug("ScreenWaker", $"Screen Opened !, {z} || {diffZ}");
                            }
                            _lastKnockTime = JavaSystem.NanoTime();
                            //Log.Debug("ScreenWaker", $"Knocked !, {diffZ}");
                        }
                    }
                    _prevZ = z;
                }
                //if (e.Sensor.Type == SensorType.Proximity)
                //{
                //    var dist = e.Values[0];
                //    if (dist < 5)
                //    {
                //        var timeDiff = (Java.Lang.JavaSystem.NanoTime() - _lastDistTime) / 1000000;
                //        if ((timeDiff > 500 && timeDiff < 1500))
                //        {
                //            TurnOnScreen();
                //            Log.Debug("ScreenWaker", $"Proximity Turned On Screen !");
                //        }
                //        _lastDistTime = Java.Lang.JavaSystem.NanoTime();
                //    }
                //    //Log.Debug("ScreenWaker", $"Proximity !, {e.Values[0]}");
                //}
            }
            catch (Exception ex)
            {
                Log.Error("OnSensorChanged", ex.ToString());
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            try
            {
                //_logger = new Logger(Environment.ExternalStorageDirectory.AbsolutePath);
                _powerManager = (PowerManager)BaseContext.GetSystemService(PowerService);
                _sensorManager = (SensorManager)BaseContext.GetSystemService(SensorService);
                _wakeLockPartial = _powerManager.NewWakeLock(WakeLockFlags.Partial, _tag);
                _screenOffReceiver = new WakeUpServiceReceiver(() =>
                {
                    new Handler().PostDelayed(() =>
                    {
                        UnregisterListener();
                        RegisterListener();
                        Log.Debug("WakeUpServiceReceiver", "Screen turned off.");
                    }, 500);
                });
                RegisterReceiver(_screenOffReceiver, new IntentFilter(Intent.ActionScreenOff));

                Status = ServiceStatus.STARTED;
                Log.Debug("WakeUpService", "Service started.");
            }
            catch (Exception ex)
            {
                Log.Error("WakeUpService", ex.ToString());
                Status = ServiceStatus.FAILED;
            }
        }

        private void RegisterListener()
        {
            var accelerometer = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            _sensorManager.RegisterListener(this, accelerometer, SensorDelay.Fastest);

            //var proximity = _sensorManager.GetDefaultSensor(SensorType.Proximity);
            //_sensorManager.RegisterListener(this, proximity, SensorDelay.Fastest);
        }

        private void UnregisterListener()
        {
            _sensorManager?.UnregisterListener(this);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            var notificationIntent = new Intent(BaseContext, typeof(MainActivity));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            var notifPendingIntent = PendingIntent.GetActivity(BaseContext, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            var notif = new Notification
            {
                ContentIntent = notifPendingIntent
            };

            Notification.Builder notifBuilder = new Notification.Builder(BaseContext);
            notifBuilder.SetContentIntent(notifPendingIntent);
            notifBuilder.SetWhen(JavaSystem.CurrentTimeMillis());
            notifBuilder.SetSmallIcon(Resource.Drawable.Knock);
            notifBuilder.SetContentTitle(Resources.GetString(Resource.String.ApplicationName));
            notifBuilder.SetContentText(Resources.GetString(Resource.String.NotifText));

            StartForeground(Process.MyPid(), notifBuilder.Build());
            
            //var notificationManager = (NotificationManager) BaseContext.GetSystemService(NotificationService);
            //notificationManager.Notify(Process.MyPid(), new Notification { ContentIntent = notifPendingIntent });

            RegisterListener();

            _wakeLockPartial?.Acquire();

            Status = ServiceStatus.STARTED;
            Log.Debug("WakeUpService", "Service start command.");
            return StartCommandResult.StickyCompatibility;
        }

        public override void OnDestroy()
        {
            Status = ServiceStatus.STOPPED;
            UnregisterReceiver(_screenOffReceiver);
            _sensorManager?.UnregisterListener(this);
            _wakeLockPartial?.Release();
            StopForeground(true);

            Log.Debug("WakeUpService", "Service stopped.");
            base.OnDestroy();
        }

        private void TurnOnScreen()
        {
            var wakeLock = _powerManager?.NewWakeLock(WakeLockFlags.Full |
                                                      WakeLockFlags.AcquireCausesWakeup |
                                                      WakeLockFlags.OnAfterRelease, _tag);
            wakeLock?.Acquire();
            wakeLock?.Release();
        }
    }
}