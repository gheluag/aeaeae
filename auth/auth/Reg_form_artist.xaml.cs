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
    /// Логика взаимодействия для Reg_form_artist.xaml
    /// </summary>
    public partial class Reg_form_artist : Window
    {
        public Reg_form_artist()
        {
            InitializeComponent();

            login_line.ToolTip = "Это поле - имя исполнителя, выступает также в качестве вашего логина";
        }




        private void regis_btn(object sender, RoutedEventArgs e)
        {
            string username = login_line.Text.Trim();
            string password = passw_line.Password.Trim();
            string artistName = login_line.Text.Trim();

            DataBase database = new DataBase();

            // Проверяем, существует ли исполнитель с таким же логином в базе данных
            if (database.CheckArtistUsernameExists(username))
            {
                MessageBox.Show("Исполнитель с таким именем уже существует. Пожалуйста, выберите другое.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Регистрируем нового исполнителя
            if (database.RegisterArtist(username, password, artistName))
            {
                MessageBox.Show("Исполнитель успешно зарегистрирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Artist_form af = new Artist_form();
                af.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Не удалось зарегистрировать исполнителя. Пожалуйста, попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void go_auth(object sender, RoutedEventArgs e)
        {
            Artist_form af = new Artist_form();
            af.Show();
            Close();
        }
    }
}
