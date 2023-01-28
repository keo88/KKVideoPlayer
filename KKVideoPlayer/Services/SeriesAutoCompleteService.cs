namespace KKVideoPlayer.Services
{
    using KKVideoPlayer.Foundation;
    using KKVideoPlayer.Models;

    public class SeriesAutoCompleteService : AutoCompleteTextBoxService<VideoProperty>
    {
        public SeriesAutoCompleteService()
        {
            VideoPropertyItems = VideosCollection.PropertiesDict[VideoPropertyEnum.Series].Values;
        }
    }
}