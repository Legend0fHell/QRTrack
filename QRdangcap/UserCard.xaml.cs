using QRdangcap.GoogleDatabase;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRdangcap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserCard : ContentPage
    {
        public UserCard()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MainLine.Text = UserData.StudentFullName;
            SubLine.Text = $"Lớp {UserData.StudentClass} - Trường {UserData.SchoolName}";
            RetrieveAllUserDb instance = new RetrieveAllUserDb();
            QRCodeImg.BarcodeValue = instance.Base64Encode(UserData.StudentIdDatabase.ToString());
            QRCodeImg.IsVisible = true;
        }
    }
}