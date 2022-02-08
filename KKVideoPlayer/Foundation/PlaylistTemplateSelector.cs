namespace KKVideoPlayer.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A type of Template(view) over playlist.
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        ///  Content view over playlist.
        /// </summary>
        ContentTemplate = 0,

        /// <summary>
        ///  Detail view over playlist.
        /// </summary>
        DetailTemplate = 1,
    }

    /// <summary>
    /// Static class for managing template switch.
    /// </summary>
    public class PlaylistTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (App.ViewModel.Playlist.PlaylistTemplate)
            {
                case TemplateType.DetailTemplate:
                    return (DataTemplate)((FrameworkElement)container).FindResource("DetailDataTemplate");
                default:
                    return (DataTemplate)((FrameworkElement)container).FindResource("ContentDataTemplate");
            }
        }
    }
}
