GAME LOGIC BRIEF

CHUYẾN ĐI CỦA NHỮNG KÝ ỨC

English title: Journey of Memories
Document: Game Logic Brief — Master Specification
Version: 1.1
Ngày cập nhật: 13/10/2026
Engine: Unity 6.6
Thể loại: Narrative Puzzle / Exploration / Casual Adventure
Nền tảng mục tiêu: Android, chơi offline
Định hướng hình ảnh: 2D pixel art, góc nhìn top-down
Tỉ lệ khung hình gameplay mục tiêu: Landscape 16:9
Đối tượng sử dụng tài liệu: Game Designer, Unity Developer, Artist, AI Coding Assistant, người tích hợp dự án.

PHẦN I — TỔNG QUAN DỰ ÁN

1.1. Ý tưởng cốt lõi

Chuyến đi của những ký ức là một tựa game phiêu lưu giải đố kể về Hạ Ân, một cô gái mang trong mình hai dòng văn hóa Việt Nam và Hàn Quốc.

Vào sinh nhật tuổi 20, Hạ Ân trở về ngôi nhà cũ và mở chiếc hộp thời gian mà cô cùng cha mẹ đã hứa sẽ mở sau 12 năm. Bên trong là cuốn nhật ký cũ, nhưng toàn bộ nội dung đã biến mất, chỉ còn những trang giấy trắng.

Để khôi phục cuốn nhật ký, Hạ Ân phải khám phá lại những không gian gắn liền với tuổi thơ, tìm kiếm những vật phẩm thuộc về ký ức gia đình, vượt qua các thử thách nhỏ và trả lời những câu hỏi về văn hóa Việt Nam — Hàn Quốc.

Mỗi vật phẩm không chỉ là một món đồ cần tìm mà còn là một mảnh ghép của câu chuyện. Khi hoàn thành từng thử thách, Hạ Ân dần hiểu rằng cô không cần lựa chọn một trong hai cội nguồn để xác định bản thân.

Cô có thể thuộc về cả hai.

1.2. Trải nghiệm cốt lõi

Người chơi cần trải nghiệm được năm điều:

Khám phá: Tự di chuyển trong môi trường, tìm kiếm vật phẩm và quan sát manh mối.

Giải đố: Hoàn thành mini-game, trả lời câu hỏi văn hóa và giải quyết các thử thách theo trình tự.

Thu thập và sắp xếp: Đưa các vật phẩm tìm được vào đúng vị trí để tái hiện ký ức.

Khám phá văn hóa: Tìm hiểu các nét đặc trưng của Việt Nam và Hàn Quốc thông qua câu hỏi, vật phẩm và câu chuyện.

Kết nối cảm xúc: Hiểu hành trình trưởng thành của Hạ Ân qua các cảnh ký ức và phần kết.

1.3. Nguyên tắc thiết kế

Gameplay phải phục vụ câu chuyện, không chỉ tồn tại như những nhiệm vụ rời rạc.

Người chơi được tự do khám phá trong phạm vi bản đồ nhưng phải tuân theo các điều kiện mở khóa đã thiết kế.

Không dùng câu hỏi trắc nghiệm để thay thế toàn bộ gameplay khám phá.

Không thể bỏ qua câu hỏi văn hóa để lấy vật phẩm.

Không thể bỏ qua mini-game để mở khóa gợi ý.

Trả lời sai không khiến người chơi mất vật phẩm đã thu thập hoặc bị phạt nặng.

Vật phẩm chỉ được thu thập, đặt và tính tiến độ một lần cho mỗi trạng thái hợp lệ.

Các hệ thống cốt lõi phải được thiết kế để tái sử dụng giữa ba màn.

Mỗi màn có nội dung, mini-game, câu hỏi, vật phẩm và cảnh ký ức riêng.

Game phải hoạt động offline; không phụ thuộc API hoặc kết nối Internet để hoàn thành gameplay chính.

PHẦN II — CỐT TRUYỆN VÀ CẤU TRÚC NARRATIVE

2.1. Nhân vật chính: Hạ Ân

Hạ Ân là cô gái 20 tuổi, có nguồn gốc văn hóa Việt Nam và Hàn Quốc.

Tuổi thơ của cô gắn với những trải nghiệm, món ăn, trang phục và truyền thống từ cả hai đất nước. Tuy nhiên, khi lớn lên, Hạ Ân dần cảm thấy mình không hoàn toàn thuộc về một phía nào.

Sự mâu thuẫn này tạo nên chủ đề trung tâm của game: bản sắc không nhất thiết phải được xác định bằng cách loại bỏ một phần nguồn gốc của bản thân.

Hình ảnh Hạ Ân lúc 8 tuổi đại diện cho một giai đoạn trong quá khứ, khi những ký ức gia đình vẫn còn nguyên vẹn. Cuộc gặp gỡ với bản thân nhỏ tuổi ở cuối game là điểm hội tụ của toàn bộ hành trình.

2.2. Cảnh mở đầu — Chiếc hộp thời gian

Bối cảnh

Hạ Ân trở về ngôi nhà cũ vào sinh nhật tuổi 20. Cô tìm thấy chiếc hộp thời gian được cất giữ từ 12 năm trước.

Diễn biến

Hạ Ân mở hộp thời gian.

Cô tìm thấy cuốn nhật ký tuổi thơ.

Khi mở nhật ký, cô phát hiện các trang viết đã trở thành giấy trắng.

Cô nhận ra mình không thể nhớ lại đầy đủ những điều từng được ghi trong đó.

Người chơi được dẫn vào hành trình khám phá các không gian ký ức.

Mục tiêu narrative

Giới thiệu nhân vật và động lực bắt đầu cuộc hành trình.

Tạo câu hỏi trung tâm: Vì sao những ký ức này cần được khôi phục?

Giới thiệu cuốn nhật ký như vật thể xuyên suốt trò chơi.

Thiết lập liên kết giữa gameplay thu thập vật phẩm và quá trình phục hồi ký ức.

Logic hệ thống

Cảnh mở đầu chỉ xuất hiện ở đầu hành trình, trừ khi người chơi chủ động bắt đầu lại game.

Sau khi cảnh mở đầu hoàn thành, game chuyển đến bản đồ đầu tiên.

Cần tránh phát lại cảnh mở đầu mỗi lần người chơi chuyển từ một màn sang màn khác.

Việc lưu trạng thái cảnh mở đầu qua các lần mở game là một quyết định triển khai cần được chốt riêng.

PHẦN III — GAMEPLAY LOOP TỔNG THỂ

3.1. Vòng lặp gameplay chính

Mỗi màn đi theo vòng lặp tổng quát:

Khám phá → Hoàn thành mini-game → Mở khóa gợi ý → Tìm vật phẩm → Trả lời câu hỏi văn hóa → Thu thập vật phẩm → Đặt vật phẩm đúng vị trí → Kích hoạt ký ức.

Đây là cấu trúc chung. Một số màn có thể thêm bước ghép vật phẩm hoặc mở đường đi trước khi kích hoạt ký ức.

3.2. Sơ đồ luồng gameplay

Bắt đầu màn

|

v

Khám phá môi trường

|

v

Tìm và tương tác với mini-game

|

v

Mini-game đã hoàn thành?

| Không

+------> Tiếp tục hoặc chơi lại

|

Có

|

v

Mở khóa gợi ý tìm vật phẩm

|

v

Tìm vật phẩm và tương tác

|

v

Hiển thị câu hỏi văn hóa

|

v

Trả lời đúng?

| Không

+------> Giải thích / gợi ý và thử lại

|

Có

|

v

Thêm vật phẩm vào Inventory

|

v

Đặt vật phẩm đúng vị trí

|

v

Đã hoàn thành mọi điều kiện?

