﻿

namespace Sales.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using Plugin.Media;
    using Plugin.Media.Abstractions;
    using Sales.Common.Models;
    using Sales.Helpers;
    using Sales.Services;
    using System;
    using System.Linq;
    using System.Windows.Input;
    using Xamarin.Forms;

    public class EditProductViewModel : BaseViewModel
    {
        #region Atrributes
        private Product product;
        private MediaFile file;
        private ImageSource imageSource;
        private ApiService apiService;
        private bool isRunning;
        private bool isEnable;

        #endregion

        #region Properties
        public Product Product
        {
            get { return this.product; }
            set { this.SetValue(ref this.product, value); }
        }
        public bool IsRunning
        {
            get { return this.isRunning; }
            set { this.SetValue(ref this.isRunning, value); }
        }

        public bool IsEnable
        {
            get { return this.isEnable; }
            set { this.SetValue(ref this.isEnable, value); }
        }

        public ImageSource ImageSource
        {
            get { return this.imageSource; }
            set { this.SetValue(ref this.imageSource, value); }
        }
        #endregion

        #region Constructors
        public EditProductViewModel(ProductItemViewModel product)
        {
            this.product = product;
            this.apiService = new ApiService();
            this.IsEnable = true;
            this.ImageSource = product.ImageFullPath;
        }

        #endregion

        #region Commands
        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(Delete);
            }
        }

        private async void Delete()
        {
            var answer = await Application.Current.MainPage.DisplayAlert(
               Languages.Confirm,
               Languages.DeleteConfirmation,
               Languages.Yes,
               Languages.No);
            if (!answer)
            {
                return;
            }

            this.IsRunning = true;
            this.IsEnable = false;

            var connection = await this.apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnable = true;
                await Application.Current.MainPage.DisplayAlert(Languages.Error, connection.Message, Languages.Accept);
                return;
            }
            var url = Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductsController"].ToString();

            var response = await this.apiService.Delete(url, prefix, controller, this.Product.ProductId, Settings.TokenType, Settings.AccessToken);
            if (!response.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnable = true;
                await Application.Current.MainPage.DisplayAlert(Languages.Error, response.Message, Languages.Accept);
                return;
            }

            var productsViewModel = ProductsViewModel.GetInstance();
            var deletedProduct = productsViewModel.MyProducts.Where(p => p.ProductId == this.Product.ProductId).FirstOrDefault();
            if (deletedProduct != null)
            {
                productsViewModel.MyProducts.Remove(deletedProduct);
            }
            productsViewModel.RefreshList();
            this.IsRunning = true;
            this.IsEnable = false;
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(Save);
            }
        }
        public ICommand ChangeImageCommand
        {
            get
            {
                return new RelayCommand(ChangeImage);
            }
        }

        private async void Save()
        {
            if (string.IsNullOrEmpty(this.Product.Description))
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.DescriptionError,
                    Languages.Accept);
                return;
            }

            if (this.Product.Price < 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PriceError,
                    Languages.Accept);
                return;
            }
            this.IsRunning = true;
            this.IsEnable = false;

            var connection = await this.apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnable = true;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    connection.Message,
                    Languages.Accept);
                return;
            }

            byte[] imageArray = null;
            if (this.file != null)
            {
                imageArray = FilesHelper.ReadFully(this.file.GetStream());
                this.Product.ImageArray = imageArray;
            }

            var url = Application.Current.Resources["UrlAPI"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductsController"].ToString();
            var response = await this.apiService.Put(url, prefix, controller, this.Product,this.Product.ProductId, Settings.TokenType, Settings.AccessToken);

            if (!response.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnable = true;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    response.Message,
                    Languages.Accept);
                return;
            }

            var newProduct = (Product)response.Result;
            var productsViewModel = ProductsViewModel.GetInstance();
            var oldProduct = productsViewModel.MyProducts.Where(p => p.ProductId == this.Product.ProductId).FirstOrDefault();
            if(oldProduct!=null)
            {
                productsViewModel.MyProducts.Remove(oldProduct);
            }

            productsViewModel.MyProducts.Add(newProduct);
            productsViewModel.RefreshList();

            this.IsRunning = false;
            this.IsEnable = true;
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        private async void ChangeImage()
        {
            await CrossMedia.Current.Initialize();

            var source = await Application.Current.MainPage.DisplayActionSheet(
                Languages.ImageSource,
                Languages.Cancel,
                null,
                Languages.FromGallery,
                Languages.NewPicture);

            if (source == Languages.Cancel)
            {
                this.file = null;
                return;
            }

            if (source == Languages.NewPicture)
            {
                this.file = await CrossMedia.Current.TakePhotoAsync(
                    new StoreCameraMediaOptions
                    {
                        Directory = "Sample",
                        Name = "test.jpg",
                        PhotoSize = PhotoSize.Small,
                    }
                );
            }
            else
            {
                this.file = await CrossMedia.Current.PickPhotoAsync();
            }

            if (this.file != null)
            {
                this.ImageSource = ImageSource.FromStream(() =>
                {
                    var stream = this.file.GetStream();
                    return stream;
                });
            }
        }

        #endregion
    }
}
