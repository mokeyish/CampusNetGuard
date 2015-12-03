using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CampusNetGuard
{
    static class WPFBaseUtils
    {
        public static void BeginStoryboard(Storyboard storyboard)
        {
            Application.Current.Dispatcher.Invoke(()=>{ Application.Current.MainWindow.BeginStoryboard(storyboard); });
        }
        public static void ShowUIElement(Dispatcher dispatcher, UIElement element)
        {
            if (!element.IsVisible)
            {
                dispatcher.Invoke(() => {
                    element.Visibility = Visibility.Visible;
                });
            }
        }
        public static void HideUIElement(Dispatcher dispatcher, UIElement element)
        {
            if (element.IsVisible)
            {
                dispatcher.Invoke(() => {
                    element.Visibility = Visibility.Collapsed;
                });
            }
        }
        public static bool isNumbericInput(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.D0:
                case System.Windows.Input.Key.D1:
                case System.Windows.Input.Key.D2:
                case System.Windows.Input.Key.D3:
                case System.Windows.Input.Key.D4:
                case System.Windows.Input.Key.D5:
                case System.Windows.Input.Key.D6:
                case System.Windows.Input.Key.D7:
                case System.Windows.Input.Key.D8:
                case System.Windows.Input.Key.D9:
                case System.Windows.Input.Key.NumPad0:
                case System.Windows.Input.Key.NumPad1:
                case System.Windows.Input.Key.NumPad2:
                case System.Windows.Input.Key.NumPad3:
                case System.Windows.Input.Key.NumPad4:
                case System.Windows.Input.Key.NumPad5:
                case System.Windows.Input.Key.NumPad6:
                case System.Windows.Input.Key.NumPad7:
                case System.Windows.Input.Key.NumPad8:
                case System.Windows.Input.Key.NumPad9:
                    return true;
                default:
                    return false;
            }
        }

    }
}
