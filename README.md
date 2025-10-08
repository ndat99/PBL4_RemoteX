# **RemoteX - Ứng dụng Điều khiển Máy tính Từ xa 🖥️**

Chào mừng bạn đến với RemoteX! Đây là một dự án ứng dụng điều khiển máy tính từ xa được xây dựng bằng C# và WPF trên nền tảng .NET. Dự án cho phép người dùng xem và điều khiển màn hình của một máy tính khác thông qua mạng, đi kèm với chức năng chat, gửi file và chụp ảnh màn hình.

## ## **Tính năng nổi bật ✨**

* **Điều khiển từ xa**: Điều khiển chuột và bàn phím của máy tính đối tác trong thời gian thực.
* **Chia sẻ Màn hình**: Xem màn hình của đối tác trong thời gian thực.
* **Chat tích hợp**: Trò chuyện văn bản trực tiếp với đối tác trong phiên kết nối.
* **Gửi file**: Truyền gửi file với đối tác thông qua Chatbox
* **Kết nối An toàn**: Các phiên kết nối được bảo vệ bằng mật khẩu ngẫu nhiên.
* **Chụp ảnh Màn hình**: Dễ dàng lưu lại khung hình hiện tại của máy đối tác thành file ảnh JPEG.
* **Kiến trúc Client-Server**: Mô hình máy chủ trung gian giúp thiết lập kết nối giữa hai client một cách dễ dàng.

## ## **Kiến trúc Kỹ thuật ⚙️**

Dự án được xây dựng theo kiến trúc Client-Server.

## ## **Hướng dẫn Cài đặt & Sử dụng 🚀**
### **Yêu cầu**
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (với .NET Desktop Development workload).
* .NET SDK (.NET 8.0).

### **Các bước thiết lập**
1.  Clone hoặc tải về toàn bộ mã nguồn của dự án.
2.  Mở file solution (`RemoteX.sln`) bằng Visual Studio.
3.  **Quan trọng**: Cấu hình để khởi chạy nhiều dự án cùng lúc:
    * Click chuột phải vào **Solution 'RemoteX'** trong cửa sổ Solution Explorer.
    * Chọn **`Configure Startup Projects...`**.
    * Chọn **`Multiple startup projects`**.
    * Đặt **Action** thành **`Start`** cho hai dự án: `RemoteX.Server` và `RemoteX.Client`.
    * Giữ nguyên **Action** là `None` cho `RemoteX.Core`.
    * Nhấn `Apply` -> `OK`.
4.  Nhấn phím **F5** hoặc nút "Start" để bắt đầu gỡ lỗi. Visual Studio sẽ tự động chạy Server và một Client.

### **Cách sử dụng**
1.  Mở RemoteX.Server.exe ở một máy ảo có public IP (hoặc trong chính máy bạn nếu muốn kiểm thử localhost)
2.  Mở file config.json và sửa IP server cho đúng với mục đích của bạn (IP Public (nếu chạy trong WAN) hoặc IPv4 Private của máy chạy Server (nếu chạy trong LAN))
3.  Mở RemoteX.Client.exe ở các máy khách.
4.  Đối với máy bị điều khiển, hãy sao chép lại **ID của bạn** và **Mật khẩu** và gửi cho phía người điều khiển.
5.  Đối với máy điều khiển, nhập ID và Mật khảu của đối tác vào mục "Điều khiển máy tính khác".
4.  Nhấn nút **Bắt đầu điều khiển**. Cửa sổ điều khiển từ xa sẽ hiện lên và bạn có thể bắt đầu sử dụng.

## ## **Cấu trúc Dự án 📂**

* **`RemoteX.Server/`**: Chứa mã nguồn cho ứng dụng Server.
* **`RemoteX.Client/`**: Chứa mã nguồn cho ứng dụng Client WPF (Views, ViewModels, Services).
* **`RemoteX.Core/`**: Chứa các lớp dùng chung (Models, Networking, Utils).

---
Cảm ơn bạn đã xem qua dự án! Chúc bạn có những trải nghiệm vui vẻ. 😄
