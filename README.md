# UserManagement – WinForms quản lý User Oracle

## 1. Mục tiêu & bối cảnh

Project này là một ứng dụng **WinForms .NET** dùng để:

- Tạo mới user trong **Oracle Database**
- Đổi mật khẩu user đã tồn tại  
- Giao tiếp với **PL/SQL package** phía Oracle (ví dụ: `user_admin_pkg.pro_create_or_alter_user`, `fn_user_exists`)

Mục tiêu của code phía .NET:

- Tách biệt rõ các tầng **UI / Service / Repository / Oracle Infra**
- Dễ đọc, dễ mở rộng, phù hợp tư duy **Clean Code**, **SOLID** :contentReference[oaicite:0]{index=0}  

---

## 2. Tech stack chính

- **.NET (WinForms)**: UI desktop – form `CreateUser`
- **C# async/await**
- **Oracle Managed Data Access**:
  - `Oracle.ManagedDataAccess.Client`
  - `Oracle.ManagedDataAccess.Types`
- Tự triển khai các abstraction:
  - `IOracleConnectionFactory`
  - `IOracleStoredProcExecutor`
  - `IUserAdminRepository`
  - `IUserAccountService`
  - `OracleParameterFactory`
  - `OracleValueConverter` :contentReference[oaicite:1]{index=1}  

---

## 3. Kiến trúc tổng quan

Kiến trúc theo dạng **layered**:

1. **UI Layer (WinForms)**
2. **Service Layer**
3. **Repository Layer**
4. **Oracle Infra (Executor + Connection + Parameter + Converter)**

Luồng end-to-end:

1. App khởi động → tạo form chính `CreateUser`
2. Người dùng nhập `Username`, `Password` → bấm nút
3. Form gọi `IUserAccountService`
4. Service gọi `IUserAdminRepository`
5. Repository dựng `OracleParameter` → gọi `IOracleStoredProcExecutor`
6. Executor tạo `OracleConnection` + `OracleCommand` → gọi Stored Procedure / Function
7. Kết quả trả về (nếu có) được chuẩn hóa bởi `OracleValueConverter` rồi trả lên trên :contentReference[oaicite:2]{index=2}  

---

## 4. Chi tiết từng tầng

### 4.1. UI Layer – `CreateUser` Form

- File: `CreateUser.cs`
- Chứa:
  - TextBox: `txtUsername`, `txtPassword`
  - Button: `btnCreate` (hoặc tương đương)
- Sự kiện chính: `BtnCreate_Click`
  - Validate rỗng:
    - Nếu `Username` trống → show MessageBox, focus lại
    - Nếu `Password` trống → show MessageBox, focus lại
  - Gọi:
    - `_userAccountService.UserExistsAsync(username)` để kiểm tra user
    - Tùy kết quả:
      - User chưa tồn tại → gọi `_userAccountService.CreateOrAlterUserAsync(...)` để **tạo**
      - User đã tồn tại → hỏi người dùng có đổi mật khẩu không → nếu đồng ý thì gọi service để **đổi mật khẩu** :contentReference[oaicite:3]{index=3}  

**Vai trò:** UI chỉ xử lý input/output với người dùng, không chứa logic truy cập DB.

---

### 4.2. Service Layer – `IUserAccountService`

- Interface (dạng ví dụ):

  ```csharp
  public interface IUserAccountService
  {
      Task<bool> UserExistsAsync(string username);
      Task CreateOrAlterUserAsync(string username, string password);
      // Optionally: Task CreateUserAsync(...), Task ChangePasswordAsync(...)
  }
