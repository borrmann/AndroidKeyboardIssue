
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using Microsoft.Maui.LifecycleEvents;


#if ANDROID
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;

#region ANRDOID EDGE TO EDGE
internal static class ActivityExtensions
{
    static global::Android.Graphics.Color DefaultLightScrim = global::Android.Graphics.Color.Argb(0xe6, 0xFF, 0xFF, 0xFF);
    static global::Android.Graphics.Color DefaultDarkScrim = global::Android.Graphics.Color.Argb(0x80, 0x1b, 0x1b, 0x1b);


    public static void EnableEdgeToEdge(this global::Android.App.Activity activity)
        => activity.EnableEdgeToEdge(
            SystemBarStyle.Auto(global::Android.Graphics.Color.Transparent, global::Android.Graphics.Color.Transparent),
            SystemBarStyle.Auto(DefaultLightScrim, DefaultDarkScrim));

    public static void EnableEdgeToEdge(this global::Android.App.Activity activity, SystemBarStyle statusBarStyle, SystemBarStyle navigationBarStyle)
        => activity.Window?.EnableEdgeToEdge(activity.Resources!, statusBarStyle, navigationBarStyle);

    public static void EnableEdgeToEdge(this global::Android.Views.Window window, global::Android.Content.Res.Resources resources)
        => window.EnableEdgeToEdge(resources,
            SystemBarStyle.Auto(global::Android.Graphics.Color.Transparent, global::Android.Graphics.Color.Transparent),
            SystemBarStyle.Auto(DefaultLightScrim, DefaultDarkScrim));


    public static void EnableEdgeToEdge(this global::Android.Views.Window window, global::Android.Content.Res.Resources resources, SystemBarStyle statusBarStyle, SystemBarStyle navigationBarStyle)
    {
        var view = window!.DecorView;
        var statusBarIsDark = Application.Current.RequestedTheme == AppTheme.Dark;
        var navigationBarIsDark = Application.Current.RequestedTheme == AppTheme.Dark;

        IEdgeToEdge impl;

        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            impl = new EdgeToEdge30();
        else if (OperatingSystem.IsAndroidVersionAtLeast(29))
            impl = new EdgeToEdgeApi29();
        else if (OperatingSystem.IsAndroidVersionAtLeast(28))
            impl = new EdgeToEdgeApi28();
        else if (OperatingSystem.IsAndroidVersionAtLeast(26))
            impl = new EdgeToEdgeApi26();
        else if (OperatingSystem.IsAndroidVersionAtLeast(23))
            impl = new EdgeToEdgeApi23();
        else if (OperatingSystem.IsAndroidVersionAtLeast(21))
            impl = new EdgeToEdgeApi21();
        else
            impl = new EdgeToEdgeBase();

        impl.Setup(statusBarStyle, navigationBarStyle, window, view, statusBarIsDark, navigationBarIsDark);

        impl.AdjustLayoutInDisplayCutoutMode(window!);
    }
}


internal class SystemBarStyle(global::Android.Graphics.Color lightScrim, global::Android.Graphics.Color darkScrim, UiMode nightMode)
{
    public static SystemBarStyle Auto(global::Android.Graphics.Color lightScrim, global::Android.Graphics.Color darkScrim)
        => new(lightScrim, darkScrim, UiMode.NightUndefined);

    public static SystemBarStyle Dark(global::Android.Graphics.Color scrim)
        => new(scrim, scrim, UiMode.NightYes);

    public static SystemBarStyle Light(global::Android.Graphics.Color scrim, global::Android.Graphics.Color darkScrim)
        => new(scrim, darkScrim, UiMode.NightNo);

    public global::Android.Graphics.Color LightScrim { get; private set; } = lightScrim;
    public global::Android.Graphics.Color DarkScrim { get; private set; } = darkScrim;

    public UiMode NightMode { get; private set; } = nightMode;

    public global::Android.Graphics.Color GetScrim(bool isDark)
    {
        return isDark ? DarkScrim : LightScrim;
    }

    public global::Android.Graphics.Color GetScrimWithEnforcedContrast(bool isDark)
    {
        if (NightMode == UiMode.NightUndefined)
            return global::Android.Graphics.Color.Transparent;

        if (isDark)
            return DarkScrim;

        return LightScrim;
    }
}

internal interface IEdgeToEdge
{
    void Setup(
        SystemBarStyle statusBarStyle,
        SystemBarStyle navigationBarStyle,
        global::Android.Views.Window window,
        global::Android.Views.View view,
        bool statusBarIsDark,
        bool navigationBarIsDark)
    {
        // No edge to edge, pre SDK 21
    }

    void AdjustLayoutInDisplayCutoutMode(global::Android.Views.Window window)
    {
        // No display cutout before SDK 28
    }
}

internal class EdgeToEdgeBase : IEdgeToEdge
{
    public virtual void Setup(
        SystemBarStyle statusBarStyle,
        SystemBarStyle navigationBarStyle,
        global::Android.Views.Window window,
        global::Android.Views.View view,
        bool statusBarIsDark,
        bool navigationBarIsDark)
    {
        // No edge to edge, pre SDK 21
    }

    public virtual void AdjustLayoutInDisplayCutoutMode(global::Android.Views.Window window)
    {
        // No display cutout before SDK 28
    }
}

