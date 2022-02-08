namespace KKVideoPlayer
{
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
    using System.Windows.Shapes;

    /// <summary>
    /// VideoControlWindow.xaml에 대한 상호 작용 논리.
    /// </summary>
    public partial class VideoControlWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoControlWindow"/> class.
        /// Video Control Window.
        /// </summary>
        public VideoControlWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Top = Mouse.GetPosition(null).Y;
            Left = Mouse.GetPosition(null).X;
        }
    }
}
