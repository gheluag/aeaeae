using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Логика взаимодействия для ArtistPage.xaml
    /// </summary>
    public partial class ArtistPage : Page
    {
        public artists Artist { get; set; }
        public List<Song> Songs { get; set; }
        public List<albums> Album { get; set; }
        public int ArtistId { get; set; }
        DataBase db = new DataBase();
        DataBase database = new DataBase();
        MusicPlayer musicPlayer = MusicPlayer.Instance;

        private int currentArtistId;

        private SongPlayer _songPlayer;
        public ArtistPage(int artistId)
        {
            InitializeComponent();
            Artist = new artists();
            int userId = db.GetUserIdByUsername(CurrentUser.Username);
            Artist = db.GetArtistById(artistId);
            currentArtistId = artistId;
            artistname.Text = Artist.ArtistName;

            string avatarfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");
            string avatarpath = Path.Combine(avatarfolder, Artist.Avatar);
            artistavatar.Source = new BitmapImage(new Uri(avatarpath));
            Songs = db.GetSongsByArtist(artistId);
            Album = db.GetAlbumsByArtist(artistId);

            songlist.ItemsSource = Songs;
            albumlist.ItemsSource = Album;

            string songfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");

            foreach (var song in Songs)
            {
                song.IsSongliked = db.IsSongliked(db.GetUserIdByUsername(CurrentUser.Username), song.Id);
                song.LikeBtnSymb = song.IsSongliked ? "♥️" : "♡";
                string songFilePath = Path.Combine(songfolder, song.PathToImage);
                song.PathToImage = songFilePath;
            }
            string albfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
            foreach (var album in Album)
            {
                album.IsAlbumLiked = db.IsAlbumLiked(db.GetUserIdByUsername(CurrentUser.Username), album.id);
                album.LikeButtonSymb = album.IsAlbumLiked ? "♥️" : "♡";
                string albumFilePath = Path.Combine(albfolder, album.cover);
                album.cover = albumFilePath;
            }

            

            Artist.IsLiked = db.IsLiked(userId, Artist.Id);
            Artist.LikeButtonSymbol = Artist.IsLiked ? "♥️" : "♡";

            DataContext = Artist;

            musicPlayer = MusicPlayer.Instance;

            _songPlayer = new SongPlayer();
            _songPlayer.SongChanged += SongPlayer_SongChanged;


            if (PlaybackManager.Instance.newSongSelected || PlaybackManager.Instance.playbackQueue.Count == 0)
            {
                PlaybackManager.Instance.ClearQueue();
                PlaybackManager.Instance.AddSongsToQueue(Songs);
                PlaybackManager.Instance.newSongSelected = false;
            }


            LoadCurrentSongFromDatabase();

            MusicPlayer.Instance.PositionChanged += Instance_PositionChanged;

            MusicPlayer.Instance.MediaEnded += MusicPlayer_MediaEnded;
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
        private void AddRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = db.GetUserIdByUsername(CurrentUser.Username);

            if (Artist.IsLiked)
            {
                db.RemoveLikedArtist(userId, Artist.Id);
                Artist.IsLiked = false;
                Artist.LikeButtonSymbol = "♡";
            }
            else
            {
                db.AddLikedArtist(userId, Artist.Id);
                Artist.IsLiked = true;
                Artist.LikeButtonSymbol = "♥️";
            }

            List<string> likedArtists = db.GetLikedArtists(db.GetUserIdByUsername(CurrentUser.Username));
            listBoxLikedArtists.ItemsSource = likedArtists;
            Page newPage = new ArtistPage(currentArtistId);
            NavigationService?.Navigate(newPage);
        }

        private void LikeButton_song(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Song song = (Song)button.DataContext;
            Utils.HandleLikeButtonClick(button, song, database);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            albums album = (albums)button.DataContext;
            Utils.HandleLikeButtonClick(button, album, database, listBoxLikedAlbums);
        }

        // album page

        private void SearchResultsAlbumsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            albums selectedAlbum = (albums)albumlist.SelectedItem;
            if (selectedAlbum != null)
            {
                int albumId = selectedAlbum.id;
                NavigationService.Navigate(new Albumpage(albumId));
            }
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


        // play song

        private void SongSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
                PlaybackManager.Instance.ClearQueue();
                PlaybackManager.Instance.AddSongsToQueue(Songs);
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


        // queue

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
            PlaybackManager.Instance.AddSongsToQueue(Songs);
            PlaybackManager.Instance.newSongSelected = false;

            Song currentSong = PlaybackManager.Instance.GetCurrentSong();
            if (currentSong != null)
            {
                SongSelectionChanged(null, null);
            }

        }
    }
}
