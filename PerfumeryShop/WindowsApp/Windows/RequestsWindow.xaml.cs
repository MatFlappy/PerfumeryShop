using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PerfumeryShop.Model;

namespace PerfumeryShop.WindowsApp.Windows
{
    public partial class RequestsWindow : Window
    {
        public class RequestView
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Phone { get; set; }
            public string ProductName { get; set; }
            public DateTime? OrderDate { get; set; }
            public string Status { get; set; }
            public string Comment { get; set; }
        }

        public RequestsWindow()
        {
            InitializeComponent();
            cbStatus.Items.Add("Новая");
            cbStatus.Items.Add("В обработке");
            cbStatus.Items.Add("Завершена");
            cbStatus.Items.Add("Отменена");

            CheckAdmin();
            LoadRequests();
        }

        private void CheckAdmin()
        {
            if (LoginWindow.CurrentUser == null || LoginWindow.CurrentUser.Role != "Admin")
            {
                MessageBox.Show("Доступ разрешен только администратору.");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void LoadRequests()
        {
            try
            {
                var requests = App.context.Orders.ToList()
                    .Select(o => new RequestView
                    {
                        Id = o.Id,
                        UserName = o.Users != null ? o.Users.FullName : "",
                        Phone = o.Users != null ? o.Users.Phone : "",
                        ProductName = o.Products != null ? o.Products.Name : "",
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        Comment = o.Comment
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                dgRequests.ItemsSource = requests;
                tbCount.Text = "Заявок: " + requests.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заявок: " + ex.Message);
            }
        }

        private RequestView GetSelectedRequest()
        {
            return dgRequests.SelectedItem as RequestView;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRequest = GetSelectedRequest();

                if (selectedRequest == null)
                {
                    MessageBox.Show("Выберите заявку.");
                    return;
                }

                var result = MessageBox.Show("Удалить выбранную заявку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var order = App.context.Orders.FirstOrDefault(o => o.Id == selectedRequest.Id);

                if (order == null)
                {
                    MessageBox.Show("Заявка не найдена.");
                    return;
                }

                App.context.Orders.Remove(order);
                App.context.SaveChanges();

                MessageBox.Show("Заявка удалена.");
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RequestView selectedRequest = dgRequests.SelectedItem as RequestView;

                if (selectedRequest == null)
                {
                    MessageBox.Show("Выберите заявку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Выберите новый статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string newStatus = cbStatus.SelectedItem.ToString();

                Orders order = App.context.Orders.FirstOrDefault(o => o.Id == selectedRequest.Id);

                if (order == null)
                {
                    MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                order.Status = newStatus;
                App.context.SaveChanges();

                MessageBox.Show("Статус обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения статуса: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}