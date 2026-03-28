using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PerfumeryShop.Model;

namespace PerfumeryShop.WindowsApp.Windows
{
    public partial class MyOrdersWindow : Window
    {
        public class MyOrderView
        {
            public int Id { get; set; }
            public string ProductName { get; set; }
            public string ImagePath { get; set; }
            public DateTime? OrderDate { get; set; }
            public string Status { get; set; }
            public string Comment { get; set; }

            public string OrderDateText
            {
                get
                {
                    return "Дата заявки: " + (OrderDate.HasValue ? OrderDate.Value.ToString("dd.MM.yyyy HH:mm") : "");
                }
            }

            public string StatusText
            {
                get
                {
                    return "Статус: " + Status;
                }
            }

            public string CommentText
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(Comment))
                        return "Комментарий: отсутствует";

                    return "Комментарий: " + Comment;
                }
            }
        }

        public MyOrdersWindow()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                if (LoginWindow.CurrentUser == null)
                {
                    MessageBox.Show("Сначала выполните вход.");
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                    return;
                }

                int userId = LoginWindow.CurrentUser.Id;

                var orders = App.context.Orders
                    .Where(o => o.UserId == userId)
                    .ToList()
                    .Select(o => new MyOrderView
                    {
                        Id = o.Id,
                        ProductName = o.Products != null ? o.Products.Name : "",
                        ImagePath = o.Products != null ? o.Products.ImagePath : "",
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        Comment = o.Comment
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                lbOrders.ItemsSource = orders;
                tbCount.Text = "Заявок: " + orders.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заявок: " + ex.Message);
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                int orderId = Convert.ToInt32(button.Tag);

                var order = App.context.Orders.FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    MessageBox.Show("Заявка не найдена.");
                    return;
                }

                var result = MessageBox.Show("Удалить эту заявку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                App.context.Orders.Remove(order);
                App.context.SaveChanges();

                MessageBox.Show("Заявка удалена.");
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления заявки: " + ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}