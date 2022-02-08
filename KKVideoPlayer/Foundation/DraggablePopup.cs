namespace KKVideoPlayer.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Markup;
    using System.Xaml;
    using KKVideoPlayer.Foundation;
    using KKVideoPlayer.ViewModels;

    /// <summary>
    ///  custom Draggable Popup.
    /// </summary>
    [ContentProperty("Child")]
    [DefaultEvent("Opened")]
    [DefaultProperty("Child")]
    [Localizability(LocalizationCategory.None)]
    public partial class DraggablePopup : Popup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DraggablePopup"/> class.
        /// </summary>
        public DraggablePopup()
        {
            MouseDown += (sender, e) =>
            {
                Thumb.RaiseEvent(e);
            };

            Thumb.DragDelta += (sender, e) =>
            {
                HorizontalOffset += e.HorizontalChange;
                VerticalOffset += e.VerticalChange;
            };
        }

        /// <summary>
        /// The original child added via Xaml.
        /// </summary>
        public UIElement TrueChild { get; private set; }

        /// <summary>
        ///  Thumb.
        /// </summary>
        public Thumb Thumb { get; private set; } = new Thumb
        {
            Width = 0,
            Height = 0,
        };

        /// <inheritdoc/>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            TrueChild = Child;

            var surrogateChild = new StackPanel();

            RemoveLogicalChild(TrueChild);

            surrogateChild.Children.Add(Thumb);
            surrogateChild.Children.Add(TrueChild);

            AddLogicalChild(surrogateChild);
            Child = surrogateChild;
        }
    }
}
