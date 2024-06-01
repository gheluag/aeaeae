using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BCrypt.Net;
using static auth.searchpage;

namespace auth
{

    // подключение
    public class DataBase
    {
        public MySqlConnection connection = new MySqlConnection("server=aimix.webtm.ru;port=3306;user=gelya; password=Justpassf0rvkr!;database=music_streaming");

        public void openConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public void closeConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }

        // авторизация

        public bool AuthenticateUser(string username, string password)
        {
            bool isAuthenticated = false;

            try
            {
                openConnection();

                string query = "SELECT passw FROM users WHERE username = @un";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@un", username);

                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string hashedPassword = reader.GetString("passw");

                    isAuthenticated = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                closeConnection();
            }

            return isAuthenticated;
        }

        // получение айди

        public int GetUserIdByUsername(string username)
        {
            int userId = 0;

            try
            {
                openConnection();

                MySqlCommand command = new MySqlCommand("SELECT id_user FROM users WHERE username = @username", connection);
                command.Parameters.AddWithValue("@username", username);

                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    userId = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                closeConnection();
            }

            return userId;
        }

        // регистрация

        public bool RegisterUser(string username, string password)
        {
            bool isRegistered = false;

            try
            {
                openConnection();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                string query = "INSERT INTO users (username, passw) VALUES (@un, @pw)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@un", username);
                command.Parameters.AddWithValue("@pw", hashedPassword);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    isRegistered = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                closeConnection();
            }

