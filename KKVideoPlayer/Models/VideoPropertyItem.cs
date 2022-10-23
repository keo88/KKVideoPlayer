namespace KKVideoPlayer.Models
{
    using System;

    public class VideoPropertyItem
    {
        public string PropName { get; set; }
        public Action OnClick { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPropertyItem"/> class.
        /// </summary>
        /// <param name="propName">Name of video property.</param>
        /// <param name="onClick">Click event on item.</param>
        public VideoPropertyItem(string propName, Action onClick)
        {
            PropName = propName;
            OnClick = onClick;
        }
    }
}