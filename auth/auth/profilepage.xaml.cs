using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для profilepage.xaml
    /// </summary>
    public partial class profilepage : Page, INotifyPropertyChanged
    {
        MusicPlayer musicPlayer = MusicPlayer.Instance;

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public profilepage(string user)
        {
            InitializeComponent();
            username = user;
            DataContext = this;
            Loaded += ProfilePage_Loaded;


        }


        private void btnBack_Click(object sender, RoutedEventArgs e)
        {

            homepage Homepage = new homepage();
            NavigationService.Navigate(Homepage);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                DataBase db = new DataBase();
                db.SaveImagePathToDatabase(filePath, db.GetUserIdByUsername(CurrentUser.Username));

                avatarImage.Source = new BitmapImage(new Uri(filePath));
            }
        }

        private void LoadProfilePage()
        {
            // Загрузка имени пользователя и других данных на страницу профиля
            DataBase db = new DataBase();
            int userId = db.GetUserIdByUsername(CurrentUser.Username);
            // Загрузка пути к изображению из базы данных
            string avatarPath = db.GetAvatarPathFromDatabase(userId);

            // Загрузка и отображение изображения на аватаре
            if (!string.IsNullOrEmpty(avatarPath))
            {
                avatarImage.Source = new BitmapImage(new Uri(avatarPath));
            }
            else
            {
                string avatarfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");
                string avatarpath = Path.Combine(avatarfolder, "нет.jpg");
                avatarImage.Source = new BitmapImage(new Uri(avatarpath));
            }

        }

        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProfilePage();
        }

        private void EditUsername_Click(object sender, RoutedEventArgs e)
        {
            DataBase db = new DataBase();
            EditUsernameDialog dialog = new EditUsernameDialog();
            if (dialog.ShowDialog() == true)
            {
                string newUsername = dialog.NewUsername;

                db.UpdateUsernameInDatabase(newUsername);

                usernameTextBlock.Text = newUsername;


                CurrentUser.Username = newUsername;
            }
        }

        private void exit_btn(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Window.GetWindow(this).Close();
            MusicPlayer.Instance.Stop();

            DataBase db = new DataBase();
            db.DeleteAllCurrentSongs();
        }
    }
}