            return isRegistered;
        }

        // поиск песен

        public List<Song> SearchSongs(string searchText)
        {
            List<Song> songResults = new List<Song>();


            openConnection();


            string query = "SELECT * FROM song " +
                "JOIN artist ON song.artist_id = artist.id_artist " +
                "join genre on song.genre_id = genre.id_genre " +
                "WHERE song_name LIKE @searchText OR artist_name LIKE @searchText";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

            try
            {

                MySqlDataReader reader = command.ExecuteReader();


                while (reader.Read())
                {
                    Song song = new Song();
                    song.Id = Convert.ToInt32(reader["id_song"]);
                    song.Title = reader["song_name"].ToString();
                    song.Artist = reader["artist_name"].ToString();
                    song.Duration = reader["duration"].ToString();
                    song.Genre = reader.GetString("genre_name");
                    song.PathToFile = reader["audio_file"].ToString();
                    song.PathToImage = reader["image_file"].ToString();

                    songResults.Add(song);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                closeConnection();
            }

            songResults = songResults.Distinct().ToList();
            return songResults;
        }


        public List<albums> SearchAlbums(string searchText)
        {
            List<albums> albumResults = new List<albums>();

            string query = "SELECT album.id_album, album.album_name, artist.artist_name, album.image_file " +
                           "FROM album " +
                           "JOIN album_song ON album.id_album = album_song.album_id " +
                           "JOIN song ON album_song.song_id = song.id_song " +
                           "JOIN artist ON song.artist_id = artist.id_artist " +
                           "WHERE album.album_name LIKE @searchText or artist.artist_name like @searchText " +
                           "GROUP BY album.id_album, album.album_name, artist.artist_name";

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

            connection.Open();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                albums album = new albums();
                album.id = reader.GetInt32("id_album");
                album.alb_name = reader.GetString("album_name");
                album.artist = reader.GetString("artist_name");
                album.cover = reader.GetString("image_file");

                albumResults.Add(album);
            }

            reader.Close();
            connection.Close();

            albumResults = albumResults.Distinct().ToList();
            return albumResults;
        }

        public List<artists> SearchArtists(string searchText)
        {
            List<artists> artistResults = new List<artists>();
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM artist WHERE artist_name LIKE '%" + searchText + "%'", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                artists artist = new artists();
                artist.Id = reader.GetInt32("id_artist");
                artist.ArtistName = reader.GetString("artist_name");
                artist.Avatar = reader.GetString("avatar_file");
                artistResults.Add(artist);
            }
            reader.Close();
            closeConnection();
            return artistResults;
        }

        // liked artist

        public void AddLikedArtist(int userId, int artistId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("INSERT INTO liked_artist (user_id, artist_id) VALUES (@userId, @artistId)", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@artistId", artistId);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void RemoveLikedArtist(int userId, int artistId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("DELETE FROM liked_artist WHERE user_id = @userId AND artist_id = @artistId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@artistId", artistId);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public bool IsLiked(int userId, int artistId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM liked_artist WHERE user_id = @userId AND artist_id = @artistId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@artistId", artistId);
            int count = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return count > 0;
        }

        // отображение понр исполн

        public List<string> GetLikedArtists(int userId)
        {
            openConnection();
            List<string> likedArtists = new List<string>();
            MySqlCommand command = new MySqlCommand("SELECT artist_name FROM liked_artist " +
                                                    "JOIN artist ON liked_artist.artist_id = artist.id_artist " +
                                                    "WHERE user_id = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string artistName = reader.GetString("artist_name");
                likedArtists.Add(artistName);
            }
            reader.Close();
            closeConnection();
            return likedArtists;
        }

        // добавление альбома

        public void AddLikedAlbum(int userId, int albumId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("INSERT INTO liked_album (user_id, album_id) VALUES (@userId, @albumId)", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@albumId", albumId);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void RemoveLikedAlbum(int userId, int albumId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("DELETE FROM liked_album WHERE user_id = @userId AND album_id = @albumId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@albumId", albumId);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public bool IsAlbumLiked(int userId, int albumId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM liked_album WHERE user_id = @userId AND album_id = @albumId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@albumId", albumId);
            int count = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return count > 0;
        }

        // liked_album

        public List<string> GetLikedAlbums(int userId)
        {
            openConnection();
            List<string> likedAlbums = new List<string>();
            MySqlCommand command = new MySqlCommand("SELECT album_name FROM liked_album " +
                                                    "JOIN album ON liked_album.album_id = album.id_album " +
                                                    "WHERE user_id = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string albumName = reader.GetString("album_name");
                likedAlbums.Add(albumName);
            }
            reader.Close();
            closeConnection();
            return likedAlbums;
        }


        // liked song

        public bool IsSongliked(int userId, int songId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM liked_song WHERE user_id = @userId AND song_id = @songId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@songId", songId);
            int count = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return count > 0;
        }

        public void AddLikedSong(int userId, int songId)
        {

            openConnection();
            MySqlCommand command = new MySqlCommand("INSERT INTO liked_song (user_id, song_id) VALUES (@userId, @songId)", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@songId", songId);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void RemoveLikedSong(int userId, int songId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("DELETE FROM liked_song WHERE user_id = @userId AND song_id = @songId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@songId", songId);
            command.ExecuteNonQuery();
            closeConnection();
        }


        // создание плейлистов

        public int GetPlaylistIdByName(string playlistName)
        {
            int playlistId = 0;

            openConnection();
            MySqlCommand cmd = new MySqlCommand("SELECT id_playlist FROM playlist WHERE playlist_name = @playlistName", connection);
            cmd.Parameters.AddWithValue("@playlistName", playlistName);
            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                playlistId = Convert.ToInt32(result);
            }
            closeConnection();

            return playlistId;
        }

        public void AddPlaylist(string playlistName)
        {
            openConnection();
            MySqlCommand cmd = new MySqlCommand("INSERT INTO playlist (playlist_name) VALUES (@playlistName)", connection);
            cmd.Parameters.AddWithValue("@playlistName", playlistName);
            cmd.ExecuteNonQuery();
            closeConnection();
        }

        public List<string> GetPlaylists()
        {
            List<string> playlists = new List<string>();

            openConnection();
            MySqlCommand cmd = new MySqlCommand("SELECT playlist_name FROM playlist", connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                playlists.Add(reader.GetString("playlist_name"));
            }
            reader.Close();
            closeConnection();

            return playlists;
        }

        public void AddPlaylistForUser(int userId, int playlistId)
        {
            openConnection();
            MySqlCommand cmd = new MySqlCommand("INSERT INTO user_playlist (user_id, playlist_id) VALUES (@userId, @playlistId)", connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@playlistId", playlistId);
            cmd.ExecuteNonQuery();
            closeConnection();
        }
        public List<string> GetUserPlaylists(int userId)
        {
            List<string> playlists = new List<string>();

            openConnection();
            MySqlCommand cmd = new MySqlCommand("SELECT p.playlist_name FROM playlist p INNER JOIN user_playlist up ON p.id_playlist = up.playlist_id WHERE up.user_id = @userId", connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                playlists.Add(reader.GetString("playlist_name"));
            }
            reader.Close();
            closeConnection();

            return playlists;
        }

        // понравившееся (плейлист)

        public List<Song> GetLikedSongs(int userId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("select * from liked_song ls " +
                    "join song s on ls.song_id = s.id_song " +
                    "join artist a on s.artist_id = a.id_artist " +
                    "join genre g on s.genre_id = g.id_genre " +
                    "where ls.user_id = @UserId", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            MySqlDataReader reader = command.ExecuteReader();

            List<Song> likedSongs = new List<Song>();
            while (reader.Read())
            {
                Song song = new Song()
                {
                    Id = Convert.ToInt32(reader["id_song"]),
                    Title = reader["song_name"].ToString(),
                    Artist = reader.GetString("artist_name"),
                    Duration = reader["duration"].ToString(),
                    Listens = Convert.ToInt32(reader["listens"]),
                    Genre = reader.GetString("genre_name"),
                    PathToFile = reader["audio_file"].ToString(),
                    PathToImage = reader["image_file"].ToString()
                };

                likedSongs.Add(song);
            }
            reader.Close();

            closeConnection();
            return likedSongs;
        }

        // аватарки

        public void SaveImagePathToDatabase(string filePath, int userId)
        {
            try
            {
                openConnection();

                MySqlCommand command = new MySqlCommand();
                command.Connection = connection;
                command.CommandText = "UPDATE users SET avatar_file = @avatarPath WHERE id_user = @userId";

                command.Parameters.AddWithValue("@avatarPath", filePath);
                command.Parameters.AddWithValue("@userId", userId);

                command.ExecuteNonQuery();

                closeConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string GetAvatarPathFromDatabase(int userId)
        {
            try
            {
                openConnection();

                MySqlCommand command = new MySqlCommand();
                command.Connection = connection;
                command.CommandText = "SELECT avatar_file FROM users WHERE id_user = @userId";

                command.Parameters.AddWithValue("@userId", userId);

                string avatarPath = (string)command.ExecuteScalar();

                closeConnection();

                return avatarPath;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        // change username

        public void UpdateUsernameInDatabase(string newUsername)
        {
            try
            {
                DataBase db = new DataBase();
                db.openConnection();
                int userId = GetUserIdByUsername(CurrentUser.Username);
                MySqlCommand command = new MySqlCommand();
                command.Connection = db.connection;
                command.CommandText = "UPDATE users SET username = @username WHERE id_user = @userId";

                command.Parameters.AddWithValue("@username", newUsername);
                command.Parameters.AddWithValue("@userId", userId);

                command.ExecuteNonQuery();

                db.closeConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // страница исполнителей

        public artists GetArtist(int artistId)
        {
            artists artist = new artists();

            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT * FROM artist WHERE id_artist = @artistId", connection);
            command.Parameters.AddWithValue("@artistId", artistId);

            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                artist.Id = Convert.ToInt32(reader["id_artist"]);
                artist.ArtistName = reader["artist_name"].ToString();
                artist.Avatar = reader["avatar_file"].ToString();
            }

            reader.Close();
            closeConnection();
            return artist;
        }

        public artists GetArtistById(int artistId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM artist WHERE id_artist = @id", connection);
            command.Parameters.AddWithValue("@id", artistId);
            MySqlDataReader reader = command.ExecuteReader();

            artists artist = null;
            if (reader.Read())
            {
                artist = new artists()
                {
                    Id = Convert.ToInt32(reader["id_artist"]),
                    ArtistName = reader["artist_name"].ToString(),
                    Avatar = reader["avatar_file"].ToString()
                };
            }

            reader.Close();
            closeConnection();

            return artist;
        }

        public List<Song> GetSongsByArtist(int artistId)
        {
            List<Song> songs = new List<Song>();
            string query = $"SELECT * FROM song s join artist a on s.artist_id = a.id_artist join genre g on s.genre_id = g.id_genre WHERE artist_id = {artistId}";

            openConnection();

            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Song song = new Song();
                song.Id = reader.GetInt32("id_song");
                song.Title = reader.GetString("song_name");
                song.Artist = reader.GetString("artist_name");
                song.Duration = reader["duration"].ToString();
                song.Listens = Convert.ToInt32(reader["listens"]);
                song.Genre = reader.GetString("genre_name");
                song.PathToImage = reader.GetString("image_file");
                song.PathToFile = reader["audio_file"].ToString();

                songs.Add(song);
            }

            reader.Close();
            closeConnection();

            return songs;
        }

        public List<albums> GetAlbumsByArtist(int artistId)
        {
            List<albums> albums = new List<albums>();
            string query = $"SELECT al.* FROM album al join album_song aso on al.id_album = aso.album_id join song s on aso.song_id = s.id_song WHERE s.artist_id = {artistId} " +
                $"GROUP BY al.id_album, al.album_name";
            openConnection();
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                albums album = new albums();
                album.id = reader.GetInt32("id_album");
                album.alb_name = reader.GetString("album_name");
                album.cover = reader.GetString("image_file");

                albums.Add(album);
            }

            reader.Close();
            closeConnection();
            return albums;
        }

        // страница альбомов

        public List<Song> GetSongsByAlbum(int albumId)
        {
            List<Song> songList = new List<Song>();

            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM song " +
                "JOIN album_song ON song.id_song = album_song.song_id " +
                "join artist on song.artist_id = artist.id_artist join genre on song.genre_id = genre.id_genre" +
                " WHERE album_song.album_id = @albumId", connection);
            command.Parameters.AddWithValue("@albumId", albumId);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Song song = new Song()
                {
                    Id = Convert.ToInt32(reader["id_song"]),
                    Title = reader["song_name"].ToString(),
                    Artist = reader["artist_name"].ToString(),
                    Album = reader["album_id"].ToString(),
                    Duration = reader["duration"].ToString(),
                    Listens = Convert.ToInt32(reader["listens"]),
                    Genre = reader.GetString("genre_name"),
                    PathToImage = reader["image_file"].ToString(),
                    PathToFile = reader["audio_file"].ToString()
                };

                songList.Add(song);
            }

            reader.Close();
            closeConnection();

            return songList;
        }

        public albums GetAlbumById(int albumId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM album WHERE id_album = @albumId", connection);
            command.Parameters.AddWithValue("@albumId", albumId);
            MySqlDataReader reader = command.ExecuteReader();

            albums album = null;

            if (reader.Read())
            {
                album = new albums()
                {
                    id = Convert.ToInt32(reader["id_album"]),
                    alb_name = reader["album_name"].ToString(),
                    cover = reader["image_file"].ToString()
                };
            }

            reader.Close();
            closeConnection();

            return album;
        }

        public string GetArtistNameByAlbumId(int albumId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT a.artist_name FROM artist a join song s on a.id_artist = s.artist_id  join album_song als on s.id_song = als.song_id JOIN album al ON als.album_id = al.id_album WHERE al.id_album = @albumId", connection);
            command.Parameters.AddWithValue("@albumId", albumId);
            string artistName = command.ExecuteScalar()?.ToString();
            closeConnection();

            return artistName;
        }

        // playlist page

        public Playlists GetPlaylist(int playlistId)
        {
            string query = "SELECT playlist_name, image_file FROM playlist WHERE id_playlist = @playlistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@playlistId", playlistId);

            openConnection();

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Playlists playlist = new Playlists();
                    playlist.Name = reader.GetString("playlist_name");
                    return playlist;
                }
            }
            closeConnection();
            return null;
        }

        public List<Song> GetPlaylistSongs(int playlistId)
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM song s " +
                "join artist a on s.artist_id = a.id_artist " +
                "JOIN playlist_song ps ON s.id_song = ps.song_id join genre g on s.genre_id = g.id_genre" +
                " WHERE ps.playlist_id = @playlistId", connection);
            command.Parameters.AddWithValue("@playlistId", playlistId);
            MySqlDataReader reader = command.ExecuteReader();

            List<Song> playlistSongs = new List<Song>();
            while (reader.Read())
            {
                Song song = new Song()
                {
                    Id = Convert.ToInt32(reader["id_song"]),
                    Title = reader["song_name"].ToString(),
                    Artist = reader.GetString("artist_name"),
                    Duration = reader["duration"].ToString(),
                    Listens = Convert.ToInt32(reader["listens"]),
                    Genre = reader.GetString("genre_name"),
                    PathToFile = reader["audio_file"].ToString(),
                    PathToImage = reader["image_file"].ToString()
                };

                playlistSongs.Add(song);
            }
            reader.Close();

            closeConnection();
            return playlistSongs;
        }

        public void RemoveFromPlaylist(int playlistId, int songId)
        {
            MySqlCommand command = new MySqlCommand("DELETE FROM playlist_song WHERE playlist_id = @playlistId AND song_id = @songId", connection);
            command.Parameters.AddWithValue("@playlistId", playlistId);
            command.Parameters.AddWithValue("@songId", songId);

            openConnection();
            command.ExecuteNonQuery();
            closeConnection();
        }


        // add to pl

        public void AddSongToPlaylist(int songId, int playlistId)
        {
            openConnection();

            MySqlCommand command = new MySqlCommand("INSERT INTO playlist_song (playlist_id, song_id) VALUES (@playlistId, @songId)", connection);
            command.Parameters.AddWithValue("@playlistId", playlistId);
            command.Parameters.AddWithValue("@songId", songId);

            command.ExecuteNonQuery();

            closeConnection();
        }

        public bool CheckSongInPlaylist(int songId, int playlistId)
        {
            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT * FROM playlist_song WHERE playlist_id = @playlistId AND song_id = @songId", connection);
            command.Parameters.AddWithValue("@playlistId", playlistId);
            command.Parameters.AddWithValue("@songId", songId);

            MySqlDataReader reader = command.ExecuteReader();
            bool songExists = reader.HasRows;
            reader.Close();

            closeConnection();
            return songExists;
        }

        // удаление плейлиста

        public void DeletePlaylist(int playlistId)
        {
            try
            {
                openConnection();

                string deleteSongsQuery = "DELETE FROM playlist_song WHERE playlist_id = @playlistId";
                MySqlCommand deleteSongsCmd = new MySqlCommand(deleteSongsQuery, connection);
                deleteSongsCmd.Parameters.AddWithValue("@playlistId", playlistId);
                deleteSongsCmd.ExecuteNonQuery();

                string deleteUserPlaylistQuery = "DELETE FROM user_playlist WHERE playlist_id = @playlistId";
                MySqlCommand deleteUserPlaylistCmd = new MySqlCommand(deleteUserPlaylistQuery, connection);
                deleteUserPlaylistCmd.Parameters.AddWithValue("@playlistId", playlistId);
                deleteUserPlaylistCmd.ExecuteNonQuery();

                string deletePlaylistQuery = "DELETE FROM playlist WHERE id_playlist = @playlistId";
                MySqlCommand deletePlaylistCmd = new MySqlCommand(deletePlaylistQuery, connection);
                deletePlaylistCmd.Parameters.AddWithValue("@playlistId", playlistId);
                deletePlaylistCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                closeConnection();
            }
        }

        // artist id

        public int GetArtistIdByName(string artistName)
        {
            int artistId = -1;

            try
            {
                openConnection();

                string query = "SELECT id_artist FROM artist WHERE artist_name = @artistName";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@artistName", artistName);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    artistId = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                closeConnection();
            }

            return artistId;
        }


        // id album

        public int GetAlbumIdByName(string albumName)
        {
            int albumId = -1;

            try
            {
                openConnection();

                string query = "SELECT id_album FROM album WHERE album_name = @albumName";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@albumName", albumName);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    albumId = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                closeConnection();
            }

            return albumId;
        }


        // artist auth

        public bool CheckArtistCredentials(string username, string password)
        {
            bool result = false;

            try
            {
                openConnection();

                MySqlCommand command = new MySqlCommand("SELECT passw FROM artist_acc WHERE artist_name_log = @username", connection);
                command.Parameters.AddWithValue("@username", username);

                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string hashedPasswordFromDB = reader.GetString("passw");

                    result = BCrypt.Net.BCrypt.Verify(password, hashedPasswordFromDB);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                closeConnection();
            }

            return result;
        }

        public bool RegisterArtist(string username, string password, string artistName)
        {
            bool result = false;

            openConnection();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            MySqlCommand command = new MySqlCommand("INSERT INTO artist_acc (artist_name_log, passw) VALUES (@username, @password)", connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", hashedPassword);

            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                int artistAccId = (int)command.LastInsertedId;

                command = new MySqlCommand("INSERT INTO artist (artist_name, artist_acc_id) VALUES (@artistName, @artistAccId)", connection);
                command.Parameters.AddWithValue("@artistName", artistName);
                command.Parameters.AddWithValue("@artistAccId", artistAccId);

                int artistRowsAffected = command.ExecuteNonQuery();
                if (artistRowsAffected > 0)
                {
                    result = true;
                }
            }

            closeConnection();

            return result;
        }

        public bool CheckArtistUsernameExists(string username)
        {
            bool result = false;

            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM artist_acc WHERE artist_name_log = @username", connection);
            command.Parameters.AddWithValue("@username", username);

            int count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                result = true;
            }

            closeConnection();

            return result;
        }

        public bool CheckUserUsernameExists(string username)
        {
            bool result = false;

            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", connection);
            command.Parameters.AddWithValue("@username", username);

            int count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                result = true;
            }

            closeConnection();

            return result;
        }

        // статистика

        public List<Song> GetArtistSongsStats(int artistId)
        {
            List<Song> songsStats = new List<Song>();

            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT s.id_song, s.song_name, s.listens, IFNULL(al.album_name, 'Сингл') AS album_name FROM song s LEFT JOIN album_song aso ON s.id_song = aso.song_id LEFT JOIN album al ON aso.album_id = al.id_album WHERE s.artist_id = @artistId ORDER BY s.listens DESC", connection);
            command.Parameters.AddWithValue("@artistId", artistId);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int Id = reader.GetInt32("id_song");
                    string songName = reader.GetString("song_name");
                    int listens = reader.GetInt32("listens");
                    string albumName = reader.GetString("album_name");

                    Song songStats = new Song
                    {
                        Id = Id,
                        Title = songName,
                        Listens = listens,
                        Album = albumName
                    };

                    songsStats.Add(songStats);
                }

                reader.Close();
            }

            closeConnection();

            return songsStats;
        }

        public int GetListensCount(int artistId)
        {
            openConnection();
            string query = "SELECT IFNULL(avg(listens), 0) FROM song WHERE artist_id = @artistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@artistId", artistId);
            int listensCount = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return listensCount;
        }

        public int GetLikedArtistsCount(int artistId)
        {
            openConnection();
            string query = "SELECT COUNT(*) FROM liked_artist WHERE artist_id = @artistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@artistId", artistId);
            int likedArtistsCount = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return likedArtistsCount;
        }

        public int GetLikedAlbumsCount(int artistId)
        {
            openConnection();
            string query = "SELECT COUNT(la.album_id) FROM liked_album la join album_song aso on la.album_id = aso.album_id JOIN song s ON aso.song_id = s.id_song WHERE s.artist_id = @artistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@artistId", artistId);
            int likedAlbumsCount = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return likedAlbumsCount;
        }

        public int GetLikedSongsCount(int artistId)
        {
            openConnection();
            string query = "SELECT COUNT(song_id) FROM liked_song INNER JOIN song ON liked_song.song_id = song.id_song WHERE song.artist_id = @artistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@artistId", artistId);
            int likedSongsCount = Convert.ToInt32(command.ExecuteScalar());
            closeConnection();
            return likedSongsCount;
        }

        public void UpdateArtistName(int artistId, string artistName)
        {
            string query = "UPDATE artist SET artist_name = @artistName WHERE id_artist = @artistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@artistId", artistId);
            command.Parameters.AddWithValue("@artistName", artistName);
            openConnection();
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void UpdateArtistImage(int artistId, string imageUrl)
        {
            string query = "UPDATE artist SET avatar_file = @imageUrl WHERE id_artist = @artistId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@artistId", artistId);
            command.Parameters.AddWithValue("@imageUrl", imageUrl);
            openConnection();
            command.ExecuteNonQuery();
            closeConnection();
        }

        // загрузка песен

        public int GetGenreId(string genreName)
        {
            int genreId = -1;

            try
            {
                openConnection();

                string query = "SELECT id_genre FROM genre WHERE genre_name = @genreName";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@genreName", genreName);

                object result = command.ExecuteScalar();
                if (result != null)
                {
                    genreId = Convert.ToInt32(result);
                }

                closeConnection();
            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"Ошибка при получении идентификатора жанра: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return genreId;
        }

        public List<string> GetGenres()
        {
            List<string> genres = new List<string>();

            try
            {
                openConnection();

                string query = "SELECT genre_name FROM genre";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string genre = reader.GetString("genre_name");
                    genres.Add(genre);
                }
                reader.Close();

                closeConnection();
            }
            catch (Exception ex)
            {
                // Обработка ошибок при получении списка жанров
                MessageBox.Show($"Ошибка при получении списка жанров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return genres;
        }


        // загрузка альбомов

        public void AddAlbum(string albumName, string coverPath)
        {
            openConnection();
            string query = "INSERT INTO album (album_name, image_file) VALUES (@name, @cover)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", albumName);
            if (coverPath.StartsWith("file:///"))
            {
                coverPath = coverPath.Substring(8);
            }
            command.Parameters.AddWithValue("@cover", coverPath);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void AddSong(string songName, string duration, int artistId, int genreId, string audioFilePath, string imagePath)
        {
            openConnection();
            string query = "INSERT INTO song (song_name, duration, artist_id, genre_id, audio_file, image_file) " +
                           "VALUES (@name, @duration, @artistId, @genreId, @audioFile, @image_file)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", songName);
            command.Parameters.AddWithValue("@duration", duration);
            command.Parameters.AddWithValue("@artistId", artistId);
            command.Parameters.AddWithValue("@genreId", genreId);
            command.Parameters.AddWithValue("@audioFile", audioFilePath);
            

            if (imagePath.StartsWith("file:///"))
            {
                imagePath = imagePath.Substring(8);
            }

            command.Parameters.AddWithValue("@image_file", imagePath);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void AddSongToAlbum(int albumId, int songId)
        {
            openConnection();
            string query = "INSERT INTO album_song (album_id, song_id) VALUES (@albumId, @songId)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@albumId", albumId);
            command.Parameters.AddWithValue("@songId", songId);
            command.ExecuteNonQuery();
            closeConnection();
        }


        public int GetLatestAlbumId()
        {
            int albumId = 0;

            openConnection();
            string query = "SELECT MAX(id_album) FROM album";
            MySqlCommand command = new MySqlCommand(query, connection);
            object result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                albumId = Convert.ToInt32(result);
            }
            closeConnection();

            return albumId;
        }

        public int GetLatestSongId()
        {
            int songId = 0;

            openConnection();
            string query = "SELECT MAX(id_song) FROM song";
            MySqlCommand command = new MySqlCommand(query, connection);
            object result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                songId = Convert.ToInt32(result);
            }
            closeConnection();

            return songId;
        }

        // новинки

        public List<Song> GetNewSongs()
        {
            openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM song s " +
                "JOIN artist ar ON s.artist_id = ar.id_artist JOIN genre g ON s.genre_id = g.id_genre " +
                "WHERE date_of_pub >= DATE_SUB(NOW(), INTERVAL 7 DAY)", connection);
            MySqlDataReader reader = command.ExecuteReader();

            List<Song> newSongs = new List<Song>();
            while (reader.Read())
            {
                Song song = new Song()
                {
                    Id = Convert.ToInt32(reader["id_song"]),
                    Title = reader["song_name"].ToString(),
                    Artist = reader.GetString("artist_name"),
                    Duration = reader["duration"].ToString(),
                    Listens = Convert.ToInt32(reader["listens"]),
                    Genre = reader.GetString("genre_name"),
                    PathToFile = reader["audio_file"].ToString(),
                    PathToImage = reader["image_file"].ToString()
                };

                newSongs.Add(song);
            }
            reader.Close();
            closeConnection();
            return newSongs;
        }


        // песня дня

        public bool IsTodaySongExist()
        {
            bool isExist = false;

            openConnection();

            string query = "SELECT COUNT(*) FROM randomsong WHERE song_date = CURDATE();";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            int count = Convert.ToInt32(cmd.ExecuteScalar());

            if (count > 0)
            {
                isExist = true;
            }

            closeConnection();

            return isExist;
        }


        public Song GetRandomSong()
        {
            Song song = null;

            openConnection();

            string query = "SELECT * FROM song " +
                "JOIN artist ON song.artist_id = artist.id_artist " +
                "join genre on song.genre_id = genre.id_genre " +
                "ORDER BY RAND() LIMIT 1;";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.Read())
            {
                song = new Song
                {
                    Id = Convert.ToInt32(dataReader["id_song"]),
                    Title = dataReader["song_name"].ToString(),
                    Artist = dataReader["artist_name"].ToString(),
                    Listens = Convert.ToInt32(dataReader["listens"]),
                    Duration = dataReader["duration"].ToString(),
                    Genre = dataReader.GetString("genre_name"),
                    PathToImage = dataReader["image_file"].ToString(),
                    PathToFile = dataReader.GetString("audio_file")
                };
            }

            dataReader.Close();

            closeConnection();

            return song;
        }


        public Song GetCurrentTodaySongFromView()
        {
            Song todaySong = null;

            openConnection();

            string query = "SELECT * FROM randomsong WHERE song_date = CURDATE();";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.Read())
            {
                todaySong = new Song
                {
                    Id = Convert.ToInt32(dataReader["id"]),
                    Title = dataReader["song_name"].ToString(),
                    Artist = dataReader["artist_name"].ToString(),
                    Listens = Convert.ToInt32(dataReader["listens"]),
                    Duration = dataReader["duration"].ToString(),
                    Genre = dataReader.GetString("genre_name"),
                    PathToImage = dataReader["image_file"].ToString(),
                    PathToFile = dataReader.GetString("audio_file")
                };
            }

            dataReader.Close();

            closeConnection();

            return todaySong;
        }

        public void SaveSongToView(Song song)
        {
            openConnection();

            string query = "INSERT INTO randomsong (id, song_name, artist_name, listens, duration, image_file, audio_file, genre_name, song_date) VALUES (@id, @title, @artist, @listens, @duration, @pathToImage, @audiofile, @genre, CURDATE());";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", song.Id);
            cmd.Parameters.AddWithValue("@title", song.Title);
            cmd.Parameters.AddWithValue("@artist", song.Artist);
            cmd.Parameters.AddWithValue("@listens", song.Listens);
            cmd.Parameters.AddWithValue("@duration", song.Duration);
            cmd.Parameters.AddWithValue("@pathToImage", song.PathToImage);
            cmd.Parameters.AddWithValue("@audiofile", song.PathToFile);
            cmd.Parameters.AddWithValue("@genre", song.Genre);
            cmd.ExecuteNonQuery();


            closeConnection();
        }


        // listens

        public void addlistens(int songId)
        {
            openConnection();
            string query = "UPDATE song SET listens = listens + 1 WHERE id_song = @songId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@songId", songId);

            command.ExecuteNonQuery();
            closeConnection();
        }


        // на основе предпочтений

        public void AddListenedSong(int genreId, int userId, int songId, int artistId)
        {
            string query = "INSERT INTO listenedsongs (genre_id, user_id, song_id, artist_id,  listenedDate) VALUES (@genreId, @userId, @songId, @artistId,  NOW())";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@genreId", genreId);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@songId", songId);
            command.Parameters.AddWithValue("@artistId", artistId);

            openConnection();
            command.ExecuteNonQuery();
            closeConnection();
        }


        public List<Song> GetRecommendedSongs(int userId, int limit)
        {
            List<Song> recommendedSongs = new List<Song>();

            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT s.* FROM listenedsongs ls " +
                "INNER JOIN song s ON ls.song_id = s.id_song " +
                "WHERE ls.user_id = @userId " +
                "ORDER BY ls.listenedDate DESC", connection);
            command.Parameters.AddWithValue("@userId", userId);

            MySqlDataReader reader = command.ExecuteReader();

            List<int> genreIds = new List<int>();
            List<int> artistIds = new List<int>();
            while (reader.Read())
            {
                int genreId = reader.GetInt32("genre_id");
                int artistId = reader.GetInt32("artist_id");

                if (!genreIds.Contains(genreId))
                {
                    genreIds.Add(genreId);
                }

                if (!artistIds.Contains(artistId))
                {
                    artistIds.Add(artistId);
                }
            }

            reader.Close();

            MySqlCommand songCommand = new MySqlCommand("SELECT * FROM song " +
                "JOIN artist ON song.artist_id = artist.id_artist " +
                "JOIN genre ON song.genre_id = genre.id_genre " +
                "WHERE (genre_id IN (@genreIds) OR artist_id IN (@artistIds)) " +
                "ORDER BY RAND() " +
                "LIMIT @limit", connection);
            songCommand.Parameters.AddWithValue("@genreIds", string.Join(",", genreIds));
            songCommand.Parameters.AddWithValue("@artistIds", string.Join(",", artistIds));
            songCommand.Parameters.AddWithValue("@limit", limit);

            MySqlDataReader songReader = songCommand.ExecuteReader();

            while (songReader.Read())
            {
                Song song = new Song();
                song.Id = songReader.GetInt32("id_song");
                song.Title = songReader.GetString("song_name");
                song.Artist = songReader.GetString("artist_name");
                song.Duration = songReader.GetString("duration");
                song.Listens = Convert.ToInt32(songReader["listens"]);
                song.Genre = songReader.GetString("genre_name");
                song.PathToFile = songReader["audio_file"].ToString();
                song.PathToImage = songReader["image_file"].ToString();
                recommendedSongs.Add(song);
            }

            songReader.Close();

            closeConnection();
            return recommendedSongs;
        }

        // удаление песен

        public void DeleteSong(int songId)
        {
            try
            {
                openConnection();

                string deleteLikedSongQuery = "DELETE FROM liked_song WHERE song_id = @songId";
                MySqlCommand cmd1 = new MySqlCommand(deleteLikedSongQuery, connection);
                cmd1.Parameters.AddWithValue("@songId", songId);
                cmd1.ExecuteNonQuery();

                string deletePlaylistSongQuery = "DELETE FROM playlist_song WHERE song_id = @songId";
                MySqlCommand cmd3 = new MySqlCommand(deletePlaylistSongQuery, connection);
                cmd3.Parameters.AddWithValue("@songId", songId);
                cmd3.ExecuteNonQuery();

                string deleteAlbumSongQuery = "DELETE FROM album_song WHERE song_id = @songId";
                MySqlCommand cmd4 = new MySqlCommand(deleteAlbumSongQuery, connection);
                cmd4.Parameters.AddWithValue("@songId", songId);
                cmd4.ExecuteNonQuery();

                string deleteSongQuery = "DELETE FROM song WHERE id_song = @songId";
                MySqlCommand cmd5 = new MySqlCommand(deleteSongQuery, connection);
                cmd5.Parameters.AddWithValue("@songId", songId);
                cmd5.ExecuteNonQuery();

                closeConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления песни: " + ex.Message);
            }
        }

        public bool DeleteAlbumIfEmptyfromartist(int albumId)
        {
            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM album_song WHERE album_id = @albumId", connection);
            command.Parameters.AddWithValue("@albumId", albumId);
            int songCount = Convert.ToInt32(command.ExecuteScalar());

            if (songCount == 0)
            {

                command = new MySqlCommand("DELETE FROM album WHERE id_album = @albumId", connection);
                command.Parameters.AddWithValue("@albumId", albumId);
                command.ExecuteNonQuery();

                closeConnection();
                return true;
            }
            else
            {
                closeConnection();
                return false;
            }
        }

        public bool DeleteAlbumIfEmptyfromuser(int albumId, int userId)
        {
            openConnection();

            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM album_song WHERE album_id = @albumId", connection);
            command.Parameters.AddWithValue("@albumId", albumId);
            int songCount = Convert.ToInt32(command.ExecuteScalar());

            if (songCount == 0)
            {
                command = new MySqlCommand("DELETE FROM liked_album WHERE album_id = @albumId AND user_id = @userId", connection);
                command.Parameters.AddWithValue("@albumId", albumId);
                command.Parameters.AddWithValue("@userId", userId);
                command.ExecuteNonQuery();

                command = new MySqlCommand("DELETE FROM album WHERE id_album = @albumId", connection);
                command.Parameters.AddWithValue("@albumId", albumId);
                command.ExecuteNonQuery();

                closeConnection();
                return true;

            }
            else
            {
                closeConnection();
                return false;
            }
        }


        // панель справа

        public Song GetCurrentSong()
        {
            Song currentSong = null;

            string query = "SELECT * FROM currentsong LIMIT 1;";
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    currentSong = new Song
                    {
                        Id = Convert.ToInt32(reader["id_curr_song"]),
                        Title = reader["song_name"].ToString(),
                        Artist = reader["artist_name"].ToString(),
                        PathToImage = reader["image_file"].ToString()
                    };
                }

                reader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                connection.Close();
            }

            return currentSong;
        }

        public void UpdateCurrentSong(Song currentSong)
        {
            string query = "UPDATE currentsong SET song_name = @Title, artist_name = @Artist, image_file = @PathToImage WHERE id_curr_song = 1;";
            MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", currentSong.Id);
            command.Parameters.AddWithValue("@Title", currentSong.Title);
            command.Parameters.AddWithValue("@Artist", currentSong.Artist);
            command.Parameters.AddWithValue("@PathToImage", currentSong.PathToImage);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Обработка ошибок
            }
            finally
            {
                connection.Close();
            }
        }

        public void InsertOrUpdateCurrentSong(Song currentSong)
        {
            string checkQuery = "SELECT COUNT(*) FROM currentsong;";
            MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);

            connection.Open();
            int count = Convert.ToInt32(checkCommand.ExecuteScalar());
            connection.Close();

            if (count == 0)
            {
                string insertQuery = "INSERT INTO currentsong (id_curr_song, song_name, artist_name, image_file) VALUES (1, @Title, @Artist, @PathToImage);";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);

                insertCommand.Parameters.AddWithValue("@Id", currentSong.Id);
                insertCommand.Parameters.AddWithValue("@Title", currentSong.Title);
                insertCommand.Parameters.AddWithValue("@Artist", currentSong.Artist);
                insertCommand.Parameters.AddWithValue("@PathToImage", currentSong.PathToImage);

                try
                {
                    connection.Open();
                    insertCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                UpdateCurrentSong(currentSong);
            }

        }

        public void DeleteAllCurrentSongs()
        {
            string deleteQuery = "DELETE FROM currentsong;";
            MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);

            try
            {
                connection.Open();
                deleteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Обработка ошибок
            }
            finally
            {
                connection.Close();
            }
        }





    }


}












