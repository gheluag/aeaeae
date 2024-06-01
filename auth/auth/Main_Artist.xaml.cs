using Microsoft.Win32;
using MySql.Data.MySqlClient;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace auth
{
    /// <summary>
    /// Логика взаимодействия для Main_Artist.xaml
    /// </summary>
    public partial class Main_Artist : Window
    {
        public artists Artist { get; set; }
        

        public static readonly DependencyProperty artistNameProperty = DependencyProperty.Register("artistName", typeof(string), typeof(Main_Artist));

        public string artistName
        {
            get { return (string)GetValue(artistNameProperty); }
            set { SetValue(artistNameProperty, value); }
        }


        public Main_Artist(string artistName)
        {
            InitializeComponent();
            this.artistName = artistName;
            LoadArtistSongs();
        }

        private void LoadArtistSongs()
        {
            DataBase database = new DataBase();
            int artistId = database.GetArtistIdByName(artistName);
            Artist = new artists();
            Artist = database.GetArtistById(artistId);
            List<Song> artistSongs = database.GetArtistSongsStats(artistId);

            songsListBox.Items.Clear();

            foreach (Song song in artistSongs)
            {
                songsListBox.Items.Add(song);
            }


            int Listens = database.GetListensCount(artistId);
            int Artist_count = database.GetLikedArtistsCount(artistId);
            int Album_count = database.GetLikedAlbumsCount(artistId);
            int Song_count = database.GetLikedSongsCount(artistId);

            listens.Text = Listens.ToString();
            artist_like.Text = Artist_count.ToString();
            album_like.Text = Album_count.ToString();
            song_like.Text = Song_count.ToString();

            listen_card.Text = Listens.ToString();
            database.GetArtist(artistId);

            string avatarfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");
            string avatarpath = Path.Combine(avatarfolder, Artist.Avatar);
            artistavatar.Source = new BitmapImage(new Uri(avatarpath));


            List<string> genres = database.GetGenres();

            foreach (string genre in genres)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = genre;
                ComboBoxItem itm = new ComboBoxItem();
                itm.Content = genre;
                GenreComboBox.Items.Add(item);
                genrComboBox.Items.Add(itm);
            }

        }


        private void UpdateName_Click(object sender, RoutedEventArgs e)
        {
            DataBase db = new DataBase();
            int artistId = db.GetArtistIdByName(artistName);
            EditUsernameDialog dialog = new EditUsernameDialog();
            if (dialog.ShowDialog() == true)
            {
                string newUsername = dialog.NewUsername;

                db.UpdateArtistName(artistId, artistName);
                artistusname.Text = newUsername;


                artistName = newUsername;
            }



        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;


                DataBase db = new DataBase();
                int artistId = db.GetArtistIdByName(artistName);
                db.UpdateArtistImage(artistId, filePath);

                artistavatar.Source = new BitmapImage(new Uri(filePath));
            }
        }

        // загрузка песен

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                using (var audioFile = new AudioFileReader(filePath))
                {
                    TimeSpan duration = audioFile.TotalTime;

                    SongLengthTextBlock.Text = $"{duration.Minutes}:{duration.Seconds.ToString("00")}";
                }

                SelectedSongTextBlock.Text = filePath;
            }
        }

        private void LoadCoverButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg; *.jpeg; *.png; *.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                CoverImage.Source = new BitmapImage(new Uri(imagePath));

            }
        }


        private void LoadSongButton_Click(object sender, RoutedEventArgs e)
        {
            DataBase db = new DataBase();
            string songName = SongNameTextBox.Text;
            string genre = ((ComboBoxItem)GenreComboBox.SelectedItem).Content.ToString();
            string audioFilePath = SelectedSongTextBlock.Text;
            string imageFilePath = CoverImage.Source.ToString();

            if (string.IsNullOrEmpty(songName) || string.IsNullOrEmpty(genre) || string.IsNullOrEmpty(audioFilePath) || string.IsNullOrEmpty(imageFilePath))
            {
                MessageBox.Show("Заполните все необходимые поля.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            try
            {
                using (MySqlConnection connection = new MySqlConnection("server=aimix.webtm.ru;port=3306;user=gelya; password=Justpassf0rvkr!;database=music_streaming"))
                {
                    connection.Open();

                    int genreId = db.GetGenreId(genre);
                    int artistId = db.GetArtistIdByName(artistName);

                    MySqlCommand command = new MySqlCommand("INSERT INTO song (song_name, duration, artist_id, genre_id, audio_file, image_file) VALUES (@name, @duration, @artistId, @genreId, @audioFile, @imageFile)", connection);
                    command.Parameters.AddWithValue("@name", songName);
                    command.Parameters.AddWithValue("@duration", SongLengthTextBlock.Text);
                    command.Parameters.AddWithValue("@artistId", artistId);
                    command.Parameters.AddWithValue("@genreId", genreId);
                    command.Parameters.AddWithValue("@audioFile", audioFilePath);

                    if (imageFilePath.StartsWith("file:///"))
                    {
                        imageFilePath = imageFilePath.Substring(8);
                    }
                    command.Parameters.AddWithValue("@imageFile", imageFilePath);

                    command.ExecuteNonQuery();

                    connection.Close();

                    List<Song> artistSongs = db.GetArtistSongsStats(artistId);

                    songsListBox.Items.Clear();
                    foreach (Song song in artistSongs)
                    {
                        songsListBox.Items.Add(song);
                    }
                }

                SongNameTextBox.Text = string.Empty;
                GenreComboBox.SelectedIndex = -1;
                SelectedSongTextBlock.Text = string.Empty;
                SongLengthTextBlock.Text = string.Empty;
                CoverImage.Source = null;

                MessageBox.Show("Песня успешно загружена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке песни: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void exit_btn(object sender, RoutedEventArgs e)
        {
            Artist_form artist_Form = new Artist_form();
            artist_Form.Show();
            Close();
        }

        // albums

        private void SelectCoverButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg; *.jpeg; *.png; *.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                albumCoverImage.Source = new BitmapImage(new Uri(imagePath));
            }

        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    StackPanel songPanel = new StackPanel();
                    songPanel.Orientation = Orientation.Horizontal;

                    TextBox songNameTextBox = new TextBox();
                    songNameTextBox.Text = "Введите название песни";
                    songNameTextBox.Width = 200;

                    TextBlock selectedSongTextBlock = new TextBlock();
                    selectedSongTextBlock.Text = filePath;
                    selectedSongTextBlock.VerticalAlignment = VerticalAlignment.Center; 

                    using (var audioFile = new AudioFileReader(filePath))
                    {
                        TimeSpan duration = audioFile.TotalTime;

                        TextBlock songLengthTextBlock = new TextBlock();
                        songLengthTextBlock.Text = $"{duration.Minutes}:{duration.Seconds.ToString("00")}";
                        songLengthTextBlock.VerticalAlignment = VerticalAlignment.Center; 
                        songLengthTextBlock.Margin = new Thickness(10, 0, 0, 0); 

                        songPanel.Children.Add(songNameTextBox);
                        songPanel.Children.Add(selectedSongTextBlock);
                        songPanel.Children.Add(songLengthTextBlock);
                    }

                    songsPanel.Children.Add(songPanel);
                }
            }
        }

        private void UploadAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            DataBase database = new DataBase();
            string albumName = albumNameTextBox.Text;
            int artistId = database.GetArtistIdByName(artistName);
            string genre = ((ComboBoxItem)genrComboBox.SelectedItem).Content.ToString();
            int genreId = database.GetGenreId(genre);
            string albumCoverImagePath = albumCoverImage.Source.ToString();


            if (string.IsNullOrEmpty(albumName) || albumCoverImage.Source == null || songsPanel.Children.Count == 0)
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            database.AddAlbum(albumName, albumCoverImagePath);


            int albumId = database.GetLatestAlbumId();


            foreach (StackPanel songPanel in songsPanel.Children)
            {
                TextBlock selectedSongTextBlock = (TextBlock)songPanel.Children[1];
                string filePath = selectedSongTextBlock.Text;


                TextBox songNameTextBox = songPanel.Children.OfType<TextBox>().FirstOrDefault();
                string songName = songNameTextBox.Text;


                using (var audioFile = new AudioFileReader(filePath))
                {
                    TimeSpan duration = audioFile.TotalTime;
                    string durationString = $"{duration.Minutes}:{duration.Seconds.ToString("00")}";

                    database.AddSong(songName, durationString, artistId, genreId, filePath, albumCoverImagePath);

                    int songId = database.GetLatestSongId();

                    database.AddSongToAlbum(albumId, songId);
                }
               
            }
            MessageBox.Show("Альбом успешно загружен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

            List<Song> artistSongs = database.GetArtistSongsStats(artistId);

            songsListBox.Items.Clear();
            foreach (Song song in artistSongs)
            {
                songsListBox.Items.Add(song);
            }

            albumNameTextBox.Text = "";
            albumCoverImage.Source = null;
            songsPanel.Children.Clear();

        }


        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Song song = button.DataContext as Song;
            var menuItem = button.ContextMenu.Items[0] as MenuItem;
            menuItem.DataContext = song;
            button.ContextMenu.IsOpen = true;
        }


        private void RemoveSong_Click(object sender, RoutedEventArgs e)
        {

            MenuItem menuItem = sender as MenuItem;
            Song song = menuItem.DataContext as Song;

            DataBase db = new DataBase();
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить песню?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                db.DeleteSong(song.Id);
            }
            int artistId = db.GetArtistIdByName(artistName);
            List<Song> artistSongs = db.GetArtistSongsStats(artistId);

            songsListBox.Items.Clear();
            foreach (Song songs in artistSongs)
            {
                songsListBox.Items.Add(songs);
            }

        }

       


    }
}
