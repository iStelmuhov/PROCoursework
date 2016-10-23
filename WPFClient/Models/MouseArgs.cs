using System.Windows;
using System.Windows.Input;

namespace WPFClient.Models
{
    public class MouseArgs
    {
        public Point Point { get; set; }
        public MouseButtonState ButtonState { get; set; }
    }
}