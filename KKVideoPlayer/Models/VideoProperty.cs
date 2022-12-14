namespace KKVideoPlayer.Models
{
    public class VideoProperty
    {
        public string PropName { get; set; }
        
        public int Count { get; set; }

        public VideoProperty(string propName)
        {
            PropName = propName;
            Count = 1;
        }
    }
}