| Không

+------> Tiếp tục khám phá

|

Có

|

v

Kích hoạt cảnh ký ức

|

v

Hoàn thành phần nhật ký

|

v

Chuyển sang màn tiếp theo

3.3. Các trạng thái chính của một màn

Mỗi màn nên có trạng thái logic rõ ràng:

NotStarted: Màn chưa bắt đầu.

Exploring: Người chơi đang khám phá.

MiniGameActive: Mini-game đang diễn ra.

HintsUnlocked: Mini-game đã hoàn thành và gợi ý đã mở.

CollectingItems: Người chơi đang tìm kiếm, trả lời câu hỏi và thu thập vật phẩm.

PlacingItems: Người chơi đang sắp xếp vật phẩm.

ResolvingFinalPuzzle: Đang thực hiện thử thách kết hợp cuối màn, nếu có.

MemoryPlaying: Cảnh ký ức đang diễn ra.

JournalEntry: Phần nhật ký của màn đang được hiển thị hoặc hoàn thành.

Completed: Màn đã hoàn thành.

Đây là các trạng thái logic đề xuất, không bắt buộc phải tạo mỗi trạng thái thành một scene Unity riêng.

Các trạng thái có thể được quản lý bằng một LevelController hoặc hệ thống tương đương. Tránh để nhiều script cùng tự quyết định trạng thái hoàn thành màn.

PHẦN IV — QUY TẮC GAMEPLAY DÙNG CHUNG

4.1. Điều khiển nhân vật

Điều khiển trên PC trong quá trình phát triển

Sử dụng WASD hoặc phím mũi tên để di chuyển.

Sử dụng phím E để tương tác với đối tượng gần nhân vật, nếu đối tượng hỗ trợ tương tác bằng phím này.

Các phím hoặc thao tác khác cần được cấu hình thống nhất theo từng UI.

Điều khiển trên Android

Sử dụng joystick ảo để di chuyển.

Sử dụng nút tương tác trên màn hình để tương tác với vật phẩm hoặc đối tượng.

Nút tương tác chỉ thực hiện thao tác hợp lệ trong ngữ cảnh hiện tại.

Không yêu cầu người chơi dùng bàn phím vật lý để hoàn thành game.

Quy tắc tương tác

Nhân vật phải ở trong phạm vi tương tác của đối tượng.

Không được kích hoạt cùng một tương tác nhiều lần do sự kiện bị gọi lặp.

Khi một UI quan trọng xuất hiện, cần quyết định rõ nhân vật có bị khóa di chuyển hay không.

Khi cảnh ký ức, câu hỏi hoặc nhật ký yêu cầu người chơi tập trung, không để joystick vô tình điều khiển nhân vật phía sau UI.

Bán kính tương tác, tốc độ di chuyển và các giá trị vật lý là tham số cấu hình, không phải hằng số áp dụng cho mọi đối tượng.

4.2. Mini-game và điều kiện mở gợi ý

Quy tắc bắt buộc: Người chơi phải hoàn thành mini-game trước khi hệ thống mở khóa gợi ý vị trí vật phẩm.

Luồng xử lý:

Người chơi tìm đến khu vực mini-game.

Tương tác để bắt đầu.

Mini-game chuyển sang trạng thái đang chơi.

Nếu chưa hoàn thành, gợi ý vẫn bị khóa.

Nếu người chơi thoát khi mini-game chưa hoàn thành, màn không được tính là đã hoàn thành mini-game.

Người chơi có thể tiếp tục hoặc chơi lại tùy thiết kế của mini-game.

Khi hoàn thành hợp lệ, hệ thống ghi nhận trạng thái hoàn thành.

Gợi ý được mở khóa.

Mini-game không cần hoàn thành lại trong cùng tiến trình của màn.

Quy tắc chống lỗi

Không mở gợi ý chỉ vì người chơi đã mở mini-game.

Không mở gợi ý chỉ vì người chơi đã thoát khỏi mini-game.

Không ghi nhận hoàn thành nhiều lần khi cùng một sự kiện hoàn thành bị gọi lặp.

Nếu cần lưu tiến trình, trạng thái mini-game đã hoàn thành phải được lưu cùng tiến trình màn.

4.3. Hệ thống gợi ý

Gợi ý cung cấp thông tin giúp người chơi tìm được vật phẩm sau khi mini-game hoàn thành.

Trước khi mini-game hoàn thành:

Gợi ý vẫn bị khóa.

Người chơi có thể khám phá môi trường nhưng chưa được nhận gợi ý vị trí theo thiết kế của màn.

Sau khi mini-game hoàn thành:

Gợi ý tương ứng được mở khóa.

Người chơi có thể sử dụng gợi ý để xác định khu vực hoặc vật thể cần tìm.

Gợi ý không được tự động thu thập vật phẩm.

Gợi ý không được tự động trả lời câu hỏi thay người chơi.

Nội dung gợi ý phải phù hợp với từng vật phẩm. Hệ thống gợi ý là hệ thống dùng chung; dữ liệu của mỗi màn được cấu hình riêng.

4.4. Tương tác vật phẩm và câu hỏi văn hóa

Mỗi vật phẩm bắt buộc phải được xác nhận thông qua câu hỏi văn hóa trước khi người chơi có thể thu thập.

Luồng chuẩn:

Người chơi đến gần vật phẩm.

Người chơi tương tác.

Hệ thống xác nhận vật phẩm chưa được thu thập.

Hệ thống mở giao diện câu hỏi tương ứng.

Người chơi chọn câu trả lời.

Nếu sai, hiển thị phản hồi và cho phép thử lại.

Nếu đúng, đóng câu hỏi hoặc chuyển sang trạng thái xác nhận.

Hệ thống thêm vật phẩm vào Inventory.

Vật phẩm trong thế giới được đánh dấu đã thu thập và không thể được thu thập lần nữa.

Tiến độ chỉ được cập nhật một lần.

Chính sách trả lời sai

Không giới hạn số lần trả lời sai.

Không mất mạng, điểm hoặc vật phẩm vì trả lời sai.

Không được bỏ qua câu hỏi để lấy vật phẩm.

Vật phẩm vẫn ở thế giới khi câu trả lời chưa đúng.

Câu hỏi phải tiếp tục cho phép người chơi trả lời đến khi đúng.

Có thể cung cấp phản hồi giải thích hoặc gợi ý học tập sau câu trả lời sai.

Phản hồi sai không nên mang tính chế giễu, trừng phạt hoặc gây áp lực không cần thiết.

Chính sách trả lời đúng

Vật phẩm được thêm vào Inventory ngay sau khi hệ thống xác nhận câu trả lời đúng.

Nếu Inventory đầy hoặc thao tác thêm vật phẩm thất bại, không được xóa vật phẩm khỏi thế giới.

Nếu thao tác thu thập thành công, những lần tương tác sau không được tạo thêm bản sao.

Mỗi vật phẩm phải gắn với một ID duy nhất trong dữ liệu màn.

4.5. Inventory

Inventory lưu trữ các vật phẩm người chơi đã thu thập thành công nhưng chưa được đặt hoặc tiêu thụ theo luật chơi.

Cấu hình hiện tại

Inventory UI dự kiến có 5 ô.

Một số màn chỉ cần 2–3 vật phẩm; số ô trống còn lại không nhất thiết phải được sử dụng.

Số ô hiển thị không đồng nghĩa với số vật phẩm bắt buộc để hoàn thành màn.

Quy tắc

Chỉ vật phẩm đã vượt qua câu hỏi văn hóa mới được thêm vào Inventory.

Mỗi vật phẩm chỉ được thêm một lần.

Icon trong ô phải phản ánh đúng vật phẩm đang sở hữu.

