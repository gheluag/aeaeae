using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace auth
{
    /// <summary>
    /// Логика взаимодействия для Selectplaylist.xaml
    /// </summary>
    public partial class Selectplaylist : Window
    {
        private Song selectedSong;
        private int currentUser;
        DataBase database = new DataBase();
        public string SelectedPlaylist { get; private set; }
        public Selectplaylist(Song song, int userId)
        {
            InitializeComponent();
            selectedSong = song;
            currentUser = userId;

            // Загрузка списка плейлистов
            LoadPlaylists();
        }

        private void LoadPlaylists()
        {
            int userid = database.GetUserIdByUsername(CurrentUser.Username);
            List<string> playlists = database.GetUserPlaylists(userid);
            PlaylistListBox.ItemsSource = playlists;
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedPlaylist = PlaylistListBox.SelectedItem as string;

            if (selectedPlaylist != null)
            {
                // Плейлист выбран, продолжаем с добавлением песни
                int playlistId = database.GetPlaylistIdByName(selectedPlaylist);

                if (!database.CheckSongInPlaylist(selectedSong.Id, playlistId))
                {
                    database.AddSongToPlaylist(selectedSong.Id, playlistId);
                    MessageBox.Show("Песня успешно добавлена в плейлист!");
                }
                else
                {
                    MessageBox.Show("Песня уже добавлена в выбранный плейлист!");
                }

                // Закрываем окно
                Close();
            }
            else
            {
                MessageBox.Show("Выберите плейлист");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно
            Close();
        }
    }
}
