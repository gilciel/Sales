namespace Sales.ViewModels
{
    using Common.Models;
    using GalaSoft.MvvmLight.Command;
    using System.Windows.Input;
    using Services;
    using Xamarin.Forms;
    using Sales.Helpers;
    using System.Linq;
    using Sales.Views;

    public class ProductItemViewModel : Product
    {
        #region Atrributes
        private ApiService apiService;
        #endregion

        #region Constructors
        public ProductItemViewModel()
        {
            this.apiService = new ApiService();
        }
        #endregion

        #region Commands
        public ICommand DeleteProductCommand
        {
            get
            {
                return new RelayCommand(DeleteProduct);
            }
        }

        public ICommand EditProductCommand
        {
            get
            {
                return new RelayCommand(EditProduct);
            }
        }

        private async void EditProduct()
        {
            MainViewModel.GetInstance().EditProduct = new EditProductViewModel(this);
            await Application.Current.MainPage.Navigation.PushAsync(new EditProductPage());
        }

        private async void DeleteProduct()
        {
            var answer = await Application.Current.MainPage.DisplayAlert(
                Languages.Confirm,
                Languages.DeleteConfirmation,
                Languages.Yes,
                Languages.No);
            if(!answer)
            {
                return;
            }
            var connection = await this.apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                await Application.Current.MainPage.DisplayAlert(Languages.Error, connection.Message, Languages.Accept);
                return;
            }
            var url = Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductsController"].ToString();

            var response = await this.apiService.Delete(url, prefix, controller,this.ProductId, Settings.TokenType, Settings.AccessToken);
            if (!response.IsSuccess)
            {
                await Application.Current.MainPage.DisplayAlert(Languages.Error, response.Message, Languages.Accept);
                return;
            }

            var productsViewModel = ProductsViewModel.GetInstance();
            var deletedProduct = productsViewModel.Products.Where(p => p.ProductId == this.ProductId).FirstOrDefault();
            if(deletedProduct != null)
            {
                productsViewModel.Products.Remove(deletedProduct);
            }
        }
        //EditProductCommand
        #endregion
    }
}
