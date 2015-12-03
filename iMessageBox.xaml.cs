using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace CampusNetGuard
{
    public enum iMessageBoxButton
    {
        OK=0,
        OKCancel=1,
        YesNo=2,
        YesNoCancel,
    }
    public enum iMessageBoxResult
    {
        //用户直接关闭了消息窗口
        None =0,
        //用户点击确定按钮
        OK = 1,
        //用户点击取消按钮
        Cancel = 2,
        //用户点击是按钮
        Yes = 3,
        //用户点击否按钮
        No = 4,
    }
    public class iMessageBoxResultEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public iMessageBoxResult Click { get; private set; }
        public iMessageBoxResultEventArgs(iMessageBoxResult result)
        {
            this.Click = result;
        }
    }
    /// <summary>
    /// iMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class iMessageBox : Window
    {
        
        private iMessageBoxButton miMessageBoxButton;
        private string mTitle;
        private string mMessage;
        private Action<object, iMessageBoxResultEventArgs> mCallback;
        private int mWidth;
        private int mHeigtht;
        private iMessageBox()
        {
            InitializeComponent();
        }
        private bool closed = false;
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!closed)
            {
                e.Cancel = true;
                Storyboard sb = FindStoryboard("StoryboardWindowClose");
                sb.Completed += (xo, xe) => { this.Close(); };
                BeginStoryboard(sb);
                closed = true;
            }
            base.OnClosing(e);
        }
        private Storyboard FindStoryboard(string name)
        {
            return FindResource(name) as Storyboard;
        }
        private void ShowUIElement(UIElement element)
        {
            if (!element.IsVisible)
            {
                Dispatcher.Invoke(() => {
                    element.Visibility = Visibility.Visible;
                });
            }
        }
        private void PrepareShow()
        {
            _Title.Text = mTitle == null ? string.Empty : mTitle;
            _Message.AppendText("\t"+(mMessage == null ? string.Empty : mMessage));
            _Border.Width = mWidth == 0 ? 260 : mWidth;
            _Border.Height = mHeigtht == 0 ? 160 : mHeigtht;
            switch (miMessageBoxButton)
            {
                case iMessageBoxButton.OK:
                    Grid.SetColumn(_Ok, 2);
                    ShowUIElement(_Ok);
                    break;
                case iMessageBoxButton.OKCancel:
                    Grid.SetColumn(_Ok, 0);
                    Grid.SetColumn(_Cancel, 2);
                    ShowUIElement(_Ok);
                    ShowUIElement(_Cancel);
                    break;
                case iMessageBoxButton.YesNo:
                    Grid.SetColumn(_Yes, 0);
                    Grid.SetColumn(_No, 2);
                    ShowUIElement(_Yes);
                    ShowUIElement(_No);
                    break;
                case iMessageBoxButton.YesNoCancel:
                    Grid.SetColumn(_Yes, 0);
                    Grid.SetColumn(_No, 1);
                    Grid.SetColumn(_Cancel, 2);
                    ShowUIElement(_Yes);
                    ShowUIElement(_No);
                    ShowUIElement(_Cancel);
                    break;
            }
        }
        private void ActionResult(iMessageBoxResult result)
        {
            if (mCallback != null)
            {
                mCallback(this, new iMessageBoxResultEventArgs(result));
            }
            this.Close();
        }
        public static void Show(string message)
        {
            InternalShow(null, message, 0,0,iMessageBoxButton.OK, null);
        }
        public static void Show(string title,string message)
        {
            InternalShow(title, message,0,0, iMessageBoxButton.OK, null);
        }
        public static void Show(string title, string message, int width, int height)
        {
            InternalShow(title, message, width, height, iMessageBoxButton.OK, null);
        }
        public static void Show(string title, string message, iMessageBoxButton messageBoxButton, Action<object, iMessageBoxResultEventArgs> callback)
        {
            InternalShow(title,message,0,0,messageBoxButton,callback);
        }
        private static void InternalShow(string title,string message,int width,int height,iMessageBoxButton messageBoxButton,Action<object,iMessageBoxResultEventArgs> callback)
        {
            Application.Current.Dispatcher.Invoke(() => {
                iMessageBox m = new iMessageBox();
                m.mTitle = title;
                m.mMessage = message;
                m.mWidth = width;
                m.mHeigtht = height;
                m.miMessageBoxButton = messageBoxButton;
                m.mCallback = callback;
                m.PrepareShow();
                m.ShowDialog();
            });
        }


        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void ClickColse(object sender, RoutedEventArgs e)
        {
            ActionResult(iMessageBoxResult.None);
        }

        private void ClickCancel(object sender, RoutedEventArgs e)
        {

            ActionResult(iMessageBoxResult.Cancel);
        }

        private void ClickOk(object sender, RoutedEventArgs e)
        {
            ActionResult(iMessageBoxResult.OK);

        }

        private void ClickYes(object sender, RoutedEventArgs e)
        {
            ActionResult(iMessageBoxResult.Yes);

        }

        private void ClickNo(object sender, RoutedEventArgs e)
        {
            ActionResult(iMessageBoxResult.No);
        }
    }
}
