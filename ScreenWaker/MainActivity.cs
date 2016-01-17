using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.Animations;
using Android.Widget;

namespace KnockKnockScreen
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@android:style/Theme.NoTitleBar")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Splash Screen
            var view = LayoutInflater.Inflate(Resource.Layout.SplashLayout, null, false);
            var anim = AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeIn);
            anim.Duration = 1000;
            anim.AnimationEnd += AnimOnAnimationEnd;
            view.StartAnimation(anim);
            SetContentView(view);
            // Version
            var info = PackageManager.GetPackageInfo(PackageName, 0);
            var versionText = FindViewById<TextView>(Resource.Id.versionText);
            versionText.Text = $"V {info.VersionName}";
        }

        private void AnimOnAnimationEnd(object sender, Animation.AnimationEndEventArgs animationEndEventArgs)
        {
            // Main Layout
            var view = LayoutInflater.Inflate(Resource.Layout.Main, null, false);
            var anim = AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeIn);
            
            anim.Duration = 1000;
            view.StartAnimation(anim);
            SetContentView(view);

            var start = FindViewById<Button>(Resource.Id.BtnStartService);
            var status = FindViewById<TextView>(Resource.Id.statusText);
            var openClose = FindViewById<TextView>(Resource.Id.openCloseText);

            if (IsServiceRunning())
            {
                start.SetBackgroundResource(Resource.Drawable.PowerOff);
                status.SetText(Resource.String.Active);
                status.SetTextColor(Resources.GetColor(Android.Resource.Color.HoloGreenLight));
                openClose.SetText(Resource.String.ClickToClose);
            }
            else
            {
                start.SetBackgroundResource(Resource.Drawable.PowerOn);
                status.SetText(Resource.String.Passive);
                status.SetTextColor(Resources.GetColor(Android.Resource.Color.HoloRedLight));
                openClose.SetText(Resource.String.ClickToOpen);
            }

            //var layout = FindViewById<RelativeLayout>(Resource.Id.MainLayout);

            start.Click += delegate
            {
                if (IsServiceRunning())
                {
                    this.StopService(new Intent(this, typeof (WakeUpService)));
                    start.SetBackgroundResource(Resource.Drawable.PowerOn);
                    status.SetText(Resource.String.Passive);
                    status.SetTextColor(Resources.GetColor(Android.Resource.Color.HoloRedLight));
                    openClose.SetText(Resource.String.ClickToOpen);
                    //Toast.MakeText(this, Resource.String.StopService, ToastLength.Long).Show();
                }
                else
                {
                    this.StartService(new Intent(this, typeof (WakeUpService)));
                    start.SetBackgroundResource(Resource.Drawable.PowerOff);
                    status.SetText(Resource.String.Active);
                    status.SetTextColor(Resources.GetColor(Android.Resource.Color.HoloGreenLight));
                    openClose.SetText(Resource.String.ClickToClose);
                    //Toast.MakeText(this, Resource.String.StartService, ToastLength.Long).Show();
                }
            };
        }

        private bool IsServiceRunning()
        {
            return WakeUpService.Status == ServiceStatus.STARTED;
        }
    }
}