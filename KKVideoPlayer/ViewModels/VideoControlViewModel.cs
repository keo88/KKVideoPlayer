namespace KKVideoPlayer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using KKVideoPlayer.Foundation;

    /// <summary>
    ///  Represents video editing control.
    /// </summary>
    public sealed class VideoControlViewModel : AttachedViewModel
    {
        private static readonly string[] ResolutionTypes = { "SD", "HD", "FHD", "QHD", "UHD" };
        private string m_PathText;
        private string m_DvdIdText;
        private string m_TitleText;
        private string m_ActorsText;
        private string m_GenresText;
        private string m_ReleaseDateText;
        private string m_SeriesText;
        private string m_DirectorsText;
        private string m_CompaniesText;
        private VideoEntry m_EditedVideo;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoControlViewModel"/> class.
        /// </summary>
        /// <param name="root">root viewmodel.</param>
        public VideoControlViewModel(RootViewModel root)
            : base(root)
        {
            // Initialize.
        }

        /// <summary>
        ///  Info about video maintained through editing process. Used to compare changes.
        /// </summary>
        public static List<string> MaintainedVideoInfo { get; set; }

        /// <summary>
        /// Parent Window of this control.
        /// </summary>
        public Window ParentWindow { get; set; }

        /// <summary>
        ///  A video entry being edited.
        /// </summary>
        public VideoEntry EditedVideo
        {
            get => m_EditedVideo;
            set => SetProperty(ref m_EditedVideo, value);
        }

        /// <summary>
        ///  File path in string, input from user.
        /// </summary>
        public string PathText
        {
            get => m_PathText;
            set => SetProperty(ref m_PathText, value);
        }

        /// <summary>
        ///  File path in string, input from user.
        /// </summary>
        public string DvdIdText
        {
            get => m_DvdIdText;
            set => SetProperty(ref m_DvdIdText, value);
        }

        /// <summary>
        ///  DvdId in string, input from user.
        /// </summary>
        public string TitleText
        {
            get => m_TitleText;
            set => SetProperty(ref m_TitleText, value);
        }

        /// <summary>
        ///  Actors in string, input from user.
        /// </summary>
        public string ActorsText
        {
            get => m_ActorsText;
            set => SetProperty(ref m_ActorsText, value);
        }

        /// <summary>
        ///  Genres in string, input from user.
        /// </summary>
        public string GenresText
        {
            get => m_GenresText;
            set => SetProperty(ref m_GenresText, value);
        }

        /// <summary>
        ///  Release date in string, input from user.
        /// </summary>
        public string ReleaseDateText
        {
            get => m_ReleaseDateText;
            set => SetProperty(ref m_ReleaseDateText, value);
        }

        /// <summary>
        ///  Series in string, input from user.
        /// </summary>
        public string SeriesText
        {
            get => m_SeriesText;
            set => SetProperty(ref m_SeriesText, value);
        }

        /// <summary>
        ///  Directors in string, input from user.
        /// </summary>
        public string DirectorsText
        {
            get => m_DirectorsText;
            set => SetProperty(ref m_DirectorsText, value);
        }

        /// <summary>
        ///  Companies in string, input from user.
        /// </summary>
        public string CompaniesText
        {
            get => m_CompaniesText;
            set => SetProperty(ref m_CompaniesText, value);
        }

        /// <summary>
        ///  Open Update window.
        /// </summary>
        public void OpenEditControl()
        {
            EditedVideo = Root.Playlist.SelectedVideo;
            VideoEntry selectedVideo = EditedVideo;
            if (selectedVideo == null ||
                string.IsNullOrEmpty(selectedVideo.Filepath) ||
                string.IsNullOrWhiteSpace(Root.CurrentVideoDirectory))
                return;

            string oldDvdId = selectedVideo.DvdId;
            string sourcePath = selectedVideo.Filepath;
            string sourceFullPath = Path.Combine(Root.CurrentVideoDirectory, sourcePath);
            bool videoIsOpen = selectedVideo.Filepath.Equals(Root.PlayingVideo?.Filepath, StringComparison.Ordinal);

            ActorsText = string.Join(", ", selectedVideo.Actors);
            GenresText = string.Join(", ", selectedVideo.Genres);
            DvdIdText = selectedVideo.DvdId;
            TitleText = selectedVideo.Title;
            PathText = selectedVideo.Filepath;
            SeriesText = selectedVideo.Series;
            DirectorsText = string.Join(", ", selectedVideo.Directors);
            CompaniesText = string.Join(", ", selectedVideo.Companies);

            if (selectedVideo.ReleaseDate.Equals(DateTime.MinValue))
                ReleaseDateText = string.Empty;
            else
                ReleaseDateText = selectedVideo.ReleaseDate.ToString("yyyy-MM-dd");

            MaintainedVideoInfo = new();
            MaintainedVideoInfo.Add(oldDvdId);
            MaintainedVideoInfo.Add(sourcePath);
            MaintainedVideoInfo.Add(sourceFullPath);
            MaintainedVideoInfo.Add(videoIsOpen.ToString());
        }

        /// <summary>
        /// Apply changes made in controls to Database.
        /// </summary>
        public async void ApplyVideoInfoChanges()
        {
            string targetPath = PathText;

            if (string.IsNullOrWhiteSpace(targetPath) || MaintainedVideoInfo == null)
                return;

            VideoEntry selectedVideo = EditedVideo;

            string oldDvdId = MaintainedVideoInfo[0];
            string sourcePath = MaintainedVideoInfo[1];
            string sourceFullPath = MaintainedVideoInfo[2];
            bool videoIsOpen = bool.Parse(MaintainedVideoInfo[3]);

            string targetFullPath = Path.Combine(Root.CurrentVideoDirectory, targetPath);
            bool targetExists = File.Exists(targetFullPath);
            bool sourceExists = File.Exists(sourceFullPath);

            // DvdId가 P.Key인데 dvdid가 빈칸이고 다른 곳은 차있으면 DB에 insert가 안되므로 return.
            if (string.IsNullOrWhiteSpace(DvdIdText) && (
                    !string.IsNullOrWhiteSpace(ActorsText) ||
                    !string.IsNullOrWhiteSpace(GenresText) ||
                    !string.IsNullOrWhiteSpace(TitleText)))
            {
                MessageBox.Show("Cannot edit descriptions while the dvdid is empty.");
                return;
            }

            selectedVideo.Actors = ActorsText.Split(',').Select(p => p.Trim()).ToList();
            selectedVideo.Genres = GenresText.Split(',').Select(p => p.Trim()).ToList();
            selectedVideo.DvdId = DvdIdText;
            selectedVideo.Title = TitleText;
            selectedVideo.Filepath = PathText;
            selectedVideo.Series = SeriesText;
            selectedVideo.Directors = DirectorsText.Split(',').Select(p => p.Trim()).ToList();
            selectedVideo.Companies = CompaniesText.Split(',').Select(p => p.Trim()).ToList();

            try
            {
                selectedVideo.ReleaseDate = DateTime.ParseExact(ReleaseDateText, "yyyy-MM-dd", null);
            }
            catch (FormatException)
            {
                selectedVideo.ReleaseDate = DateTime.MinValue;
            }

            using (SQLiteConnection conn =
                   new SQLiteConnection("Data Source=" + Root.CurrentVideoDirectory + "/deepdark.db"))
            {
                conn.Open();
                string sql;

                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    SQLiteCommand cmd = new SQLiteCommand(string.Empty, conn);
                    cmd.Transaction = tr;

                    sql = $"UPDATE Files SET dvdId='{selectedVideo.DvdId}' WHERE filepath = '{sourcePath}'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    // XOR(sourceExists, targetExists)의 결과가 true이면 File, Favorites에 대해 Path를 Update.
                    if (sourceExists ^ targetExists)
                    {
                        sql = $"UPDATE Files SET filepath = '{selectedVideo.Filepath}' WHERE filepath = '{sourcePath}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        sql =
                            $"UPDATE Favorites SET filepath = '{selectedVideo.Filepath}' WHERE filepath = '{sourcePath}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        // target은 없고 source만 존재하면 OS에서 rename 필요.
                        if (sourceExists && !targetExists)
                        {
                            try
                            {
                                if (videoIsOpen)
                                {
                                    if (await App.ViewModel.MediaElement.Close())
                                        File.Move(sourceFullPath, targetFullPath);
                                }
                                else
                                {
                                    File.Move(sourceFullPath, targetFullPath);
                                }
                            }
                            catch (IOException ex)
                            {
                                _ = MessageBox.Show($"Unable to rename file.\n{ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                tr.Rollback();
                                conn.Close();
                                return;
                            }

                            PlaylistViewModel.MigrateSubtitle(sourceFullPath, targetFullPath);
                        }
                    }
                    else if (!sourceFullPath.Equals(targetFullPath))
                    {
                        if (targetExists)
                            MessageBox.Show($"Another file already exists at '{targetFullPath}'");
                        else
                            MessageBox.Show($"Cannot bind new file name, '{targetFullPath}'\nThere is no such file.");
                    }

                    sql = $"DELETE FROM Dvd WHERE dvdId = '{oldDvdId}' or dvdId = '{selectedVideo.DvdId}'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    sql =
                        "INSERT INTO Dvd(dvdId, dvdTitle, actors, genres, releaseDate, director, company, series, metaSource) VALUES (@dvdId, @dvdTitle, @actors, @genres, @releaseDate, @director, @company, @series, @metaSource)";
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@dvdId", selectedVideo.DvdId);
                    cmd.Parameters.AddWithValue("@dvdTitle", selectedVideo.Title);
                    cmd.Parameters.AddWithValue("@actors", string.Join(", ", selectedVideo.Actors));
                    cmd.Parameters.AddWithValue("@genres", string.Join(", ", selectedVideo.Genres));
                    cmd.Parameters.AddWithValue("@releaseDate",
                        selectedVideo.ReleaseDate != DateTime.MinValue
                            ? selectedVideo.ReleaseDate.ToString("yyyy-MM-dd")
                            : null);
                    cmd.Parameters.AddWithValue("@director", string.Join(", ", selectedVideo.Directors));
                    cmd.Parameters.AddWithValue("@company", string.Join(", ", selectedVideo.Companies));
                    cmd.Parameters.AddWithValue("@series", selectedVideo.Series);
                    cmd.Parameters.AddWithValue("@metaSource", null);
                    cmd.ExecuteNonQuery();

                    tr.Commit();
                }

                conn.Close();
            }

            // Root.Playlist.RefreshPlaylist();
        }

        /// <summary>
        /// Deformat file path into video info.
        /// </summary>
        public void DeformatFilepath()
        {
            string fp = PathText;

            string[] fpSplitByDot = fp.Split('.');

            try
            {
                string extender = fpSplitByDot[^1];

                string[] fpSplitComa = string.Join('.', fpSplitByDot.SkipLast(1)).Split(',');
                string[] fpSplitSharp = fpSplitComa.Length > 1
                    ? string.Join(',', fpSplitComa.SkipLast(1)).Split('#')
                    : fpSplitComa[0].Split('#');
                string fpHeader, fpBody;

                if (fpSplitSharp.Length > 1)
                {
                    fpHeader = fpSplitSharp[0].Trim();
                    fpBody = fpSplitSharp[1].Trim();
                }
                else
                {
                    fpHeader = fpSplitSharp[0];
                    fpBody = string.Empty;
                }

                string fpFooter = fpSplitComa[^1].Trim();
                string[] fpFooterSplit = fpFooter.Split(' ');

                string dvdid = fpFooterSplit[^1];
                if (dvdid.Contains('['))
                    dvdid = string.Empty;

                List<string> genres = new List<string>();
                List<string> actors = new List<string>();

                string[] titleSplit = fpHeader.Split(new char[] {')', ']'});
                string title = titleSplit[^1];

                if (fpSplitComa.Length == 1)
                {
                    dvdid = AssumeVideoCode(fp);
                    if (string.IsNullOrWhiteSpace(dvdid))
                        dvdid = title;
                }

                string[] frontTags = BracketSplit(fpHeader, '[', ']');
                foreach (string item in frontTags)
                    genres.Add(item.ToUpper());

                if (!string.IsNullOrWhiteSpace(fpBody))
                {
                    string[] bodyTags = fpBody.Split(new char[] {' ', '#'});
                    foreach (string item in bodyTags)
                        genres.Add(item.ToUpper());
                }

                string[] footerTags = BracketSplit(fpFooter, '[', ']');
                foreach (string item in footerTags)
                    genres.Add(item.ToUpper());

                string[] actorsStr = BracketSplit(fpHeader, '(', ')');
                if (actorsStr.Length > 0)
                {
                    foreach (string actor in actorsStr[0].Split(','))
                        actors.Add(actor.Trim());
                }

                actors = actors.Distinct().ToList();
                genres = genres.Distinct().ToList();

                // If there is no resolution typed genre in the genres list, try make one.
                if (!genres.Any((item) => ResolutionTypes.Contains(item)))
                {
                    bool gotResolution = TryGetVideoResolution(out string resolution);
                    if (gotResolution)
                        genres.Add(resolution);
                }

                string actressesStr = string.Join(", ", actors);
                string tagsStr = string.Join(", ", genres);

                DvdIdText = dvdid;
                ActorsText = actressesStr;
                GenresText = tagsStr;
                TitleText = title;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to process {fp}\n" + ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///  Format file. Don't use it if you don't know it.
        /// </summary>
        public void FormatFilepath()
        {
            string[] fpSplitByDot = PathText.Split('.');
            string extender = fpSplitByDot[fpSplitByDot.Length - 1];

            string filePath = string.Empty;

            List<string> genresList = GenresText.Split(", ").ToList();
            string actors = string.Empty;
            string title = TitleText;
            string dvdId = DvdIdText;

            if (!string.IsNullOrWhiteSpace(ActorsText))
            {
                actors = "(" + ActorsText + ")";
            }

            List<string> resolutions = genresList.Where(item => ResolutionTypes.Contains(item)).ToList();
            string resolution = string.Empty;

            if (resolutions.Count > 0)
            {
                resolution = resolutions[0];
                genresList.Remove(resolution);
                resolution = " [" + resolution + "]";
            }
            else
            {
                if (Root.PlayingVideo == EditedVideo)
                {
                    bool gotResolution = TryGetVideoResolution(out resolution);

                    if (gotResolution)
                        resolution = " [" + resolution + "]";
                }
            }

            string firstGenre = string.Empty;
            if (genresList.Count > 0)
            {
                firstGenre = genresList[0];
                genresList.Remove(firstGenre);
                firstGenre = "[" + firstGenre + "]";
            }

            string genres = string.Empty;
            if (genresList.Count > 0)
            {
                genres = " #" + string.Join(" ", genresList);
            }

            filePath = $"{firstGenre}{actors}{title}{genres},{resolution} {dvdId}.{extender}";
            PathText = filePath;
        }

        private bool TryGetVideoResolution(out string resolution)
        {
            resolution = string.Empty;
            bool gotValues = Root.TryGetVideoResolution(out int width, out int height);

            if (!gotValues)
                return false;

            if (width > 0 && width < 1280)
            {
                resolution = ResolutionTypes[0];
            }
            else if (width >= 1280 && width < 1980)
            {
                resolution = ResolutionTypes[1];
            }
            else if (width >= 1980 && width < 2560)
            {
                resolution = ResolutionTypes[2];
            }
            else if (width >= 2560 && width < 3840)
            {
                resolution = ResolutionTypes[3];
            }
            else if (width >= 3840)
            {
                resolution = ResolutionTypes[4];
            }

            return !string.IsNullOrWhiteSpace(resolution);
        }

        /// <summary>
        ///  Crawling from the web to get video info.
        /// </summary>
        public async void CrawlVideoInfoAsync()
        {
            string assumedVideoCode = AssumeVideoCode(PathText);

            if (!string.IsNullOrWhiteSpace(assumedVideoCode))
                DvdIdText = assumedVideoCode;

            VideoEntry v = new();

            Task<bool> navigateTask = App.ViewModel.Crawler.NavigateJavDb(DvdIdText, v);
            bool ret = await navigateTask;

            if (ret)
            {
                if (!string.IsNullOrWhiteSpace(v.Title))
                    TitleText = v.Title;

                if (!v.ReleaseDate.Equals(DateTime.MinValue))
                    ReleaseDateText = v.ReleaseDate.ToString("yyyy-MM-dd");

                if (v.Directors.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(DirectorsText))
                        DirectorsText += ", " + string.Join(", ", v.Directors);
                    else
                        DirectorsText = string.Join(", ", v.Directors);
                }

                if (v.Companies.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(CompaniesText))
                        CompaniesText += ", " + string.Join(", ", v.Companies);
                    else
                        CompaniesText = string.Join(", ", v.Companies);
                }

                if (v.Actors.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(ActorsText))
                        ActorsText += ", " + string.Join(", ", v.Actors);
                    else
                        ActorsText = string.Join(", ", v.Actors);
                }

                if (v.Genres.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(GenresText))
                        GenresText += ", " + string.Join(", ", v.Genres);
                    else
                        GenresText = string.Join(", ", v.Genres);
                }
            }
            else
            {
                _ = MessageBox.Show(
                    "Something went wrong.\nCannot connect to requested url.\nCheck your DNS/SNI Settings.\nIt may also mean your search id is not present in the Web DB.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string[] BracketSplit(string str, char sep1, char sep2)
        {
            List<string> output = new List<string>();

            try
            {
                var splitStr = str.Split(sep1);
                var strPrev = splitStr.Skip(1);

                foreach (string piece in strPrev)
                {
                    output.Add(piece.Split(sep2)[0]);
                }
            }
            catch (NullReferenceException)
            {
                return null;
            }

            return output.ToArray();
        }

        private static List<string> RemoveDuplicates(List<string> list)
        {
            list.Reverse();
            var duplicates = list.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var dup in duplicates)
                list.Remove(dup);
            list.Reverse();

            return list;
        }

        private string AssumeVideoCode(string raw)
        {
            string dvdId = String.Empty;
            foreach (Match match in Regex.Matches(raw, @"\w{3,}-\d{3,}"))
            {
                dvdId = match.ToString();
                DvdIdText = dvdId;
            }
            
            return dvdId;
        }
    }
}