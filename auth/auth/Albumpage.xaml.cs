using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.IO;

namespace auth
{
    /// <summary>
    /// Логика взаимодействия для Albumpage.xaml
    /// </summary>
    public partial class Albumpage : Page
    {
        private albums Album { get; set; }
        private DataBase db = new DataBase();
        private int currentAlbumId;
        private DataBase database = new DataBase();
        MusicPlayer musicPlayer = MusicPlayer.Instance;
        private SongPlayer _songPlayer;
        public List<Song> songs;

        public Albumpage(int albumId)
        {
            InitializeComponent();

            Album = db.GetAlbumById(albumId);
            currentAlbumId = albumId;
            int userId = db.GetUserIdByUsername(CurrentUser.Username);
            Album.artist = db.GetArtistNameByAlbumId(albumId);
            albumname.Text = Album.alb_name;
            string albfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
            string albpath = Path.Combine(albfolder, Album.cover);
            albcover.Source = new BitmapImage(new Uri(albpath));
            artistname.Text = db.GetArtistNameByAlbumId(Album.id);
            string songfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
            songs = db.GetSongsByAlbum(albumId);
            songlist.ItemsSource = songs;
            foreach (var song in songs)
            {
                song.IsSongliked = db.IsSongliked(db.GetUserIdByUsername(CurrentUser.Username), song.Id);
                song.LikeBtnSymb = song.IsSongliked ? "♥️" : "♡";
                string songFilePath = Path.Combine(songfolder, song.PathToImage);
                song.PathToImage = songFilePath;
            }

            Album.IsAlbumLiked = db.IsAlbumLiked(userId, Album.id);
            Album.LikeButtonSymb = Album.IsAlbumLiked ? "♥️" : "♡";
            DataContext = Album;
            musicPlayer = MusicPlayer.Instance;

            _songPlayer = new SongPlayer();
            _songPlayer.SongChanged += SongPlayer_SongChanged;

            if (PlaybackManager.Instance.newSongSelected || PlaybackManager.Instance.playbackQueue.Count == 0)
            {
                PlaybackManager.Instance.ClearQueue();
                PlaybackManager.Instance.AddSongsToQueue(songs);
                PlaybackManager.Instance.newSongSelected = false;
            }


            LoadCurrentSongFromDatabase();

            MusicPlayer.Instance.MediaEnded += MusicPlayer_MediaEnded;

            MusicPlayer.Instance.PositionChanged += Instance_PositionChanged;
        }


        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            Utils.HandleMediaEnded(_songPlayer, playStop_btn, database);
        }


