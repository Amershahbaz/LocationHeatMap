using Android.App;
using Android.Content.PM;
using Android.OS;

namespace LocationHeatMap
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Request location permissions explicitly on Android
            if (CheckSelfPermission(
                Android.Manifest.Permission.AccessFineLocation)
                != Permission.Granted)
            {
                RequestPermissions(new[]
                {
                    Android.Manifest.Permission.AccessFineLocation,
                    Android.Manifest.Permission.AccessCoarseLocation
                }, 0);
            }
        }
    }
}
