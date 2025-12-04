using UserManagement.Extensions;
using UserManagement.Services;

namespace UserManagement;

public partial class Login : Form
{
    private readonly ILoginService _loginService;
    public Login()
    {
        InitializeComponent();
        _loginService = CompositionRoot.BuildLoginService();
    }

    private async void BtnLogin_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            MessageBox.Show("Vui lòng nhập Username.");
            txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Vui lòng nhập Password.");
            txtPassword.Focus();
            return;
        }

        var result = await _loginService.LoginAsync(username, password);

        if (!result.Success)
        {
            MessageBox.Show(result.ErrorMessage, "Đăng nhập thất bại");
            return;
        }

        MessageBox.Show("Đăng nhập thành công!", "Thông báo");

        new CreateUser().Show();
        Hide();
    }
}
