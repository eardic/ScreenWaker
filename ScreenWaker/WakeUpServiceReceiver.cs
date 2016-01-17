using System;
using Android.App;
using Android.Content;

namespace KnockKnockScreen
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] {Intent.ActionScreenOff, Intent.ActionBootCompleted})]
    public class WakeUpServiceReceiver : BroadcastReceiver
    {
        private readonly Action _registerAction;

        public WakeUpServiceReceiver() : this(null)
        {
        }

        public WakeUpServiceReceiver(Action registerAction)
        {
            _registerAction = registerAction;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionScreenOff)
            {
                _registerAction?.Invoke();
            }
            //if (intent.Action == Intent.ActionBootCompleted)
            //{
            //    context?.StartService(new Intent(context, typeof (WakeUpService)));
            //}
        }
    }
}