# UserManagement – WinForms quản lý User Oracle

## 1. Mục tiêu & bối cảnh

Project này là một ứng dụng **WinForms .NET** dùng để:

- Tạo mới user trong **Oracle Database**
- Đổi mật khẩu user đã tồn tại  
- Giao tiếp với **PL/SQL package** phía Oracle (ví dụ: `user_admin_pkg.pro_create_or_alter_user`, `fn_user_exists`)

Mục tiêu của code phía .NET:

- Tách biệt rõ các tầng **UI / Service / Repository / Oracle Infra**
- Dễ đọc, dễ mở rộng, phù hợp tư duy **Clean Code**, **SOLID**

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
  - `OracleValueConverter`

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
7. Kết quả trả về (nếu có) được chuẩn hóa bởi `OracleValueConverter` rồi trả lên trên

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
      - User đã tồn tại → hỏi người dùng có đổi mật khẩu không → nếu đồng ý thì gọi service để **đổi mật khẩu**

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
  ```

* Implementation:

  * Nhận vào `IUserAdminRepository` qua constructor
  * Đóng vai trò **orchestrator**:

    * Gọi repository để check user
    * Quyết định gọi SP tạo user, đổi mật khẩu, hoặc combine logic khác

**Vai trò:** Chứa **business logic** mức ứng dụng, không phụ thuộc trực tiếp Oracle cụ thể.

---

### 4.3. Repository Layer – `OracleUserAdminRepository`

* Interface: `IUserAdminRepository`

  * `Task<bool> UserExistsAsync(string username)`
  * `Task CreateUserAsync(string username, string password)`
  * `Task ChangePasswordAsync(string username, string newPassword)`
  * `Task CreateOrAlterUserAsync(string username, string password)`

* Implementation: `OracleUserAdminRepository`

  * Dùng:

    * `IOracleStoredProcExecutor` để thực thi SP/Function
    * `OracleParameterFactory` để tạo `OracleParameter`
  * Ví dụ:

    * Gọi `fn_user_exists`:

      ```csharp
      var returnParam = OracleParameterFactory.Return(OracleDbType.Int32);
      var userParam   = OracleParameterFactory.In("p_username", OracleDbType.Varchar2, username);

      int result = await _executor.ExecuteFunctionAsync<int>(
          "user_admin_pkg.fn_user_exists",
          returnParam,
          userParam);
      return result == 1;
      ```

    * Gọi `pro_create_or_alter_user`:

      ```csharp
      var pUser = OracleParameterFactory.In("p_username", OracleDbType.Varchar2, username);
      var pPass = OracleParameterFactory.In("p_password", OracleDbType.Varchar2, password);

      await _executor.ExecuteNonQueryAsync(
          "user_admin_pkg.pro_create_or_alter_user",
          pUser,
          pPass);
      ```

**Vai trò:** Mapping giữa **C#** và **PL/SQL package**.

---

### 4.4. Oracle Infra

#### 4.4.1. `IOracleConnectionFactory`

* Trách nhiệm:

  * Đọc connection string (từ `App.config` / code)
  * Tạo `OracleConnection` mỗi lần cần

* Lợi ích:

  * Tách config & khởi tạo connection khỏi các lớp khác
  * Dễ thay đổi connection string/schema nếu cần 

---

#### 4.4.2. `OracleStoredProcExecutor` – `IOracleStoredProcExecutor`

* Hai hàm chính:

  * `ExecuteNonQueryAsync`:

    * Dùng cho **procedure** không trả giá trị
    * Các bước:

      1. Lấy `OracleConnection` từ `IOracleConnectionFactory`
      2. Tạo `OracleCommand`
      3. `CommandType = StoredProcedure`
      4. `BindByName = true`
      5. Add parameters
      6. `OpenAsync()` → `ExecuteNonQueryAsync()`
  * `ExecuteFunctionAsync<T>`:

    * Dùng cho **function** có **ReturnValue**
    * Thêm `returnParameter` vào trước (theo convention ODP.NET)
    * Sau khi thực thi:

      * Lấy `returnParameter.Value`
      * Gọi `OracleValueConverter.ConvertTo<T>(...)` để convert đúng kiểu (int, bool, string, …) 

**Vai trò:** Chuẩn hóa **cách thực thi SP/function** → repository chỉ cần tập trung vào tên package + logic nghiệp vụ.

---

#### 4.4.3. `OracleParameterFactory`

Helper để tạo `OracleParameter`:

* `In(name, dbType, value)`:

  * Input parameter
  * `Direction = Input`
  * `Value = value ?? DBNull.Value`
* `Out(name, dbType, size)`:

  * Output parameter
* `Return(dbType, name = "p_return")`:

  * Return value parameter cho function

**Lưu ý:**

* Sử dụng `BindByName = true` nên `ParameterName` phải trùng tên tham số PL/SQL (ví dụ `"p_username"`). 

---

#### 4.4.4. `OracleValueConverter`

* Mục tiêu: chuyển đổi kiểu provider-specific *Oracle* sang CLR type.
* Xử lý các case như:

  * `OracleDecimal` → `int`, `long`, `decimal`
  * `OracleString` → `string`
  * `OracleDate` → `DateTime`
* Tránh lỗi:

  * `Unable to cast OracleDecimal to IConvertible` khi dùng trực tiếp `Convert.ToInt32(object)` 

**Vai trò:** Gom toàn bộ logic convert vào một chỗ → code repository & service gọn, dễ bảo trì.
