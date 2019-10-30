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
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        public static string RegisterMdfPath { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            RegisterMdfPath = File.ReadAllText(SavePath);
            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True";

            if (!varmi())
            {              
                if (RegisterUserNameWindow.Text == "")
                {
                    MessageBox.Show("Please enter User name", "Error", MessageBoxButton.OK);
                    RegisterUserNameWindow.Focus();
                    return;
                }
                if (RegisterPasswordName.Password == "")
                {
                    MessageBox.Show("Please enter Password", "Error", MessageBoxButton.OK);
                    RegisterPasswordName.Focus();
                    return;
                }
                try
                {
                    SqlConnection RegisterConnect = default(SqlConnection);
                    RegisterConnect = new SqlConnection(path);

                    SqlCommand RegisterCommand = default(SqlCommand);
                    RegisterCommand = new SqlCommand("INSERT INTO logincredentials(id,username,password) VALUES(@id,@username,@password)", RegisterConnect);

                    Guid guid = Guid.NewGuid();
                    Random random = new Random();
                    int id = random.Next();

                    RegisterCommand.Parameters.AddWithValue("@id", id);
                    RegisterCommand.Parameters.AddWithValue("@username", RegisterUserNameWindow.Text);
                    RegisterCommand.Parameters.AddWithValue("@password", RegisterPasswordName.Password);
                    RegisterCommand.Connection.Open();

                    SqlDataReader LoginReader = RegisterCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                MessageBox.Show("Register successfully");
                MainWindow main = new MainWindow();
                main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Login Failed... Try Again !");
            }           
        }

        private bool varmi()
        {
            object kayit = null;
            using (SqlConnection baglan = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True"))
            {
                string sor = "SELECT [username] FROM [logincredentials] WHERE [username]=@username";
                using (SqlCommand komut = new SqlCommand(sor, baglan))
                {
                    komut.Parameters.AddWithValue("@username", RegisterUserNameWindow.Text);
                    baglan.Open();
                    kayit = komut.ExecuteScalar();
                }
            }
            return kayit != null;
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Register Register = new Register();
            Register.Close();
            this.Hide();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Hide();
        }
    }
}