Ô trống phải được hiển thị nhất quán.

Khi vật phẩm được đặt thành công vào vị trí cần thiết, Inventory phải cập nhật theo một quy tắc thống nhất.

Không được mất vật phẩm nếu thao tác đặt thất bại.

UI và dữ liệu logic phải đồng bộ.

Cơ chế lựa chọn vật phẩm

Game hỗ trợ quy trình:

Chọn vật phẩm trong Inventory.

Thử đặt vật phẩm vào vị trí hợp lệ.

Nếu đúng, hệ thống xác nhận và xử lý.

Nếu sai, vật phẩm vẫn nằm trong Inventory và tiến độ không tăng.

Nếu thiết kế cho phép tương tác trực tiếp với một slot để đặt vật phẩm, hệ thống cần xác định rõ vật phẩm đang được chọn và slot đang được nhắm đến.

Quyết định cần chốt: Khi đặt vật phẩm thành công, vật phẩm sẽ bị tiêu thụ khỏi Inventory hay vẫn hiển thị như một vật phẩm đã đặt. Hai cách đều có thể triển khai, nhưng UI, dữ liệu và logic phải thống nhất.

4.6. Hệ thống đặt vật phẩm

Mỗi vị trí đặt phải có ID và danh sách vật phẩm hợp lệ.

Ví dụ:

FoodSlot_MainDish chỉ chấp nhận vật phẩm thịt kho trứng.

FoodSlot_Kimchi chỉ chấp nhận kimchi.

FoodSlot_Kimbap chỉ chấp nhận kimbap.

Đây là ID minh họa, cần được đối chiếu với tên thật trong dự án.

Quy tắc đặt

Vật phẩm đúng vị trí: đặt thành công.

Vật phẩm sai vị trí: không đặt, không mất vật phẩm, không tăng tiến độ.

Vị trí đã được hoàn thành: không xử lý thêm lần nữa.

Tiến độ chỉ tăng khi có một lần đặt hợp lệ mới.

Vật phẩm đã đặt không được nhân bản.

Không kích hoạt cảnh ký ức cho đến khi mọi điều kiện bắt buộc của màn đã hoàn tất.

Hệ thống không nên dựa hoàn toàn vào tên hiển thị. Hãy dùng ID ổn định để xác định vật phẩm và slot hợp lệ.

4.7. Quy tắc chống trùng lặp sự kiện

Tất cả hệ thống cần tránh việc một sự kiện bị xử lý nhiều lần.

Ví dụ:

Một câu trả lời đúng chỉ tạo một lần thu thập.

Một lần đặt đúng chỉ tăng tiến độ một lần.

Mini-game hoàn thành chỉ mở gợi ý một lần.

Màn hoàn thành chỉ kích hoạt cảnh ký ức một lần.

Nút Continue không được chuyển scene nhiều lần do nhấn liên tục.

Nên dùng trạng thái như isCollected, isCompleted, isPlaced hoặc cơ chế tương đương, tùy cấu trúc code thực tế.

Các cờ trạng thái phải được quản lý nhất quán, tránh để hai script khác nhau cùng sở hữu trạng thái cuối cùng của một sự kiện.

4.8. Điều kiện hoàn thành màn

Một màn chỉ hoàn thành khi mọi điều kiện bắt buộc của màn đó đều đúng.

Điều kiện tổng quát:

Mini-game bắt buộc đã hoàn thành.

Các vật phẩm bắt buộc đã được thu thập hợp lệ.

Các câu hỏi văn hóa liên quan đã được trả lời đúng.

Các vị trí đặt vật phẩm bắt buộc đã được hoàn thành.

Thử thách kết hợp hoặc mở đường, nếu có, đã hoàn thành.

Các điều kiện bổ sung riêng của màn đã được thỏa mãn.

Không được kích hoạt cảnh ký ức chỉ vì một biến đếm đạt số lượng yêu cầu nếu dữ liệu thực tế cho thấy một vị trí vẫn chưa được đặt đúng.

4.9. Tạm dừng và khóa điều khiển

Pause System cần có các chức năng cơ bản:

Tạm dừng gameplay.

Tiếp tục chơi.

Mở Settings nếu được hỗ trợ.

Thoát về menu hoặc màn trước theo thiết kế đã thống nhất.

Khi Pause Panel đang mở:

Nhân vật không di chuyển.

Không được thu thập vật phẩm.

Không được đặt vật phẩm.

Không được kích hoạt tương tác thế giới phía sau UI.

Không được để thời gian mini-game tiếp tục chạy nếu mini-game được thiết kế tạm dừng cùng game.

Cần phân biệt việc khóa di chuyển với việc khóa toàn bộ tương tác. Ví dụ, khi Quiz Panel mở, người chơi vẫn cần chọn đáp án trên UI dù không thể điều khiển nhân vật.

PHẦN V — MÀN 1: CĂN BẾP GIA ĐÌNH

5.1. Chủ đề

Chủ đề cảm xúc: Bữa cơm gia đình, sự chăm sóc và khả năng cùng tồn tại của hai nền văn hóa.

Căn bếp tái hiện một không gian gia đình ấm áp. Các món ăn đại diện cho những trải nghiệm văn hóa khác nhau nhưng cùng có thể xuất hiện trên một mâm cơm.

5.2. Mini-game: Tương tác với mèo

Mục tiêu

Người chơi hoàn thành thử thách tương tác với mèo trước khi được mở khóa gợi ý vị trí các món ăn.

Luồng logic

Người chơi khám phá căn bếp.

Tìm đến khu vực mini-game với mèo.

Tương tác để bắt đầu.

Thực hiện các thao tác theo thiết kế của mini-game.

Khi chưa hoàn thành, gợi ý các món ăn vẫn bị khóa.

Khi hoàn thành, hệ thống đánh dấu mini-game đã hoàn thành và mở gợi ý.

Nội dung thao tác chi tiết của mini-game với mèo cần được xác nhận riêng. Tài liệu này không tự đặt ra cách chơi như vuốt ve, cho ăn hay đuổi bắt nếu nhóm chưa thống nhất.

5.3. Vật phẩm bắt buộc

Màn 1 có ba món ăn:

| ID minh họa | Vật phẩm | Vị trí đích | Vai trò |

| --- | --- | --- | --- |

| Food_ThitKhoTrung | Thịt kho trứng | Vị trí món chính | Đại diện món ăn Việt Nam và sự chăm sóc của gia đình |

| Food_Kimchi | Kimchi | Vị trí kimchi | Đại diện ẩm thực Hàn Quốc |

| Food_ComCuon | Kimbap / cơm cuộn Hàn Quốc | Vị trí cơm cuộn | Đại diện ẩm thực Hàn Quốc |

Tên ID chỉ là đề xuất minh họa; phải dùng đúng ID trong project sau khi thống nhất với nhóm.

5.4. Logic từng vật phẩm

A. Thịt kho trứng

Vị trí xuất hiện: Trong một chiếc nồi có nắp ở góc bếp.

Gợi ý: Hướng người chơi đến khu vực nồi ở góc bếp, không tự động thu thập món ăn.

Luồng xử lý:

Mini-game với mèo hoàn thành.

Gợi ý vị trí nồi được mở khóa.

Người chơi tìm đến nồi.

Tương tác với nồi để phát hiện hoặc tiếp cận món ăn.

Hiển thị câu hỏi văn hóa tương ứng.

Trả lời sai: cho phép thử lại.

Trả lời đúng: thêm thịt kho trứng vào Inventory.

Người chơi chọn món và đặt vào vị trí món chính trên mâm.

Nếu đặt đúng, slot được xác nhận hoàn thành.

Ý nghĩa ký ức: Gợi nhớ món ăn mẹ nấu, sự quan tâm và hơi ấm gia đình.

