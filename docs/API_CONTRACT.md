# Core API Contract — Journey of Memories

Tài liệu này là điểm giao tiếp bắt buộc giữa Dev A, Dev B và các scene. Core là nguồn dữ liệu duy nhất; UI và world object chỉ gửi yêu cầu rồi hiển thị kết quả.

## 1. Quy tắc không được phá vỡ

1. Không dùng tên hiển thị làm ID. Dùng hằng số trong `GameIds`.
2. Không tạo inventory, quiz manager, bộ đếm slot hoặc state map riêng trong scene.
3. Không tự đánh dấu map hoàn thành từ UI.
4. Không gọi `SceneManager.LoadScene` để đi map kế tiếp; dùng `SceneFlowManager.RequestAndLoad`.
5. Mọi thay đổi tiến độ bền vững phải đi qua Core để được auto-save.

## 2. API dùng chung

| Nghiệp vụ | API | Kết quả |
|---|---|---|
| Trả lời quiz | `QuizManager.SubmitQuizAnswer(quizId, option)` | `QuizSubmitStatus`, feedback, reward ID |
| Chọn vật phẩm | `InventoryUI.SelectedItemId` | ID hoặc `null`; UI không sở hữu item |
| Đặt vật phẩm | `PlacementManager.TryPlaceItem(itemId, slotId)` | `PlacementResult` |
| Ghép ngọc | `CombineManager.TryCombineGems()` | `CombineResult`, gem ID, message |
| Mini-game hoàn tất | `GameStateManager.SetMiniGameDone(mapIndex, true)` | `bool` |
| Nhặt mảnh bản đồ | `CollectMapFragment(1..4)` | `bool` |
| Đặt mảnh bản đồ | `PlaceMapFragment(1..4)` | `bool`; đủ 4 tự hoàn tất mini-game |
| Đặt ngọc lên bệ | `PlacementManager.TryPlaceItem(Gem_Whole, Slot_Pedestal)` | Core tự ghi nhận pedestal |
| Cầu dựng xong | `GameStateManager.MarkBridgeReady()` | Chỉ hợp lệ sau pedestal |
| Cuộc gặp kết thúc | `GameStateManager.MarkReunionSeen()` | Chỉ hợp lệ sau bridge |
| Ký ức phát xong | `GameStateManager.NotifyMemoryFinished()` | Chỉ từ `MemoryPending` |
| Xác nhận nhật ký | `GameStateManager.ConfirmJournal(mapId)` | Chỉ từ `JournalPending` |
| Sang map | `SceneFlowManager.RequestAndLoad(targetMap)` | Chỉ đúng tuyến và đủ điều kiện |
| Xác nhận trang cuối | `GameStateManager.ConfirmFinalPage()` | Chỉ trong Ending |
| Hoàn tất game | `GameStateManager.CompleteGame()` | Chỉ sau khi xác nhận trang cuối |

## 3. Ownership

| Phần | Chủ trì | Không được tự tạo |
|---|---|---|
| Menu, Intro, Map 1, Ending | Dev A | Inventory/quiz/state riêng |
| Map 2, Map 3 | Dev B | Save manager, scene transition hoặc ID mới không đăng ký |
| Core/Data, Core/Managers, contract | Dev A review chính; cả hai duyệt thay đổi | Sửa trực tiếp đồng thời ở hai branch |

Khi Dev B cần API mới, cập nhật contract và test Core trước, merge Core trước rồi mới tiếp tục scene map.

## 4. Definition of Done cho mỗi map

- Không có bộ đếm tiến độ riêng ngoài Core.
- Thao tác sai không làm mất vật phẩm.
- Nhấn lặp không cấp trùng hoặc phát event hoàn thành hai lần.
- Đóng và mở lại game phục hồi đúng inventory, slot và state.
- Chỉ chuyển map qua `SceneFlowManager`.
- Unit test Core và test thủ công các ca tương ứng trong workflow đều đạt.
