﻿using System;
using True_Love.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Microsoft.QueryStringDotNET;
using static True_Love.Helpers.Generic;

namespace True_Love
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            //TileUpdateManager.CreateTileUpdaterForApplication().Clear(); // 清空队列
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true); // 实现通知循环
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                //if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                //{
                //    // TODO: 从之前挂起的应用程序加载状态
                //}

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();

                if (localSettings.Values["SetLiveTiles"] == null)
                {   // 首次打开先过一边设置
                    localSettings.Values["SetLiveTiles"] = true;
                    localSettings.Values["SetHideCommandBar"] = false;
                    localSettings.Values["OnlyLiveTiles"] = true;
                    localSettings.Values["SetBackgroundColor"] = true;
                    localSettings.Values["IsToastPush"] = false;
                    var a = new SettingsPage();
                    //a.FirstRun();
                    a = null;
                    GC.Collect();
                }
                else
                {
                    localSettings.Values["IsToastPush"] = false;
                }

                if (!IdentifyDeviceFamily("mobile")) HideTitleBar();
                //else HideStatusBar();
            }
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            // Handle notification activation
            if (e is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                QueryString args = QueryString.Parse(toastActivationArgs.Argument);
                switch (args["action"])
                {
                    case "Settings":
                        var settings = new Uri("ms-settings:network-status");
                        var success = await Windows.System.Launcher.LaunchUriAsync(settings);
                        localSettings.Values["IsToastPush"] = true;
                        break;
                    case "Close":
                        localSettings.Values["IsToastPush"] = true;
                        break;
                }
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        /// <summary>
        /// 沉淀状态栏 for PC
        /// </summary>
        private void HideTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 173, 219, 224);
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 173, 219, 224);
            titleBar.ButtonHoverForegroundColor = Colors.Black;
        }

        /// <summary>
        /// 沉淀状态栏 for Phone
        /// </summary>
        //private void HideStatusBar()
        //{
        //    var applicationView = ApplicationView.GetForCurrentView();
        //    applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        //    var statusbar = StatusBar.GetForCurrentView();
        //    statusbar.BackgroundColor = Colors.Transparent;
        //}
    }
}