Lưu ý: Không tự xác định câu hỏi, đáp án hoặc vị trí chính xác của slot nếu chưa được nhóm chốt.

B. Kimchi

Vị trí xuất hiện: Cần được xác định trong thiết kế môi trường Màn 1.

Luồng xử lý:

Người chơi sử dụng gợi ý sau khi mini-game hoàn thành.

Tìm kimchi.

Tương tác để mở câu hỏi văn hóa về kimchi.

Trả lời sai: chưa được thu thập.

Trả lời đúng: kimchi được thêm vào Inventory.

Đặt kimchi vào slot dành riêng.

Hệ thống xác nhận slot hoàn thành.

C. Kimbap / cơm cuộn Hàn Quốc

Tên file hiện có theo tiến trình trước: Food_ComCuon và Slot_ComCuon.

Luồng xử lý:

Người chơi tìm món cơm cuộn theo gợi ý.

Tương tác với vật phẩm.

Hiển thị câu hỏi văn hóa tương ứng.

Trả lời sai: tiếp tục câu hỏi.

Trả lời đúng: thêm món vào Inventory.

Chọn vật phẩm và đặt vào Slot_ComCuon hoặc slot tương ứng trong scene.

Đặt đúng thì slot hoàn thành.

5.5. Điều kiện hoàn thành Màn 1

Màn 1 chỉ được hoàn thành khi:

Mini-game với mèo đã hoàn thành.

Thịt kho trứng đã được thu thập và đặt đúng.

Kimchi đã được thu thập và đặt đúng.

Kimbap đã được thu thập và đặt đúng.

Ba vị trí đặt đều được hệ thống xác nhận.

Khi điều kiện được thỏa mãn, kích hoạt cảnh ký ức gia đình.

5.6. Cảnh ký ức sau Màn 1

Cảnh ký ức nhấn mạnh sự ấm áp của bữa cơm gia đình và việc các món ăn từ hai nền văn hóa có thể cùng xuất hiện.

Thông điệp: Không phải mọi khác biệt đều cần được giải quyết bằng cách chọn một bên. Sự hòa hợp có thể được xây dựng thông qua thấu hiểu, tôn trọng và nhường nhịn.

5.7. Tiêu chí nghiệm thu Màn 1

Không thể mở gợi ý trước khi mini-game hoàn thành.

Trả lời sai không thu thập được món ăn.

Trả lời đúng thu thập đúng một vật phẩm.

Đặt sai món không làm mất vật phẩm.

Mỗi slot chỉ hoàn thành một lần.

Cảnh ký ức không xuất hiện khi chưa đặt đủ ba món.

Khi đủ điều kiện, cảnh ký ức được kích hoạt đúng một lần.

PHẦN VI — MÀN 2: PHÒNG THAY ĐỒ

6.1. Chủ đề

Chủ đề cảm xúc: Bản sắc, truyền thống, niềm tự hào và quyền tự xác định bản thân.

Phòng thay đồ lưu giữ những hình ảnh gắn với trang phục truyền thống Việt Nam và Hàn Quốc. Người chơi khám phá một bức tranh bị phủ bụi, từ đó nhìn lại hình ảnh của cha mẹ trong hai loại trang phục truyền thống.

6.2. Mini-game: Lau bụi bức tranh

Mục tiêu

Người chơi cần hoàn thành mini-game lau bụi để khám phá bức tranh và mở khóa gợi ý vị trí hai bộ trang phục.

Luồng logic

Người chơi khám phá phòng thay đồ.

Tìm bức tranh phủ bụi.

Tương tác để bắt đầu mini-game.

Thực hiện thao tác lau bụi theo cơ chế mini-game đã thiết kế.

Khi chưa hoàn thành, gợi ý trang phục vẫn bị khóa.

Khi hoàn thành, bức tranh được làm sạch và hình ảnh cha mặc Hanbok, mẹ mặc áo dài được hé lộ.

Hệ thống mở gợi ý tìm hai bộ trang phục.

Yêu cầu

Không mở gợi ý chỉ vì người chơi đã tương tác với tranh.

Không ghi nhận hoàn thành nếu mini-game chưa đạt điều kiện hoàn thành.

Không chạy lại hiệu ứng hoàn thành nhiều lần.

Sau khi hoàn thành, trạng thái bức tranh phải nhất quán với trạng thái mở gợi ý.

6.3. Vật phẩm bắt buộc

| ID minh họa | Vật phẩm | Vị trí đích | Ý nghĩa |

| --- | --- | --- | --- |

| Clothing_AoDai | Áo dài | Mannequin dành cho áo dài | Cội nguồn văn hóa Việt Nam |

| Clothing_Hanbok | Hanbok | Mannequin dành cho Hanbok | Cội nguồn văn hóa Hàn Quốc |

6.4. Logic thu thập trang phục

A. Áo dài

Mini-game lau bụi hoàn thành.

Gợi ý tìm áo dài được mở khóa.

Người chơi tìm đến vị trí trang phục.

Tương tác để mở câu hỏi văn hóa về áo dài.

Trả lời sai: tiếp tục thử lại, chưa thu thập.

Trả lời đúng: áo dài được thêm vào Inventory.

Người chơi chọn áo dài và đặt lên mannequin tương ứng.

Đặt đúng thì hệ thống xác nhận slot hoàn thành.

B. Hanbok

Mini-game hoàn thành.

Gợi ý tìm Hanbok được mở khóa.

Người chơi tìm và tương tác với Hanbok.

Hiển thị câu hỏi văn hóa về Hanbok.

Trả lời sai: chưa thu thập.

Trả lời đúng: Hanbok được thêm vào Inventory.

Người chơi đặt Hanbok lên mannequin tương ứng.

Đặt đúng thì slot hoàn thành.

6.5. Điều kiện hoàn thành Màn 2

Mini-game lau bụi đã hoàn thành.

Áo dài đã được thu thập hợp lệ và đặt đúng.

Hanbok đã được thu thập hợp lệ và đặt đúng.

Cả hai mannequin đã được xác nhận hoàn thành.

Khi tất cả điều kiện được thỏa mãn, kích hoạt cảnh ký ức.

6.6. Cảnh ký ức sau Màn 2

Bức tranh sau khi được làm sạch cho thấy cha mẹ Hạ Ân trong trang phục truyền thống.

Cảnh ký ức cần kết nối hình ảnh này với cảm giác tự hào về nguồn gốc và sự chấp nhận bản thân.

Thông điệp: Trang phục truyền thống là biểu hiện của văn hóa và nguồn gốc, không phải một khuôn mẫu buộc Hạ Ân phải lựa chọn một bên hoặc đáp ứng kỳ vọng của người khác.

6.7. Tiêu chí nghiệm thu Màn 2

Gợi ý không mở trước khi mini-game hoàn thành.

Mỗi trang phục có câu hỏi riêng.

Trả lời sai không thu thập trang phục.

Trả lời đúng chỉ thu thập một lần.

Áo dài không thể đặt vào slot Hanbok và ngược lại.

Đặt sai không làm mất vật phẩm.

Không kích hoạt cảnh ký ức khi một mannequin chưa hoàn thành.

PHẦN VII — MÀN 3: KHU VƯỜN LÚC HOÀNG HÔN

7.1. Chủ đề

Chủ đề cảm xúc: Chấp nhận bản thân, kết nối hai nguồn gốc và hòa giải với quá khứ.

Màn cuối đưa Hạ Ân đến khu vườn lúc hoàng hôn. Tại đây, người chơi giải quyết thử thách ghép bản đồ, tìm hai nửa ngọc bội và tạo ra cây cầu dẫn đến cuộc gặp gỡ với Hạ Ân lúc 8 tuổi.

