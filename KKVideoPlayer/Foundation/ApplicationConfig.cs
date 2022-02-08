namespace KKVideoPlayer.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Serialization;

#pragma warning disable CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
    /// <summary>
    ///  A config for application gerneral.
    /// </summary>
    [Serializable]
    public class ApplicationConfig : INotifyPropertyChanged
    {
        private WindowConfig m_WindowSettings;
        private PlaylistConfig m_PlaylistSettings;
        private ControllerConfig m_ControllerSettings;

        public ApplicationConfig()
        {
            // Constructor is called only if there is no pre-existent application config.
            WindowSettings = new WindowConfig()
            {
                Height = 720,
                Width = 1280,
            };

            PlaylistSettings = new PlaylistConfig()
            {
                SortDirection = true,
                DirectoryComboIndex = -1,
                SortComboIndex = 0,
            };

            ControllerSettings = new ControllerConfig()
            {
                Volume = 1.0,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public WindowConfig WindowSettings
        {
            get
            {
                return m_WindowSettings;
            }
            set
            {
                m_WindowSettings = value;
                OnPropertyChanged(nameof(WindowSettings));
            }
        }

        public PlaylistConfig PlaylistSettings
        {
            get
            {
                return m_PlaylistSettings;
            }
            set
            {
                m_PlaylistSettings = value;
                OnPropertyChanged(nameof(PlaylistSettings));
            }
        }

        public ControllerConfig ControllerSettings
        {
            get
            {
                return m_ControllerSettings;
            }
            set
            {
                m_ControllerSettings = value;
                OnPropertyChanged(nameof(ControllerSettings));
            }
        }

        public static void Save(string filePath, ApplicationConfig appConfig)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationConfig));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, appConfig);
            }
        }

        public static ApplicationConfig Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ApplicationConfig));

                using (StreamReader reader = new StreamReader(filePath))
                {
                    return (ApplicationConfig)serializer.Deserialize(reader);
                }
            }
            else
            {
                return new ApplicationConfig();
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// Configuration for window properties.
    /// </summary>
    [Serializable]
    public class WindowConfig : INotifyPropertyChanged
    {
        private int m_Height;
        private int m_Width;
        private int m_Left;
        private int m_Top;

        public WindowConfig()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                m_Height = value;
                OnPropertyChanged("Height");
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                m_Width = value;
                OnPropertyChanged("Width");
            }
        }

        public int Left
        {
            get
            {
                return m_Left;
            }
            set
            {
                m_Left = value;
                OnPropertyChanged("Left");
            }
        }

        public int Top
        {
            get
            {
                return m_Top;
            }
            set
            {
                m_Top = value;
                OnPropertyChanged("Top");
            }
        }

        public WindowState WindowState { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// Configuration for window properties.
    /// </summary>
    [Serializable]
    public class PlaylistConfig : INotifyPropertyChanged
    {
        private bool m_SortDirection;
        private int m_SortComboIndex;
        private int m_DirectoryComboIndex;

        public PlaylistConfig()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sort Direction Config for video order. When true, ascending.
        /// </summary>
        public bool SortDirection
        {
            get
            {
                return m_SortDirection;
            }
            set
            {
                m_SortDirection = value;
                OnPropertyChanged(nameof(SortDirection));
            }
        }

        public int SortComboIndex
        {
            get
            {
                return m_SortComboIndex;
            }
            set
            {
                m_SortComboIndex = value;
                OnPropertyChanged(nameof(SortComboIndex));
            }
        }

        public int DirectoryComboIndex
        {
            get
            {
                return m_DirectoryComboIndex;
            }
            set
            {
                m_DirectoryComboIndex = value;
                OnPropertyChanged(nameof(DirectoryComboIndex));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// Config for controller panel.
    /// </summary>
    [Serializable]
    public class ControllerConfig : INotifyPropertyChanged
    {
        private double m_Volume;

        public ControllerConfig()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double Volume
        {
            get
            {
                return m_Volume;
            }
            set
            {
                m_Volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
#pragma warning restore CS1591 // 공개된 형식 또는 멤버에 대한 XML 주석이 없습니다.
}
