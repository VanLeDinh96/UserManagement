using UserManagement.Extensions;
using UserManagement.Services;

namespace UserManagement;

public partial class CreateUser : Form
{
    private readonly IUserAccountService _userAccountService;
    public CreateUser()
    {
        InitializeComponent();
        _userAccountService = CompositionRoot.BuildUserAccountService();

    }

    private async void BtnCreate_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text.Trim();

        if (username.Equals(""))
        {
            MessageBox.Show("Chưa nhập username!");
            txtUsername.Focus();
            return;
        }

        if (!_userAccountService.IsValidOracleIdentifier(username))
        {
            MessageBox.Show("Username không hợp lệ. Chỉ được dùng chữ cái, số, '_' và bắt đầu bằng chữ!");
            txtUsername.Focus();
            return;
        }

        if (password.Equals(""))
        {
            MessageBox.Show("Chưa nhập password!");
            txtPassword.Focus();
            return;
        }

        if (!await _userAccountService.UserExistsAsync(username))
        {
            await _userAccountService.CreateOrAlterUserAsync(username, password);
            MessageBox.Show($"Tạo tài khoản: {username} thành công");
        }
        else
        {
            var res = MessageBox.Show(
                $"Bạn có muốn thay đổi mật khẩu User: {username}?",
                "Thông báo",
                MessageBoxButtons.YesNo);

            if (res == DialogResult.Yes)
            {
                await _userAccountService.CreateOrAlterUserAsync(username, password);
                MessageBox.Show($"Đổi mật khẩu tài khoản: {username} thành công");
            }
        }
    }
}