7.2. Giai đoạn 1 — Tìm mảnh bản đồ

Người chơi khám phá khu vườn và tìm các mảnh bản đồ bị thất lạc.

Logic:

Các mảnh bản đồ được đặt trong môi trường.

Người chơi khám phá và thu thập theo cơ chế đã thiết kế.

Khi đủ các mảnh bắt buộc, hệ thống mở hoặc cho phép bắt đầu mini-game ghép bản đồ.

Điểm cần chốt: Các mảnh bản đồ có bắt buộc trả lời câu hỏi văn hóa để thu thập hay không. Quy tắc chung về câu hỏi văn hóa chỉ áp dụng cho các vật phẩm được xác định là có câu hỏi; không nên tự động thêm câu hỏi cho mọi mảnh ghép.

7.3. Giai đoạn 2 — Ghép bản đồ

Mục tiêu

Hoàn thành bản đồ để mở khóa gợi ý vị trí hai nửa ngọc bội.

Luồng logic

Người chơi mở giao diện ghép bản đồ khi đủ điều kiện.

Thực hiện thao tác ghép theo cơ chế đã thống nhất.

Nếu chưa ghép hoàn chỉnh, gợi ý ngọc bội vẫn bị khóa.

Khi bản đồ hoàn chỉnh, hệ thống ghi nhận mini-game hoàn thành.

Gợi ý vị trí hai nửa ngọc bội được mở khóa.

Không được coi mini-game hoàn thành chỉ vì người chơi đã mở giao diện hoặc đã bắt đầu kéo mảnh bản đồ.

7.4. Giai đoạn 3 — Thu thập hai nửa ngọc bội

| ID minh họa | Vật phẩm | Biểu tượng | Vai trò |

| --- | --- | --- | --- |

| Gem_LotusHalf | Nửa ngọc bội họa tiết sen | Hoa sen | Đại diện biểu tượng văn hóa Việt Nam |

| Gem_MugunghwaHalf | Nửa ngọc bội họa tiết Mugunghwa | Hoa Mugunghwa | Đại diện biểu tượng văn hóa Hàn Quốc |

Tên ID chỉ là đề xuất. Các ID cuối cùng cần được thống nhất trong project.

Nửa ngọc bội hoa sen

Bản đồ hoàn chỉnh.

Gợi ý vị trí nửa ngọc bội hoa sen được mở khóa.

Người chơi tìm và tương tác.

Hiển thị câu hỏi văn hóa tương ứng.

Trả lời sai: chưa thu thập.

Trả lời đúng: thêm nửa ngọc bội vào Inventory.

Nửa ngọc bội Mugunghwa

Bản đồ hoàn chỉnh.

Gợi ý vị trí nửa ngọc bội Mugunghwa được mở khóa.

Người chơi tìm và tương tác.

Hiển thị câu hỏi văn hóa tương ứng.

Trả lời sai: chưa thu thập.

Trả lời đúng: thêm nửa ngọc bội vào Inventory.

7.5. Giai đoạn 4 — Ghép ngọc bội

Khi người chơi đã thu thập đủ hai nửa ngọc bội:

Hệ thống kiểm tra hai nửa đã được thu thập hợp lệ.

Kích hoạt khả năng ghép ngọc bội.

Người chơi thực hiện thao tác ghép theo UI hoặc cơ chế đã thống nhất.

Khi ghép thành công, hai nửa tạo thành một ngọc bội hoàn chỉnh.

Trạng thái ghép được ghi nhận.

Không thể tạo nhiều bản sao của ngọc bội hoàn chỉnh.

Điểm cần chốt: Ngọc bội hoàn chỉnh có chiếm một ô Inventory hay không. Nếu có, nhóm cần xác định cách chuyển đổi từ hai vật phẩm thành một vật phẩm mới và cập nhật số ô Inventory.

7.6. Giai đoạn 5 — Đặt ngọc bội và tạo cây cầu

Người chơi mang ngọc bội hoàn chỉnh đến bệ đá.

Tương tác với bệ đá.

Đặt ngọc bội.

Hệ thống kiểm tra ngọc bội đã được ghép đúng.

Nếu điều kiện hợp lệ, xác nhận thao tác đặt.

Kích hoạt hiệu ứng cây cầu xuất hiện.

Mở đường đi đến khu vực gặp Hạ Ân lúc 8 tuổi.

Không tạo cây cầu nếu người chơi mới thu thập được một nửa ngọc bội hoặc chưa hoàn thành bước ghép.

7.7. Giai đoạn 6 — Gặp lại bản thân lúc 8 tuổi

Sau khi cây cầu xuất hiện:

Người chơi di chuyển qua cầu.

Đi đến điểm kích hoạt cuộc gặp gỡ.

Khóa điều khiển nhân vật khi cần.

Kích hoạt cảnh gặp Hạ Ân lúc 8 tuổi.

Thể hiện khoảnh khắc Hạ Ân trưởng thành ôm lấy bản thân nhỏ tuổi.

Chuyển sang đoạn kết.

Cuộc gặp gỡ này là kết quả của toàn bộ hành trình, không nên bị kích hoạt trước khi người chơi giải quyết xong thử thách cuối.

7.8. Tiêu chí nghiệm thu Màn 3

Không mở gợi ý ngọc bội trước khi bản đồ được ghép hoàn chỉnh.

Hai nửa ngọc bội có câu hỏi riêng.

Trả lời sai không cho phép thu thập.

Trả lời đúng không tạo vật phẩm trùng.

Không ghép được ngọc bội nếu thiếu một nửa.

Không tạo cầu nếu chưa đặt ngọc bội hoàn chỉnh.

Không kích hoạt cảnh kết nếu chưa đến đúng điểm sau cây cầu.

Các hiệu ứng hoàn thành chỉ chạy một lần.

PHẦN VIII — ĐOẠN KẾT VÀ HOÀN THÀNH TRÒ CHƠI

8.1. Cảnh kết

Sau khi gặp lại Hạ Ân lúc 8 tuổi, câu chuyện chuyển về hiện tại.

Hạ Ân nhận ra mình không phải là một mảnh ghép lỗi, cũng không cần chia đôi bản thân để được chấp nhận. Cô có thể gìn giữ cả hai nguồn gốc và tự xây dựng bản sắc của riêng mình.

Cuốn nhật ký được khôi phục. Trang cuối cùng vẫn còn trống để Hạ Ân tự viết lời kết.

8.2. Nội dung trang nhật ký cuối

“Nhiều năm qua, con đã luôn hoang mang vì nghĩ mình là một mảnh ghép lỗi, không vừa vặn với bức tranh của bất kỳ ai. Nhưng khi đi lại những bước chân này, con đã hiểu. Một trái tim không cần chia đôi để thuộc về hai quê hương. Con không phải là một nửa của nơi này, cộng với một nửa của nơi kia. Con là điểm giao thoa. Con là một nhịp cầu. Cảm ơn ba mẹ đã cho con hai cội nguồn, để thế giới của con rộng lớn gấp đôi.”

8.3. Logic kết thúc

Cảnh gặp Hạ Ân lúc 8 tuổi hoàn thành.

Chuyển sang cảnh hiện tại.

Hiển thị nhật ký được khôi phục.

Hiển thị trang cuối và nội dung lời kết.

Người chơi nhấn Continue hoặc nút tương ứng để tiếp tục.

Hạ Ân khép nhật ký.

Cảnh rời khỏi ngôi nhà cũ vào buổi sáng diễn ra.

Hiển thị trạng thái hoàn thành game.

Cần xác định rõ Continue ở mỗi đoạn là tiếp tục cảnh, sang trang hay chuyển scene. Không để cùng một nút có nhiều hành vi không rõ ràng.

