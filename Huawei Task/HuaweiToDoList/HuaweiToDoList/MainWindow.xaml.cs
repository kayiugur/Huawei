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
//SQL Kütüphaneleri
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Data.Common;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace HuaweiToDoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        public static string MdfPath { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            if (File.Exists(SavePath))
            {

                MdfPath = File.ReadAllText(SavePath);
            }
            else
            {
                MdfPath = PathFile.Text;
                File.WriteAllText(SavePath, MdfPath);
            }

            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + MdfPath + ";Integrated Security=True";
            if (UsernameWindow.Text == "")
            {
                MessageBox.Show("Please enter User name", "Error", MessageBoxButton.OK);
                UsernameWindow.Focus();
                return;
            }
            if (PasswordWindow.Password == "")
            {
                MessageBox.Show("Please enter Password", "Error", MessageBoxButton.OK);
            }
            try
            {
                SqlConnection LoginConnect = default(SqlConnection);
                LoginConnect = new SqlConnection(path);

                SqlCommand LoginCommand = default(SqlCommand);
                LoginCommand = new SqlCommand("SELECT username,password FROM logincredentials Where username=@username AND password=@password", LoginConnect);

                SqlParameter uName = new SqlParameter("@username", SqlDbType.VarChar);
                SqlParameter uPassword = new SqlParameter("@password", SqlDbType.VarChar);

                uName.Value = UsernameWindow.Text;
                uPassword.Value = PasswordWindow.Password;

                LoginCommand.Parameters.Add(uName);
                LoginCommand.Parameters.Add(uPassword);

                LoginCommand.Connection.Open();

                SqlDataReader LoginReader = LoginCommand.ExecuteReader(CommandBehavior.CloseConnection);

                if (LoginReader.Read() == true)
                {
                    MessageBox.Show("You have logged in successfully");
                    HomePage home = new HomePage();
                    home.UserInformation = UsernameWindow.Text;
                    home.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Login Failed... Try Again !");
                }

                if (LoginConnect.State == ConnectionState.Open)
                {
                    LoginConnect.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MainWindow Main = new MainWindow();
            Main.Close();
            this.Hide();
        }

        private void To_Do_List_Activated(object sender, EventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            if (File.Exists(SavePath))
            {
                PathFile.Visibility = System.Windows.Visibility.Hidden;
                label_Copy1.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Register RegisterForm = new Register();
            RegisterForm.Show();
            this.Hide();
        }

        private void To_Do_List_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}