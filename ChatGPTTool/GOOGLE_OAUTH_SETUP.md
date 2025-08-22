# 🔐 Hướng dẫn cấu hình Google OAuth cho ChatGPT Tool

## 📋 **Bước 1: Tạo Google Cloud Project**

1. **Truy cập Google Cloud Console:**
   ```
   https://console.cloud.google.com/
   ```

2. **Tạo project mới hoặc chọn project có sẵn**

3. **Bật Google+ API và People API:**
   - Vào "APIs & Services" > "Library"
   - Tìm và bật "Google+ API"
   - Tìm và bật "People API"

## 🔑 **Bước 2: Tạo OAuth 2.0 Credentials**

1. **Vào "APIs & Services" > "Credentials"**

2. **Click "Create Credentials" > "OAuth 2.0 Client IDs"**

3. **Chọn "Web application"**

4. **Điền thông tin:**
   - **Name:** ChatGPT Tool OAuth
   - **Authorized JavaScript origins:**
     ```
     https://localhost:7000
     http://localhost:5000
     ```
   - **Authorized redirect URIs:**
     ```
     https://localhost:7000/auth/google/callback
     http://localhost:5000/auth/google/callback
     ```

5. **Click "Create"**

6. **Lưu lại Client ID và Client Secret**

## ⚙️ **Bước 3: Cập nhật appsettings.json**

Thay thế các giá trị trong `appsettings.json`:

```json
{
  "GoogleOAuth": {
    "ClientId": "YOUR_ACTUAL_CLIENT_ID.apps.googleusercontent.com",
    "ClientSecret": "YOUR_ACTUAL_CLIENT_SECRET",
    "RedirectUri": "https://localhost:7000/auth/google/callback"
  }
}
```

## 🚀 **Bước 4: Chạy ứng dụng**

1. **Build và chạy project:**
   ```bash
   dotnet build
   dotnet run
   ```

2. **Truy cập trang đăng nhập:**
   ```
   https://localhost:7000/login
   ```

3. **Click nút "Đăng nhập với Google"**

## 🔄 **Luồng hoạt động OAuth**

### **1. User click "Đăng nhập với Google"**
- Nút sẽ redirect đến `/auth/google`
- Controller tạo Google OAuth URL
- User được redirect đến Google login page

### **2. User đăng nhập Google**
- Google hiển thị trang đăng nhập
- User nhập credentials
- Google xác thực và hiển thị consent screen

### **3. Google callback**
- Google redirect về `/auth/google/callback` với authorization code
- Controller đổi code lấy access token
- Lấy thông tin user từ Google People API
- Redirect về trang chính với thông tin user

## 🛠️ **Troubleshooting**

### **Lỗi "redirect_uri_mismatch"**
- Kiểm tra redirect URI trong Google Cloud Console
- Đảm bảo URI khớp chính xác với appsettings.json

### **Lỗi "invalid_client"**
- Kiểm tra Client ID và Client Secret
- Đảm bảo đã copy đúng từ Google Cloud Console

### **Lỗi "access_denied"**
- User đã từ chối quyền truy cập
- Kiểm tra scopes trong GoogleAuthService

### **Lỗi "invalid_grant"**
- Authorization code đã hết hạn
- Code chỉ sử dụng được 1 lần

## 🔒 **Bảo mật**

1. **Không commit Client Secret vào Git**
2. **Sử dụng User Secrets trong development**
3. **Sử dụng Environment Variables trong production**
4. **Giới hạn redirect URIs chỉ cho domain của bạn**

## 📱 **Production Deployment**

1. **Cập nhật redirect URIs:**
   ```
   https://yourdomain.com/auth/google/callback
   ```

2. **Sử dụng Environment Variables:**
   ```bash
   export GoogleOAuth__ClientId="your_client_id"
   export GoogleOAuth__ClientSecret="your_client_secret"
   export GoogleOAuth__RedirectUri="https://yourdomain.com/auth/google/callback"
   ```

## 🎯 **Kết quả cuối cùng**

Sau khi hoàn thành, khi user click "Đăng nhập với Google":
- ✅ Mở trang Google OAuth
- ✅ User đăng nhập Google
- ✅ Lấy thông tin user profile
- ✅ Redirect về ứng dụng với thông tin user
- ✅ UI đẹp và responsive

---

**🎉 Chúc mừng! Bạn đã tích hợp Google OAuth thành công!**