8.4. Điều kiện kết thúc game

Game chỉ được đánh dấu hoàn thành khi đoạn kết đã đi đến trạng thái cuối cùng theo thiết kế.

Không được đánh dấu hoàn thành ngay khi cây cầu xuất hiện nếu cảnh gặp lại bản thân và phần nhật ký vẫn còn là nội dung bắt buộc.

PHẦN IX — KIẾN TRÚC HỆ THỐNG UNITY

9.1. Nguyên tắc kiến trúc

Một hệ thống cốt lõi chỉ nên có một nguồn quản lý trạng thái chính.

Không tạo các hệ thống riêng biệt cho từng màn nếu chúng có cùng trách nhiệm và có thể tái sử dụng.

Dữ liệu riêng của từng màn nên tách khỏi logic xử lý chung.

UI không nên tự quyết định một màn đã hoàn thành chỉ dựa trên việc người chơi nhấn nút.

Các thao tác quan trọng phải được xác nhận bởi hệ thống logic.

Tên class và API cuối cùng phải dựa trên code hiện tại và API_CONTRACT.md, không mặc định sử dụng tên ví dụ trong tài liệu này.

9.2. Các hệ thống logic đề xuất

A. Game Flow / Scene Management

Trách nhiệm:

Quản lý thứ tự các scene.

Chuyển từ menu đến màn chơi.

Chuyển giữa các màn.

Điều phối cảnh mở đầu và đoạn kết.

Tránh tải cùng một scene nhiều lần do sự kiện lặp.

B. Player Movement

Trách nhiệm:

Nhận input từ bàn phím hoặc joystick.

Di chuyển nhân vật.

Cho phép khóa và mở khóa điều khiển.

Không tự quyết định câu hỏi, thu thập vật phẩm hoặc điều kiện hoàn thành màn.

C. Player Interaction

Trách nhiệm:

Xác định đối tượng có thể tương tác trong phạm vi.

Xử lý phím E hoặc nút tương tác trên mobile.

Gửi yêu cầu tương tác đến hệ thống tương ứng.

Không tự thêm vật phẩm vào Inventory nếu vật phẩm yêu cầu xác nhận câu hỏi.

D. Mini-game System

Trách nhiệm:

Khởi tạo mini-game.

Theo dõi trạng thái đang chơi, hoàn thành hoặc chưa hoàn thành.

Thông báo hoàn thành hợp lệ.

Chặn việc hoàn thành nhiều lần.

Không trực tiếp thay thế toàn bộ hệ thống tiến độ của màn.

E. Hint System

Trách nhiệm:

Theo dõi điều kiện mở gợi ý.

Chỉ mở gợi ý khi mini-game liên quan hoàn thành.

Hiển thị gợi ý của từng vật phẩm.

Không tự thu thập vật phẩm.

F. Quiz System

Trách nhiệm:

Hiển thị câu hỏi và các lựa chọn.

Kiểm tra đáp án.

Trả về trạng thái đúng hoặc sai.

Cho phép thử lại khi sai.

Phát sự kiện trả lời đúng khi được xác nhận hợp lệ.

Không thêm bản sao vật phẩm khi cùng một câu trả lời được xử lý nhiều lần.

G. Inventory System

Trách nhiệm:

Lưu vật phẩm đã thu thập.

Quản lý số ô và trạng thái vật phẩm.

Xử lý thêm, chọn, tiêu thụ hoặc chuyển đổi vật phẩm.

Thông báo cho UI khi dữ liệu thay đổi.

H. Inventory UI

Trách nhiệm:

Hiển thị icon vật phẩm.

Hiển thị ô trống.

Hiển thị vật phẩm được chọn.

Phản ánh dữ liệu từ Inventory System.

UI không được là nơi duy nhất lưu dữ liệu về vật phẩm.

I. Placement System

Trách nhiệm:

Kiểm tra vật phẩm và slot có tương thích hay không.

Xác nhận đặt vật phẩm hợp lệ.

Giữ nguyên vật phẩm nếu thao tác sai.

Chặn đặt trùng vào slot đã hoàn thành.

Thông báo tiến độ khi đặt thành công.

J. Progress / Level System

Trách nhiệm:

Theo dõi các điều kiện bắt buộc của màn.

Xác nhận mini-game, thu thập, đặt vật phẩm và thử thách phụ.

Kiểm tra điều kiện hoàn thành tổng thể.

Phát sự kiện màn hoàn thành một lần.

K. Memory / Journal System

Trách nhiệm:

Kích hoạt cảnh ký ức sau khi màn hoàn thành.

Hiển thị nội dung ký ức.

Điều phối phần nhật ký.

Chuyển sang bước tiếp theo khi người chơi xác nhận.

L. Pause / Settings System

Trách nhiệm:

Tạm dừng và tiếp tục gameplay.

Khóa input thế giới khi pause.

Điều khiển các UI thiết lập.

Không thay đổi tiến độ màn khi chỉ mở hoặc đóng Pause Panel.

M. Audio System

Trách nhiệm:

Phát nhạc menu và nhạc của từng màn.

Phát âm thanh tương tác, thu thập, trả lời và hoàn thành nếu có.

Tránh phát chồng nhạc ngoài ý muốn khi chuyển scene.

Tuân theo trạng thái pause khi thiết kế yêu cầu.

Đây là danh sách trách nhiệm logic, không phải yêu cầu tạo mới toàn bộ các class tương ứng. Dự án đã có một số script như PlayerMovement, PlayerInteraction, FoodItem, FoodSlot, InventoryUI, LevelManager, MemorySceneController, JournalController và PauseController. Cần kiểm tra và mở rộng chúng khi phù hợp, không tạo bản sao song song mà chưa có lý do.

PHẦN X — QUẢN LÝ DỮ LIỆU THEO MÀN

10.1. Nguyên tắc

Logic xử lý chung nên tách khỏi dữ liệu nội dung của từng màn.

Ví dụ, hệ thống Quiz không nên có nhiều nhánh code đặc biệt chỉ để xử lý câu hỏi về kimchi, áo dài hoặc ngọc bội. Thay vào đó, mỗi câu hỏi cung cấp dữ liệu riêng, còn hệ thống Quiz xử lý cùng một quy trình.

10.2. Dữ liệu vật phẩm

Mỗi vật phẩm nên có các thông tin tương đương:

ID duy nhất.

Tên hiển thị.

Icon Inventory.

ID câu hỏi liên quan, nếu có.

ID gợi ý liên quan.

ID slot hợp lệ.

Trạng thái thu thập.

Trạng thái đặt hoặc tiêu thụ, nếu áp dụng.

10.3. Dữ liệu câu hỏi

Mỗi câu hỏi nên có:

ID câu hỏi.

Nội dung câu hỏi.

Các lựa chọn.

ID đáp án đúng.

Phản hồi khi trả lời sai.

Giải thích hoặc thông tin văn hóa.

ID vật phẩm được mở khóa khi trả lời đúng.

Không nên xác định đáp án đúng chỉ bằng vị trí lựa chọn, chẳng hạn luôn coi lựa chọn thứ nhất là đúng. Nên lưu ID đáp án hoặc một giá trị tương đương ổn định.

10.4. Dữ liệu mini-game

Mỗi mini-game nên có:

ID mini-game.

Scene hoặc đối tượng khởi tạo.

Điều kiện bắt đầu.

Điều kiện hoàn thành.

Trạng thái hoàn thành.

Các gợi ý được mở khi hoàn thành.

Chính sách tiếp tục hoặc chơi lại.

Cách khôi phục trạng thái nếu có lưu tiến trình.

10.5. Dữ liệu slot

Mỗi slot nên có:

ID slot.

ID vật phẩm được chấp nhận.

