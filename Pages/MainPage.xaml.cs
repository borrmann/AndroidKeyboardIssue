using AndroidKeyboardIssue.Models;
using AndroidKeyboardIssue.PageModels;

namespace AndroidKeyboardIssue.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}