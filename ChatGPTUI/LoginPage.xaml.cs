using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
#if ANDROID || IOS
using Microsoft.Maui.Authentication;
#endif

namespace ChatGPTUI
{
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080") // Đổi thành địa chỉ Nginx gateway nếu cần
        };

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/user/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                // Xử lý lưu token, chuyển màn hình...
                MessageLabel.Text = "Đăng nhập thành công!";
            }
            else
            {
                MessageLabel.Text = "Sai email hoặc mật khẩu!";
            }
        }

        private async void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            // 1. Mở trình duyệt để lấy Google token (OAuth2)
            var googleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth?..."; // Tạo URL phù hợp với client_id của bạn
            try
            {
#if ANDROID || IOS
                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(googleAuthUrl),
                    new Uri("yourapp://redirect")); // Đặt đúng redirect URI
                var googleToken = result?.AccessToken;
#else
                // Trên desktop, có thể mở trình duyệt mặc định và yêu cầu người dùng dán mã xác thực
                var googleToken = await GetGoogleTokenDesktop(googleAuthUrl);
#endif
                if (!string.IsNullOrEmpty(googleToken))
                {
                    // 2. Gửi token này tới API backend
                    var googleLoginRequest = new { Token = googleToken };
                    var content = new StringContent(JsonConvert.SerializeObject(googleLoginRequest), System.Text.Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("/api/user/login-with-google", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageLabel.Text = "Đăng nhập Google thành công!";
                        // Xử lý lưu token, chuyển màn hình...
                    }
                    else
                    {
                        MessageLabel.Text = "Đăng nhập Google thất bại!";
                    }
                }
                else
                {
                    MessageLabel.Text = "Không lấy được Google token!";
                }
            }
            catch (Exception ex)
            {
                MessageLabel.Text = "Lỗi đăng nhập Google: " + ex.Message;
            }
        }

#if !ANDROID && !IOS
        private async Task<string> GetGoogleTokenDesktop(string googleAuthUrl)
        {
            try
            {
                // Mở trình duyệt mặc định
                await Launcher.Default.OpenAsync(googleAuthUrl);
                // Hiển thị dialog yêu cầu người dùng dán mã xác thực
                string token = await Application.Current.MainPage.DisplayPromptAsync("Google OAuth", "Dán mã xác thực từ Google:");
                return token;
            }
            catch
            {
                return null;
            }
        }
#endif
    }
} 