namespace KKVideoPlayer.Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// AddDirectoryWindow.xaml에 대한 상호 작용 논리.
    /// </summary>
    public partial class AddDirectoryWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddDirectoryWindow"/> class.
        /// </summary>
        public AddDirectoryWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window mainWindow = App.Current.MainWindow;

            this.Left = mainWindow.Left + ((mainWindow.Width - this.ActualWidth) / 2);
            this.Top = mainWindow.Top + ((mainWindow.Height - this.ActualHeight) / 2);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog().ToString();
            if (result.Equals("OK"))
            {
                DirectoryTextBox.Text = openFileDlg.SelectedPath;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = Directory.Exists(DirectoryTextBox.Text) ? true : false;
        }
    }
}