internal class EdgeToEdgeApi21 : EdgeToEdgeBase
{
    public override void Setup(
        SystemBarStyle statusBarStyle,
        SystemBarStyle navigationBarStyle,
        global::Android.Views.Window window,
        global::Android.Views.View view,
        bool statusBarIsDark,
        bool navigationBarIsDark)
    {
        WindowCompat.SetDecorFitsSystemWindows(window, false);
        window.AddFlags(WindowManagerFlags.TranslucentStatus);
        window.AddFlags(WindowManagerFlags.TranslucentNavigation);
    }
}

internal class EdgeToEdgeApi23 : EdgeToEdgeBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
    public override void Setup(
        SystemBarStyle statusBarStyle,
        SystemBarStyle navigationBarStyle,
        global::Android.Views.Window window,
        global::Android.Views.View view,
        bool statusBarIsDark,
        bool navigationBarIsDark)
    {
        WindowCompat.SetDecorFitsSystemWindows(window, false);
        window.SetStatusBarColor(statusBarStyle.GetScrim(statusBarIsDark));
        window.SetNavigationBarColor(navigationBarStyle.DarkScrim);
        new WindowInsetsControllerCompat(window, view).AppearanceLightStatusBars = !statusBarIsDark;
    }
}

internal class EdgeToEdgeApi26 : EdgeToEdgeBase
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
    public override void Setup(
        SystemBarStyle statusBarStyle,
        SystemBarStyle navigationBarStyle,
        global::Android.Views.Window window,
        global::Android.Views.View view,
        bool statusBarIsDark,
        bool navigationBarIsDark)
    {
        WindowCompat.SetDecorFitsSystemWindows(window, false);
        window.SetStatusBarColor(statusBarStyle.GetScrim(statusBarIsDark));
        window.SetNavigationBarColor(navigationBarStyle.GetScrim(navigationBarIsDark));
        var c = new WindowInsetsControllerCompat(window, view);
        c.AppearanceLightStatusBars = !statusBarIsDark;
        c.AppearanceLightNavigationBars = !navigationBarIsDark;
    }
}

internal class EdgeToEdgeApi28 : EdgeToEdgeApi26
{
    public override void AdjustLayoutInDisplayCutoutMode(global::Android.Views.Window window)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
            window.Attributes!.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
    }
}

internal class EdgeToEdgeApi29 : EdgeToEdgeApi28
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
    public override void Setup(
        SystemBarStyle statusBarStyle,
        SystemBarStyle navigationBarStyle,
        global::Android.Views.Window window,
        global::Android.Views.View view,
        bool statusBarIsDark,
        bool navigationBarIsDark)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            WindowCompat.SetDecorFitsSystemWindows(window, false);
            window.SetStatusBarColor(statusBarStyle.GetScrimWithEnforcedContrast(navigationBarIsDark));
            window.SetNavigationBarColor(navigationBarStyle.GetScrimWithEnforcedContrast(navigationBarIsDark));
            window.StatusBarContrastEnforced = false;
            window.NavigationBarContrastEnforced = navigationBarStyle.NightMode == UiMode.NightUndefined;

            var c = new WindowInsetsControllerCompat(window, view);
            c.AppearanceLightStatusBars = !statusBarIsDark;
            c.AppearanceLightNavigationBars = !navigationBarIsDark;
        }
    }
}

internal class EdgeToEdge30 : EdgeToEdgeApi29
{
    public override void AdjustLayoutInDisplayCutoutMode(global::Android.Views.Window window)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            window.Attributes!.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.Always;
    }
}

public class MyFragmentLifecycleCallbacks(Action<AndroidX.Fragment.App.FragmentManager, AndroidX.Fragment.App.Fragment> onFragmentStarted) : AndroidX.Fragment.App.FragmentManager.FragmentLifecycleCallbacks
{
    public override void OnFragmentStarted(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f)
    {
        onFragmentStarted?.Invoke(fm, f);
        base.OnFragmentStarted(fm, f);
    }
}

#endregion 

#endif


namespace AndroidKeyboardIssue
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                })
#if ANDROID
            .ConfigureLifecycleEvents(lifecycleBuilder =>
            {
                lifecycleBuilder.AddAndroid(androidLifecycleBuilder =>
                {
                    androidLifecycleBuilder.OnCreate((activity, savedInstance) =>
                    {
                        if (activity is AndroidX.Activity.ComponentActivity componentActivity)
                        {
                            // Enable Edge to Edge for the activity
                            AndroidX.Activity.EdgeToEdge.Enable(componentActivity);

                            // Also wire up a fragment lifecycle callback so we can enable edge to edge on fragments
                            componentActivity.GetFragmentManager()?.RegisterFragmentLifecycleCallbacks(new MyFragmentLifecycleCallbacks((fragmentManager, fragment) =>
                            {
                                // Modals in MAUI in NET9 use DialogFragment
                                if (fragment is AndroidX.Fragment.App.DialogFragment dialogFragment)
                                {
                                    // Edge to Edge on the fragment's window
                                    dialogFragment.Dialog!.Window!.EnableEdgeToEdge(dialogFragment.Dialog!.Window!.DecorView!.Resources!);
                                }
                            }), false);
                        }
                    });
                });
            })
#endif
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton<ProjectRepository>();
            builder.Services.AddSingleton<TaskRepository>();
            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<TagRepository>();
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<ProjectListPageModel>();
            builder.Services.AddSingleton<ManageMetaPageModel>();

            builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            return builder.Build();
        }
    }
}
