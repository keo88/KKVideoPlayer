namespace KKVideoPlayer.ViewModels
{
    using System;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Shell;
    using System.Windows.Threading;
    using System.Xaml;
    using KKVideoPlayer.Foundation;
    using Unosquare.FFME;
    using Unosquare.FFME.Common;

    /// <summary>
    /// Represents the application-wide view model.
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public sealed class RootViewModel : ViewModelBase
    {
        private readonly object screenshotSyncLock = new object();
        private string m_WindowTitle = string.Empty;
        private string m_NotificationMessage = string.Empty;
        private double m_PlaybackProgress;
        private TaskbarItemProgressState m_PlaybackProgressState;
        private bool m_IsPlaylistPanelOpen = App.IsInDesignMode;
        private bool m_IsPropertiesPanelOpen = App.IsInDesignMode;
        private bool m_IsApplicationLoaded = App.IsInDesignMode;
        private MediaElement m_MediaElement;
        private bool m_IsCaptureInProgress;
        private VideoEntry m_PlayingVideo;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootViewModel"/> class.
        /// </summary>
        public RootViewModel()
        {
            // Set and create an app data directory
            WindowTitle = "Application Loading . . .";
            AppVersion = typeof(RootViewModel).Assembly.GetName().Version.ToString();
            AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductName);
            if (Directory.Exists(AppDataDirectory) == false)
                Directory.CreateDirectory(AppDataDirectory);

            var configPath = AppDataDirectory + "/config.xml";
            if (!File.Exists(configPath))
            {
                AppConfig = new ApplicationConfig();
                ApplicationConfig.Save(configPath, AppConfig);
            }
            else
            {
                AppConfig = ApplicationConfig.Load(configPath);
            }

            Crawler = new WebCrawlDriver();

            // Attached ViewModel Initialization
            Playlist = new PlaylistViewModel(this);
            Controller = new ControllerViewModel(this);
            VideoCtrl = new VideoControlViewModel(this);
        }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public static string ProductName => "KKVideoPlayer";

        /// <summary>
        /// Gets the playlist ViewModel.
        /// </summary>
        public PlaylistViewModel Playlist { get; }

        /// <summary>
        /// Gets the controller.
        /// </summary>
        public ControllerViewModel Controller { get; }

        /// <summary>
        /// Gets the video editor VM.
        /// </summary>
        public VideoControlViewModel VideoCtrl { get; }

        /// <summary>
        ///  Selenium driver service for web crawling.
        /// </summary>
        public WebCrawlDriver Crawler { get; }

        /// <summary>
        /// Gets the application version.
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        public string AppDataDirectory { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string WindowTitle
        {
            get => m_WindowTitle;
            private set => SetProperty(ref m_WindowTitle, value);
        }

        /// <summary>
        ///  Application configuration.
        /// </summary>
        public ApplicationConfig AppConfig { get; set; }

        /// <summary>
        ///  A name of the current video file. If the media is not playing any video, the value is an empty string.
        /// </summary>
        public string VideoFileName
        {
            get;
            set;
        }

        /// <summary>
        ///  A path to the current video file.
        /// </summary>
        public string VideoFilePath
        {
            get;
            set;
        }

        /// <summary>
        ///  Directory where deepdasrk.db file is present.
        /// </summary>
        public string CurrentVideoDirectory { get; set; }

        /// <summary>
        /// Gets or sets the notification message to be displayed.
        /// </summary>
        public string NotificationMessage
        {
            get
            {
                return m_NotificationMessage;
            }

            set
            {
                m_NotificationMessage = value;
                NotifyPropertyChanged(nameof(NotificationMessage));
            }
        }

        /// <summary>
        /// Gets or sets the playback progress.
        /// </summary>
        public double PlaybackProgress
        {
            get
            {
                return m_PlaybackProgress;
            }

            set
            {
                m_PlaybackProgress = value;
                NotifyPropertyChanged(nameof(PlaybackProgress));
            }
        }

        /// <summary>
        /// Gets or sets the state of the playback progress.
        /// </summary>
        public TaskbarItemProgressState PlaybackProgressState
        {
            get
            {
                return m_PlaybackProgressState;
            }

            set
            {
                m_PlaybackProgressState = value;
                NotifyPropertyChanged(nameof(PlaybackProgressState));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is playlist panel open.
        /// </summary>
        public bool IsPlaylistPanelOpen
        {
            get => m_IsPlaylistPanelOpen;
            set => SetProperty(ref m_IsPlaylistPanelOpen, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is properties panel open.
        /// </summary>
        public bool IsPropertiesPanelOpen
        {
            get => m_IsPropertiesPanelOpen;
            set => SetProperty(ref m_IsPropertiesPanelOpen, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is application loaded.
        /// </summary>
        public bool IsApplicationLoaded
        {
            get => m_IsApplicationLoaded;
            set => SetProperty(ref m_IsApplicationLoaded, value);
        }

        /// <summary>
        /// Provides access to application-wide commands.
        /// </summary>
        public AppCommands Commands { get; } = new AppCommands();

        /// <summary>
        /// Gets the media element hosted by the main window.
        /// </summary>
        public MediaElement MediaElement
        {
            get
            {
                if (m_MediaElement == null)
                    m_MediaElement = (Application.Current.MainWindow as MainWindow)?.Media;

                return m_MediaElement;
            }
        }

        /// <summary>
        /// Provides access to the current media options.
        /// This is an unsupported usage of media options.
        /// </summary>
        public MediaOptions CurrentMediaOptions { get; set; }

        /// <summary>
        ///  Currently Played Video.
        /// </summary>
        public VideoEntry PlayingVideo
        {
            get => m_PlayingVideo;

            set
            {
                SetProperty(ref m_PlayingVideo, value);
            }
        }

        /// <summary>
        /// A flag indicating whether screenshot capture progress is currently active.
        /// </summary>
        private bool IsCaptureInProgress
        {
            get { lock (screenshotSyncLock) return m_IsCaptureInProgress; }
            set { lock (screenshotSyncLock) m_IsCaptureInProgress = value; }
        }

        /// <summary>
        ///  Capture.
        /// </summary>
        /// <param name="width">width.</param>
        /// <param name="height">height.</param>
        /// <returns>task.</returns>
        public async Task<Bitmap> CaptureScreenshotAsync(int width, int height)
        {
            Bitmap capturedBitmap = null;

            // Don't run the capture operation as it is in progress
            // GDI requires exclusive access to files when writing
            // so we do this one at a time
            if (IsCaptureInProgress)
                return null;

            // Immediately set the progress to true.
            IsCaptureInProgress = true;

            // Send the capture to the background so we don't have frames skipping
            // on the UI. This prvents frame jittering.
            var captureTask = Task.Run(() =>
            {
                Bitmap bmp = null;
                try
                {
                    // Obtain the bitmap
                    bmp = MediaElement.CaptureBitmapAsync().GetAwaiter().GetResult();

                    // prevent firther processing if we did not get a bitmap.
                    // bmp?.Save(App.GetCaptureFilePath("Screenshot", "jpeg"), ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                            App.Current.MainWindow,
                            $"Capturing Video Frame Failed: {ex.GetType()}\r\n{ex.Message}",
                            $"{nameof(MediaElement)} Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);
                    /*var messageTask = Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show(
                            App.Current.MainWindow,
                            $"Capturing Video Frame Failed: {ex.GetType()}\r\n{ex.Message}",
                            $"{nameof(MediaElement)} Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK);
                    });*/
                }
                finally
                {
                    // unlock for further captures.
                    IsCaptureInProgress = false;
                }

                return bmp;
            });

            capturedBitmap = await captureTask;
            if (capturedBitmap != null)
            {
                System.Drawing.Size resize = new System.Drawing.Size(width, height);
                return new Bitmap(capturedBitmap, resize);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  Convert.
        /// </summary>
        /// <param name="rawBitmap">bitmap.</param>
        /// <returns>byte.</returns>
        public byte[] ConvertBitmapToByteArray(Bitmap rawBitmap)
        {
            // Debug.WriteLine("PixelSize: " + bitmap.PixelFormat);
            Bitmap bitmap = rawBitmap;

            byte[] result = null;
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                result = stream.ToArray();
            }
            else
            {
                Console.WriteLine("Bitmap is null.");
            }

            return result;
        }

        /// <summary>
        ///  Convert.
        /// </summary>
        /// <param name="rawBitmap">bitmap.</param>
        /// <param name="cropArea">crop area.</param>
        /// <returns>byte.</returns>
        public byte[] ConvertBitmapToByteArray(Bitmap rawBitmap, Rectangle cropArea)
        {
            // Debug.WriteLine("PixelSize: " + bitmap.PixelFormat);
            Bitmap bitmap;
            if (cropArea != Rectangle.Empty)
            {
                bitmap = rawBitmap.Clone(cropArea, rawBitmap.PixelFormat);
            }
            else
            {
                bitmap = rawBitmap;
            }

            byte[] result = null;
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                result = stream.ToArray();
            }
            else
            {
                Console.WriteLine("Bitmap is null.");
            }

            return result;
        }

        /// <summary>
        /// Export currently playing video resolution.
        /// </summary>
        /// <param name="width">Output video width.</param>
        /// <param name="height">Output video height.</param>
        public bool TryGetVideoResolution(out int width, out int height)
        {
            if (MediaElement.IsOpen)
            {
                width = MediaElement.NaturalVideoWidth;
                height = MediaElement.NaturalVideoHeight;
                return true;
            }
            else
            {
                width = 0;
                height = 0;
                return false;
            }
        }

        /// <summary>
        /// Called when application has finished loading.
        /// </summary>
        internal void OnApplicationLoaded()
        {
            if (IsApplicationLoaded)
                return;

            Playlist.OnApplicationLoaded();
            Controller.OnApplicationLoaded();

            var m = MediaElement;
            /*m.WhenChanged(UpdateWindowTitle,
                nameof(m.IsOpen),
                nameof(m.IsOpening),
                nameof(m.MediaState),
                nameof(m.Source));*/

            m.MediaOpening += (s, e) =>
            {
                UpdateWindowTitle();
            };

            m.MediaOpened += (s, e) =>
            {
                // Reset the Zoom
                Controller.MediaElementZoom = 1d;

                // Update the Controls
                Playlist.IsInOpenMode = false;

                // IsPlaylistPanelOpen = false;
                Playlist.OpenMediaSource = e.Info.MediaSource;

                PlayingVideo = Playlist.SelectedVideo;
                PlayingVideo.ViewCount++;

                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + CurrentVideoDirectory + "/deepdark.db"))
                {
                    conn.Open();
                    string sql = $"UPDATE File Set count = '{PlayingVideo.ViewCount}' WHERE filepath = '{PlayingVideo.Filepath}'";
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    conn.Close();
                }
            };

            m.MediaClosed += (s, e) =>
            {
                PlayingVideo = null;
            };

            IsPlaylistPanelOpen = true;
            IsApplicationLoaded = true;
        }

        /// <summary>
        /// When Application is closed, this function is called.
        /// </summary>
        internal void OnApplicationClosed()
        {
            ApplicationConfig.Save(AppDataDirectory + "/config.xml", AppConfig);
        }

        /// <summary>
        /// Updates the window title according to the current state.
        /// </summary>
        private void UpdateWindowTitle()
        {
            var m = MediaElement;
            Uri file_path = m?.Source;
            string title;
            var state = m?.MediaState.ToString();

            if (file_path?.IsFile ?? false)
            {
                title = file_path.ToString();
                string[] splitted_string = file_path.OriginalString.Split(new char[] { '\\' });
                VideoFileName = splitted_string[splitted_string.Length - 1];
                VideoFilePath = splitted_string[0];
            }
            else
            {
                title = "(No media loaded)";
            }

            // WindowTitle = $"KKPlayer v{AppVersion} {title} {state} " + $"FFmpeg {Library.FFmpegVersionInfo} ({(Debugger.IsAttached ? "Debug" : "Release")})";
            WindowTitle = $"KKPlayer v{AppVersion}";
        }
    }
}
