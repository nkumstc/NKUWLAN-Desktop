﻿using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GatewayClient;
using System.Windows.Forms;

namespace Desktop_GUI
{
    /// <summary>
    /// 信息显示窗口
    /// Info.xaml 的交互逻辑
    /// </summary>
    public partial class InfoWindow : Window
    {
        /// <summary>
        /// 账号信息
        /// </summary>
        AccountInfo Info
        {
            set
            {
                UIDText.Text = value.Uid;
                UIDText.ToolTip = "本月累计使用:" + value.Time;
                RFlowText.Text = value.RFlow;
                FlowText.Text = value.Flow;
                FeeText.Text = value.Fee;
                SpeedText.Text = value.Speed;
            }
        }
        private delegate bool TimerDispatcherDelegate();

        /// <summary>
        /// 窗体靠边隐藏位置
        /// </summary>
        internal AnchorStyles anchorStatus = AnchorStyles.None;
        public AnchorStyles Anchor
        {
            get
            {
                return anchorStatus;
            }
            set
            {
                const int SIDE_WIDTH = 10;
                switch (value)
                {
                    case AnchorStyles.Top:
                        this.Top = (this.Height - SIDE_WIDTH) * (-1);
                        break;
                    case AnchorStyles.Left:
                        this.Left = (-1) * (this.Width - SIDE_WIDTH);
                        break;
                    case AnchorStyles.Right:
                        this.Left = Screen.PrimaryScreen.Bounds.Width - SIDE_WIDTH;
                        break;
                    case AnchorStyles.Bottom:
                        this.Top = (Screen.PrimaryScreen.Bounds.Height - SIDE_WIDTH);
                        break;
                    case AnchorStyles.None://取消隐藏
                        switch (this.anchorStatus)
                        {
                            case AnchorStyles.Top:
                                this.Top = 0;
                                break;
                            case AnchorStyles.Left:
                                this.Left = 0;
                                break;
                            case AnchorStyles.Right:
                                this.Left = Screen.PrimaryScreen.Bounds.Width - this.Width;
                                break;
                            case AnchorStyles.Bottom:
                                this.Top = Screen.PrimaryScreen.Bounds.Height - this.Height;
                                break;
                        }
                        break;
                }
                anchorStatus = value;
            }
        }


        public InfoWindow()
        {
            InitializeComponent();
        }

        public bool Logout()
        {
            if (Gateway.Logout())
            {
                App.Current.MainWindow = new LoginWindow();
                this.Close();
                App.Current.MainWindow.Show();
                return true;
            }
            else
            {
                tipText.Text = "网关连接异常";
                System.Windows.MessageBox.Show("注销异常，可能已经断网!", "网络异常");
                return false;
            }
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        /// <returns></returns>
        bool UpdateInfo()
        {
            var info = Gateway.Info;
            if (info == null)
            {
                var login = Gateway.Login();
                if (login == true)
                {
                    tipText.Text = DateTime.Now.ToString("HH:mm:ss") + "已重新登录";
                    return true;

                }
                else if (login == false)
                {
                    tipText.Text = DateTime.Now.ToString("HH:mm:ss") + "登录失败";
                }
                else
                {
                    tipText.Text = DateTime.Now.ToString("HH:mm:ss") + "网关连接正常";
                }
                return false;
            }
            else
            {
                Info = info.Value;
                tipText.Text = DateTime.Now.ToString("HH:mm") + " 更新";
                return true;
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            App.Current.Shutdown();
        }

        private void hideBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            TrayNotify.Start();
        }

        /// <summary>
        /// 拖动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        /// <summary>
        /// 窗体加载之后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            var timer = new System.Timers.Timer(10001);
            timer.Elapsed += Timer_Update;
            timer.Enabled = true;
            timer.Start();
            this.Activated += InfoWindow_Activated;
            this.MouseLeave += InfoWindow_MouseLeave;
        }

        /// <summary>
        /// 窗体激活
        /// </summary>
        private void InfoWindow_Activated(object sender, EventArgs e)
        {
            //显示窗体
            Anchor = AnchorStyles.None;
            this.MouseLeave += InfoWindow_MouseLeave;
        }

        //定时更新信息
        private void Timer_Update(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new TimerDispatcherDelegate(UpdateInfo));
        }

        /// <summary>
        /// 鼠标进入窗体
        /// </summary>
        private void InfoWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Anchor != AnchorStyles.None)
            {
                Anchor = AnchorStyles.None;
                this.MouseEnter -= InfoWindow_MouseEnter;
                this.MouseLeave += InfoWindow_MouseLeave;
            }
        }

        /// <summary>
        /// 鼠标移出窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoWindow_MouseLeave(object sender, Object e)
        {
            var current = GetCurretnAnchor();
            if (current != Anchor)
            {
                Anchor = current;
                if (Anchor != AnchorStyles.None)
                {
                    this.MouseEnter += InfoWindow_MouseEnter;
                    this.MouseLeave -= InfoWindow_MouseLeave;
                }
            }
        }

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <returns></returns>
        private AnchorStyles GetCurretnAnchor()
        {
            if (this.Top <= 1)
            {
                return AnchorStyles.Top;
            }
            else if (this.Left <= 1)
            {
                return AnchorStyles.Left;
            }
            else if (this.Top + 1 >= Screen.PrimaryScreen.Bounds.Height - this.Height)
            {
                return AnchorStyles.Bottom;
            }
            else if (this.Left + this.Width + 1 >= Screen.PrimaryScreen.Bounds.Width)
            {

                return AnchorStyles.Right;
            }
            else
            {
                return AnchorStyles.None;
            }
        }
    }
}
