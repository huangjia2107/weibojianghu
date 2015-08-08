
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace TryClient
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Portrait)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        private async void SendEmail()
        {
            //predefine Recipient
            EmailRecipient sendTo = new EmailRecipient()
            {
                Address = "654664168@qq.com"
            };

            string FirmwareVersion = "";//DeviceStatus.DeviceFirmwareVersion;
            string DeviceName = "";// DeviceStatus.DeviceName;

            //generate mail object
            EmailMessage mail = new EmailMessage();
            mail.Subject = @"关于《微博江湖1.0》应用的反馈";
            mail.Body = "\n\r\n\r\n\r我的手机:" + DeviceName + "\n\r系统版本:" + FirmwareVersion;

            //add recipients to the mail object
            mail.To.Add(sendTo);

            //open the share contract with Mail only:
            await EmailManager.ShowComposeNewEmailAsync(mail);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            SendEmail();
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
                  new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }
    }
}
