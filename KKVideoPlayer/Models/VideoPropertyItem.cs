namespace KKVideoPlayer.Models
{
    using System;

    public class VideoPropertyItem : VideoProperty
    {
        public Action OnClick { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPropertyItem"/> class.
        /// </summary>
        /// <param name="propName">Name of video property.</param>
        /// <param name="onClick">Click event on item.</param>
        public VideoPropertyItem(string propName, Action onClick)
            : base(propName)
        {
            OnClick = onClick;
        }
    }
}