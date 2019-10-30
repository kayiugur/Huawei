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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Window
    {
        public HomePage()
        {
            InitializeComponent();            
        }
        public string UserInformation { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {        
            UserInfo.Text = UserInformation.ToString();
            kayitGetir();
            dataGridShow.Columns[0].IsReadOnly = true;
            UserInfo.IsReadOnly = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage home = new HomePage();
            home.Close();
            this.Hide();
        }
        public int id { get; set; }
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            string RegisterMdfPath = File.ReadAllText(SavePath);
            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True";

            if (TodoText.Text == "")
            {
                MessageBox.Show("Please enter User name !!", "Error", MessageBoxButton.OK);
                TodoText.Focus();
                return;
            }
            if (DueDate.Text == "")
            {
                MessageBox.Show("Please enter Due Date !!", "Error", MessageBoxButton.OK);
                DueDate.Focus();
                return;
            }

            try
            {
                SqlConnection RegisterConnect = default(SqlConnection);
                RegisterConnect = new SqlConnection(path);

                SqlCommand RegisterCommand = default(SqlCommand);
                RegisterCommand = new SqlCommand("INSERT INTO ToDoData(id,userlogin,todo,duedate,completed) VALUES(@id,@userlogin,@todo,@duedate,@completed)", RegisterConnect);

                Guid guid = Guid.NewGuid();
                Random random = new Random();
                id = random.Next();
                
                RegisterCommand.Parameters.AddWithValue("@id", id);
                RegisterCommand.Parameters.AddWithValue("@userlogin", UserInfo.Text);
                RegisterCommand.Parameters.AddWithValue("@todo", TodoText.Text);
                RegisterCommand.Parameters.AddWithValue("@duedate", DueDate.Text);
                RegisterCommand.Parameters.AddWithValue("@completed", 0);
                RegisterCommand.Connection.Open();

                SqlDataReader LoginReader = RegisterCommand.ExecuteReader(CommandBehavior.CloseConnection);
                
                MessageBox.Show("New task added successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                kayitGetir();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void kayitGetir()
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            string RegisterMdfPath = File.ReadAllText(SavePath);
            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True";
            try
            {
                SqlConnection RegisterConnect = default(SqlConnection);
                RegisterConnect = new SqlConnection(path);

                SqlCommand RegisterCommand = default(SqlCommand);
                RegisterCommand = new SqlCommand("SELECT userlogin AS 'User',todo AS 'To Do',duedate AS 'Due Date',completed as 'Completed' FROM tododata Where userlogin='" + UserInfo.Text + "'", RegisterConnect);

                SqlDataAdapter data = new SqlDataAdapter(RegisterCommand);

                DataTable dt = new DataTable("ToDoList");
                data.Fill(dt);
                dataGridShow.ItemsSource = dt.DefaultView;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            dataGridShow.Columns[0].IsReadOnly = true;
            dataGridShow.Columns[0].IsReadOnly = true;
        }

        private void DueDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            if (DueDate.SelectedDate <= DateTime.Today)
            {
                MessageBox.Show("You must enter dates ahead of today", "Error", MessageBoxButton.OK);
                DueDate.Focus();
                return;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            string RegisterMdfPath = File.ReadAllText(SavePath);
            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True";

            try
            {
                SqlConnection RegisterConnect = default(SqlConnection);
                RegisterConnect = new SqlConnection(path);

                SqlCommand RegisterCommand = default(SqlCommand);
                RegisterCommand = new SqlCommand("DELETE FROM ToDoData WHERE todo=@todo", RegisterConnect);

                string deleteToDo;
                deleteToDo = ((TextBlock)dataGridShow.Columns[1].GetCellContent(dataGridShow.SelectedItem)).Text;

                RegisterCommand.Parameters.AddWithValue("@todo", deleteToDo);
                RegisterCommand.Connection.Open();

                SqlDataReader LoginReader = RegisterCommand.ExecuteReader(CommandBehavior.CloseConnection);

                MessageBox.Show("Successfully deleted task!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                kayitGetir();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string NullOrEmpty { get; set; }
        public string DueDateNullOrEmpty { get; set; }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            string RegisterMdfPath = File.ReadAllText(SavePath);
            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True";

            try
            {
                if (LoadedToDo != ((TextBlock)dataGridShow.Columns[1].GetCellContent(dataGridShow.SelectedItem)).Text)
                {
                    //To Do Güncelleme
                    string UpdateToDo;

                    SqlConnection ToDoConnect = default(SqlConnection);
                    ToDoConnect = new SqlConnection(path);

                    SqlCommand ToDoCommand = default(SqlCommand);
                    ToDoCommand = new SqlCommand("UPDATE ToDoData set ToDo=@ToDo WHERE todo='" + LoadedToDo + "'", ToDoConnect);

                    UpdateToDo = ((TextBlock)dataGridShow.Columns[1].GetCellContent(dataGridShow.SelectedItem)).Text;

                    ToDoCommand.Parameters.AddWithValue("@todo", UpdateToDo);

                    ToDoCommand.Connection.Open();

                    SqlDataReader LoginReader = ToDoCommand.ExecuteReader(CommandBehavior.CloseConnection);

                    kayitGetir();
                }

                if (LoadedDueDate != DueDateNullOrEmpty)
                {
                    //Due Date Güncelleme
                    string UpdateDueDate;

                    SqlConnection DueDateConnect = default(SqlConnection);
                    DueDateConnect = new SqlConnection(path);

                    SqlCommand DueDateCommand = default(SqlCommand);
                    DueDateCommand = new SqlCommand("UPDATE ToDoData set DueDate=@DueDate WHERE Todo='" + LoadedToDo + "' and DueDate='" + LoadedDueDate + "'", DueDateConnect);

                    UpdateDueDate = DueDateNullOrEmpty;

                    DueDateCommand.Parameters.AddWithValue("@Duedate", UpdateDueDate);

                    DueDateCommand.Connection.Open();

                    SqlDataReader DueDateReader = DueDateCommand.ExecuteReader(CommandBehavior.CloseConnection);

                    kayitGetir();
                }

                if (LoadedComp.ToString() != NullOrEmpty)
                { 
                    //Completed Güncelleme
                    String UpdateComp;

                    SqlConnection CompConnect = default(SqlConnection);
                    CompConnect = new SqlConnection(path);

                    if (LoadedComp.ToString() == "System.Windows.Controls.CheckBox Content: IsChecked:True")
                    {
                        QueryBitString = "1" ;
                    }

                    if (LoadedComp.ToString() == "System.Windows.Controls.CheckBox Content: IsChecked:False")
                    {
                        QueryBitString = "0";
                    }

                    SqlCommand CompCommand = default(SqlCommand);
                    CompCommand = new SqlCommand("UPDATE ToDoData set Completed=@Completed WHERE Todo='" + LoadedToDo + "' and Completed='" + QueryBitString + "'", CompConnect);

                    UpdateComp = NullOrEmpty;

                    if (UpdateComp == "System.Windows.Controls.CheckBox Content: IsChecked:True")
                    {
                        CompCommand.Parameters.AddWithValue("@Completed", 1);
                    }

                    if (UpdateComp == "System.Windows.Controls.CheckBox Content: IsChecked:False")
                    {
                        CompCommand.Parameters.AddWithValue("@Completed", 0);
                    }

                    CompCommand.Connection.Open();

                    SqlDataReader CompReader = CompCommand.ExecuteReader(CommandBehavior.CloseConnection);

                    kayitGetir();
                }

                MessageBox.Show("Task successfully updated!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string SavePath = @"C:\Temp\MdfPath.txt";
            string RegisterMdfPath = File.ReadAllText(SavePath);
            string path = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + RegisterMdfPath + ";Integrated Security=True";
            try
            {
                SqlConnection RegisterConnect = default(SqlConnection);
                RegisterConnect = new SqlConnection(path);

                SqlCommand RegisterCommand = default(SqlCommand);
                RegisterCommand = new SqlCommand("SELECT userlogin AS 'User',todo AS 'To Do',duedate AS 'Due Date',completed as 'Completed' FROM tododata Where todo LIKE '" + "%" + SearchBox.Text+ "%" + "'", RegisterConnect);

                SqlDataAdapter data = new SqlDataAdapter(RegisterCommand);

                DataTable dt = new DataTable("ToDoList");
                data.Fill(dt);
                dataGridShow.ItemsSource = dt.DefaultView;

                if (SearchBox.Text == "")
                {
                    kayitGetir();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string LoadedToDo { get; set; }
        public string LoadedDueDate { get; set; }
        public string LoadedComp { get; set; }
        public string QueryBitString { get; set; }
        private void DataGridShow_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            LoadedToDo = ((TextBlock)dataGridShow.Columns[1].GetCellContent(dataGridShow.SelectedItem)).Text;
            LoadedDueDate = ((TextBlock)dataGridShow.Columns[2].GetCellContent(dataGridShow.SelectedItem)).Text;
            LoadedComp = ((CheckBox)dataGridShow.Columns[3].GetCellContent(dataGridShow.SelectedItem)).ToString();

            DueDateNullOrEmpty = ((TextBlock)dataGridShow.Columns[2].GetCellContent(dataGridShow.SelectedItem)).Text;
            NullOrEmpty = ((CheckBox)dataGridShow.Columns[3].GetCellContent(dataGridShow.SelectedItem)).ToString();
        }
    }
}
