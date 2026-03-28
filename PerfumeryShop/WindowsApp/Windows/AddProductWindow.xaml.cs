using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PerfumeryShop.Model;

namespace PerfumeryShop.WindowsApp.Windows
{
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
            CheckAdmin();
            LoadCategories();
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

        private void LoadCategories()
        {
            try
            {
                var categories = App.context.Categories.ToList();
                cbCategory.ItemsSource = categories;
                cbCategory.DisplayMemberPath = "Name";
                cbCategory.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string imagePath = tbImagePath.Text.Trim();

                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    MessageBox.Show("Введите ссылку на изображение.");
                    return;
                }

                imgPreview.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить изображение: " + ex.Message);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = tbName.Text.Trim();
                string description = tbDescription.Text.Trim();
                string priceText = tbPrice.Text.Trim();
                string imagePath = tbImagePath.Text.Trim();

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(description) ||
                    string.IsNullOrWhiteSpace(priceText) ||
                    cbCategory.SelectedValue == null ||
                    string.IsNullOrWhiteSpace(imagePath))
                {
                    MessageBox.Show("Заполните все поля.");
                    return;
                }

                decimal price;
                if (!decimal.TryParse(priceText, out price))
                {
                    MessageBox.Show("Введите корректную цену.");
                    return;
                }

                Products product = new Products()
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    CategoryId = Convert.ToInt32(cbCategory.SelectedValue),
                    ImagePath = imagePath
                };

                App.context.Products.Add(product);
                App.context.SaveChanges();

                MessageBox.Show("Товар успешно добавлен.");

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении товара: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}