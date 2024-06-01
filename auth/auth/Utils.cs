using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace auth
{
   public class Utils
    {
        public static void HandlePlayPauseButton(Button playStop_btn)
        {
            if (MusicPlayer.Instance.IsPlaying)
            {
                MusicPlayer.Instance.Pause();
                playStop_btn.Content = new TextBlock() { Text = "▶️" };
            }
            else
            {
                MusicPlayer.Instance.Play();
                playStop_btn.Content = new TextBlock() { Text = "⏸" };
            }
        }

        public static void Update_SongChanged(Image albumArtImage, TextBlock songTitleTextBlock, TextBlock artistTextBlock, SongChangedEventArgs e)
        {
            string songfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
            string songFilePath = Path.Combine(songfolder, e.NewSong.PathToImage);
            albumArtImage.Source = new BitmapImage(new Uri(songFilePath));
            songTitleTextBlock.Text = e.NewSong.Title;
            artistTextBlock.Text = e.NewSong.Artist;
        }

        public static void DeletePlaylist(TextBlock sender, ListBox PlaylistListBox, DataBase database)
        {
            string playlistName = sender.Text;
            int playlistId = database.GetPlaylistIdByName(playlistName);
            int userId = database.GetUserIdByUsername(CurrentUser.Username);

            if (playlistId != -1)
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить плейлист?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    database.DeletePlaylist(playlistId);
                    PlaylistListBox.ItemsSource = database.GetUserPlaylists(userId);
                }
            }
        }

        public static void OpenCreatePlaylistWindow(ObservableCollection<string> playlists, ListBox PlaylistListBox, DataBase database)
        {
            createplaylist createPlaylistWindow = new createplaylist();
            createPlaylistWindow.ShowDialog();
            if (createPlaylistWindow.DialogResult == true)
            {
                int userId = database.GetUserIdByUsername(CurrentUser.Username);
                List<string> playlistNames = database.GetUserPlaylists(userId);

                playlists.Clear();
                foreach (string playlistName in playlistNames)
                {
                    playlists.Add(playlistName);
                }

                PlaylistListBox.ItemsSource = playlists;
            }
        }


        public static void HandleMediaEnded(SongPlayer _songPlayer, Button playStop_btn, DataBase database)
        {
            Song currentSong = PlaybackManager.Instance.GetCurrentSong();

            if (currentSong != null)
            {
                MusicPlayer.Instance.CurrentSong = currentSong;
                _songPlayer.CurrentSong = currentSong;
                MusicPlayer.Instance.Play();

                int userId = database.GetUserIdByUsername(CurrentUser.Username);
                database.addlistens(currentSong.Id);
                int genreId = database.GetGenreId(currentSong.Genre);
                int artistId = database.GetArtistIdByName(currentSong.Artist);
                database.AddListenedSong(genreId, userId, currentSong.Id, artistId);
                database.InsertOrUpdateCurrentSong(currentSong);
            }
            else
            {
                MusicPlayer.Instance.Stop();
                playStop_btn.Content = new TextBlock() { Text = "▶️" };
            }
        }

        public static void OpenArtistPage(TextBlock sender, NavigationService navigationService, DataBase database)
        {
            string artistName = sender.Text;
            int artistId = database.GetArtistIdByName(artistName);

            if (artistId != -1)
            {
                ArtistPage artistPage = new ArtistPage(artistId);
                navigationService.Navigate(artistPage);
            }
        }

        public static void OpenAlbumPage(TextBlock sender, NavigationService navigationService, DataBase database)
        {
            string albumName = sender.Text;
            int albumId = database.GetAlbumIdByName(albumName);

            if (albumId != -1)
            {
                Albumpage albumPage = new Albumpage(albumId);
                navigationService.Navigate(albumPage);
            }
        }

        public static void OpenLikedPlaylist(NavigationService navigationService, DataBase database)
        {
            string username = CurrentUser.Username;
            int userId = 0;

            try
            {
                userId = database.GetUserIdByUsername(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            LikedPlaylistPage likedPlaylistPage = new LikedPlaylistPage();
            likedPlaylistPage.UserId = userId;
            navigationService.Navigate(likedPlaylistPage);
        }

        public static void NavigateToPlaylistPage(TextBlock textBlock, NavigationService navigationService, DataBase database)
        {
            if (textBlock != null)
            {
                string playlistName = textBlock.Text;
                int playlistId = database.GetPlaylistIdByName(playlistName);

                PlaylistPage playlistPage = new PlaylistPage(playlistId);
                navigationService.Navigate(playlistPage);
            }
        }

        public static void NavigateToHomePage(NavigationService navigationService)
        {
            homepage hp = new homepage();
            navigationService.Navigate(hp);
        }

        public static void NavigateToSearchPage(NavigationService navigationService)
        {
            searchpage sp = new searchpage();
            navigationService.Navigate(sp);
        }

        public static void AddToPlaylist(MenuItem menuItem, DataBase database)
        {
            Song song = menuItem.DataContext as Song;
            int userId = database.GetUserIdByUsername(CurrentUser.Username);
            Selectplaylist selectPlaylist = new Selectplaylist(song, userId);
            selectPlaylist.ShowDialog();
        }


        public static void OpenContextMenu(Button button, Song song)
        {
            var menuItem = button.ContextMenu.Items[0] as MenuItem;
            menuItem.DataContext = song;
            button.ContextMenu.IsOpen = true;
        }

        public static void HandleLikeButtonClick(Button button, Song song,  DataBase database)
        {
            if (song.IsSongliked)
            {
                button.Content = "♡";
                database.RemoveLikedSong(database.GetUserIdByUsername(CurrentUser.Username), song.Id);
                song.IsSongliked = false;
            }
            else
            {
                button.Content = "♥️";
                database.AddLikedSong(database.GetUserIdByUsername(CurrentUser.Username), song.Id);
                song.IsSongliked = true;
            }
        }


        public static void HandleNextSongButtonClick(MusicPlayer musicPlayer, SongPlayer songPlayer, DataBase database)
        {
            Song nextSong = PlaybackManager.Instance.GetNextSong();
            if (nextSong != null)
            {
                musicPlayer.CurrentSong = nextSong;
                songPlayer.CurrentSong = nextSong;
                musicPlayer.Play();

                int userId = database.GetUserIdByUsername(CurrentUser.Username);
                database.addlistens(nextSong.Id);
                int genreId = database.GetGenreId(nextSong.Genre);
                int artistId = database.GetArtistIdByName(nextSong.Artist);
                database.AddListenedSong(genreId, userId, nextSong.Id, artistId);

                database.InsertOrUpdateCurrentSong(nextSong);
            }
        }

        public static void HandlePreviousSongButtonClick(MusicPlayer musicPlayer, SongPlayer songPlayer, DataBase database)
        {
            Song previousSong = PlaybackManager.Instance.GetPreviousSong();
            if (previousSong != null)
            {
                musicPlayer.CurrentSong = previousSong;
                songPlayer.CurrentSong = previousSong;
                musicPlayer.Play();

                int userId = database.GetUserIdByUsername(CurrentUser.Username);
                database.addlistens(previousSong.Id);
                int genreId = database.GetGenreId(previousSong.Genre);
                int artistId = database.GetArtistIdByName(previousSong.Artist);
                database.AddListenedSong(genreId, userId, previousSong.Id, artistId);

                database.InsertOrUpdateCurrentSong(previousSong);
            }
        }

        private static bool isUpdatingPosition = false;

        public static void HandleSongProgressValueChanged(Slider songProgress, double newValue)
        {
            if (MusicPlayer.Instance.IsPlaying && !isUpdatingPosition)
            {
                isUpdatingPosition = true;
                MusicPlayer.Instance.Position = TimeSpan.FromSeconds(newValue);
                isUpdatingPosition = false;
            }
        }

        public static void HandlePlayerPositionChanged(Slider songProgress)
        {
            if (MusicPlayer.Instance.Duration.TotalSeconds > 0 && !isUpdatingPosition)
            {
                isUpdatingPosition = true;
                songProgress.Maximum = MusicPlayer.Instance.Duration.TotalSeconds;
                songProgress.Value = MusicPlayer.Instance.Position.TotalSeconds;
                isUpdatingPosition = false;
            }
        }

        public static void HandleSongSelectionChanged(ListBox listBox, MusicPlayer musicPlayer, SongPlayer songPlayer, Button playStopButton, DataBase database)
        {
            if (listBox != null && listBox.SelectedItem != null)
            {
                Song selectedSong = listBox.SelectedItem as Song;
                if (!PlaybackManager.Instance.playbackQueue.Contains(selectedSong))
                {
                    PlaybackManager.Instance.AddSongsToQueue(new List<Song> { selectedSong });
                }

                int selectedSongIndex = PlaybackManager.Instance.playbackQueue.IndexOf(selectedSong);
                PlaybackManager.Instance.SetCurrentSongIndex(selectedSongIndex);
                PlaybackManager.Instance.lastPlayedSongIndex = selectedSongIndex;

                musicPlayer.CurrentSong = PlaybackManager.Instance.GetCurrentSong();
                songPlayer.CurrentSong = PlaybackManager.Instance.GetCurrentSong();
                musicPlayer.Play();
                playStopButton.Content = new TextBlock() { Text = "⏸" };

                int userId = database.GetUserIdByUsername(CurrentUser.Username);
                database.addlistens(selectedSong.Id);
                int genreId = database.GetGenreId(selectedSong.Genre);
                int artistId = database.GetArtistIdByName(selectedSong.Artist);
                database.AddListenedSong(genreId, userId, selectedSong.Id, artistId);
                database.InsertOrUpdateCurrentSong(selectedSong);
            }
        }

        public static void HandleLikeButtonClick(Button button, albums album, DataBase database, ListBox likedAlbumsListBox)
        {
            if (album.IsAlbumLiked)
            {
                button.Content = "♡";
                database.RemoveLikedAlbum(database.GetUserIdByUsername(CurrentUser.Username), album.id);
                album.IsAlbumLiked = false;
            }
            else
            {
                button.Content = "♥️";
                database.AddLikedAlbum(database.GetUserIdByUsername(CurrentUser.Username), album.id);
                album.IsAlbumLiked = true;
            }

            List<string> likedAlbums = database.GetLikedAlbums(database.GetUserIdByUsername(CurrentUser.Username));
            likedAlbumsListBox.ItemsSource = likedAlbums;
        }

        public static void LoadPageData(DataBase database,  ListBox listBoxLikedArtists, ListBox listBoxLikedAlbums, Button playStop_btn)
        {
            int userId = database.GetUserIdByUsername(CurrentUser.Username);
            
            List<string> likedArtists = database.GetLikedArtists(userId);
            listBoxLikedArtists.ItemsSource = likedArtists;

            List<string> likedAlbums = database.GetLikedAlbums(userId);
            listBoxLikedAlbums.ItemsSource = likedAlbums;

            // Удаляем пустые альбомы из списка likedAlbums
            for (int i = likedAlbums.Count - 1; i >= 0; i--)
            {
                int albumId = database.GetAlbumIdByName(likedAlbums[i]);
                if (database.DeleteAlbumIfEmptyfromuser(albumId, userId))
                {
                    likedAlbums.RemoveAt(i);
                }
            }

            if (MusicPlayer.Instance.IsPlaying)
                playStop_btn.Content = new TextBlock() { Text = "⏸" };
            else
                playStop_btn.Content = new TextBlock() { Text = "▶️" };
        }
        public static void SetUnderlineAndHandCursor(TextBlock textBlock)
        {
            textBlock.TextDecorations = TextDecorations.Underline;
            textBlock.Cursor = Cursors.Hand;
        }

        public static void RemoveUnderlineAndSetArrowCursor(TextBlock textBlock)
        {
            textBlock.TextDecorations = null;
            textBlock.Cursor = Cursors.Arrow;
        }

    }

}