Hình ảnh hoặc vị trí hiển thị.

Trạng thái hoàn thành.

Quy tắc xử lý khi đặt đúng.

Quy tắc xử lý khi đặt sai.

10.6. Lợi ích

Cách tổ chức này giúp:

Thêm câu hỏi mà không phải viết lại toàn bộ Quiz System.

Thêm vật phẩm mà không sao chép logic thu thập.

Điều chỉnh nội dung từng màn mà không ảnh hưởng đến màn khác.

Dễ kiểm thử và bảo trì.

Giảm xung đột code khi hai người làm việc song song.

Có thể sử dụng ScriptableObject, cấu hình serializable hoặc phương án phù hợp với kiến trúc hiện có. Không cần chuyển toàn bộ dự án sang một mô hình dữ liệu mới chỉ vì tài liệu đề xuất.

PHẦN XI — QUẢN LÝ TRẠNG THÁI VÀ LƯU TIẾN TRÌNH

11.1. Các trạng thái cần theo dõi

Tùy mức độ lưu game mà nhóm chọn, các trạng thái có thể bao gồm:

Cảnh mở đầu đã hoàn thành hay chưa.

Màn hiện tại.

Mini-game đã hoàn thành hay chưa.

Gợi ý đã mở hay chưa.

Vật phẩm nào đã được thu thập.

Vật phẩm nào đang nằm trong Inventory.

Slot nào đã được hoàn thành.

Ngọc bội đã được ghép hay chưa.

Cây cầu đã xuất hiện hay chưa.

Cảnh ký ức nào đã hoàn thành.

Đoạn kết game đã hoàn thành hay chưa.

11.2. Trạng thái trong phiên chơi và lưu lâu dài

Cần phân biệt:

Trạng thái trong phiên chơi: Dùng khi người chơi đang ở một scene hoặc mini-game.

Trạng thái tiến trình: Dùng để tiếp tục từ một điểm trước đó sau khi chuyển scene hoặc đóng game.

Nếu phiên bản đầu chưa có tính năng lưu game, có thể quản lý trạng thái trong phiên và reset theo quy tắc đã thống nhất. Tuy nhiên, chuyển scene không được vô tình xóa trạng thái cần thiết để hoàn thành quy trình của màn hiện tại.

11.3. Quy tắc khôi phục

Nếu hỗ trợ tiếp tục game:

Không thu thập lại vật phẩm đã thu thập.

Không mở lại gợi ý theo cách gây mất đồng bộ với mini-game.

Không đặt lại slot đã hoàn thành.

Không kích hoạt lại cảnh ký ức đã hoàn thành ngoài chủ đích.

Không tạo nhiều bản sao ngọc bội sau khi ghép.

Quyết định cần chốt: Phiên bản thi đấu có cần lưu tiến trình qua lần đóng ứng dụng hay không. Tài liệu này chưa giả định tính năng đó đã được triển khai.

PHẦN XII — QUY TẮC UI/UX

12.1. Inventory UI

Hiển thị đúng số ô theo cấu hình.

Icon vật phẩm phải xuất hiện ngay sau khi thu thập thành công.

Khi chọn vật phẩm, phải có phản hồi trực quan.

Khi đặt đúng, UI phải phản ánh trạng thái mới.

Không để UI hiển thị một vật phẩm không tồn tại trong dữ liệu logic.

Không để thao tác chọn vật phẩm vô tình kích hoạt tương tác thế giới phía sau.

12.2. Quiz UI

Hiển thị rõ câu hỏi và lựa chọn.

Phản hồi đúng/sai phải dễ hiểu.

Trả lời sai phải cho phép thử lại.

Không đóng câu hỏi theo cách khiến người chơi có thể lấy vật phẩm dù chưa trả lời đúng.

Tránh nhấn liên tục tạo nhiều lần thu thập.

Câu hỏi dài cần được trình bày phù hợp với màn hình điện thoại.

12.3. Hint UI

Trạng thái khóa phải dễ nhận biết.

Gợi ý chỉ xuất hiện khi điều kiện mở đã thỏa mãn.

Gợi ý phải giúp người chơi định hướng chứ không thay thế hành động khám phá.

Khi chuyển trạng thái mini-game sang hoàn thành, UI gợi ý cần cập nhật đúng.

12.4. Placement UI

Slot phải thể hiện vị trí hoặc vật phẩm cần đặt.

Phản hồi khi đặt đúng và sai phải rõ ràng.

Không để một slot hoàn thành nhận thêm vật phẩm và làm tăng tiến độ lần nữa.

12.5. Mobile UI

Các nút phải dễ chạm.

Joystick và nút tương tác không che khuất thông tin quan trọng.

UI cần phù hợp với tỉ lệ màn hình khác nhau.

Cần thống nhất landscape 16:9 cho gameplay.

Splash Screen và Main Menu hiện có cần được kiểm tra để bảo đảm cùng định hướng màn hình với gameplay.

PHẦN XIII — QUY TẮC SCENE VÀ TÍCH HỢP DỰ ÁN

13.1. Nguyên tắc scene

Các scene cần có trách nhiệm rõ ràng. Ví dụ:

Splash / Main Menu: khởi đầu và điều hướng.

Opening: cảnh Hạ Ân mở hộp thời gian.

Map 1: căn bếp.

Map 2: phòng thay đồ.

Map 3: khu vườn.

Ending: đoạn kết, nếu nhóm chọn tách riêng.

Đây là cấu trúc tham khảo. Tên scene và cách chia scene thực tế phải thống nhất với project và build settings.

13.2. Quy tắc chuyển scene

Chỉ chuyển scene sau khi điều kiện tương ứng được xác nhận.

Không để nhiều nút hoặc sự kiện cùng chuyển scene.

Khi rời scene, cần giải phóng hoặc dừng các tác vụ phù hợp.

Các dữ liệu cần thiết cho màn tiếp theo phải được giữ lại.

Các scene phải được đăng ký đúng trong cấu hình build.

13.3. Phân chia công việc

Định hướng phân công đã bàn:

Thành viên A: Main Menu và Màn 1.

Thành viên B: Màn 2 và Màn 3.

Tuy nhiên, hệ thống dùng chung cần có người chịu trách nhiệm chính. Ví dụ:

Inventory: một người phụ trách logic chính.

Quiz: một người phụ trách logic chính.

Placement: một người phụ trách logic chính.

Level Progress: một người phụ trách logic chính.

Scene Flow: một người phụ trách logic chính.

Người còn lại sử dụng API đã thống nhất, không tự tạo một hệ thống song song chỉ vì cách sử dụng khác nhau.

13.4. Git workflow

Làm việc trên branch riêng nếu quy trình nhóm cho phép.

Không đồng thời sửa cùng một scene hoặc script cốt lõi mà chưa trao đổi.

Commit theo từng thay đổi có mục đích rõ ràng.

Kiểm tra git status trước khi commit.

Giữ lại file .meta của Unity.

Không đưa các thư mục sinh tự động như Library, Temp, Obj, Logs và UserSettings lên repository.

Pull hoặc đồng bộ thay đổi mới trước khi bắt đầu công việc có nguy cơ xung đột.

Khi sửa API chung, thông báo người đang sử dụng API đó.

Kiểm thử scene sau khi merge.

13.5. Quy tắc sử dụng AI coding assistant

AI phải:

Đọc file hiện có trước khi viết lại.

Không giả định một class hoặc hàm đã tồn tại nếu chưa kiểm tra.

Không tự tạo class trùng chức năng với hệ thống hiện có.

Không tự đổi tên các public API mà scene khác đang sử dụng.

Không tự thay đổi quy tắc gameplay đã thống nhất.

Không sửa file ngoài phạm vi được giao nếu không cần thiết.

