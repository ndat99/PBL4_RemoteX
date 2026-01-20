# RemoteX - Ứng dụng Điều khiển Máy tính Từ xa

RemoteX xin chào! Đây là ứng dụng điều khiển máy tính từ xa được xây dựng bằng C# và WPF trên nền tảng .NET. Cho phép người dùng xem và điều khiển màn hình của một máy tính khác, cùng các chức năng chat, gửi file và chụp ảnh màn hình.

## **Tính năng**

* **Điều khiển từ xa**: Điều khiển chuột và bàn phím của máy tính đối tác trong thời gian thực.
* **Chia sẻ Màn hình**: Xem màn hình của đối tác trong thời gian thực.
* **Chat tích hợp**: Trò chuyện văn bản trực tiếp với đối tác trong phiên kết nối.
* **Gửi file**: Truyền gửi file với đối tác thông qua Chatbox.
* **Kết nối An toàn**: Các phiên kết nối được bảo vệ bằng mật khẩu ngẫu nhiên mỗi khi mở ứng dụng.
* **Chụp ảnh Màn hình**: Lưu lại khung hình hiện tại của máy đối tác thành file ảnh JPEG.

## **Kiến trúc Kỹ thuật**

Ứng dụng được xây dựng theo kiến trúc Client-Server.

## **Hướng dẫn Cài đặt & Sử dụng**
### **Yêu cầu**
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (với .NET Desktop Development workload).
* .NET SDK (.NET 8.0).

### **Cách sử dụng**
1.  Mở RemoteX.Server.exe ở một máy ảo có public IP (hoặc trong chính máy bạn nếu muốn kiểm thử localhost)
2.  Mở file config.json trong RemoteX_Client/bin/Release và sửa IP server cho đúng với mục đích của bạn (IP Public (nếu chạy trong WAN) hoặc IPv4 Private của máy chạy Server (nếu chạy trong LAN))
3.  Mở RemoteX.Client.exe ở các máy khách.
4.  Đối với máy bị điều khiển, hãy sao chép lại **ID của bạn** và **Mật khẩu** và gửi cho phía người điều khiển.
5.  Đối với máy điều khiển, nhập ID và Mật khảu của đối tác vào mục "Điều khiển máy tính khác".
4.  Nhấn nút **Bắt đầu điều khiển**. Cửa sổ điều khiển từ xa sẽ hiện lên và bạn có thể bắt đầu sử dụng.

## **Cấu trúc Dự án**

* **`RemoteX.Server/`**: Chứa mã nguồn cho ứng dụng Server.
* **`RemoteX.Client/`**: Chứa mã nguồn cho ứng dụng Client.
* **`RemoteX.Core/`**: Chứa các lớp dùng chung.

## **Screenshot**
* Server:
<img style="width: 30%; height: auto;" alt="Screenshot 2025-12-21 203902" src="https://github.com/user-attachments/assets/98372815-8799-4424-8abc-83969a625c97" />

* Client:
<img style="width: 30%; height: auto;" alt="Screenshot 2025-12-21 203544" src="https://github.com/user-attachments/assets/e2646825-f663-482e-8793-115ac471a016" />
<img style="width: 30%; height: auto;" alt="Screenshot 2025-12-21 203957" src="https://github.com/user-attachments/assets/02f9826e-9aa3-4ec7-bebc-c1881ff444bb" />

* Remote Screen:
<img style="width: 80%; height: auto;" alt="Screenshot 2025-12-21 203310" src="https://github.com/user-attachments/assets/cdcf4761-51e4-4f06-ac16-158b10e82ce2" />

---
Cảm ơn bạn đã xem qua dự án!
