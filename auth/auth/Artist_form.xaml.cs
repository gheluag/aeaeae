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
    /// Логика взаимодействия для Artist_form.xaml
    /// </summary>
    public partial class Artist_form : Window
    {
        public Artist_form()
        {
            InitializeComponent();
        }

        private void go_to_reg(object sender, RoutedEventArgs e)
        {
            Reg_form_artist rga = new Reg_form_artist();
            rga.Show();
            Close();
        }

        private void auth_btn(object sender, RoutedEventArgs e)
        {
            string username = login_line.Text.Trim();
            string password = passw_line.Password.Trim();

            DataBase database = new DataBase();


            if (username == "" || password == "")
            {
                MessageBox.Show("данные не введены!");
            }

            else if (database.CheckArtistCredentials(username, password))
            {
                string artist_name = login_line.Text.Trim();
                Main_Artist artistMainWindow = new Main_Artist(artist_name);
                artistMainWindow.Show();
                Close();
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
            string password = passw_line.Password.Trim();

            DataBase database = new DataBase();


            if (username == "" || password == "")
            {
                MessageBox.Show("данные не введены!");
            }

            else if (database.CheckArtistCredentials(username, password))
            {
                string artist_name = login_line.Text.Trim();
                Main_Artist artistMainWindow = new Main_Artist(artist_name);
                artistMainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Неверные имя пользователя или пароль!");
            }
        }

        private void us_auth(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}
