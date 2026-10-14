# Tài liệu Nghiệp vụ: Cấu trúc & Workflow Phần CORE LOGIC

**Dự án:** Journey of Memories

## 1. Nguyên tắc cốt lõi (Single Source of Truth)

- **Core Logic** là hệ thống trung tâm duy nhất nắm giữ toàn bộ trạng thái (State) và tiến trình của trò chơi.
- **Tách biệt UI và Logic:** Giao diện (UI) ở các map chỉ làm nhiệm vụ hiển thị và gửi yêu cầu (Event) tới Core. UI tuyệt đối không được tự ý thay đổi dữ liệu hay tự xác nhận hoàn thành nhiệm vụ.

## 2. Chi tiết Workflow các Module Lõi

### 2.1. Hệ thống Quản lý Câu đố (QuizManager)
Hệ thống này xử lý toàn bộ logic khi người chơi tương tác với vật phẩm văn hóa và trả lời câu hỏi.
- **Input:** ID Câu hỏi (vd: `Q01`), ID Đáp án được chọn.
- **Workflow:**
  1. Kiểm tra trạng thái: Câu hỏi này đã được giải quyết chưa? (Nếu rồi -> Bỏ qua).
  2. So khớp đáp án người chơi chọn với đáp án cấu hình sẵn.
  3. **Nhánh Sai:** Trả về tín hiệu `False` kèm theo chuỗi text giải thích (lấy từ Data) để UI hiển thị. Trạng thái vật phẩm giữ nguyên ở map.
  4. **Nhánh Đúng:**
     - Ghi nhận `quiz_[ID]_solved = true`.
     - Kích hoạt sự kiện cấp vật phẩm tương ứng (vd: `Food_ThitKhoTrung`).
     - Báo cho `InventoryManager` đưa vật phẩm vào túi.
     - Trả tín hiệu `True` để UI đóng bảng câu đố và hiển thị thông báo thu thập thành công.

### 2.2. Hệ thống Quản lý Túi đồ (InventoryManager)
Quản lý tối đa 5 ô chứa đồ, độc lập hoàn toàn với việc hiển thị của túi trên màn hình.
- **Workflow thêm đồ (Add):**
  1. Nhận yêu cầu thêm đồ từ `QuizManager` hoặc `CombineManager` (ghép ngọc).
  2. Kiểm tra sức chứa. Nếu còn chỗ, lưu ID vật phẩm vào danh sách túi.
  3. Gửi sự kiện `OnInventoryUpdated` để Inventory UI cập nhật lại danh sách icon.
- **Workflow lấy đồ (Remove):**
  1. Chỉ được gọi bởi `PlacementManager` khi người chơi đã đặt thành công, hoặc khi ghép ngọc thành công.
  2. Xóa ID vật phẩm khỏi danh sách và gửi tín hiệu cho UI cập nhật.

### 2.3. Hệ thống Đặt vật phẩm (PlacementManager)
Đây là hệ thống phân xử việc kéo/thả hoặc bấm chọn đồ vật đặt lên các hốc (slot) chỉ định (như mâm cơm, ma nơ canh, bệ đá).
- **Input:** ID Vật phẩm đang chọn trong túi, ID Slot đích (vd: `Slot_ThitKhoTrung`).
- **Workflow:**
  1. Kiểm tra Slot đích đã có đồ chưa. Nếu có -> Trả về lỗi `SlotAlreadyFilled`.
  2. So khớp ID Vật phẩm với cấu hình cho phép của Slot đích.
  3. **Nhánh Sai:** Trả về tín hiệu thất bại. UI hiển thị gợi ý (vd: "Vị trí này chưa khớp..."). Vật phẩm vẫn giữ nguyên trong Inventory.
  4. **Nhánh Đúng:**
     - Lưu trạng thái slot `Slot_[ID]_Filled = true`.
     - Gọi hàm Remove của `InventoryManager` để tiêu thụ vật phẩm.
     - Gọi `GameManager` để kiểm tra điều kiện hoàn thành map.

### 2.4. Hệ thống Trạng thái & Chuyển chặng (StateManager / GameManager)
Điều phối tiến trình tổng thể của game và theo dõi các cột mốc hoàn thành.
- **Các trạng thái (State):** `NotStarted` -> `Active` -> `MemoryPending` (Chờ xem ký ức) -> `JournalPending` (Chờ xác nhận nhật ký) -> `Completed`.
- **Workflow kiểm tra hoàn thành (Map 1 làm ví dụ):**
  1. Lắng nghe tín hiệu từ các module khác.
  2. Kiểm tra cổng logic: `MiniGame Mèo == Done` + `Slot_ThitKhoTrung == Filled` + `Slot_Kimchi == Filled` + `Slot_ComCuon == Filled`.
  3. Nếu đủ điều kiện: Đổi State sang `MemoryPending`.
  4. Khóa toàn bộ điều khiển nhân vật, gửi tín hiệu cho UI phát cảnh Ký ức.
  5. Khi UI báo người chơi ấn "Ghi nhớ" ở trang nhật ký, đổi State thành `Completed`, mở cửa chặn sang map sau.

### 2.5. Hệ thống Lưu/Tải tiến trình (SaveLoadManager)
Đảm bảo tiến trình không bị mất và ngăn chặn việc bị nhân đôi đồ vật khi đóng ứng dụng giữa chừng.
- **Quy tắc Lưu (Save):** Tự động lưu sau bất kỳ trạng thái nào được cập nhật thành công (Trả lời quiz đúng, Đặt đồ đúng chỗ, Nhặt được mảnh ghép, Khép nhật ký). Dữ liệu được ghi thẳng xuống file cục bộ dưới dạng JSON (hoặc cấu trúc tương đương).
- **Quy tắc Phục hồi (Load):**
  1. Khi mở game, khởi tạo Core Logic trước tiên.
  2. Đọc file save. Nếu có, đắp dữ liệu vào `InventoryManager` (các đồ vật trong túi), `PlacementManager` (các slot đã đầy) và `GameManager` (trạng thái map hiện tại).
  3. Khi sinh ra các đối tượng trong môi trường (3D/2D), đối chiếu với Core Logic. Đồ nào đã nằm trong túi hoặc slot thì bị ẩn (Hide) ở ngoài map để không bị lặp.

## 3. Hợp đồng Giao tiếp Nội bộ (Internal API Contract)

*Đây là quy chuẩn để hai lập trình viên gọi hàm của nhau mà không bị xung đột.*

| Tên Hàm (Action) | Component Gọi (Từ đâu) | Component Xử lý (Core) | Tham số truyền vào (Input) | Kết quả trả về (Output / Callback) |
| :--- | :--- | :--- | :--- | :--- |
| `SubmitQuizAnswer` | Quiz UI | QuizManager | `QuizID`, `OptionID` | `QuizSubmitStatus`, `FeedbackText`, `RewardItemID` |
| `TryPlaceItem` | Inventory UI | PlacementManager | `ItemID`, `SlotID` | `Success`, `ItemMismatch`, `SlotFull` |
| `TryCombineGems` | MiniGame UI (Map 3) | CombineManager | Không (tự quét túi) | `CombineResult`, `Gem_Whole`, thông báo |
| `ConfirmJournal` | Journal UI | GameStateManager | `MapID` | Chỉ thành công từ `JournalPending` |
| `RequestAndLoad`| Trigger Cửa | SceneFlowManager | `TargetMapID` | Kiểm tra tuyến, lưu state rồi load scene |

Danh sách ID chuẩn nằm duy nhất trong `GameIds.cs`. Chi tiết trách nhiệm và API bắt buộc xem `docs/API_CONTRACT.md`.
