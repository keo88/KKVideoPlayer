using System.Diagnostics;

namespace KKVideoPlayer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Xaml;
    using Unosquare.FFME.Common;
    using Controls;
    using Foundation;

    /// <summary>
    /// Represents the Playlist.
    /// </summary>
    /// <seealso cref="AttachedViewModel" />
    public sealed class PlaylistViewModel : AttachedViewModel
    {
        #region Private State

        // Constants
        private const int MinimumSearchLength = 2;

        private static string[] VideoExtensions =
            {"mp4", "mov", "wmv", "avi", "avchd", "flv", "f4v", "swf", "mkv", "webm"};

        private static string[] SubtitleExtensions = {"srt", "smi"};

        // Private state management
        private readonly TimeSpan SearchActionDelay = TimeSpan.FromSeconds(0.25);
        private bool HasTakenThumbnail;
        private DeferredAction SearchAction;
        private string FilterString = string.Empty;

        // Property Backing
        private bool m_IsInOpenMode = App.IsInDesignMode;
        private bool m_IsPlaylistEnabled = true;
        private string m_OpenMediaSource = string.Empty;
        private string m_PlaylistSearchString = string.Empty;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        public PlaylistViewModel(RootViewModel root)
            : base(root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            Videos = new ObservableCollection<VideoEntry>();
            VideosViewSource = new CollectionViewSource();
            VideosViewSource.Source = Videos;

            PlaylistTemplate = TemplateType.ContentTemplate;

            // Directory list initialization
            if (File.Exists(root.AppDataDirectory + "/directories.xaml"))
            {
                DirectoryList =
                    (ObservableCollection<string>) XamlServices.Load(root.AppDataDirectory + "/directories.xaml");
            }
            else
            {
                DirectoryList = new ObservableCollection<string>();
            }

            SortbyList = new List<string>();
            SortbyList.Add("Dvd Id");
            SortbyList.Add("Title");
            SortbyList.Add("File Name");
            SortbyList.Add("Actor");
            SortbyList.Add("Genre");
            SortbyList.Add("Release Date");
            SortbyList.Add("Last Modified Date");
            SortbyList.Add("Rating");
            SortbyList.Add("Random");

            // Set and create a thumbnails directory
            ThumbsDirectory = Path.Combine(root.AppDataDirectory, "Thumbnails");
            if (Directory.Exists(ThumbsDirectory) == false)
                Directory.CreateDirectory(ThumbsDirectory);

            // Set and create a index directory
            IndexDirectory = Path.Combine(root.AppDataDirectory, "SeekIndexes");
            if (Directory.Exists(IndexDirectory) == false)
                Directory.CreateDirectory(IndexDirectory);

            PlaylistFilePath = Path.Combine(root.AppDataDirectory, "ffme.m3u8");

            EntriesView = VideosViewSource.View;

            EntriesView.Filter = item =>
            {
                var searchString = PlaylistSearchString;

                if (string.IsNullOrWhiteSpace(searchString) || searchString.Trim().Length < MinimumSearchLength)
                    return true;

                if (item is VideoEntry entry)
                {
                    var title = entry.Title ?? string.Empty;
                    var source = entry.Filepath ?? string.Empty;
                    List<string> genres = entry.Genres ?? new List<string>();
                    List<string> actors = entry.Actors ?? new List<string>();

                    string[] keywords = searchString.ToUpperInvariant().Split(',');

                    foreach (string keyword in keywords)
                    {
                        if (string.IsNullOrWhiteSpace(keyword))
                            continue;
                        if (keyword.Contains("ACTOR:"))
                        {
                            string[] split = keyword.Split("ACTOR:");
                            string actorName = split[split.Length - 1].Trim();
                            if (!actors.Contains(actorName))
                                return false;
                        }
                        else if (keyword.Contains("GENRE:"))
                        {
                            string[] split = keyword.Split("GENRE:");
                            string genreName = split[split.Length - 1].Trim();
                            if (!genres.Contains(genreName))
                                return false;
                        }
                        else
                        {
                            string[] pieces = keyword.Split(" ");

                            foreach (string piece in pieces)
                            {
                                if (title.ToUpperInvariant().Contains(piece) ||
                                    source.ToUpperInvariant().Contains(piece) ||
                                    genres.Contains(piece) ||
                                    actors.Contains(piece))
                                {
                                    continue;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            };

            // BatchFormatter();
        }

        /// <summary>
        /// Video Control Window.
        /// </summary>
        public Window VideoCtrlWindow { get; set; }

        /// <summary>
        /// Gets the custom playlist. Do not use for data-binding.
        /// </summary>
        public CustomPlaylistEntryCollection Entries { get; }

        /// <summary>
        /// Gets the custom playlist entries as a view that can be used in data binding scenarios.
        /// </summary>
        public ICollectionView EntriesView { get; }

        /// <summary>
        /// Gets the full path where thumbnails are stored.
        /// </summary>
        public string ThumbsDirectory { get; }

        /// <summary>
        /// Gets the seek index base directory.
        /// </summary>
        public string IndexDirectory { get; }

        /// <summary>
        /// Gets the playlist file path.
        /// </summary>
        public string PlaylistFilePath { get; }

        /// <summary>
        /// Gets or sets the playlist search string.
        /// </summary>
        public string PlaylistSearchString
        {
            get => m_PlaylistSearchString;
            set
            {
                if (!SetProperty(ref m_PlaylistSearchString, value))
                    return;

                if (SearchAction == null)
                {
                    SearchAction = DeferredAction.Create(context =>
                    {
                        var futureSearch = PlaylistSearchString ?? string.Empty;
                        var currentSearch = FilterString ?? string.Empty;

                        if (currentSearch == futureSearch) return;
                        if (futureSearch.Length < MinimumSearchLength &&
                            currentSearch.Length < MinimumSearchLength) return;

                        EntriesView.Refresh();
                        FilterString = new string(m_PlaylistSearchString.ToCharArray());
                    });
                }

                SearchAction.Defer(SearchActionDelay);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is playlist enabled.
        /// </summary>
        public bool IsPlaylistEnabled
        {
            get => m_IsPlaylistEnabled;
            set => SetProperty(ref m_IsPlaylistEnabled, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in open mode.
        /// </summary>
        public bool IsInOpenMode
        {
            get => m_IsInOpenMode;
            set => SetProperty(ref m_IsInOpenMode, value);
        }

        /// <summary>
        /// Gets or sets the open model URL.
        /// </summary>
        public string OpenMediaSource
        {
            get => m_OpenMediaSource;
            set => SetProperty(ref m_OpenMediaSource, value);
        }

        /// <summary>
        ///  Tem.
        /// </summary>
        public TemplateType PlaylistTemplate { get; set; }

        /// <summary>
        ///  A list of file directories where deepdark.db is located.
        /// </summary>
        public ObservableCollection<string> DirectoryList { get; set; }

        /// <summary>
        ///  A selected directory.
        /// </summary>
        public string SelectedDirectory { get; set; }

        /// <summary>
        ///  Selected Directory for Migration Combobox.
        /// </summary>
        public string SelectedMigrateDirectory { get; set; }

        /// <summary>
        ///  The list of sort options.
        /// </summary>
        public List<string> SortbyList { get; set; }

        /// <summary>
        ///  Itemsource to FileLists.
        /// </summary>
        public ObservableCollection<VideoEntry> Videos { get; set; }

        /// <summary>
        ///  A collection View source for Playlists.
        /// </summary>
        public CollectionViewSource VideosViewSource { get; set; }

        /// <summary>
        ///  The selected video.
        /// </summary>
        public VideoEntry SelectedVideo { get; set; }

        /// <summary>
        ///  The selected genre.
        /// </summary>
        public string SelectedGenre { get; set; }

        /// <summary>
        ///  The selected actor.
        /// </summary>
        public string SelectedActor { get; set; }

        /// <summary>
        ///  Migrate subtitle.
        /// </summary>
        /// <param name="sourceBody">file name of source.</param>
        /// <param name="targetBody">file name of target.</param>
        public static void MigrateSubtitle(string sourceBody, string targetBody)
        {
            if (Path.HasExtension(sourceBody))
            {
                sourceBody = Path.Combine(Path.GetDirectoryName(sourceBody),
                    Path.GetFileNameWithoutExtension(sourceBody));
            }

            if (Path.HasExtension(targetBody))
            {
                targetBody = Path.Combine(Path.GetDirectoryName(targetBody),
                    Path.GetFileNameWithoutExtension(targetBody));
            }

            foreach (string extender in SubtitleExtensions)
            {
                try
                {
                    string fileName = sourceBody + "." + extender;
                    if (File.Exists(fileName))
                    {
                        string targetFileName = targetBody + "." + extender;
                        File.Move(fileName, targetFileName);
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        #region Update Playlists

        /// <summary>
        ///  Update Playlists.
        /// </summary>
        public void RefreshPlaylist()
        {
            if (string.IsNullOrWhiteSpace(Root.CurrentVideoDirectory) && !Directory.Exists(Root.CurrentVideoDirectory))
                return;
            byte[] emptyThumb = File.ReadAllBytes("../../../Empty.png");
            string strConn = "Data Source=" + Root.CurrentVideoDirectory + "/deepdark.db";
            using (SQLiteConnection conn = new SQLiteConnection(strConn))
            {
                conn.Open();
                string sql =
                    "SELECT * FROM Files left outer natural join Dvd";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                Videos.Clear();

                while (rdr.Read())
                {
                    string filepath = rdr["filepath"].ToString();
                    string f_date = rdr["fileDate"].ToString();
                    long f_size = 0;

                    string full_path = Root.CurrentVideoDirectory + "\\" + filepath;

                    if (File.Exists(full_path))
                    {
                        f_date = File.GetLastWriteTime(Root.CurrentVideoDirectory + "\\" + filepath)
                            .ToString("yyyy-MM-dd HH:mm:ss");
                        f_size = new FileInfo(full_path).Length;
                    }

                    Videos.Add(new VideoEntry(
                        dvdId: rdr["dvdId"].ToString(),
                        title: rdr["dvdTitle"].ToString(),
                        filepath: filepath,
                        viewCount: rdr["count"].ToString(),
                        rating: rdr["stars"].ToString(),
                        genresStr: rdr["genres"].ToString(),
                        actorsStr: rdr["actors"].ToString(),
                        dbDate: rdr["dbDate"].ToString(),
                        fileDate: f_date,
                        releaseDate: rdr["releaseDate"].ToString(),
                        directorsStr: rdr["director"].ToString(),
                        companiesStr: rdr["company"].ToString(),
                        series: rdr["series"].ToString(),
                        comment: rdr["hashTag"].ToString(),
                        fileSize: f_size,
                        thumb: rdr["thumb"] is DBNull ? emptyThumb : (byte[]) rdr["thumb"]));
                }

                conn.Close();
            }
        }

        /// <summary>
        ///  Check if there are new files in the current directory, and add them to the file Db.
        /// </summary>
        /// <param name="targetDirectory">target directory to update.</param>
        public void UpdatePlaylist(string targetDirectory)
        {
            if (targetDirectory == null)
                return;

            List<string> dbFilepaths = new();
            List<string> actualFilepaths = new();

            string[] items = Directory.GetFiles(targetDirectory);
            foreach (string item in items)
            {
                bool isInDb = VideoExtensions.Any(x => item.ToLowerInvariant().EndsWith(x));
                if (isInDb)
                {
                    string[] splittedItem = item.Split('\\');
                    actualFilepaths.Add(splittedItem[splittedItem.Length - 1]);
                }
            }

            SqlManager.CheckOrCreateDb(targetDirectory);
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + targetDirectory + "/deepdark.db"))
            {
                conn.Open();
                string sql = "SELECT filepath FROM Files";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    dbFilepaths.Add(rdr["filepath"].ToString());
                }

                List<string> newFilepath = actualFilepaths
                    .Where(item => dbFilepaths.All(f => !item.Equals(f, StringComparison.Ordinal))).ToList();
                List<string> inexistantFilepath = dbFilepaths
                    .Where(item => actualFilepaths.All(f => !item.Equals(f, StringComparison.Ordinal))).ToList();

                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    cmd = new SQLiteCommand(string.Empty, conn);
                    cmd.Transaction = tr;

                    foreach (string item in newFilepath)
                    {
                        sql =
                            "INSERT INTO Files(filepath, stars, dbDate, fileDate) VALUES (@filepath, @stars, @dbDate, @fileDate)";
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@filepath", item);
                        cmd.Parameters.AddWithValue("@stars", 0.0);
                        cmd.Parameters.AddWithValue("@dbDate", DateTime.Now.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fileDate",
                            File.GetLastWriteTime(targetDirectory + "\\" + item).ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }

                    tr.Commit();
                }

                conn.Close();
            }

            RefreshPlaylist();
        }

        /// <summary>
        ///  Delete items in a list of item path in DB, files table.
        /// </summary>
        /// <param name="itemPathList"> The list of item path to delete in DB.</param>
        /// <param name="deleteFiles"> Option to delete items with files.</param>
        public void DeleteItemFromList(IEnumerable<string> itemPathList, bool deleteFiles = false)
        {
            try
            {
                using (SQLiteConnection conn =
                       new SQLiteConnection("Data Source=" + Root.CurrentVideoDirectory + "/deepdark.db"))
                {
                    conn.Open();
                    string sql = string.Empty;
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);

                    using (SQLiteTransaction tr = conn.BeginTransaction())
                    {
                        cmd.Transaction = tr;

                        foreach (string itemPath in itemPathList)
                        {
                            if (deleteFiles)
                            {
                                if (Root.PlayingVideo != null &&
                                    Root.PlayingVideo.Filepath.Equals(itemPath, StringComparison.Ordinal))
                                {
                                    _ = MessageBox.Show("Close the video and try again.", "Error", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    continue;
                                }

                                File.Delete(Path.Combine(Root.CurrentVideoDirectory, itemPath));
                            }

                            sql = $"DELETE FROM Files WHERE filepath = '{itemPath}'";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }

                    conn.Close();
                }

                RefreshPlaylist();
            }
            catch (SQLiteException se)
            {
                MessageBox.Show("Something went wrong.\n" + se.Message);
            }
        }

        #endregion

        #region File Tags Management

        /// <summary>
        ///  Migrate list of video entries to another DB.
        /// </summary>
        /// <param name="targetVideos">list of videos to migrate.</param>
        /// <param name="targetHighlights">list of highlights to migrate.</param>
        /// <param name="sourceFolderPath">source directory to be migrated.</param>
        /// <param name="targetFolderPath">target directory to be migrated.</param>
        public async void MigrateFile(List<VideoEntry> targetVideos, List<List<HighlightEntry>> targetHighlights,
            string sourceFolderPath, string targetFolderPath)
        {
            foreach (VideoEntry v in targetVideos)
            {
                if (Root.PlayingVideo != null && (bool) Root.PlayingVideo?.Filepath.Equals(v.Filepath))
                {
                    continue;
                }
                else
                {
                    List<string> splited = v.Filepath.Split('.').SkipLast<string>(1).ToList<string>();
                    string body = string.Join('.', splited);

                    // File.Move(Path.Combine(sourceFolderPath, v.Filepath), Path.Combine(targetFolderPath, v.Filepath));
                    Task moveTask = MoveAsync(Path.Combine(sourceFolderPath, v.Filepath),
                        Path.Combine(targetFolderPath, v.Filepath));
                    MigrateSubtitle(Path.Combine(sourceFolderPath, body), Path.Combine(targetFolderPath, body));
                    await moveTask;
                }
            }

            UpdatePlaylist(targetFolderPath);

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + targetFolderPath + "/deepdark.db"))
            {
                conn.Open();
                string sql;

                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    SQLiteCommand cmd = new SQLiteCommand(string.Empty, conn);
                    cmd.Transaction = tr;

                    for (int index = 0; index < targetVideos.Count; index++)
                    {
                        VideoEntry v = targetVideos[index];
                        List<HighlightEntry> highList = targetHighlights[index];

                        sql = $"UPDATE Files SET dvdId=@dvdId, thumb=@thumb WHERE filepath = @filepath";
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@dvdId", v.DvdId);
                        cmd.Parameters.AddWithValue("@thumb", v.Thumbnail);
                        cmd.Parameters.AddWithValue("@filepath", v.Filepath);
                        cmd.ExecuteNonQuery();

                        sql = $"DELETE FROM Dvd WHERE dvdId = '{v.DvdId}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        sql =
                            "INSERT INTO Dvd VALUES (@dvdId, @dvdTitle, @actors, @genres, @releaseDate, @director, @company, @series, @metaSource)";
                        cmd.CommandText = sql;

                        cmd.Parameters.AddWithValue("@dvdId", v.DvdId);
                        cmd.Parameters.AddWithValue("@dvdTitle", v.Title);
                        cmd.Parameters.AddWithValue("@actors", string.Join(", ", v.Actors));
                        cmd.Parameters.AddWithValue("@genres", string.Join(", ", v.Genres));
                        cmd.Parameters.AddWithValue("@releaseDate",
                            v.ReleaseDate != DateTime.MinValue ? v.ReleaseDate.ToString("yyyy-MM-dd") : null);
                        cmd.Parameters.AddWithValue("@director", string.Join(",", v.Directors));
                        cmd.Parameters.AddWithValue("@company", string.Join(",", v.Companies));
                        cmd.Parameters.AddWithValue("@series", v.Series);
                        cmd.Parameters.AddWithValue("@metaSource", null);
                        cmd.ExecuteNonQuery();

                        sql = $"DELETE FROM Favorites WHERE filepath = '{v.Filepath}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        foreach (HighlightEntry he in highList)
                        {
                            sql =
                                "INSERT INTO Favorites(filepath, start, length, thumb) VALUES (@filepath, @start, @length, @thumb)";
                            cmd.CommandText = sql;

                            cmd.Parameters.AddWithValue("@filepath", v.Filepath);
                            cmd.Parameters.AddWithValue("@start", he.HighlightText);
                            cmd.Parameters.AddWithValue("@length", 0.0);
                            cmd.Parameters.AddWithValue("@thumb", he.HighlightImage);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tr.Commit();
                }

                conn.Close();
            }
        }

        #endregion

        /// <summary>
        ///  Change sort feature.
        /// </summary>
        /// <param name="sortBy">sort by this attribute.</param>
        /// <param name="dir">List sort direction.</param>
        public void PlaylistSortDescriptionUpdate(string sortBy, ListSortDirection dir)
        {
            EntriesView.SortDescriptions.Clear();
            EntriesView.SortDescriptions.Add(new SortDescription(sortBy, dir));
        }

        /// <inheritdoc />
        internal override void OnApplicationLoaded()
        {
            base.OnApplicationLoaded();
            var m = Root.MediaElement;

            // m.WhenChanged(() => IsPlaylistEnabled = m.IsOpening == false, nameof(m.IsOpening));
            m.MediaOpened += OnMediaOpened;
            m.RenderingVideo += OnRenderingVideo;

            VideoCtrlWindow = new VideoControlWindow();
        }

        /// <summary>
        /// Add new directory to directory list.
        /// </summary>
        /// <param name="directoryPath">string path to directory.</param>
        public void AddDirectory(string directoryPath)
        {
            DirectoryList.Add(directoryPath);
            XamlServices.Save(Root.AppDataDirectory + "/directories.xaml", DirectoryList);
        }

        public void RemoveDirectory(string directoryPath)
        {
            try
            {
                int targetIndex = DirectoryList.IndexOf(directoryPath);

                DirectoryList.RemoveAt(targetIndex);
                XamlServices.Save(Root.AppDataDirectory + "/directories.xaml", DirectoryList);
            }
            catch
            {
                Debug.WriteLine("Failed to delete folder");
            }
        }

        public bool ValidateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                RemoveDirectory(directoryPath);
                return false;
            }
            else
            {
                return true;
            }
        }

        private static async Task MoveAsync(string sourceFilePath, string destFilePath)
        {
            if (!File.Exists(sourceFilePath) || File.Exists(destFilePath))
                return;

            FileInfo sourceFileInfo = new FileInfo(sourceFilePath);
            string sourceDrive = Path.GetPathRoot(sourceFileInfo.FullName);

            FileInfo destFileInfo = new FileInfo(destFilePath);
            string destDrive = Path.GetPathRoot(destFileInfo.FullName);

            if (sourceDrive == destDrive)
            {
                File.Move(sourceFilePath, destFilePath);
                return;
            }

            using (Stream source = File.Open(sourceFilePath, FileMode.Open))
            {
                using (Stream dest = File.Create(destFilePath))
                {
                    await source.CopyToAsync(dest);
                }
            }
        }

        private void BatchFormatter()
        {
            foreach (string targetDirectory in DirectoryList)
            {
                SelectedDirectory = targetDirectory;
                Root.CurrentVideoDirectory = targetDirectory;
                RefreshPlaylist();

                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + targetDirectory + "/deepdark.db"))
                {
                    conn.Open();
                    string sql = string.Empty;
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);

                    using (SQLiteTransaction tr = conn.BeginTransaction())
                    {
                        cmd = new SQLiteCommand(string.Empty, conn);
                        cmd.Transaction = tr;

                        foreach (VideoEntry v in Videos)
                        {
                            sql = "UPDATE Files SET fileDate=@fileDate WHERE filepath=@filePath";
                            cmd.CommandText = sql;
                            cmd.Parameters.AddWithValue("@fileDate",
                                File.GetLastWriteTime(targetDirectory + "\\" + v.Filepath)
                                    .ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@filePath", v.Filepath);
                            cmd.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }

                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Called when Media is opened.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMediaOpened(object sender, EventArgs e)
        {
            HasTakenThumbnail = false;
            var m = Root.MediaElement;
            /*
            Entries.AddOrUpdateEntry(m.Source, m.MediaInfo);
            Entries.SaveEntries();
            */
        }

        /// <summary>
        /// Handles the RenderingVideo event of the Media control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RenderingVideoEventArgs"/> instance containing the event data.</param>
        private void OnRenderingVideo(object sender, RenderingVideoEventArgs e)
        {
            const double snapshotPosition = 3;

            if (HasTakenThumbnail) return;

            var state = e.EngineState;

            if (state.Source == null)
                return;

            if (!state.HasMediaEnded && state.Position.TotalSeconds < snapshotPosition &&
                (!state.PlaybackEndTime.HasValue || state.PlaybackEndTime.Value.TotalSeconds > snapshotPosition))
                return;
        }
    }
}