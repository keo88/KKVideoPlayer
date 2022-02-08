namespace KKVideoPlayer.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Diagnostics;
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
    /// Interaction logic for RatingsControl.xaml.
    /// </summary>
    public partial class RatingsControl : UserControl
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingsControl"/> class.
        /// </summary>
        public RatingsControl()
        {
            InitializeComponent();
        }
        #endregion

        #region BackgroundColor

        /// <summary>
        /// BackgroundColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(SolidColorBrush),
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
#pragma warning disable SA1201 // Elements should appear in the correct order
            typeof(RatingsControl),
            new FrameworkPropertyMetadata(Brushes.Transparent,
            new PropertyChangedCallback(OnBackgroundColorChanged)));

        /// <summary>
        /// Gets or sets the BackgroundColor property.
        /// </summary>
        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the BackgroundColor property.
        /// </summary>
        private static void OnBackgroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            RatingsControl control = (RatingsControl)d;
            foreach (StarControl star in control.spStars.Children)
                star.BackgroundColor = (SolidColorBrush)e.NewValue;
        }
        #endregion

        #region StarForegroundColor

        /// <summary>
        /// StarForegroundColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty StarForegroundColorProperty =
            DependencyProperty.Register("StarForegroundColor", typeof(SolidColorBrush),
            typeof(RatingsControl),
            new FrameworkPropertyMetadata((SolidColorBrush)Brushes.Transparent,
            new PropertyChangedCallback(OnStarForegroundColorChanged)));

        /// <summary>
        /// Gets or sets the StarForegroundColor property.
        /// </summary>
        public SolidColorBrush StarForegroundColor
        {
            get { return (SolidColorBrush)GetValue(StarForegroundColorProperty); }
            set { SetValue(StarForegroundColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the StarForegroundColor property.
        /// </summary>
        private static void OnStarForegroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            RatingsControl control = (RatingsControl)d;
            foreach (StarControl star in control.spStars.Children)
                star.StarForegroundColor = (SolidColorBrush)e.NewValue;
        }
        #endregion

        #region StarOutlineColor

        /// <summary>
        /// StarOutlineColor Dependency Property.
        /// </summary>
        public static readonly DependencyProperty StarOutlineColorProperty =
            DependencyProperty.Register("StarOutlineColor", typeof(SolidColorBrush),
            typeof(RatingsControl),
            new FrameworkPropertyMetadata((SolidColorBrush)Brushes.Transparent,
            new PropertyChangedCallback(OnStarOutlineColorChanged)));

        /// <summary>
        /// Gets or sets the StarOutlineColor property.
        /// </summary>
        public SolidColorBrush StarOutlineColor
        {
            get { return (SolidColorBrush)GetValue(StarOutlineColorProperty); }
            set { SetValue(StarOutlineColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the StarOutlineColor property.
        /// </summary>
        private static void OnStarOutlineColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            RatingsControl control = (RatingsControl)d;
            foreach (StarControl star in control.spStars.Children)
                star.StarOutlineColor = (SolidColorBrush)e.NewValue;
        }
        #endregion

        #region Value

        /// <summary>
        /// Value Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(decimal),
            typeof(RatingsControl),
            new FrameworkPropertyMetadata(0.0M,
            new PropertyChangedCallback(OnValueChanged),
            new CoerceValueCallback(CoerceValueValue)));

        /// <summary>
        /// Gets or sets the Value property.
        /// </summary>
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Value property.
        /// </summary>
        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(MinimumProperty);
            d.CoerceValue(MaximumProperty);
            RatingsControl ratingsControl = (RatingsControl)d;
            SetupStars(ratingsControl);
        }

        /// <summary>
        /// Coerces the Value value.
        /// </summary>
        private static object CoerceValueValue(DependencyObject d, object value)
        {
            RatingsControl ratingsControl = (RatingsControl)d;
            decimal current = (decimal)value;
            if (current < ratingsControl.Minimum) current = ratingsControl.Minimum;
            if (current > ratingsControl.Maximum) current = ratingsControl.Maximum;
            return current;
        }
        #endregion

        #region NumberOfStars

        /// <summary>
        /// NumberOfStars Dependency Property.
        /// </summary>
        public static readonly DependencyProperty NumberOfStarsProperty =
            DependencyProperty.Register("NumberOfStars", typeof(int), typeof(RatingsControl),
                new FrameworkPropertyMetadata((int)5,
                    new PropertyChangedCallback(OnNumberOfStarsChanged),
                    new CoerceValueCallback(CoerceNumberOfStarsValue)));

        /// <summary>
        /// Gets or sets the NumberOfStars property.
        /// </summary>
        public int NumberOfStars
        {
            get { return (int)GetValue(NumberOfStarsProperty); }
            set { SetValue(NumberOfStarsProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NumberOfStars property.
        /// </summary>
        private static void OnNumberOfStarsChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(MinimumProperty);
            d.CoerceValue(MaximumProperty);
            RatingsControl ratingsControl = (RatingsControl)d;
            SetupStars(ratingsControl);
        }

        /// <summary>
        /// Coerces the NumberOfStars value.
        /// </summary>
        private static object CoerceNumberOfStarsValue(DependencyObject d, object value)
        {
            RatingsControl ratingsControl = (RatingsControl)d;
            int current = (int)value;
            if (current < ratingsControl.Minimum) current = ratingsControl.Minimum;
            if (current > ratingsControl.Maximum) current = ratingsControl.Maximum;
            return current;
        }

        #endregion

        #region Maximum

        /// <summary>
        /// Maximum Dependency Property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(RatingsControl),
                new FrameworkPropertyMetadata((int)10));

        /// <summary>
        /// Gets or sets the Maximum property.
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        #endregion

        #region Minimum

        /// <summary>
        /// Minimum Dependency Property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(RatingsControl),
                new FrameworkPropertyMetadata(1));

        /// <summary>
        /// Gets or sets the Minimum property.
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Sets up stars when Value or NumberOfStars properties change
        /// Will only show up to the number of stars requested (up to Maximum)
        /// so if Value > NumberOfStars * 1, then Value is clipped to maximum
        /// number of full stars.
        /// </summary>
        /// <param name="ratingsControl">rating.</param>
        private static void SetupStars(RatingsControl ratingsControl)
        {
            decimal localValue = ratingsControl.Value;

            ratingsControl.spStars.Children.Clear();
            for (int i = 0; i < ratingsControl.NumberOfStars; i++)
            {
                StarControl star = new StarControl();
                star.BackgroundColor = ratingsControl.BackgroundColor;
                star.StarForegroundColor = ratingsControl.StarForegroundColor;
                star.StarOutlineColor = ratingsControl.StarOutlineColor;
                if (localValue > 1)
                {
                    star.Value = 1.0m;
                }
                else if (localValue > 0)
                {
                    star.Value = localValue;
                }
                else
                {
                    star.Value = 0.0m;
                }

                localValue -= 1.0m;
                ratingsControl.spStars.Children.Insert(i, star);
            }
        }

        private void Ratings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UserControl control = (UserControl)sender;
            Point ctrl_p = control.TransformToAncestor(this).Transform(new Point(0, 0));
            Point p = e.GetPosition(this);

            double x = p.X;
            double act_x = ctrl_p.X;
            double act_w = control.ActualWidth;

            Value = Math.Round((decimal)((x - act_x) / act_w) * NumberOfStars, 1);
        }
        #endregion

#pragma warning restore SA1117 // Parameters should be on same line or separate lines
#pragma warning restore SA1201 // Elements should appear in the correct order
    }
}
