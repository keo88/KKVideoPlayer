namespace KKVideoPlayer.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SQLite;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Video entry which contains detailed information about the video.
    /// </summary>
    public class VideoEntry : INotifyPropertyChanged
    {
        private static readonly Random Rnd = new Random();
        private decimal m_rating;
        private bool initialFlag = true;
        private string m_DvdId;
        private string m_Title;
        private string m_Filepath;
        private int m_ViewCount;
        private List<string> m_Genres;
        private List<string> m_Actors;
        private DateTime m_DbDate;
        private DateTime m_FileDate;
        private DateTime m_ReleaseDate;
        private List<string> m_Companies;
        private List<string> m_Directors;
        private string m_Series;
        private string m_Comment;
        private long m_FileSize;
        private byte[] m_Thumbnail;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoEntry"/> class.
        ///  Null construction of videoentry.
        /// </summary>
        public VideoEntry()
        {
            DvdId = string.Empty;
            Title = string.Empty;
            Filepath = string.Empty;
            ViewCount = 0;
            Rating = 0.0M;
            Genres = new List<string>();
            Actors = new List<string>();
            DbDate = DateTime.MinValue;
            FileDate = DateTime.MinValue;
            ReleaseDate = DateTime.MinValue;
            Directors = new();
            Companies = new();
            Series = string.Empty;
            Comment = string.Empty;
            FileSize = 0;
            Thumbnail = Array.Empty<byte>();
            RandomInt = Rnd.Next();
            initialFlag = true;
        }

#pragma warning disable SA1611 // Element parameters should be documented
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoEntry"/> class.
        /// </summary>
        public VideoEntry(
            string dvdId = "",
            string title = "",
            string filepath = "",
            string viewCount = "0",
            string rating = "0.0",
            string genresStr = "",
            string actorsStr = "",
            string dbDate = "",
            string fileDate = "",
            string releaseDate = "",
            string directorsStr = "",
            string companiesStr = "",
            string series = "",
            string comment = "",
            long fileSize = 0,
            byte[] thumb = null)
#pragma warning restore SA1611 // Element parameters should be documented
        {
            initialFlag = true;

            DvdId = dvdId;
            Title = title;
            Filepath = filepath;
            ViewCount = int.Parse(viewCount);
            Rating = decimal.Parse(rating);
            Genres = string.IsNullOrWhiteSpace(genresStr)
                ? new List<string>()
                : genresStr.Split(',').Select(p => p.Trim()).ToList();
            Actors = string.IsNullOrWhiteSpace(actorsStr)
                ? new List<string>()
                : actorsStr.Split(',').Select(p => p.Trim()).ToList();
            DbDate = string.IsNullOrEmpty(dbDate) ? DateTime.MinValue : DateTime.ParseExact(dbDate, "yyyy-MM-dd", null);
            FileDate = string.IsNullOrEmpty(fileDate) ? DateTime.MinValue : DateTime.ParseExact(fileDate, "yyyy-MM-dd HH:mm:ss", null);
            ReleaseDate = string.IsNullOrEmpty(releaseDate) ? DateTime.MinValue : DateTime.ParseExact(releaseDate, "yyyy-MM-dd", null);
            Directors = directorsStr.Split(',').Select(p => p.Trim()).ToList();
            Companies = companiesStr.Split(',').Select(p => p.Trim()).ToList();
            Series = series;
            Comment = comment;
            FileSize = fileSize;
            Thumbnail = thumb;
            RandomInt = Rnd.Next();

            initialFlag = false;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///  The unique ID of the video. It can be the title, for series of alphabets and numbers.
        /// </summary>
        public string DvdId
        {
            get => m_DvdId;
            set
            {
                m_DvdId = value;
                OnPropertyChanged(nameof(DvdId));
            }
        }

        /// <summary>
        ///  Title of the video.
        /// </summary>
        public string Title
        {
            get => m_Title;
            set
            {
                m_Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        ///  File name.
        /// </summary>
        public string Filepath
        {
            get => m_Filepath;
            set
            {
                m_Filepath = value;
                OnPropertyChanged(nameof(Filepath));
            }
        }

        /// <summary>
        ///  How many times the video has been played.
        /// </summary>
        public int ViewCount
        {
            get => m_ViewCount;
            set
            {
                m_ViewCount = value;
                OnPropertyChanged(nameof(ViewCount));
            }
        }

        /// <summary>
        ///  A rating of the video.
        /// </summary>
        public decimal Rating
        {
            get => m_rating;

            set
            {
                m_rating = value;
                OnPropertyChanged(nameof(Rating));

                if (!initialFlag)
                {
                    using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + App.ViewModel.CurrentVideoDirectory + "/deepdark.db"))
                    {
                        conn.Open();
                        string sql;

                        using (SQLiteTransaction tr = conn.BeginTransaction())
                        {
                            SQLiteCommand cmd = new SQLiteCommand(string.Empty, conn);
                            cmd.Transaction = tr;

                            sql = $"UPDATE Files SET stars=@rating WHERE filepath = @filepath";
                            cmd.CommandText = sql;
                            cmd.Parameters.AddWithValue("@rating", m_rating.ToString());
                            cmd.Parameters.AddWithValue("@filepath", Filepath);
                            cmd.ExecuteNonQuery();

                            tr.Commit();
                        }

                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        ///  Genres of the video.
        /// </summary>
        public List<string> Genres
        {
            get => m_Genres;
            set
            {
                m_Genres = value;
                OnPropertyChanged(nameof(Genres));
            }
        }

        /// <summary>
        ///  Actors in the video.
        /// </summary>
        public List<string> Actors
        {
            get => m_Actors;
            set
            {
                m_Actors = value;
                OnPropertyChanged(nameof(Actors));
            }
        }

        /// <summary>
        ///  The date which the video is added to the DB.
        /// </summary>
        public DateTime DbDate
        {
            get => m_DbDate;
            set
            {
                m_DbDate = value;
                OnPropertyChanged(nameof(DbDate));
            }
        }

        /// <summary>
        ///  The last modified date of the video.
        /// </summary>
        public DateTime FileDate
        {
            get => m_FileDate;
            set
            {
                m_FileDate = value;
                OnPropertyChanged(nameof(FileDate));
            }
        }

        /// <summary>
        ///  The date which the video was released.
        /// </summary>
        public DateTime ReleaseDate
        {
            get => m_ReleaseDate;
            set
            {
                m_ReleaseDate = value;
                OnPropertyChanged(nameof(ReleaseDate));
            }
        }

        /// <summary>
        ///  The director of the video.
        /// </summary>
        public List<string> Directors
        {
            get => m_Directors;
            set
            {
                m_Directors = value;
                OnPropertyChanged(nameof(Directors));
            }
        }

        /// <summary>
        ///  The company involved in making the video.
        /// </summary>
        public List<string> Companies
        {
            get => m_Companies;
            set
            {
                m_Companies = value;
                OnPropertyChanged(nameof(Companies));
            }
        }

        /// <summary>
        ///  Series.
        /// </summary>
        public string Series
        {
            get => m_Series;
            set
            {
                m_Series = value;
                OnPropertyChanged(nameof(Series));
            }
        }

        /// <summary>
        ///  A simple user-made comment on video.
        /// </summary>
        public string Comment
        {
            get => m_Comment;
            set
            {
                m_Comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        /// <summary>
        ///  File size in bytes.
        /// </summary>
        public long FileSize
        {
            get => m_FileSize;
            set
            {
                m_FileSize = value;
                OnPropertyChanged(nameof(FileSize));
            }
        }

        /// <summary>
        ///  Thumbnail of the video screenshot.
        /// </summary>
        public byte[] Thumbnail
        {
            get => m_Thumbnail;
            set
            {
                m_Thumbnail = value;
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        /// <summary>
        ///  Random integer used for random sorting.
        /// </summary>
        public int RandomInt { get; }

        /// <summary>
        ///  On property changed.
        /// </summary>
        /// <param name="name">name.</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
