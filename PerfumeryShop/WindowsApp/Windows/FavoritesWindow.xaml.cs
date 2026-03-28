using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PerfumeryShop.Model;

namespace PerfumeryShop.WindowsApp.Windows
{
    public partial class FavoritesWindow : Window
    {
        public class FavoriteView
        {
            public int FavoriteId { get; set; }
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal? Price { get; set; }
            public string CategoryName { get; set; }
            public string ImagePath { get; set; }
        }

        public FavoritesWindow()
        {
            InitializeComponent();
            LoadFavorites();
        }

        private void LoadFavorites()
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

                var favorites = App.context.Favorites
                    .Where(f => f.UserId == userId)
                    .ToList()
                    .Select(f => new FavoriteView
                    {
                        FavoriteId = f.Id,
                        ProductId = f.ProductId ?? 0,
                        Name = f.Products != null ? f.Products.Name : "",
                        Description = f.Products != null ? f.Products.Description : "",
                        Price = f.Products != null ? f.Products.Price : 0,
                        CategoryName = f.Products != null && f.Products.Categories != null ? f.Products.Categories.Name : "",
                        ImagePath = f.Products != null ? f.Products.ImagePath : ""
                    })
                    .ToList();

                lbFavorites.ItemsSource = favorites;
                tbCount.Text = "Товаров: " + favorites.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки избранного: " + ex.Message);
            }
        }

        private void RemoveFavorite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                int favoriteId = Convert.ToInt32(button.Tag);

                var favorite = App.context.Favorites.FirstOrDefault(f => f.Id == favoriteId);

                if (favorite == null)
                {
                    MessageBox.Show("Запись не найдена.");
                    return;
                }

                App.context.Favorites.Remove(favorite);
                App.context.SaveChanges();

                MessageBox.Show("Товар удален из избранного.");
                LoadFavorites();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }

        private void lbFavorites_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FavoriteView selectedItem = lbFavorites.SelectedItem as FavoriteView;

                if (selectedItem == null)
                    return;

                var product = App.context.Products.FirstOrDefault(p => p.Id == selectedItem.ProductId);

                if (product == null)
                {
                    MessageBox.Show("Товар не найден.");
                    return;
                }

                ProductDetailsWindow productDetailsWindow = new ProductDetailsWindow(product);
                productDetailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия товара: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFavorites();
        }
    }
}