using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;



namespace auth
{
    /// <summary>
    /// Логика взаимодействия для searchpage.xaml
    /// </summary>
    public partial class searchpage : Page
    {
        private DataBase database = new DataBase();
        public ObservableCollection<string> SearchResults { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SearchResultsAlbums { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SearchResultsArtists { get; set; } = new ObservableCollection<string>();

        MusicPlayer musicPlayer = MusicPlayer.Instance;
        private SongPlayer _songPlayer;


        public searchpage()
        {
            InitializeComponent();
            DataContext = this;
            _songPlayer = new SongPlayer();
            _songPlayer.SongChanged += SongPlayer_SongChanged;

            LoadCurrentSongFromDatabase();

            MusicPlayer.Instance.MediaEnded += MusicPlayer_MediaEnded;
            MusicPlayer.Instance.PositionChanged += Instance_PositionChanged;
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

        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            Utils.HandleMediaEnded(_songPlayer, playStop_btn, database);
        }


        private void SongPlayer_SongChanged(object sender, SongChangedEventArgs e)
        {
            Utils.Update_SongChanged(albumArtImage, songTitleTextBlock, artistTextBlock, e);
        }

        private void gohome(object sender, RoutedEventArgs e)
        {
            Utils.NavigateToHomePage(NavigationService);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search_Func();
        }

        List<Song> songResults;
        private void Search_Func()
        {
            string searchText = SearchTextBox.Text.Trim();

            DataBase database = new DataBase();

            SearchResultsSongsListBox.Items.Clear();
            SearchResultsAlbumsListBox.Items.Clear();
            SearchResultsArtistsListBox.Items.Clear();

            songResults = database.SearchSongs(searchText);
            List<albums> albumResults = database.SearchAlbums(searchText);
            List<artists> artistResults = database.SearchArtists(searchText);

            string songfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");
            foreach (var song in songResults)
            {
                song.IsSongliked = database.IsSongliked(database.GetUserIdByUsername(CurrentUser.Username), song.Id);
                song.LikeBtnSymb = song.IsSongliked ? "♥️" : "♡";
                string songFilePath = Path.Combine(songfolder, song.PathToImage);
                song.PathToImage = songFilePath;
                SearchResultsSongsListBox.Items.Add(song);
            }

            string albfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "oblojka");

            foreach (var album in albumResults)
            {
                album.IsAlbumLiked = database.IsAlbumLiked(database.GetUserIdByUsername(CurrentUser.Username), album.id);
                album.LikeButtonSymb = album.IsAlbumLiked ? "♥️" : "♡";
                string albumFilePath = Path.Combine(albfolder, album.cover);
                album.cover = albumFilePath;
                SearchResultsAlbumsListBox.Items.Add(album);
            }

            string avatarFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");

            foreach (var artist in artistResults)
            {
                artist.IsLiked = database.IsLiked(database.GetUserIdByUsername(CurrentUser.Username), artist.Id);
                artist.LikeButtonSymbol = artist.IsLiked ? "♥️" : "♡";

                string avatarFilePath = Path.Combine(avatarFolder, artist.Avatar);
                artist.Avatar = avatarFilePath;
                SearchResultsArtistsListBox.Items.Add(artist);
            }

            if (PlaybackManager.Instance.newSongSelected || PlaybackManager.Instance.playbackQueue.Count == 0)
            {
                PlaybackManager.Instance.ClearQueue();
                PlaybackManager.Instance.AddSongsToQueue(songResults);
                PlaybackManager.Instance.newSongSelected = false;
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Func();
            }

        }

        // btn_like

        private void AddRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            DataBase database = new DataBase();
            Button button = (Button)sender;
            artists artist = (artists)button.DataContext;

            if (artist.IsLiked)
            {
                button.Content = "♡";
                database.RemoveLikedArtist(database.GetUserIdByUsername(CurrentUser.Username), artist.Id);
                artist.IsLiked = false;
            }
            else
            {
                button.Content = "♥️";
                database.AddLikedArtist(database.GetUserIdByUsername(CurrentUser.Username), artist.Id);
                artist.IsLiked = true;
            }
            List<string> likedArtists = database.GetLikedArtists(database.GetUserIdByUsername(CurrentUser.Username));
            listBoxLikedArtists.ItemsSource = likedArtists;

        }

        // альбом лайу

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
           
            Button button = (Button)sender;
            albums album = (albums)button.DataContext;
            Utils.HandleLikeButtonClick(button, album, database, listBoxLikedAlbums);
        }

        // like song

        private void LikeButton_song(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Song song = (Song)button.DataContext;
            Utils.HandleLikeButtonClick(button, song, database);
        }

        // play btn
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

        // переход на исполнител


        private void ArtistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            artists selectedArtist = (sender as ListBox).SelectedItem as artists;

            if (selectedArtist != null)
            {
                int artistId = selectedArtist.Id;
                ArtistPage artistPage = new ArtistPage(artistId);
                NavigationService.Navigate(artistPage);
            }
        }

        // переход на альбом

        private void SearchResultsAlbumsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            albums selectedAlbum = (albums)SearchResultsAlbumsListBox.SelectedItem;
            if (selectedAlbum != null)
            {
                int albumId = selectedAlbum.id;
                NavigationService.Navigate(new Albumpage(albumId));
            }
        }

        // context menu
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

        // воспроизведение
        private void SongSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
                PlaybackManager.Instance.ClearQueue();
                PlaybackManager.Instance.AddSongsToQueue(songResults);
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
            PlaybackManager.Instance.AddSongsToQueue(songResults);
            PlaybackManager.Instance.newSongSelected = false;

            Song currentSong = PlaybackManager.Instance.GetCurrentSong();
            if (currentSong != null)
            {
                SongSelectionChanged(null, null);
            }

        }
    }
}
