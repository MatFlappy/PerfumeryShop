using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using PerfumeryShop.Model;

namespace PerfumeryShop.WindowsApp.Windows
{
    public partial class ProductDetailsWindow : Window
    {
        private Products _product;

        public ProductDetailsWindow(Products product)
        {
            InitializeComponent();
            _product = product;
            LoadProductInfo();

            if (LoginWindow.CurrentUser != null && LoginWindow.CurrentUser.Role == "Admin")
            {
                btnOrder.Visibility = Visibility.Collapsed;
                btnFavorite.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadProductInfo()
        {
            tbName.Text = _product.Name;
            tbDescription.Text = _product.Description;
            tbPrice.Text = "Цена: " + _product.Price.ToString() + " руб.";
            tbCategory.Text = "Категория: " + (_product.Categories != null ? _product.Categories.Name : "");

            if (!string.IsNullOrWhiteSpace(_product.ImagePath))
            {
                imgProduct.Source = new BitmapImage(new Uri(_product.ImagePath, UriKind.Absolute));
            }
        }

        private void btnFavorite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int userId = LoginWindow.CurrentUser.Id;

                var checkFavorite = App.context.Favorites.FirstOrDefault(f =>
                    f.UserId == userId && f.ProductId == _product.Id);

                if (checkFavorite != null)
                {
                    MessageBox.Show("Товар уже есть в избранном.");
                    return;
                }

                Favorites favorite = new Favorites()
                {
                    UserId = userId,
                    ProductId = _product.Id
                };

                App.context.Favorites.Add(favorite);
                App.context.SaveChanges();

                MessageBox.Show("Товар добавлен в избранное.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow(_product);
            orderWindow.ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}