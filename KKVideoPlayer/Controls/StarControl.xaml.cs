﻿namespace KKVideoPlayer.Controls
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
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for StarControl.xaml.
    /// </summary>
    public partial class StarControl : UserControl
    {
        /// <summary>
        /// BackgroundColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor",
            typeof(SolidColorBrush),
            typeof(StarControl),
            new FrameworkPropertyMetadata((SolidColorBrush)Brushes.Transparent,
            new PropertyChangedCallback(OnBackgroundColorChanged)));

        /// <summary>
        /// StarForegroundColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty StarForegroundColorProperty =
            DependencyProperty.Register("StarForegroundColor",
            typeof(SolidColorBrush),
            typeof(StarControl),
            new FrameworkPropertyMetadata((SolidColorBrush)Brushes.Transparent,
            new PropertyChangedCallback(OnStarForegroundColorChanged)));

        /// <summary>
        /// StarOutlineColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty StarOutlineColorProperty =
            DependencyProperty.Register("StarOutlineColor",
            typeof(SolidColorBrush),
            typeof(StarControl),
            new FrameworkPropertyMetadata((SolidColorBrush)Brushes.Transparent,
            new PropertyChangedCallback(OnStarOutlineColorChanged)));

        /// <summary>
        /// Value Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
            typeof(decimal),
            typeof(StarControl),
            new FrameworkPropertyMetadata(0.0M,
            new PropertyChangedCallback(OnValueChanged),
            new CoerceValueCallback(CoerceValueValue)));

        /// <summary>
        /// Maximum Dependency Property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum",
                typeof(decimal),
                typeof(StarControl),
                new FrameworkPropertyMetadata(1.0M));

        /// <summary>
        /// Minimum Dependency Property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum",
                typeof(decimal),
                typeof(StarControl),
                new FrameworkPropertyMetadata(0.0M));

        private const int STARSIZE = 20;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StarControl"/> class.
        /// </summary>
        public StarControl()
        {
            this.DataContext = this;
            InitializeComponent();

            gdStar.Width = STARSIZE;
            gdStar.Height = STARSIZE;
            gdStar.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, STARSIZE, STARSIZE)
            };

            mask.Width = STARSIZE;
            mask.Height = STARSIZE;
        }
        #endregion

        /// <summary>
        /// Gets or sets the BackgroundColor property.
        /// </summary>
        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the StarForegroundColor property.
        /// </summary>
        public SolidColorBrush StarForegroundColor
        {
            get { return (SolidColorBrush)GetValue(StarForegroundColorProperty); }
            set { SetValue(StarForegroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the StarOutlineColor property.
        /// </summary>
        public SolidColorBrush StarOutlineColor
        {
            get { return (SolidColorBrush)GetValue(StarOutlineColorProperty); }
            set { SetValue(StarOutlineColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Value property.
        /// </summary>
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Maximum property.
        /// </summary>
        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Minimum property.
        /// </summary>
        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Handles changes to the BackgroundColor property.
        /// </summary>
        private static void OnBackgroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            StarControl control = (StarControl)d;
            control.gdStar.Background = (SolidColorBrush)e.NewValue;
            control.mask.Fill = (SolidColorBrush)e.NewValue;
        }

        /// <summary>
        /// Handles changes to the StarForegroundColor property.
        /// </summary>
        private static void OnStarForegroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            StarControl control = (StarControl)d;
            control.starForeground.Fill = (SolidColorBrush)e.NewValue;
        }

        /// <summary>
        /// Handles changes to the StarOutlineColor property.
        /// </summary>
        private static void OnStarOutlineColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            StarControl control = (StarControl)d;
            control.starOutline.Stroke = (SolidColorBrush)e.NewValue;
        }

        /// <summary>
        /// Handles changes to the Value property.
        /// </summary>
        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(MinimumProperty);
            d.CoerceValue(MaximumProperty);
            StarControl starControl = (StarControl)d;
            if (starControl.Value == 0.0m)
            {
                starControl.starForeground.Fill = Brushes.Gray;
            }
            else
            {
                starControl.starForeground.Fill = starControl.StarForegroundColor;
            }

            int marginLeftOffset = (int)(starControl.Value * (decimal)STARSIZE);
            starControl.mask.Margin = new Thickness(marginLeftOffset, 0, 0, 0);
            starControl.InvalidateArrange();
            starControl.InvalidateMeasure();
            starControl.InvalidateVisual();
        }

        /// <summary>
        /// Coerces the Value value.
        /// </summary>
        private static object CoerceValueValue(DependencyObject d, object value)
        {
            StarControl starControl = (StarControl)d;
            decimal current = (decimal)value;
            if (current < starControl.Minimum) current = starControl.Minimum;
            if (current > starControl.Maximum) current = starControl.Maximum;
            return current;
        }
    }
}
