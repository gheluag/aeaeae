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
    /// Логика взаимодействия для registr.xaml
    /// </summary>
    public partial class registr : Window
    {
        public registr()
        {
            InitializeComponent();
        }

        private void go_auth(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void regis_btn(object sender, RoutedEventArgs e)
        {
            string username = login_line.Text.Trim();
            string password = passw_line.Password.Trim();

            DataBase db = new DataBase();

            if (username.Length < 5 || password.Length < 8)
            {
                if (username.Length < 5)
                {
                    login_line.ToolTip = "имя пользователя должно содержать минимум 5 символов";
                    login_line.Background = Brushes.DarkRed;
                }
                else
                {
                    login_line.ToolTip = "";
                    login_line.Background = Brushes.Transparent;
                }

                if (password.Length < 8)
                {
                    passw_line.ToolTip = "пароль должен содержать минимум 8 символов";
                    passw_line.Background = Brushes.DarkRed;
                }
                else
                {
                    passw_line.ToolTip = "";
                    passw_line.Background = Brushes.Transparent;
                }

                return;
            }

            bool isRegistered = db.RegisterUser(username, password);

            if (isRegistered)
            {
                MessageBox.Show("Регистрация успешна!");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            else if (db.CheckUserUsernameExists(username))
            {
                MessageBox.Show("Пользователь с таким именем уже существует. Пожалуйста, выберите другое.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Произошла ошибка при регистрации. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
    
}
