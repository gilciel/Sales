namespace Sales.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using Sales.Helpers;
    using System;
    using System.Windows.Input;
    using Xamarin.Forms;

    public class LoginViewModel : BaseViewModel
    {
        #region Attributes

        private bool isRunning;
        private bool isEnable;

        #endregion

        #region Properties

        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsRemember { get; set; }
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

        #endregion

        #region Constructors

        public LoginViewModel()
        {
            this.isEnable = true;
            this.IsRemember = true;
        }

        #endregion

        #region Commands

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand(Login);
            }
        }

        private async void Login()
        {
            if(string.IsNullOrEmpty(this.Email))
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.EmailValidation,
                    Languages.Accept);
                return;
            }
            if (string.IsNullOrEmpty(this.Password))
            {
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    Languages.PasswordValidation,
                    Languages.Accept);
                return;
            }
        }

        #endregion
    }
}
