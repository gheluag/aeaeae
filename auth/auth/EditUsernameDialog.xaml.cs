﻿using System;
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
    /// Логика взаимодействия для EditUsernameDialog.xaml
    /// </summary>
    public partial class EditUsernameDialog : Window
    {
        public string NewUsername { get; private set; }

        public EditUsernameDialog()
        {
            InitializeComponent();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NewUsername = usernameTextBox.Text;
            DialogResult = true;
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
