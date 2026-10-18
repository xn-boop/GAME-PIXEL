# Đánh giá rủi ro — Ô nhập mật khẩu LoginMenu

## Phạm vi

Tài liệu và test `LoginPasswordInputTests` kiểm tra việc **che ký tự trên giao diện** của `UnityEngine.UI.InputField` trong `Assets/Scenes/LoginMenu.unity`.

Việc hiển thị dấu `*` chỉ bảo vệ khỏi người nhìn trực tiếp vào màn hình. Nó không phải mã hóa mật khẩu và không tạo hệ thống tài khoản thật.

## Ma trận rủi ro

| Rủi ro | Mức độ | Cách phát hiện | Cách giảm thiểu |
| --- | --- | --- | --- |
| Mật khẩu hiện nguyên văn vì `Content Type` bị đổi nhầm sang `Standard` | Cao | `PasswordField_UsesPasswordContentType` | Giữ `InputField.ContentType.Password`; chạy Test Runner sau khi sửa prefab/scene. |
| Ký tự che không đúng yêu cầu thiết kế | Thấp | `PasswordField_UsesAsteriskAsMaskCharacter` | Đặt `asteriskChar = '*'` trong Inspector hoặc ở lúc khởi tạo. |
| Lập trình viên thay password thật bằng chuỗi `*****` | Cao | `PasswordField_MasksVisibleText_ButRetainsActualValueForLogin` | Chỉ che phần hiển thị; luôn dùng `passwordInput.text` làm dữ liệu đầu vào. |
| Cấu hình Password áp nhầm vào Email | Trung bình | `EmailField_RemainsEmailInput_AndIsNotMasked` | Giữ Email ở `ContentType.EmailAddress`. |
| Có newline hoặc lỗi thao tác do password field đa dòng | Thấp | `PasswordField_IsSingleLine` | Giữ `Line Type = Single Line`. |
| Password bị lưu dạng rõ trong PlayerPrefs, JSON hoặc log | Cao | Không thể kết luận chỉ bằng test UI | Không lưu password vào PlayerPrefs/save/log/GitHub; dùng backend hoặc hash + salt nếu sau này làm tài khoản offline thật. |
| Lộ mật khẩu vì screenshot, screen recording hoặc người đứng cạnh | Trung bình | Test UI không chặn được | Mặc định che ký tự; chỉ cho phép hiện mật khẩu khi người chơi chủ động nhấn icon mắt (nếu có). |
| Cảm giác an toàn giả vì Login hiện là Local Mock Demo | Cao | Rà soát `LoginMenuController` | Ghi rõ trong tài liệu/game: đây chưa phải xác thực thật; không hứa hẹn bảo mật tài khoản. |

## Kết quả cần đạt

Chạy **Window → General → Test Runner → EditMode → Run All**. Năm test trong `LoginPasswordInputTests` phải xanh.

Nếu các test xanh, chỉ có thể kết luận: cấu hình scene LoginMenu đang che password bằng `*` và không che nhầm email. Các rủi ro về lưu trữ/xác thực vẫn cần được xử lý riêng khi game không còn dùng Local Mock Demo.
