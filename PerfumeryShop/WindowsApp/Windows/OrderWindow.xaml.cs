using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using PerfumeryShop.Model;

namespace PerfumeryShop.WindowsApp.Windows
{
    public partial class OrderWindow : Window
    {
        private Products _product;

        public OrderWindow(Products product)
        {
            InitializeComponent();
            _product = product;
            LoadInfo();
        }

        private void LoadInfo()
        {
            tbProductName.Text = "Товар: " + _product.Name;
            tbPrice.Text = "Цена: " + _product.Price.ToString() + " руб.";

            if (LoginWindow.CurrentUser != null && !string.IsNullOrWhiteSpace(LoginWindow.CurrentUser.Phone))
            {
                tbPhone.Text = LoginWindow.CurrentUser.Phone;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_product.ImagePath))
                {
                    imgProduct.Source = new BitmapImage(new Uri(_product.ImagePath, UriKind.Absolute));
                }
            }
            catch
            {
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginWindow.CurrentUser == null)
                {
                    MessageBox.Show("Сначала выполните вход.");
                    return;
                }

                string phone = tbPhone.Text.Trim();

                if (string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Введите телефон.");
                    return;
                }

                var currentUser = App.context.Users.FirstOrDefault(u => u.Id == LoginWindow.CurrentUser.Id);

                if (currentUser != null)
                {
                    currentUser.Phone = phone;
                }

                Orders newOrder = new Orders()
                {
                    UserId = LoginWindow.CurrentUser.Id,
                    ProductId = _product.Id,
                    OrderDate = DateTime.Now,
                    Status = "Новая"
                };

                App.context.Orders.Add(newOrder);
                App.context.SaveChanges();

                MessageBox.Show("Заявка успешно оформлена.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заявки: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}