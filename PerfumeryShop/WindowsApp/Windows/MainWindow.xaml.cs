using PerfumeryShop.Model;
using PerfumeryShop.WindowsApp.Windows;
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


namespace PerfumeryShop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class ProductView
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal? Price { get; set; }
            public int? CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string ImagePath { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
            CheckUserRole();
        }

        private void CheckUserRole()
        {
            if (LoginWindow.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
                return;
            }

            string role = LoginWindow.CurrentUser.Role.Trim();

            if (role == "Client")
            {
                btnAddProduct.Visibility = Visibility.Collapsed;
                btnDeleteProduct.Visibility = Visibility.Collapsed;
                btnRequests.Visibility = Visibility.Collapsed;
            }

            if (role == "Admin")
            {
                btnFavorite.Visibility = Visibility.Collapsed;
                btnOpenFavorites.Visibility = Visibility.Collapsed;
                btnRequest.Visibility = Visibility.Collapsed;
                btnMyOrders.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = App.context.Categories.ToList();

                List<object> categoryList = new List<object>();
                categoryList.Add(new { Id = 0, Name = "Все категории" });

                foreach (var category in categories)
                {
                    categoryList.Add(new
                    {
                        Id = category.Id,
                        Name = category.Name
                    });
                }

                cbCategories.ItemsSource = categoryList;
                cbCategories.DisplayMemberPath = "Name";
                cbCategories.SelectedValuePath = "Id";
                cbCategories.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void LoadProducts()
        {
            try
            {
                string searchText = tbSearch.Text.Trim().ToLower();

                int selectedCategoryId = 0;
                if (cbCategories.SelectedValue != null)
                    selectedCategoryId = Convert.ToInt32(cbCategories.SelectedValue);

                var products = App.context.Products.ToList();

                var productList = products.Select(p => new ProductView
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Categories != null ? p.Categories.Name : "",
                    ImagePath = p.ImagePath
                }).ToList();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    productList = productList
                        .Where(p => p.Name != null && p.Name.ToLower().Contains(searchText))
                        .ToList();
                }

                if (selectedCategoryId != 0)
                {
                    productList = productList
                        .Where(p => p.CategoryId == selectedCategoryId)
                        .ToList();
                }

                lbProducts.ItemsSource = productList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message);
            }
        }

        private ProductView GetSelectedProduct()
        {
            return lbProducts.SelectedItem as ProductView;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCategories != null)
                LoadProducts();
        }

        private void btnFavorite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedProduct = GetSelectedProduct();

                if (selectedProduct == null)
                {
                    MessageBox.Show("Выберите товар.");
                    return;
                }

                int userId = LoginWindow.CurrentUser.Id;

                var checkFavorite = App.context.Favorites.FirstOrDefault(f =>
                    f.UserId == userId && f.ProductId == selectedProduct.Id);

                if (checkFavorite != null)
                {
                    MessageBox.Show("Товар уже есть в избранном.");
                    return;
                }

                Favorites favorite = new Favorites()
                {
                    UserId = userId,
                    ProductId = selectedProduct.Id
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

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedProduct = GetSelectedProduct();

                if (selectedProduct == null)
                {
                    MessageBox.Show("Выберите товар.");
                    return;
                }

                var product = App.context.Products.FirstOrDefault(p => p.Id == selectedProduct.Id);

                if (product == null)
                {
                    MessageBox.Show("Товар не найден.");
                    return;
                }

                OrderWindow orderWindow = new OrderWindow(product);
                orderWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            addProductWindow.Show();
            this.Close();
        }

        private void btnMyOrders_Click(object sender, RoutedEventArgs e)
        {
            MyOrdersWindow myOrdersWindow = new MyOrdersWindow();
            myOrdersWindow.Show();
            this.Close();
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginWindow.CurrentUser.Role != "Admin")
                {
                    MessageBox.Show("Удалять товар может только администратор.");
                    return;
                }

                var selectedProduct = GetSelectedProduct();

                if (selectedProduct == null)
                {
                    MessageBox.Show("Выберите товар.");
                    return;
                }

                var product = App.context.Products.FirstOrDefault(p => p.Id == selectedProduct.Id);

                if (product == null)
                {
                    MessageBox.Show("Товар не найден.");
                    return;
                }

                var favorites = App.context.Favorites.Where(f => f.ProductId == product.Id).ToList();
                foreach (var item in favorites)
                    App.context.Favorites.Remove(item);

                var orders = App.context.Orders.Where(o => o.ProductId == product.Id).ToList();
                foreach (var item in orders)
                    App.context.Orders.Remove(item);

                App.context.Products.Remove(product);
                App.context.SaveChanges();

                MessageBox.Show("Товар удален.");
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void btnRequests_Click(object sender, RoutedEventArgs e)
        {
            RequestsWindow requestsWindow = new RequestsWindow();
            requestsWindow.Show();
            this.Close();
        }

        private void lbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void lbProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedProduct = GetSelectedProduct();

            if (selectedProduct == null)
                return;

            var product = App.context.Products.FirstOrDefault(p => p.Id == selectedProduct.Id);

            if (product == null)
            {
                MessageBox.Show("Товар не найден.");
                return;
            }

            ProductDetailsWindow window = new ProductDetailsWindow(product);
            window.ShowDialog();
        }

        private void btnOpenFavorites_Click(object sender, RoutedEventArgs e)
        {
            FavoritesWindow favoritesWindow = new FavoritesWindow();
            favoritesWindow.Show();
            this.Close();
        }



    }
}
