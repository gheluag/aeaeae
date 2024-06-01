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
    /// Логика взаимодействия для music_stream.xaml
    /// </summary>
    public partial class music_stream : Window
    {
        private Page currentPage;
        private void SwitchToPage(Page page)
        {
            if (currentPage != null)
                currentPage.Visibility = Visibility.Collapsed;

            currentPage = page;
            currentPage.Visibility = Visibility.Visible;
        }

        public music_stream()
        {
            InitializeComponent();
            DataBase database = new DataBase();
            MainFrame.Navigate(new homepage());
            /*WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;*/
            Application.Current.Exit += Application_Exit;

        }

        DataBase database = new DataBase();
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Удаляем все записи из таблицы CurrentSong при выходе из приложения
            database.DeleteAllCurrentSongs();
        }



    }
}
