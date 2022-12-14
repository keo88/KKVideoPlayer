namespace KKVideoPlayer.Services
{
    using KKVideoPlayer.Models;

    public class ActorAutoCompleteService : AutoCompleteTextBoxService<VideoProperty>
    {
        public ActorAutoCompleteService()
        {
            VideoPropertyItems = VideosCollection.PropertiesDict[VideoPropertyEnum.Actor].Values;
        }
    }
}