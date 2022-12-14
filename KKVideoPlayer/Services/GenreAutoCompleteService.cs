namespace KKVideoPlayer.Services
{
    using KKVideoPlayer.Models;

    public class GenreAutoCompleteService : AutoCompleteTextBoxService<VideoProperty>
    {
        public GenreAutoCompleteService()
        {
            VideoPropertyItems = VideosCollection.PropertiesDict[VideoPropertyEnum.Genre].Values;
        }
    }
}