        private void SongPlayer_SongChanged(object sender, SongChangedEventArgs e)
        {
            Utils.Update_SongChanged(albumArtImage, songTitleTextBlock, artistTextBlock, e);
        }
        private void artistname_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Utils.SetUnderlineAndHandCursor(textBlock);
        }

        private void artistname_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Utils.RemoveUnderlineAndSetArrowCursor(textBlock);
        }

        private void artistname_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Utils.OpenArtistPage((TextBlock)sender, NavigationService, database);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {

            int userId = db.GetUserIdByUsername(CurrentUser.Username);

            if (Album.IsAlbumLiked)
            {
                db.RemoveLikedAlbum(userId, Album.id);
                Album.IsAlbumLiked = false;
                Album.LikeButtonSymb = "♡";
            }
            else
            {
                db.AddLikedAlbum(userId, Album.id);
                Album.IsAlbumLiked = true;
                Album.LikeButtonSymb = "♥️";
            }
            List<string> likedAlbum = db.GetLikedAlbums(db.GetUserIdByUsername(CurrentUser.Username));
            listBoxLikedAlbums.ItemsSource = likedAlbum;
            Page newPage = new Albumpage(currentAlbumId);
            NavigationService?.Navigate(newPage);
        }

        private void LikeButton_song(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Song song = (Song)button.DataContext;
            Utils.HandleLikeButtonClick(button, song, database);
        }

        // панель боковая

        private void gohome(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToHomePage(NavigationService);
        }
        private void gosearch(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToSearchPage(NavigationService);
        }

        private void PlayStop_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandlePlayPauseButton(playStop_btn);
        }

        private void OpenLikedPlaylist(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenLikedPlaylist(NavigationService, database);
        }

        private void OpenCreatePlaylistWindow(object sender, RoutedEventArgs e)
        {
            Utils.OpenCreatePlaylistWindow(playlists, PlaylistListBox, database);
        }

        // плейлист

        private ObservableCollection<string> playlists;


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            DataBase db = new DataBase();
            int userId = db.GetUserIdByUsername(CurrentUser.Username);
            List<string> playlistNames = db.GetUserPlaylists(userId);

            playlists = new ObservableCollection<string>(playlistNames);

            PlaylistListBox.ItemsSource = playlists;

            Utils.LoadPageData(database, listBoxLikedArtists, listBoxLikedAlbums, playStop_btn);
        }

        // открыть плейлист

        private void OpenPlaylistPage(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            Utils.NavigateToPlaylistPage(textBlock, NavigationService, database);
        }


        // удалить пл

        private void DeletePlaylist(object sender, MouseButtonEventArgs e)
        {
            Utils.DeletePlaylist((TextBlock)sender, PlaylistListBox, database);
        }

        // menu

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Song song = button.DataContext as Song;
            Utils.OpenContextMenu(button, song);
        }

        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Utils.AddToPlaylist(sender as MenuItem, database);
        }


        // artist page

        private void OpenArtistPage(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenArtistPage((TextBlock)sender, NavigationService, database);
        }


        // страница альбомов

        private void OpenAlbumPage(object sender, MouseButtonEventArgs e)
        {
            Utils.OpenAlbumPage((TextBlock)sender, NavigationService, database);
        }


        // play song

        private void SongSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            PlaybackManager.Instance.ClearQueue();
            PlaybackManager.Instance.AddSongsToQueue(songs);
            PlaybackManager.Instance.newSongSelected = false;
            Utils.HandleSongSelectionChanged(listBox, musicPlayer, _songPlayer, playStop_btn, database);
        }

        public void LoadCurrentSongFromDatabase()
        {
            Song currentSong = database.GetCurrentSong();

            if (currentSong != null)
            {
                _songPlayer.CurrentSong = currentSong;
            }
        }


        // очередь



        private void NextSongButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandleNextSongButtonClick(musicPlayer, _songPlayer, database);
        }

        private void PreviousSongButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.HandlePreviousSongButtonClick(musicPlayer, _songPlayer, database);
        }

        private void songProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Utils.HandleSongProgressValueChanged(songProgress, e.NewValue);
        }

        private void Instance_PositionChanged(object sender, EventArgs e)
        {
            Utils.HandlePlayerPositionChanged(songProgress);
        }


        private bool isMixButtonChecked = false;

        private void mix_btn_Checked(object sender, RoutedEventArgs e)
        {
            isMixButtonChecked = true;
            PlaybackManager.Instance.ShuffleQueue();
        }

        private void mix_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            isMixButtonChecked = false;
            PlaybackManager.Instance.ClearQueue();
            PlaybackManager.Instance.AddSongsToQueue(songs);
            PlaybackManager.Instance.newSongSelected = false;
            Song currentSong = PlaybackManager.Instance.GetCurrentSong();
            if (currentSong != null)
            {
                SongSelectionChanged(null, null);
            }

        }

        private void mix_btn_Click(object sender, RoutedEventArgs e)
        {
            if (isMixButtonChecked == true)
            {
                PlaybackManager.Instance.ShuffleQueue();
            }
            else
            {

                Song currentSong = PlaybackManager.Instance.GetCurrentSong();
                if (currentSong != null)
                {
                    SongSelectionChanged(null, null);
                }
            }
        }
    }
}
