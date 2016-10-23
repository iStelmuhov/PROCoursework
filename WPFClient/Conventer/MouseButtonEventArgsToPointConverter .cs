using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using WPFClient.Models;

namespace WPFClient.Conventer
{
    public class MouseButtonEventArgsToPointConverter : IEventArgsConverter
    {
        public object Convert(object value, object parameter)
        {
            var args = (MouseEventArgs)value;
            var element = (FrameworkElement)parameter;
            var point = args.GetPosition(element);

            return new MouseArgs(){Point=point,ButtonState = args.LeftButton};
        }
    }
}