Báo rõ file nào được thêm, sửa hoặc xóa.

Nêu các bước cấu hình trong Unity Inspector khi cần.

Đưa ra cách kiểm thử sau khi thay đổi.

Không tuyên bố code đã chạy thành công khi chưa thực sự kiểm thử.

PHẦN XIV — DANH SÁCH KIỂM THỬ TOÀN GAME

14.1. Kiểm thử mini-game

Mở màn nhưng chưa hoàn thành mini-game: gợi ý vẫn khóa.

Bắt đầu mini-game rồi thoát giữa chừng: không được ghi nhận hoàn thành.

Hoàn thành mini-game: gợi ý được mở.

Sự kiện hoàn thành bị gọi lại: không phát sinh tiến độ trùng.

Chuyển scene và quay lại theo chính sách đã chọn: trạng thái nhất quán.

14.2. Kiểm thử câu hỏi

Tương tác với vật phẩm mở đúng câu hỏi.

Chọn sai: không thu thập.

Chọn sai nhiều lần: vẫn có thể thử tiếp.

Chọn đúng: thu thập vật phẩm.

Nhấn đáp án liên tục: không tạo bản sao vật phẩm.

Câu hỏi của vật phẩm này không làm thu thập vật phẩm khác.

UI không bị kẹt khi người chơi trả lời sai.

14.3. Kiểm thử Inventory

Icon xuất hiện sau khi thu thập.

Chọn vật phẩm cập nhật trạng thái lựa chọn.

Không vượt giới hạn số ô.

Không thêm trùng vật phẩm.

Dữ liệu và UI đồng bộ.

Không làm mất vật phẩm khi thao tác đặt sai.

14.4. Kiểm thử placement

Đặt đúng vật phẩm vào slot tương ứng: thành công.

Đặt sai vật phẩm: thất bại, vật phẩm vẫn được giữ.

Slot đã hoàn thành không tăng tiến độ lần nữa.

Đặt đủ vật phẩm nhưng còn thiếu điều kiện khác: chưa hoàn thành màn.

Hoàn thành tất cả điều kiện: kích hoạt ký ức đúng một lần.

14.5. Kiểm thử từng màn

Màn 1

Mini-game mèo.

Gợi ý ba món ăn.

Câu hỏi riêng cho từng món.

Thu thập ba món.

Đặt đủ ba món.

Kích hoạt ký ức gia đình.

Màn 2

Mini-game lau bụi tranh.

Bức tranh được làm sạch.

Gợi ý áo dài và Hanbok.

Câu hỏi riêng cho từng trang phục.

Đặt đúng hai mannequin.

Kích hoạt ký ức.

Màn 3

Thu thập các mảnh bản đồ.

Ghép bản đồ.

Mở gợi ý hai nửa ngọc bội.

Trả lời đúng hai câu hỏi.

Ghép ngọc bội.

Đặt lên bệ đá.

Cây cầu xuất hiện.

Gặp Hạ Ân lúc 8 tuổi.

Kích hoạt đoạn kết.

14.6. Kiểm thử toàn game

Cảnh mở đầu xuất hiện đúng.

Chuyển màn theo đúng thứ tự.

Nhạc không bị phát chồng.

Pause và Resume hoạt động.

Không thể điều khiển nhân vật phía sau UI modal khi bị khóa.

Không có lỗi tham chiếu giữa các scene.

Không có lỗi khi nhấn nhanh nhiều lần.

Game chạy bằng điều khiển mobile.

Build Android không phụ thuộc kết nối Internet.

Định hướng màn hình nhất quán.

PHẦN XV — NHỮNG QUYẾT ĐỊNH CẦN CHỐT TRƯỚC KHI TÍCH HỢP

Các điểm dưới đây chưa được xác nhận đầy đủ trong brief. Nhóm cần chốt trước khi xem chúng là yêu cầu chính thức.

Inventory: Khi đặt đúng, vật phẩm biến mất khỏi Inventory hay vẫn hiển thị như vật phẩm đã đặt?

Inventory đầy: Xử lý thế nào nếu số vật phẩm vượt quá số ô? Các màn hiện tại có cần cho phép vượt giới hạn không?

Câu hỏi văn hóa: Màn 1, Màn 2 và hai nửa ngọc bội đã có bộ câu hỏi, đáp án và phần giải thích chính thức chưa?

Mini-game mèo: Chính xác người chơi thực hiện thao tác gì, và điều kiện thắng là gì?

Mini-game lau tranh: Điều kiện hoàn thành, tiến trình lau và hiệu ứng khi hoàn thành là gì?

Mini-game ghép bản đồ: Số mảnh, thao tác ghép và điều kiện xác nhận hoàn chỉnh là gì?

Mảnh bản đồ: Có cần câu hỏi văn hóa để thu thập hay chỉ thu thập theo cơ chế mini-game?

Ghép ngọc bội: Ghép tự động khi có đủ hai nửa hay cần mở UI và thao tác thủ công?

Ngọc bội hoàn chỉnh: Có chiếm một ô Inventory không?

Lưu tiến trình: Game có hỗ trợ tiếp tục sau khi đóng ứng dụng không, hay chỉ giữ tiến trình trong phiên chơi?

Chuyển scene: Cảnh ký ức và nhật ký nằm trong scene riêng hay là UI/panel trong scene hiện tại?

Pause: Có cho phép pause trong mini-game và cảnh ký ức không?

Âm thanh: Có cần âm thanh riêng cho trả lời đúng/sai, đặt vật phẩm, mở gợi ý và chuyển cảnh không?

Định hướng màn hình: Splash Screen, Main Menu và gameplay phải thống nhất landscape hay có ngoại lệ?

API chung: Các class, hàm, tham số và sự kiện cần được đối chiếu với code thực tế để hoàn thiện API_CONTRACT.md.

Không nên tự quyết định các mục này trong code rồi để thành viên còn lại phát hiện sau khi tích hợp.

PHẦN XVI — ĐỊNH NGHĨA HOÀN THÀNH DỰ ÁN

Game được xem là đạt yêu cầu logic khi:

Người chơi có thể hoàn thành toàn bộ hành trình từ cảnh mở đầu đến đoạn kết.

Ba màn tuân thủ đúng trình tự mini-game, mở gợi ý, thu thập, câu hỏi và placement.

Không có cách bỏ qua câu hỏi hoặc mini-game để vượt tiến độ.

Không có lỗi nhân bản vật phẩm hoặc tăng tiến độ nhiều lần.

UI phản ánh đúng trạng thái của các hệ thống.

Các màn sử dụng chung hệ thống cốt lõi theo kiến trúc đã thống nhất.

Nội dung từng màn có thể điều chỉnh mà không phải viết lại toàn bộ logic chung.

Các cảnh ký ức chỉ xuất hiện khi điều kiện của màn đã hoàn thành.

Đoạn kết truyền tải được thông điệp về bản sắc, gia đình và sự hòa hợp giữa hai cội nguồn.

Game chạy được trên Android theo định hướng đã chốt.

Kết luận

Journey of Memories không chỉ là game tìm vật phẩm. Gameplay là phương tiện để người chơi từng bước khôi phục những ký ức và khám phá ý nghĩa của chúng.

Ba màn có thể sử dụng các mini-game khác nhau, nhưng cùng chia sẻ một nguyên tắc: người chơi phải thực sự hoàn thành thử thách, hiểu manh mối, trả lời câu hỏi và đặt vật phẩm đúng chỗ thì mới tiến được đến ký ức tiếp theo.

Toàn bộ kiến trúc code, dữ liệu và UI cần phục vụ nguyên tắc này, đồng thời giữ cho ba màn nhất quán về cách chơi và đủ linh hoạt để nhóm có thể phát triển song song.
