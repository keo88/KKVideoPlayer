using KKVideoPlayer.Models;

namespace KKVideoPlayer.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public partial class VideoPropertyControl : UserControl
    {
        public static readonly DependencyProperty PropertyNameDependencyProperty = DependencyProperty.Register(
            "PropertyName",
            typeof(string),
            typeof(VideoPropertyControl));

        public static readonly DependencyProperty PropertyItemDependencyProperty = DependencyProperty.Register(
            "PropertyItem",
            typeof(VideoPropertyItem),
            typeof(VideoPropertyControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPropertyControl"/> class.
        /// </summary>
        public VideoPropertyControl() => InitializeComponent();

        public string PropertyName
        {
            get => (string)GetValue(PropertyNameDependencyProperty);
            set => SetValue(PropertyNameDependencyProperty, value);
        }

        public VideoPropertyItem PropertyItem
        {
            get => (VideoPropertyItem)GetValue(PropertyItemDependencyProperty);
            set => SetValue(PropertyItemDependencyProperty, value);
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            PropertyItem.OnClick();
        }
    }
}