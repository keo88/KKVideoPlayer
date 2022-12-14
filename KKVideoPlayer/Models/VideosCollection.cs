namespace KKVideoPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using KKVideoPlayer.Foundation;

    public enum VideoPropertyEnum {
        Actor,
        Genre,
        Series,
        Director,
        Company
    }

    public class VideosCollection : ObservableCollection<VideoEntry>
    {
        public static readonly Dictionary<VideoPropertyEnum, Dictionary<string, VideoProperty>> PropertiesDict = new ();
        public static VideosCollection Instance = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="VideosCollection"/> class.
        /// </summary>
        private VideosCollection()
        {
            PropertiesDict.Clear();
            foreach (VideoPropertyEnum vEnum in Enum.GetValues<VideoPropertyEnum>())
            {
                PropertiesDict.Add(vEnum, new Dictionary<string, VideoProperty>());
            }
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, VideoEntry item)
        {
            if (item != null)
            {
                if (item.Actors != null)
                {
                    AddAllVideoProperties(PropertiesDict[VideoPropertyEnum.Actor], item.Actors);
                }

                if (item.Genres != null)
                {
                    AddAllVideoProperties(PropertiesDict[VideoPropertyEnum.Genre], item.Genres);
                }

                if (item.Series != null)
                {
                    AddToVideoProperties(PropertiesDict[VideoPropertyEnum.Series], item.Series);
                }

                if (item.Directors != null)
                {
                    AddAllVideoProperties(PropertiesDict[VideoPropertyEnum.Director], item.Directors);
                }

                if (item.Companies != null)
                {
                    AddAllVideoProperties(PropertiesDict[VideoPropertyEnum.Company], item.Companies);
                }
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            VideoEntry removedVideo = this[index];

            if (removedVideo.Actors != null)
            {
                RemoveAllVideoProperties(PropertiesDict[VideoPropertyEnum.Actor], removedVideo.Actors);
            }

            if (removedVideo.Genres != null)
            {
                RemoveAllVideoProperties(PropertiesDict[VideoPropertyEnum.Genre], removedVideo.Genres);
            }

            if (removedVideo.Series != null)
            {
                RemoveVideoProperties(PropertiesDict[VideoPropertyEnum.Series], removedVideo.Series);
            }

            if (removedVideo.Directors != null)
            {
                RemoveAllVideoProperties(PropertiesDict[VideoPropertyEnum.Director], removedVideo.Directors);
            }

            if (removedVideo.Companies != null)
            {
                RemoveAllVideoProperties(PropertiesDict[VideoPropertyEnum.Company], removedVideo.Companies);
            }

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            Instance = new VideosCollection();

            base.ClearItems();
        }

        private void AddAllVideoProperties(IDictionary<string, VideoProperty> dict, IEnumerable<string> vpKeys)
        {
            foreach (string vpKey in vpKeys)
            {
                AddToVideoProperties(dict, vpKey);
            }
        }

        private void AddToVideoProperties(IDictionary<string, VideoProperty> dict, string vpKey)
        {
            if (dict.ContainsKey(vpKey))
            {
                dict[vpKey].Count += 1;
            }
            else
            {
                var videoProperty = new VideoProperty(vpKey);
                dict.Add(vpKey, videoProperty);
            }
        }

        private void RemoveAllVideoProperties(IDictionary<string, VideoProperty> dict, IEnumerable<string> vpKeys)
        {
            foreach (string vpKey in vpKeys)
            {
                RemoveVideoProperties(dict, vpKey);
            }
        }

        private void RemoveVideoProperties(IDictionary<string, VideoProperty> dict, string vpKey)
        {
            if (!dict.ContainsKey(vpKey)) return;

            if (dict[vpKey].Count > 1)
            {
                dict[vpKey].Count -= 1;
            }
            else
            {
                dict.Remove(vpKey);
            }
        }
    }
}