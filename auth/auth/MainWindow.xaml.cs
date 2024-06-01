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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;


namespace auth
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();

        }

       

        private void go_to_reg(object sender, RoutedEventArgs e)
        {
            registr reg = new registr();
            reg.Show();
            Close();

        }

        private void auth_btn(object sender, RoutedEventArgs e)
        {
            string username = login_line.Text.Trim();
            string password = passw_line.Password.Trim();

            DataBase dataBase = new DataBase();

            bool isAuthenticated = dataBase.AuthenticateUser(username, password);



            if (username == "" || password == "")
            {
                MessageBox.Show("данные не введены!");
            }

            else if (isAuthenticated)
            {
                music_stream music_Stream = new music_stream();
                music_Stream.Show();
                Close();
                CurrentUser.Username = login_line.Text.Trim();
                int userId = dataBase.GetUserIdByUsername(CurrentUser.Username);
                CurrentUser.UserId = userId;
                // Дополнительные действия при успешной авторизации
            }
            else
            {
                MessageBox.Show("Неверные имя пользователя или пароль!");
            }

        }


        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        private void Login()
        {
            string username = login_line.Text.Trim();
            string password = passw_line.Password;  

            DataBase dataBase = new DataBase();

            bool isAuthenticated = dataBase.AuthenticateUser(username, password);



            if (username == "" || password == "")
            {
                MessageBox.Show("данные не введены!");
            }

            else if (isAuthenticated)
            {
                music_stream music_Stream = new music_stream();
                music_Stream.Show();
                Close();
                CurrentUser.Username = login_line.Text.Trim();
                int userId = dataBase.GetUserIdByUsername(CurrentUser.Username);
                CurrentUser.UserId = userId;
            }
            else
            {
                MessageBox.Show("Неверные имя пользователя или пароль!");
            }
        }

        private void artist_auth(object sender, RoutedEventArgs e)
        {
            Artist_form af = new Artist_form();
            af.Show();
            Close();
        }
    }
}
