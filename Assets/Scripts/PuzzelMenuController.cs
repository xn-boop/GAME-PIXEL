using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý giao diện và logic màn hình "Puzzel" (Menu Ghép Hình Ký Ức).
/// Hoạt động hoàn toàn độc lập (Standalone), không liên kết hay làm thay đổi bất kỳ scene nào khác trong game.
/// Thiết kế tinh gọn chuẩn Pixel Art:
/// - Nền động 30 khung hình (UIFrameAnimator) hoa bay trong gió.
/// - Nhấn trực tiếp vào khung tranh hoặc phím Space / Enter để bắt đầu thử thách ghép hình 3x3.
/// - Minigame ghép hình trượt 3x3 (Sliding Puzzle) ngay trên vị trí bức tranh gia đình.
/// - Bộ tạo âm thanh procedural 8-bit/16-bit retro (click, bước đi, fanfare chiến thắng) không phụ thuộc audio asset ngoài.
/// - Bảng Cài đặt âm lượng nhạc nền (BGM) & hiệu ứng (SFX) kích hoạt bằng phím Escape.
/// - Hiệu ứng chuyển cảnh ScreenFadeOverlay mượt mà.
/// </summary>
public class PuzzelMenuController : MonoBehaviour
{
    #region Diary Story Content Constants
    // Trang 1: Tết ở Việt Nam
    public const string DIARY_PAGE1_RIGHT_CONTENT = 
@"31/01/2014

Hôm nay là mùng Một Tết. Năm nay cả nhà
tôi quyết định ở Việt Nam đón giao thừa.

Nhà cửa mấy hôm nay rộn ràng hẳn lên, hoa
mai nở vàng trước sân, trong bếp thơm mùi
bánh tét và thịt kho. Đêm qua cả nhà cùng
xem pháo hoa, chúc nhau năm mới bình an.

Sáng nay Hạ Ân được mặc quần áo mới, theo
bố mẹ đi chúc Tết ông bà và nhận những
bao lì xì đỏ. Nhìn con bé vui vẻ chạy quanh
nhà, tôi chợt thấy Tết chẳng cần gì cầu kỳ,
chỉ cần cả nhà được sum vầy bên nhau là đủ.";

    public const string DIARY_PAGE1_LEFT_CAPTION = "Chúc mừng năm mới";

    // Trang 2: Hội An, Việt Nam
    public const string DIARY_PAGE2_RIGHT_CONTENT = 
@"23/03/2014 – Hội An, Việt Nam

Hội An đẹp hơn tôi tưởng. Ban ngày, những
bức tường vàng, mái ngói cũ và giàn hoa
giấy khiến phố cổ trông thật yên bình. Cả
nhà tranh thủ đi dạo, ăn cao lầu và bánh mì
Hội An. Hạ Ân thích món bánh mì đến mức cứ
đòi ăn thêm.

Tối đến, cả con phố sáng rực bởi những
chiếc đèn lồng đủ màu. Ánh đèn phản chiếu
xuống dòng sông Hoài trông đẹp vô cùng. Hạ
Ân cứ mải mê ngắm nhìn, còn tôi tranh thủ
chụp thật nhiều ảnh cho cả nhà.

Hội An không quá ồn ào hay náo nhiệt, nhưng
có một nét duyên rất riêng. Đi rồi mới hiểu
vì sao người ta lại nhớ nơi này đến vậy.";

    public const string DIARY_PAGE2_LEFT_CAPTION = "Phố cổ mùa đèn lồng";

    // Trang 3: Seoul, Hàn Quốc (Tháp N Seoul, công viên NamSan)
    public const string DIARY_PAGE3_RIGHT_CONTENT = 
@"05/04/2014 – Tháp N Seoul, công viên NamSan

Hôm nay cả nhà cùng đến Namsan ngắm
hoa anh đào. May mắn là hoa đang nở rộ,
khắp con đường lên núi đều phủ một màu
hồng rất đẹp. Hạ Ân cứ thích thú ngắm
những cánh hoa bay trong gió, thỉnh thoảng
lại kéo bố mẹ dừng lại để chụp ảnh.

Sau đó, cả nhà đi cáp treo lên tháp
N Seoul. Từ trên cao, Seoul hiện ra rộng
lớn và nhộn nhịp. Xung quanh tháp có rất
nhiều ổ khóa tình yêu đủ màu sắc. Chúng
tôi cũng dành thời gian đi dạo, chụp vài
tấm ảnh rồi cùng nhau ngắm cảnh.

Một ngày đi chơi thật vui, lại có thêm bao
nhiêu khoảnh khắc đáng nhớ.";

    public const string DIARY_PAGE3_LEFT_CAPTION = "Mùa hoa anh đào";

    // Trang 4: Cung điện Gyeongbokgung, Seoul, Hàn Quốc
    public const string DIARY_PAGE4_RIGHT_CONTENT = 
@"07/04/2014 – Cung điện Gyeongbokgung

Lần này quay lại Seoul, cả nhà ghé thăm
cung điện Gyeongbokgung. Nơi đây rộng hơn
tôi tưởng, với những mái ngói cong, cổng
cung điện và khoảng sân lớn mang đậm
nét kiến trúc Hàn Quốc xưa.

Hạ Ân thích thú ngắm những bộ hanbok đủ
màu sắc. Con bé còn hỏi tôi vì sao mái nhà
lại cong như vậy, khiến tôi cũng phải bật
cười vì không biết giải thích thế nào.

Cả nhà chụp được một tấm ảnh trước cung
điện. Nhìn con trong ảnh, tôi chợt nhận ra
Hạ Ân đã lớn hơn nhiều rồi. Thời gian
trôi nhanh thật!";

    public const string DIARY_PAGE4_LEFT_CAPTION = "Dấu ấn hoàng cung Seoul";

    // Trang 5: Tết Seollal, Hàn Quốc
    public const string DIARY_PAGE5_RIGHT_CONTENT = 
@"19/02/2015 – Tết Seollal, Hàn Quốc

Năm nay, gia đình tôi quyết định về quê
chồng ở Hàn Quốc để đón Tết Seollal.
Nhà cửa những ngày này nhộn nhịp hẳn lên,
mọi người cùng nhau chuẩn bị thức ăn và
dọn dẹp nhà cửa. Mẹ chồng còn hướng
dẫn tôi chuẩn bị món canh bánh gạo
tteokguk – món ăn không thể thiếu trong
ngày đầu năm mới.

Sáng nay, cả nhà cùng mặc hanbok, thực
hiện nghi lễ cúi lạy chúc Tết người lớn rồi
quây quần bên mâm cơm đầu năm. Hạ Ân
được ông bà lì xì, vui vẻ ríu rít suốt cả buổi.
Một cái Tết thật ấm cúng khi được sum họp
cùng gia đình bên nội, không biết bên
Việt Nam giờ này Tết như nào rồi nhỉ!";

    public const string DIARY_PAGE5_LEFT_CAPTION = "Tết đầu tiên ở quê nội";

    // Trang 6: Vịnh Hạ Long, Việt Nam
    public const string DIARY_PAGE6_RIGHT_CONTENT = 
@"18/04/2015 – Vịnh Hạ Long

Hôm nay cả nhà cùng đi thuyền khám phá
Vịnh Hạ Long. Những hòn đảo đá lớn nhỏ
nhấp nhô giữa làn nước xanh biếc, nhìn
từ xa đẹp như một bức tranh. Thuyền lướt
qua những vách núi cao và hang đá kỳ thú,
khiến cả nhà cứ mải mê ngắm cảnh.

Hạ Ân hào hứng chỉ trỏ những hòn đảo
phía xa, thỉnh thoảng lại gọi bố mẹ xem.
Có lúc gió thổi mạnh làm tóc cả nhà rối
tung, bố vẫn tranh thủ chụp ảnh, còn tôi
vừa cười vừa giữ đồ đạc. Trên thuyền,
cả nhà còn cùng nhau thưởng thức ít trái
cây và ngắm cảnh biển.

Chuyến đi hơi mệt nhưng đầy ắp tiếng
cười. Tôi nghĩ sau này cả nhà sẽ còn
nhắc lại ngày hôm nay nhiều lắm!";

    public const string DIARY_PAGE6_LEFT_CAPTION = "Vịnh biển đẹp tựa tranh vẽ";

    // Trang 7: Busan, Hàn Quốc
    public const string DIARY_PAGE7_RIGHT_CONTENT = 
@"13/10/2015 – Busan, Hàn Quốc

Busan mang một vẻ đẹp rất riêng, khác
hẳn với Seoul. Thành phố nằm bên bờ biển,
có những bãi cát dài, làn nước xanh trong
và những con đường ven biển lộng gió.
Hôm nay cả nhà cùng đi dạo, ngắm biển
và chụp ảnh. Khung cảnh nơi đây thật
thoáng đãng, dễ chịu.

Hạ Ân thích nhất là nhặt những chiếc
vỏ sò nhỏ trên cát. Con bé nhặt được chiếc
nào cũng đem khoe mẹ, còn nhất quyết đòi
mang về làm kỷ niệm. Chiều đến, cả nhà
cùng thưởng thức một bữa hải sản ngon
lành trước khi trở về khách sạn.

Chuyến đi lần này không quá bận rộn, nhưng
tôi rất thích những khoảnh khắc cả nhà
được thong thả bên nhau như thế này.";

    public const string DIARY_PAGE7_LEFT_CAPTION = "Lưu giữ chút nắng gió Busan";

    public const int TOTAL_DIARY_PAGES = 7;

    public const string DIARY_LEFT_PAGE_CONTENT = "";

    // Giữ alias tương thích ngược
    public const string DIARY_RIGHT_PAGE_CONTENT = DIARY_PAGE1_RIGHT_CONTENT;

    public const string FULL_DIARY_CONTENT = 
@"31/01/2014

Hôm nay là mùng Một Tết. Năm nay cả nhà tôi quyết định ở Việt Nam đón giao thừa.

Nhà cửa mấy hôm nay rộn ràng hẳn lên, hoa mai nở vàng trước sân, trong bếp thơm mùi bánh tét và thịt kho. Đêm qua cả nhà cùng xem pháo hoa, chúc nhau năm mới bình an.

Sáng nay Hạ Ân được mặc quần áo mới, theo bố mẹ đi chúc Tết ông bà và nhận những bao lì xì đỏ. Nhìn con bé vui vẻ chạy quanh nhà, tôi chợt thấy Tết chẳng cần gì cầu kỳ, chỉ cần cả nhà được sum vầy bên nhau là đủ.

---

23/03/2014 – Hội An, Việt Nam

Hội An đẹp hơn tôi tưởng. Ban ngày, những bức tường vàng, mái ngói cũ và giàn hoa giấy khiến phố cổ trông thật yên bình. Cả nhà tranh thủ đi dạo, ăn cao lầu và bánh mì Hội An. Hạ Ân thích món bánh mì đến mức cứ đòi ăn thêm.

Tối đến, cả con phố sáng rực bởi những chiếc đèn lồng đủ màu. Ánh đèn phản chiếu xuống dòng sông Hoài trông đẹp vô cùng. Hạ Ân cứ mải mê ngắm nhìn, còn tôi tranh thủ chụp thật nhiều ảnh cho cả nhà.

Hội An không quá ồn ào hay náo nhiệt, nhưng có một nét duyên rất riêng. Đi rồi mới hiểu vì sao người ta lại nhớ nơi này đến vậy.

---

05/04/2014 – Tháp N Seoul, công viên NamSan

Hôm nay cả nhà cùng đến Namsan ngắm hoa anh đào. May mắn là hoa đang nở rộ, khắp con đường lên núi đều phủ một màu hồng rất đẹp. Hạ Ân cứ thích thú ngắm những cánh hoa bay trong gió, thỉnh thoảng lại kéo bố mẹ dừng lại để chụp ảnh.

Sau đó, cả nhà đi cáp treo lên tháp N Seoul. Từ trên cao, Seoul hiện ra rộng lớn và nhộn nhịp. Xung quanh tháp có rất nhiều ổ khóa tình yêu đủ màu sắc. Chúng tôi cũng dành thời gian đi dạo, chụp vài tấm ảnh rồi cùng nhau ngắm cảnh.

Một ngày đi chơi thật vui, lại có thêm bao nhiêu khoảnh khắc đáng nhớ.

---

07/04/2014 – Cung điện Gyeongbokgung

Lần này quay lại Seoul, cả nhà ghé thăm cung điện Gyeongbokgung. Nơi đây rộng hơn tôi tưởng, với những mái ngói cong, cổng cung điện và khoảng sân lớn mang đậm nét kiến trúc Hàn Quốc xưa.

Hạ Ân thích thú ngắm những bộ hanbok đủ màu sắc. Con bé còn hỏi tôi vì sao mái nhà lại cong như vậy, khiến tôi cũng phải bật cười vì không biết giải thích thế nào.

Cả nhà chụp được một tấm ảnh trước cung điện. Nhìn con trong ảnh, tôi chợt nhận ra Hạ Ân đã lớn hơn nhiều rồi. Thời gian trôi nhanh thật!

---

19/02/2015 – Tết Seollal, Hàn Quốc

Năm nay, gia đình tôi quyết định về quê chồng ở Hàn Quốc để đón Tết Seollal. Nhà cửa những ngày này nhộn nhịp hẳn lên, mọi người cùng nhau chuẩn bị thức ăn và dọn dẹp nhà cửa. Mẹ chồng còn hướng dẫn tôi chuẩn bị món canh bánh gạo tteokguk – món ăn không thể thiếu trong ngày đầu năm mới.

Sáng nay, cả nhà cùng mặc hanbok, thực hiện nghi lễ cúi lạy chúc Tết người lớn rồi quây quần bên mâm cơm đầu năm. Hạ Ân được ông bà lì xì, vui vẻ ríu rít suốt cả buổi. Một cái Tết thật ấm cúng khi được sum họp cùng gia đình bên nội, không biết bên Việt Nam giờ này Tết như nào rồi nhỉ!

---

18/04/2015 – Vịnh Hạ Long

Hôm nay cả nhà cùng đi thuyền khám phá Vịnh Hạ Long. Những hòn đảo đá lớn nhỏ nhấp nhô giữa làn nước xanh biếc, nhìn từ xa đẹp như một bức tranh. Thuyền lướt qua những vách núi cao và hang đá kỳ thú, khiến cả nhà cứ mải mê ngắm cảnh.

Hạ Ân hào hứng chỉ trỏ những hòn đảo phía xa, thỉnh thoảng lại gọi bố mẹ xem. Có lúc gió thổi mạnh làm tóc cả nhà rối tung, bố vẫn tranh thủ chụp ảnh, còn tôi vừa cười vừa giữ đồ đạc. Trên thuyền, cả nhà còn cùng nhau thưởng thức ít trái cây và ngắm cảnh biển.

Chuyến đi hơi mệt nhưng đầy ắp tiếng cười. Tôi nghĩ sau này cả nhà sẽ còn nhắc lại ngày hôm nay nhiều lắm!

---

13/10/2015 – Busan, Hàn Quốc

Busan mang một vẻ đẹp rất riêng, khác hẳn với Seoul. Thành phố nằm bên bờ biển, có những bãi cát dài, làn nước xanh trong và những con đường ven biển lộng gió. Hôm nay cả nhà cùng đi dạo, ngắm biển và chụp ảnh. Khung cảnh nơi đây thật thoáng đãng, dễ chịu.

Hạ Ân thích nhất là nhặt những chiếc vỏ sò nhỏ trên cát. Con bé nhặt được chiếc nào cũng đem khoe mẹ, còn nhất quyết đòi mang về làm kỷ niệm. Chiều đến, cả nhà cùng thưởng thức một bữa hải sản ngon lành trước khi trở về khách sạn.

Chuyến đi lần này không quá bận rộn, nhưng tôi rất thích những khoảnh khắc cả nhà được thong thả bên nhau như thế này.";
    #endregion

    [Header("Standalone Configuration")]
    [Tooltip("Chế độ màn riêng lẻ: Đảm bảo không chuyển hay liên kết với bất kỳ scene nào hiện tại")]
    public bool isStandaloneMode = true;

    [Tooltip("Thời gian hiệu ứng làm mờ màn hình (giây)")]
    public float fadeDuration = 0.35f;

    [Tooltip("Âm lượng hiệu ứng âm thanh SFX")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.85f;

    [Header("UI Main Panels")]
    [Tooltip("Khung tiêu đề phía trên")]
    public GameObject headerPanel;

    [Tooltip("Bảng điều khiển menu bên trái (nếu có)")]
    public GameObject mainControlPanel;

    [Tooltip("Bảng thẻ thông tin ký ức bên phải")]
    public GameObject rightInfoPanel;

    [Tooltip("Bảng Cài đặt âm thanh")]
    public GameObject settingsPanel;

    [Tooltip("Bảng Bộ sưu tập tranh ghép ký ức")]
    public GameObject puzzleCollectionPanel;

    [Tooltip("Bảng trò chơi Ghép hình tương tác")]
    public GameObject puzzleBoardPanel;

    [Tooltip("Nút thoát chế độ xem tranh động")]
    public GameObject exitViewArtButton;

    [Header("Interactive Triggers")]
    [Tooltip("Nút tương tác bấm trực tiếp vào bức tranh ở giữa để bắt đầu ghép")]
    public Button portraitButton;

    [Header("Puzzle Progression Slices (Map 1 -> Map 2 -> Map 3)")]
    [Tooltip("Container chứa 3 lớp phủ che các mảnh ảnh")]
    public GameObject slicesContainer;

    [Tooltip("Lớp phủ mảnh 1 (Bên trái 1/3)")]
    public CanvasGroup sliceOverlay1;

    [Tooltip("Lớp phủ mảnh 2 (Ở giữa 1/3)")]
    public CanvasGroup sliceOverlay2;

    [Tooltip("Lớp phủ mảnh 3 (Bên phải 1/3)")]
    public CanvasGroup sliceOverlay3;

    [Tooltip("Thời gian hiệu ứng hiện tranh ra (mặc định 4.0s)")]
    public float sliceRevealDuration = 4.0f;

    [Tooltip("Nút Tiếp tục chuyển màn sau khi xem xong mảnh ảnh mới")]
    public Button nextStageButton;

    [Tooltip("Text trên nút Tiếp tục")]
    public Text nextStageButtonText;

    [Tooltip("Banner thông báo mở khóa mảnh ghép mới")]
    public GameObject sliceUnlockBanner;

    [Tooltip("Text mô tả mảnh ghép mới mở khóa")]
    public Text sliceUnlockText;

    [Header("Memory Chest Animation")]
    [Tooltip("12 Frame hoạt ảnh mở nắp rương")]
    public Sprite[] chestOpeningFrames;

    [Tooltip("Image hiển thị hoạt ảnh mở nắp rương")]
    public Image chestAnimationImage;

    [Tooltip("Nút tương tác chạm/click vào chiếc rương trên bàn")]
    public Button chestButton;

    [Tooltip("Nút tương tác chạm vào cuốn nhật ký trong rương")]
    public Button diaryInChestButton;

    [Tooltip("Huy hiệu chỉ dẫn chạm vào cuốn nhật ký")]
    public GameObject diaryPromptBadge;

    [Tooltip("Hình ảnh chiếc rương khi đã mở nắp")]
    public GameObject chestOpenOverlay;

    [Tooltip("Hiệu ứng ánh sáng hào quang phát sáng quanh rương")]
    public GameObject chestGlowEffect;

    [Tooltip("Tia sáng thần kỳ chiếu lên từ chiếc rương khi mở")]
    public GameObject chestRaysEffect;

    [Tooltip("Huy hiệu / Mũi tên nhấp nháy chỉ dẫn chạm vào rương")]
    public GameObject chestPromptBadge;

    [Header("Diary Floating & Opening Animation")]
    [Tooltip("12 Frame hoạt ảnh mở cuốn nhật ký")]
    public Sprite[] bookOpeningFrames;

    [Tooltip("Image hiển thị cuốn nhật ký bay và mở trang")]
    public Image floatingDiaryImage;

    [Header("Diary Page Content UI")]
    [Tooltip("Khung chứa 2 trang nội dung văn bản nhật ký trên cuốn nhật ký")]
    public GameObject diaryContentContainer;

    [Tooltip("Văn bản trang bên trái cuốn nhật ký")]
    public Text diaryLeftPageText;

    [Tooltip("Văn bản trang bên phải cuốn nhật ký")]
    public Text diaryRightPageText;

    [Tooltip("Bức tranh kỷ niệm Tết chèn vào trang 1 bên trái")]
    public Sprite diaryTetPhotoSprite;

    [Tooltip("Bức tranh kỷ niệm Hội An chèn vào trang 2 bên trái")]
    public Sprite diaryHoiAnPhotoSprite;

    [Tooltip("Bức tranh kỷ niệm Seoul hoa anh đào chèn vào trang 3 bên trái")]
    public Sprite diarySeoulPhotoSprite;

    [Tooltip("Bức tranh kỷ niệm Cung điện Gyeongbokgung chèn vào trang 4 bên trái")]
    public Sprite diaryGyeongbokgungPhotoSprite;

    [Tooltip("Bức tranh kỷ niệm Tết Seollal Hàn Quốc chèn vào trang 5 bên trái")]
    public Sprite diarySeollalPhotoSprite;

    [Tooltip("Bức tranh kỷ niệm Vịnh Hạ Long chèn vào trang 6 bên trái")]
    public Sprite diaryHaLongPhotoSprite;

    [Tooltip("Bức tranh kỷ niệm Busan chèn vào trang 7 bên trái")]
    public Sprite diaryBusanPhotoSprite;

    [Tooltip("12 Frame hoạt ảnh lật trang sách từ phải sang trái")]
    public Sprite[] pageFlipFrames;

    [Tooltip("Image hiển thị bức ảnh Tết trang bên trái")]
    public Image diaryLeftPhoto;

    [Tooltip("Text chú thích bên dưới bức ảnh (Chúc mừng năm mới)")]
    public Text diaryLeftPhotoCaption;

    [Tooltip("Vùng / Nút chạm để lật sang trang sau")]
    public Button nextDiaryPageButton;

    [Tooltip("Vùng / Nút chạm để lật quay lại trang trước")]
    public Button prevDiaryPageButton;

    [Tooltip("Gợi ý nhấp nháy chuyển sang trang")]
    public GameObject nextPagePrompt;

    [Tooltip("Gợi ý nhấp nháy quay lại trang trước")]
    public GameObject prevPagePrompt;

    private int currentDiaryPage = 1;
    private bool isPageFlipping = false;

    [Tooltip("Vầng hào quang phát sáng nhẹ quanh cuốn nhật ký")]
    public GameObject diaryGlowEffect;

    [Tooltip("Vòng xoay tia sáng hào quang thiên giới sau cuốn nhật ký")]
    public GameObject diaryRaysEffect;

    [Tooltip("Hiệu ứng các ngôi sao lấp lánh quanh cuốn nhật ký")]
    public GameObject diarySparklesEffect;

    [Tooltip("Khung thông điệp / Bức thư kết thúc game khi mở rương")]
    public GameObject endingLetterModal;

    [Tooltip("Nút hoàn thành game trên bảng thông điệp kết thúc")]
    public Button finishGameButton;

    [Header("Diary Navigation Buttons (Thanh điều hướng chuyển trang)")]
    [Tooltip("Nút mũi tên lùi trang trên thanh điều hướng dưới cuốn nhật ký")]
    public Button navPrevButton;

    [Tooltip("Nút mũi tên tiến trang trên thanh điều hướng dưới cuốn nhật ký")]
    public Button navNextButton;

    [Tooltip("Text mũi tên lùi trang")]
    public Text navPrevText;

    [Tooltip("Text mũi tên tiến trang")]
    public Text navNextText;

    [Tooltip("Text ở giữa thanh điều hướng nhật ký")]
    public Text navCenterText;

    [Header("Background Music & Audio")]
    [Tooltip("AudioSource phát nhạc nền BGM")]
    public AudioSource bgmAudioSource;

    [Tooltip("Clip nhạc nền cho màn nhật ký ký ức")]
    public AudioClip backgroundMusicClip;

    [Tooltip("Text nội dung trên huy hiệu chỉ dẫn rương")]
    public Text chestPromptBadgeText;

    [Tooltip("Nút Bắt đầu ghép hình (dự phòng)")]
    public Button playPuzzleButton;

    [Tooltip("Nút Mở bộ sưu tập tranh ký ức (dự phòng)")]
    public Button puzzleListButton;

    [Tooltip("Nút Xem tranh động toàn cảnh (dự phòng)")]
    public Button viewArtButton;

    [Tooltip("Nút Mở bảng cài đặt (dự phòng)")]
    public Button settingsButton;

    [Tooltip("Nút Thoát (dự phòng)")]
    public Button quitButton;

    [Header("Settings Panel UI")]
    [Tooltip("Slider âm lượng nhạc nền (BGM)")]
    public Slider volumeSlider;

    [Tooltip("Slider âm lượng hiệu ứng (SFX)")]
    public Slider sfxSlider;

    [Tooltip("Nút Đóng bảng cài đặt")]
    public Button closeSettingsButton;

    [Header("Puzzle Collection Panel UI")]
    [Tooltip("Nút Đóng bảng bộ sưu tập")]
    public Button closeCollectionButton;

    [Tooltip("Danh sách nút chọn tranh trong bộ sưu tập")]
    public Button[] collectionItemButtons;

    [Header("Puzzle Board Minigame")]
    [Tooltip("9 nút ô vuông mảnh ghép 3x3")]
    public Button[] puzzleTileButtons;

    [Tooltip("9 hình ảnh Image tương ứng các ô vuông 3x3")]
    public Image[] puzzleTileImages;

    [Tooltip("9 Sprite gốc mảnh ghép gia đình (tile_0 đến tile_8)")]
    public Sprite[] tileSprites;

    [Tooltip("Nút Xáo trộn mảnh ghép")]
    public Button shuffleButton;

    [Tooltip("Nút Tự động giải / Hoàn thành tranh")]
    public Button autoSolveButton;

    [Tooltip("Nút Gợi ý nước đi tiếp theo")]
    public Button hintButton;

    [Tooltip("Nút Đóng bảng ghép hình")]
    public Button closePuzzleBoardButton;

    [Tooltip("Text đếm số bước đi")]
    public Text moveCountText;

    [Tooltip("Text hiển thị đồng hồ thời gian")]
    public Text timerText;

    [Tooltip("Text trạng thái trong bảng ghép hình")]
    public Text puzzleStatusText;

    [Tooltip("Banner thông báo chiến thắng khi ghép xong")]
    public GameObject victoryBanner;

    [Header("Background & Effects")]
    [Tooltip("Component diễn hoạt 30 khung hình nền hoa")]
    public UIFrameAnimator bgAnimator;

    [Tooltip("Text thông báo phản hồi người dùng ở dưới cùng")]
    public Text statusText;

    [Tooltip("Lớp phủ mờ dần màn hình khi vào/thoát cảnh")]
    public CanvasGroup screenFadeOverlay;

    [Tooltip("AudioSource phát âm thanh UI")]
    public AudioSource audioSource;

    // Trạng thái minigame ghép hình 3x3
    private int[] currentGrid = new int[9]; // currentGrid[slotIndex] = pieceIndex (8 là ô trống)
    private int emptySlot = 8;
    private int moveCount = 0;
    private float puzzleTimer = 0f;
    private bool isPuzzleActive = false;
    private bool isPuzzleSolved = true;
    private bool isTransitioning = false;
    private int currentCompletedMapIndex = 1;
    private Coroutine statusCoroutine;

    // Bộ nhớ cache âm thanh procedural
    private AudioClip clickChime;
    private AudioClip slideChime;
    private AudioClip winChime;
    private AudioClip shuffleChime;
    private AudioClip magicChime;
    private AudioClip chestOpenChime;
    private AudioClip bookLiftChime;
    private AudioClip pageFlipChime;

    // Trạng thái tương tác chiếc rương thời gian và cuốn nhật ký
    private bool isChestUnlocked = false;
    private bool isChestOpened = false;
    private bool isDiaryFloating = false;
    private bool isDiaryOpened = false;
    private Vector3 chestBadgeBasePos = new Vector3(-683f, 20f, 0f);
    private Vector3 diaryBadgeBasePos = new Vector3(-683f, 20f, 0f);
    private Vector2 chestCenterPos = new Vector2(-683f, -130f);
    private Vector2 bookInChestPos = new Vector2(-683f, -120f);

    private void Awake()
    {
        EnsureSlicesFitFrameAndColor();
        InitializeAudio();

        // Gán sự kiện nút bấm vào khung tranh
        if (portraitButton != null)
        {
            portraitButton.onClick.RemoveListener(OpenPuzzleBoard);
            portraitButton.onClick.AddListener(OpenPuzzleBoard);
        }

        // Gán sự kiện các nút Menu chính (nếu có)
        if (playPuzzleButton != null)
        {
            playPuzzleButton.onClick.RemoveListener(OpenPuzzleBoard);
            playPuzzleButton.onClick.AddListener(OpenPuzzleBoard);
        }

        if (puzzleListButton != null)
        {
            puzzleListButton.onClick.RemoveListener(OpenPuzzleCollection);
            puzzleListButton.onClick.AddListener(OpenPuzzleCollection);
        }

        if (viewArtButton != null)
        {
            viewArtButton.onClick.RemoveListener(EnterViewArtMode);
            viewArtButton.onClick.AddListener(EnterViewArtMode);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Gán sự kiện bảng Cài đặt
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        // Gán sự kiện bảng Bộ sưu tập
        if (closeCollectionButton != null)
        {
            closeCollectionButton.onClick.RemoveListener(ClosePuzzleCollection);
            closeCollectionButton.onClick.AddListener(ClosePuzzleCollection);
        }

        if (collectionItemButtons != null)
        {
            for (int i = 0; i < collectionItemButtons.Length; i++)
            {
                int itemIdx = i;
                if (collectionItemButtons[i] != null)
                {
                    collectionItemButtons[i].onClick.RemoveAllListeners();
                    collectionItemButtons[i].onClick.AddListener(() => OnCollectionItemClicked(itemIdx));
                }
            }
        }

        // Gán sự kiện bảng Minigame Puzzle
        if (closePuzzleBoardButton != null)
        {
            closePuzzleBoardButton.onClick.RemoveListener(ClosePuzzleBoard);
            closePuzzleBoardButton.onClick.AddListener(ClosePuzzleBoard);
        }

        if (shuffleButton != null)
        {
            shuffleButton.onClick.RemoveListener(ShufflePuzzle);
            shuffleButton.onClick.AddListener(ShufflePuzzle);
        }

        if (autoSolveButton != null)
        {
            autoSolveButton.onClick.RemoveListener(AutoSolvePuzzle);
            autoSolveButton.onClick.AddListener(AutoSolvePuzzle);
        }

        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(ProvideHint);
            hintButton.onClick.AddListener(ProvideHint);
        }

        // Gán sự kiện 9 ô mảnh ghép
        if (puzzleTileButtons != null)
        {
            for (int i = 0; i < puzzleTileButtons.Length; i++)
            {
                int slotIndex = i;
                if (puzzleTileButtons[i] != null)
                {
                    puzzleTileButtons[i].onClick.RemoveAllListeners();
                    puzzleTileButtons[i].onClick.AddListener(() => OnTileClicked(slotIndex));
                }
            }
        }

        // Nút thoát chế độ xem tranh động
        if (exitViewArtButton != null)
        {
            Button evBtn = exitViewArtButton.GetComponent<Button>();
            if (evBtn != null)
            {
                evBtn.onClick.RemoveListener(ExitViewArtMode);
                evBtn.onClick.AddListener(ExitViewArtMode);
            }
        }

        // Gán sự kiện nút Tiếp tục chuyển màn
        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(OnDefaultNextStageClicked);
            nextStageButton.onClick.AddListener(OnDefaultNextStageClicked);
        }

        // Gán sự kiện nút Chiếc rương thời gian, Nhật ký trong rương và Nút kết thúc game
        if (chestButton != null)
        {
            chestButton.onClick.RemoveListener(OnChestClicked);
            chestButton.onClick.AddListener(OnChestClicked);
        }

        if (diaryInChestButton != null)
        {
            diaryInChestButton.onClick.RemoveListener(OnDiaryInChestClicked);
            diaryInChestButton.onClick.AddListener(OnDiaryInChestClicked);
        }

        if (finishGameButton != null)
        {
            finishGameButton.onClick.RemoveListener(OnFinishGameButtonClicked);
            finishGameButton.onClick.AddListener(OnFinishGameButtonClicked);
        }
    }

    private void Start()
    {
        // Đặt trạng thái ban đầu của các panel
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (puzzleCollectionPanel != null) puzzleCollectionPanel.SetActive(false);
        if (puzzleBoardPanel != null) puzzleBoardPanel.SetActive(false);
        if (exitViewArtButton != null) exitViewArtButton.SetActive(false);
        if (victoryBanner != null) victoryBanner.SetActive(false);

        // Đặt trạng thái ban đầu của Chiếc rương thời gian và Cuốn nhật ký
        if (chestAnimationImage != null)
        {
            chestAnimationImage.gameObject.SetActive(false);
            if (chestOpeningFrames != null && chestOpeningFrames.Length > 0)
            {
                chestAnimationImage.sprite = chestOpeningFrames[0];
            }
        }
        if (chestOpenOverlay != null) chestOpenOverlay.SetActive(false);
        if (chestGlowEffect != null) chestGlowEffect.SetActive(false);
        if (chestRaysEffect != null) chestRaysEffect.SetActive(false);
        if (endingLetterModal != null) endingLetterModal.SetActive(false);
        if (diaryInChestButton != null) diaryInChestButton.gameObject.SetActive(false);
        if (floatingDiaryImage != null) floatingDiaryImage.gameObject.SetActive(false);
        if (diaryGlowEffect != null) diaryGlowEffect.SetActive(false);
        if (diaryRaysEffect != null) diaryRaysEffect.SetActive(false);
        if (diarySparklesEffect != null) diarySparklesEffect.SetActive(false);
        if (chestPromptBadge != null)
        {
            chestBadgeBasePos = chestPromptBadge.transform.localPosition;
            chestPromptBadge.SetActive(false);
        }
        if (diaryPromptBadge != null)
        {
            diaryBadgeBasePos = diaryPromptBadge.transform.localPosition;
            diaryPromptBadge.SetActive(false);
        }
        if (diaryContentContainer != null) diaryContentContainer.SetActive(false);
        EnsureDiaryContentUI();

        // Khởi tạo slider âm lượng
        if (volumeSlider != null) volumeSlider.value = 0.75f;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        // Khởi tạo trạng thái lưới puzzle
        ResetGridToSolved();

        // Mở màn hình mượt mà (Fade In)
        if (screenFadeOverlay != null)
        {
            StartCoroutine(FadeInRoutine());
        }

        // Khởi tạo các lớp phủ mảnh ghép ký ức
        int recentMap = PlayerPrefs.GetInt("Puzzle_RecentCompletedMap", 0);
        int unlockedPieces = PlayerPrefs.GetInt("Puzzle_UnlockedPieces", 0);

        if (recentMap > 0)
        {
            currentCompletedMapIndex = recentMap;
            // Vừa hoàn thành một màn chơi: Kích hoạt hiệu ứng xuất hiện dần mảnh ảnh tương ứng
            StartCoroutine(PlaySliceRevealSequence(recentMap));
        }
        else
        {
            // Mở scene trực tiếp hoặc tự do:
            if (unlockedPieces <= 0)
            {
                // Mặc định hiển thị tranh hoàn chỉnh như ban đầu
                SetSlicesAlpha(0f, 0f, 0f);
            }
            else
            {
                // Hiển thị trạng thái theo số mảnh đã mở khóa:
                float a1 = (unlockedPieces >= 1) ? 0f : 1f;
                float a2 = (unlockedPieces >= 2) ? 0f : 1f;
                float a3 = (unlockedPieces >= 3) ? 0f : 1f;
                SetSlicesAlpha(a1, a2, a3);
            }

            if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);
            if (sliceUnlockBanner != null) sliceUnlockBanner.SetActive(false);
        }

        ShowStatus("Nhấn vào khung tranh hoặc bấm phím Space để bắt đầu ghép hình!", 4f);
    }

    private void Update()
    {
        if (isTransitioning) return;

        // Sync BGM with global mute
        if (bgmAudioSource != null && AudioManager.Instance != null)
        {
            if (AudioManager.Instance.IsMuted && bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Pause();
            }
            else if (!AudioManager.Instance.IsMuted && !bgmAudioSource.isPlaying && backgroundMusicClip != null)
            {
                bgmAudioSource.UnPause();
            }
        }

        // Phím tắt kiểm thử nhanh cho QA / Editor:
        // Phím 1: Xem hiệu ứng mở mảnh 1 (sau Map 1)
        // Phím 2: Xem hiệu ứng mở mảnh 2 (sau Map 2)
        // Phím 3: Xem hiệu ứng mở mảnh 3 & hoàn thành tranh (sau Map 3)
        // Phím 0: Đặt lại tranh hoàn chỉnh đầy đủ
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentCompletedMapIndex = 1;
            StartCoroutine(PlaySliceRevealSequence(1));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentCompletedMapIndex = 2;
            StartCoroutine(PlaySliceRevealSequence(2));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentCompletedMapIndex = 3;
            StartCoroutine(PlaySliceRevealSequence(3));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetSlicesAlpha(0f, 0f, 0f);
            if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);
            UnlockChestInteraction();
            ShowStatus("Đã hiển thị tranh hoàn chỉnh và mở khóa chiếc rương thời gian!", 3f);
        }

        // Bắt sự kiện click chuột hoặc chạm màn hình điện thoại vào ô Tiếp tục chuyển màn
        if (nextStageButton != null && nextStageButton.gameObject.activeSelf && !isTransitioning)
        {
            bool hasNextStageInput = false;
            Vector2 inputPos = Vector2.zero;

            if (Input.GetMouseButtonDown(0))
            {
                hasNextStageInput = true;
                inputPos = Input.mousePosition;
            }
            else if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    hasNextStageInput = true;
                    inputPos = t.position;
                }
            }

            if (hasNextStageInput)
            {
                RectTransform rt = nextStageButton.GetComponent<RectTransform>();
                if (rt != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, inputPos, null, out Vector2 localMousePos))
                {
                    if (rt.rect.Contains(localMousePos))
                    {
                        OnNextStageClicked(currentCompletedMapIndex);
                        return;
                    }
                }
            }
        }

        // Bắt sự kiện chạm/nhấp vào Chiếc rương thời gian trên điện thoại và PC
        if (isChestUnlocked && !isChestOpened && !isTransitioning)
        {
            bool hasChestInput = false;
            Vector2 chestInputPos = Vector2.zero;

            if (Input.GetMouseButtonDown(0))
            {
                hasChestInput = true;
                chestInputPos = Input.mousePosition;
            }
            else if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    hasChestInput = true;
                    chestInputPos = t.position;
                }
            }

            if (hasChestInput && chestButton != null)
            {
                RectTransform chestRt = chestButton.GetComponent<RectTransform>();
                if (chestRt != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(chestRt, chestInputPos, null, out Vector2 localChestPos))
                {
                    if (chestRt.rect.Contains(localChestPos))
                    {
                        OnChestClicked();
                        return;
                    }
                }
            }

            // Phím tắt C trên bàn phím để mở rương kiểm thử
            if (Input.GetKeyDown(KeyCode.C))
            {
                OnChestClicked();
                return;
            }
        }

        // Bắt sự kiện chạm/nhấp vào Cuốn nhật ký trong rương trên điện thoại và PC
        if (isChestOpened && !isDiaryFloating && !isDiaryOpened && !isTransitioning)
        {
            bool hasDiaryInput = false;
            Vector2 diaryInputPos = Vector2.zero;

            if (Input.GetMouseButtonDown(0))
            {
                hasDiaryInput = true;
                diaryInputPos = Input.mousePosition;
            }
            else if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    hasDiaryInput = true;
                    diaryInputPos = t.position;
                }
            }

            if (hasDiaryInput && diaryInChestButton != null && diaryInChestButton.gameObject.activeSelf)
            {
                RectTransform diaryRt = diaryInChestButton.GetComponent<RectTransform>();
                if (diaryRt != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(diaryRt, diaryInputPos, null, out Vector2 localDiaryPos))
                {
                    if (diaryRt.rect.Contains(localDiaryPos))
                    {
                        OnDiaryInChestClicked();
                        return;
                    }
                }
            }

            // Phím tắt B trên bàn phím để kích hoạt mở nhật ký kiểm thử
            if (Input.GetKeyDown(KeyCode.B))
            {
                OnDiaryInChestClicked();
                return;
            }
        }

        // Bắt sự kiện chạm/nhấp vào nút Kết thúc game (Về menu chính)
        if (finishGameButton != null && finishGameButton.gameObject.activeSelf && !isTransitioning)
        {
            bool hasFinishInput = false;
            Vector2 finishInputPos = Vector2.zero;

            if (Input.GetMouseButtonDown(0))
            {
                hasFinishInput = true;
                finishInputPos = Input.mousePosition;
            }
            else if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    hasFinishInput = true;
                    finishInputPos = t.position;
                }
            }

            if (hasFinishInput)
            {
                RectTransform finishRt = finishGameButton.GetComponent<RectTransform>();
                if (finishRt != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(finishRt, finishInputPos, null, out Vector2 localFinishPos))
                {
                    if (finishRt.rect.Contains(localFinishPos))
                    {
                        if (currentDiaryPage < TOTAL_DIARY_PAGES)
                        {
                            // Nhấn nửa bên trái -> quay lại trang sau (trang trước)
                            if (localFinishPos.x < -30f)
                            {
                                OnClickPrevDiaryPage();
                            }
                            // Nhấn nửa bên phải -> chuyển sang trang tiếp theo
                            else
                            {
                                OnClickNextDiaryPage();
                            }
                        }
                        else
                        {
                            // Ở trang cuối: nếu bấm nút ◄ bên trái thì quay lại trang trước
                            if (localFinishPos.x < -140f)
                            {
                                OnClickPrevDiaryPage();
                            }
                            else
                            {
                                OnFinishGameClicked();
                            }
                        }
                        return;
                    }
                }
            }
        }

        // Hoạt ảnh nhấp nhô nhẹ của huy hiệu chỉ dẫn rương
        if (chestPromptBadge != null && chestPromptBadge.activeSelf)
        {
            float bounce = Mathf.Sin(Time.unscaledTime * 5f) * 6f;
            chestPromptBadge.transform.localPosition = chestBadgeBasePos + new Vector3(0f, bounce, 0f);
        }

        // Hoạt ảnh nhấp nhô nhẹ của huy hiệu chỉ dẫn nhật ký
        if (diaryPromptBadge != null && diaryPromptBadge.activeSelf)
        {
            float bounce = Mathf.Sin(Time.unscaledTime * 5f) * 6f;
            diaryPromptBadge.transform.localPosition = diaryBadgeBasePos + new Vector3(0f, bounce, 0f);
        }

        // Hoạt ảnh trôi bồng bềnh và hào quang của cuốn nhật ký khi đã mở hoàn toàn
        if (floatingDiaryImage != null && floatingDiaryImage.gameObject.activeSelf && isDiaryOpened)
        {
            float hoverY = Mathf.Sin(Time.unscaledTime * 2.2f) * 6f;
            floatingDiaryImage.rectTransform.anchoredPosition = new Vector2(0f, hoverY);

            if (diaryGlowEffect != null && diaryGlowEffect.activeSelf)
            {
                CanvasGroup glowCg = diaryGlowEffect.GetComponent<CanvasGroup>();
                if (glowCg != null)
                {
                    glowCg.alpha = 0.65f + 0.20f * Mathf.Sin(Time.unscaledTime * 2.8f);
                }
            }

            if (diaryRaysEffect != null && diaryRaysEffect.activeSelf)
            {
                diaryRaysEffect.transform.Rotate(0f, 0f, -10f * Time.unscaledDeltaTime);
                CanvasGroup raysCg = diaryRaysEffect.GetComponent<CanvasGroup>();
                if (raysCg != null)
                {
                    raysCg.alpha = 0.60f + 0.18f * Mathf.Sin(Time.unscaledTime * 3.2f);
                }
            }

            if (diarySparklesEffect != null && diarySparklesEffect.activeSelf)
            {
                CanvasGroup sparkCg = diarySparklesEffect.GetComponent<CanvasGroup>();
                if (sparkCg != null)
                {
                    sparkCg.alpha = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 4.5f);
                }
            }

            // Hiệu ứng nhấp nháy nhẹ cho gợi ý lật trang nhật ký
            if (nextPagePrompt != null && nextPagePrompt.activeSelf)
            {
                Text pTxt = nextPagePrompt.GetComponent<Text>();
                if (pTxt != null)
                {
                    Color c = pTxt.color;
                    c.a = 0.55f + 0.35f * Mathf.Sin(Time.unscaledTime * 3.5f);
                    pTxt.color = c;
                }
            }
            if (prevPagePrompt != null && prevPagePrompt.activeSelf)
            {
                Text pTxt = prevPagePrompt.GetComponent<Text>();
                if (pTxt != null)
                {
                    Color c = pTxt.color;
                    c.a = 0.55f + 0.35f * Mathf.Sin(Time.unscaledTime * 3.5f);
                    pTxt.color = c;
                }
            }

            // Hỗ trợ phím tắt điều hướng lật trang sách
            if (!isPageFlipping)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.PageDown))
                {
                    OnClickNextDiaryPage();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.PageUp))
                {
                    OnClickPrevDiaryPage();
                }
            }
        }

        // Hoạt ảnh hào quang phát sáng nhẹ của chiếc rương
        if (chestGlowEffect != null && chestGlowEffect.activeSelf)
        {
            CanvasGroup glowCg = chestGlowEffect.GetComponent<CanvasGroup>();
            if (glowCg != null)
            {
                glowCg.alpha = 0.60f + 0.25f * Mathf.Sin(Time.unscaledTime * 3.5f);
            }
        }

        if (chestRaysEffect != null && chestRaysEffect.activeSelf)
        {
            CanvasGroup chestRaysCg = chestRaysEffect.GetComponent<CanvasGroup>();
            if (chestRaysCg != null)
            {
                chestRaysCg.alpha = 0.70f + 0.25f * Mathf.Sin(Time.unscaledTime * 3.8f);
            }
        }

        // Đồng hồ đếm giờ khi đang chơi puzzle
        if (isPuzzleActive && !isPuzzleSolved)
        {
            puzzleTimer += Time.unscaledDeltaTime;
            UpdateTimerDisplay();
        }

        // Phím Escape: Đóng panel đang mở hoặc mở Cài đặt
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitViewArtButton != null && exitViewArtButton.activeSelf)
            {
                ExitViewArtMode();
            }
            else if (puzzleBoardPanel != null && puzzleBoardPanel.activeSelf)
            {
                ClosePuzzleBoard();
            }
            else if (puzzleCollectionPanel != null && puzzleCollectionPanel.activeSelf)
            {
                ClosePuzzleCollection();
            }
            else if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }

        // Phím Space hoặc Return: Bắt đầu chơi ghép hình nếu chưa mở panel nào và không đang hiển thị nút Tiếp tục
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            bool anyOpen = (puzzleBoardPanel != null && puzzleBoardPanel.activeSelf) ||
                           (settingsPanel != null && settingsPanel.activeSelf) ||
                           (puzzleCollectionPanel != null && puzzleCollectionPanel.activeSelf) ||
                           (nextStageButton != null && nextStageButton.gameObject.activeSelf);
            if (!anyOpen)
            {
                OpenPuzzleBoard();
            }
        }

        // Điều khiển phím mũi tên hoặc WASD khi đang chơi ghép hình
        if (isPuzzleActive && !isPuzzleSolved)
        {
            int row = emptySlot / 3;
            int col = emptySlot % 3;

            // Bấm Mũi tên lên / W -> Đẩy ô phía dưới lên ô trống
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (row < 2) OnTileClicked((row + 1) * 3 + col);
            }
            // Bấm Mũi tên xuống / S -> Đẩy ô phía trên xuống ô trống
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (row > 0) OnTileClicked((row - 1) * 3 + col);
            }
            // Bấm Mũi tên trái / A -> Đẩy ô bên phải sang ô trống
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                if (col < 2) OnTileClicked(row * 3 + (col + 1));
            }
            // Bấm Mũi tên phải / D -> Đẩy ô bên trái sang ô trống
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                if (col > 0) OnTileClicked(row * 3 + (col - 1));
            }
        }
    }

    #region Main Actions

    public void OpenPuzzleBoard()
    {
        PlaySound(clickChime);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (puzzleCollectionPanel != null) puzzleCollectionPanel.SetActive(false);

        if (puzzleBoardPanel != null)
        {
            puzzleBoardPanel.SetActive(true);
            isPuzzleActive = true;
            ShufflePuzzle();
            ShowStatus("Đã bắt đầu ghép tranh 3x3! Click hoặc dùng phím WASD / Mũi tên để di chuyển.", 3.5f);
        }
    }

    public void ClosePuzzleBoard()
    {
        PlaySound(clickChime);
        isPuzzleActive = false;
        if (puzzleBoardPanel != null)
        {
            puzzleBoardPanel.SetActive(false);
        }
        ShowStatus("Đã trở về màn hình chính Puzzel", 2f);
    }

    public void OpenPuzzleCollection()
    {
        PlaySound(clickChime);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (puzzleBoardPanel != null) puzzleBoardPanel.SetActive(false);

        if (puzzleCollectionPanel != null)
        {
            puzzleCollectionPanel.SetActive(true);
            ShowStatus("Khám phá các bức tranh và biểu tượng cội nguồn", 2.5f);
        }
    }

    public void ClosePuzzleCollection()
    {
        PlaySound(clickChime);
        if (puzzleCollectionPanel != null)
        {
            puzzleCollectionPanel.SetActive(false);
            ShowStatus("Đã đóng Bộ sưu tập tranh", 2f);
        }
    }

    public void EnterViewArtMode()
    {
        PlaySound(clickChime);
        if (headerPanel != null) headerPanel.SetActive(false);
        if (mainControlPanel != null) mainControlPanel.SetActive(false);
        if (rightInfoPanel != null) rightInfoPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (puzzleCollectionPanel != null) puzzleCollectionPanel.SetActive(false);
        if (puzzleBoardPanel != null) puzzleBoardPanel.SetActive(false);

        if (exitViewArtButton != null) exitViewArtButton.SetActive(true);
        ShowStatus("Chế độ xem tranh toàn cảnh. Nhấn ESC hoặc nút 'TRỞ LẠI MENU' để tiếp tục.", 3.5f);
    }

    public void ExitViewArtMode()
    {
        PlaySound(clickChime);
        if (headerPanel != null) headerPanel.SetActive(true);
        if (mainControlPanel != null) mainControlPanel.SetActive(true);
        if (rightInfoPanel != null) rightInfoPanel.SetActive(true);
        if (exitViewArtButton != null) exitViewArtButton.SetActive(false);

        ShowStatus("Đã trở lại Menu Puzzel", 2f);
    }

    public void OpenSettings()
    {
        PlaySound(clickChime);
        if (puzzleCollectionPanel != null) puzzleCollectionPanel.SetActive(false);
        if (puzzleBoardPanel != null) puzzleBoardPanel.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            ShowStatus("Đã mở bảng Cài đặt", 2f);
        }
    }

    public void CloseSettings()
    {
        PlaySound(clickChime);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            ShowStatus("Đã đóng bảng Cài đặt", 2f);
        }
    }

    public void OnQuitClicked()
    {
        PlaySound(clickChime);
        ShowStatus("Đang thoát màn Puzzel...", 2f);
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOutRoutine());

        Debug.Log("[PuzzelMenu] Đang thoát Menu Puzzel...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Puzzle Minigame Logic (3x3 Sliding Tile)

    private void ResetGridToSolved()
    {
        for (int i = 0; i < 9; i++)
        {
            currentGrid[i] = i;
        }
        emptySlot = 8;
        moveCount = 0;
        puzzleTimer = 0f;
        isPuzzleSolved = true;
        UpdateGridVisuals();
        UpdateMoveText();
        UpdateTimerDisplay();
        if (victoryBanner != null) victoryBanner.SetActive(false);
    }

    public void ShufflePuzzle()
    {
        PlaySound(shuffleChime);
        ResetGridToSolved();

        // Xáo trộn 36 bước đi ngẫu nhiên hợp lệ từ vị trí đích để luôn đảm bảo giải được (Solvable)
        int lastMoved = -1;
        System.Random rnd = new System.Random();

        for (int step = 0; step < 36; step++)
        {
            List<int> validNeighbors = GetValidNeighbors(emptySlot);
            validNeighbors.Remove(lastMoved);
            if (validNeighbors.Count == 0) validNeighbors = GetValidNeighbors(emptySlot);

            int chosen = validNeighbors[rnd.Next(validNeighbors.Count)];
            SwapSlots(chosen, emptySlot);
            lastMoved = emptySlot;
            emptySlot = chosen;
        }

        moveCount = 0;
        puzzleTimer = 0f;
        isPuzzleSolved = false;
        UpdateGridVisuals();
        UpdateMoveText();
        UpdateTimerDisplay();
        if (victoryBanner != null) victoryBanner.SetActive(false);
        if (puzzleStatusText != null) puzzleStatusText.text = "Trượt các mảnh ghép để tái hiện bức tranh sum vầy!";
    }

    public void OnTileClicked(int slotIndex)
    {
        if (isPuzzleSolved || slotIndex < 0 || slotIndex >= 9) return;

        if (IsAdjacent(slotIndex, emptySlot))
        {
            PlaySound(slideChime);
            SwapSlots(slotIndex, emptySlot);
            emptySlot = slotIndex;
            moveCount++;
            UpdateGridVisuals();
            UpdateMoveText();

            // Kiểm tra xem đã hoàn thành chưa
            if (CheckIsSolved())
            {
                OnPuzzleCompleted();
            }
        }
    }

    public void AutoSolvePuzzle()
    {
        PlaySound(clickChime);
        ResetGridToSolved();
        OnPuzzleCompleted();
        ShowStatus("Đã tự động ghép hoàn chỉnh bức tranh ký ức!", 3f);
    }

    public void ProvideHint()
    {
        PlaySound(clickChime);
        if (isPuzzleSolved)
        {
            ShowStatus("Bức tranh đã hoàn chỉnh!", 2f);
            return;
        }

        List<int> neighbors = GetValidNeighbors(emptySlot);
        if (neighbors.Count > 0)
        {
            int hintSlot = neighbors[0];
            int pieceIdx = currentGrid[hintSlot];
            if (puzzleStatusText != null)
            {
                puzzleStatusText.text = $"Gợi ý: Thử nhấn mảnh #{pieceIdx + 1} tại vị trí hàng {(hintSlot / 3) + 1}, cột {(hintSlot % 3) + 1}!";
            }
            ShowStatus($"Gợi ý: Trượt mảnh #{pieceIdx + 1} vào ô trống", 2.5f);
        }
    }

    private bool IsAdjacent(int slotA, int slotB)
    {
        int rA = slotA / 3, cA = slotA % 3;
        int rB = slotB / 3, cB = slotB % 3;
        return (Mathf.Abs(rA - rB) + Mathf.Abs(cA - cB)) == 1;
    }

    private List<int> GetValidNeighbors(int slot)
    {
        List<int> list = new List<int>();
        int r = slot / 3, c = slot % 3;
        if (r > 0) list.Add((r - 1) * 3 + c);
        if (r < 2) list.Add((r + 1) * 3 + c);
        if (c > 0) list.Add(r * 3 + (c - 1));
        if (c < 2) list.Add(r * 3 + (c + 1));
        return list;
    }

    private void SwapSlots(int slotA, int slotB)
    {
        int temp = currentGrid[slotA];
        currentGrid[slotA] = currentGrid[slotB];
        currentGrid[slotB] = temp;
    }

    private bool CheckIsSolved()
    {
        for (int i = 0; i < 9; i++)
        {
            if (currentGrid[i] != i) return false;
        }
        return true;
    }

    private void OnPuzzleCompleted()
    {
        isPuzzleSolved = true;
        PlaySound(winChime);

        // Hiển thị lại đầy đủ cả 9 mảnh (kể cả ô trống thứ 8)
        UpdateGridVisuals(showAllTiles: true);

        if (victoryBanner != null) victoryBanner.SetActive(true);
        if (puzzleStatusText != null)
        {
            puzzleStatusText.text = $"XUẤT SẮC! Hoàn thành sau {moveCount} bước ({FormatTime(puzzleTimer)})!";
        }
        ShowStatus("Tuyệt vời! Bạn đã ghép trọn vẹn bức tranh sum vầy của gia đình Hạ Ân!", 4.5f);
    }

    private void UpdateGridVisuals(bool showAllTiles = false)
    {
        if (puzzleTileImages == null || puzzleTileImages.Length < 9) return;

        for (int slot = 0; slot < 9; slot++)
        {
            int piece = currentGrid[slot];
            Image img = puzzleTileImages[slot];
            if (img == null) continue;

            if (piece == 8 && !showAllTiles && !isPuzzleSolved)
            {
                img.color = new Color(0.12f, 0.08f, 0.06f, 0.92f);
                img.sprite = null;
            }
            else
            {
                img.color = Color.white;
                if (tileSprites != null && piece >= 0 && piece < tileSprites.Length)
                {
                    img.sprite = tileSprites[piece];
                }
            }
        }
    }

    private void UpdateMoveText()
    {
        if (moveCountText != null)
        {
            moveCountText.text = $"Số bước: {moveCount}";
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"Thời gian: {FormatTime(puzzleTimer)}";
        }
    }

    private string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    #endregion

    #region Collection Panel Logic

    private void OnCollectionItemClicked(int index)
    {
        PlaySound(clickChime);
        switch (index)
        {
            case 0:
                ShowStatus("Bức Tranh Sum Vầy: Ký ức trọn vẹn bên bố mẹ và chú mèo hoa (Đang kích hoạt)", 3.5f);
                break;
            case 1:
                ShowStatus("Nửa Ngọc Hoa Sen: Biểu tượng văn hóa Việt Nam (Mảnh ghép mở khóa tại Màn 1 & 2)", 3.5f);
                break;
            case 2:
                ShowStatus("Nửa Ngọc Mugunghwa: Quốc hoa Hàn Quốc kiên cường (Mảnh ghép mở khóa tại Màn 3)", 3.5f);
                break;
            case 3:
                ShowStatus("Cây Cầu Ký Ức: Điểm giao thoa nối liền hai miền quê hương của Hạ Ân", 3.5f);
                break;
            default:
                ShowStatus($"Đã chọn mảnh ký ức #{index + 1}", 2f);
                break;
        }
    }

    #endregion

    #region Audio & Volume Controls

    private void InitializeAudio()
    {
        // 1. Khởi tạo AudioSource cho hiệu ứng âm thanh SFX
        if (audioSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources != null && sources.Length > 0)
            {
                audioSource = sources[0];
            }
            else
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = sfxVolume;
        }

        // 2. Khởi tạo AudioSource cho Nhạc nền BGM
        if (bgmAudioSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources != null && sources.Length > 1)
            {
                bgmAudioSource = sources[1];
            }
            else
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        if (bgmAudioSource != null)
        {
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.spatialBlend = 0f;
            bgmAudioSource.volume = volumeSlider != null ? volumeSlider.value : 0.60f;
        }

        // Tự động tải nhạc nền BGM du dương, hoài niệm nếu chưa gán
        if (backgroundMusicClip == null)
        {
            try { backgroundMusicClip = Resources.Load<AudioClip>("Audio/BGM/BGM_Level03_Bridge"); } catch { }
#if UNITY_EDITOR
            if (backgroundMusicClip == null)
            {
                try { backgroundMusicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level03_Bridge.mp3"); } catch { }
            }
            if (backgroundMusicClip == null)
            {
                try { backgroundMusicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_MainMenu.mp3"); } catch { }
            }
#endif
        }

        bool isMuted = AudioManager.Instance != null ? AudioManager.Instance.IsMuted : (PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1);
        if (bgmAudioSource != null && backgroundMusicClip != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.clip = backgroundMusicClip;
            bgmAudioSource.mute = isMuted;
            if (!isMuted)
            {
                bgmAudioSource.Play();
            }
        }

        EnsureSoundController();

        // Tạo sẵn các clip âm thanh procedural retro
        clickChime = CreateProceduralChime(950f, 0.10f, 25f);
        slideChime = CreateProceduralChime(640f, 0.08f, 35f);
        shuffleChime = CreateProceduralChime(520f, 0.15f, 15f);
        magicChime = CreateMagicalRevealChime();
        winChime = CreateVictoryFanfare();
        chestOpenChime = CreateChestOpenChime();
        bookLiftChime = CreateBookLiftChime();
        pageFlipChime = CreatePageFlipChime();
    }

    private void EnsureSoundController()
    {
        var soundCtrl = GetComponent<MenuSoundController>();
        if (soundCtrl == null)
        {
            soundCtrl = gameObject.AddComponent<MenuSoundController>();
        }
    }

    public void OnMusicVolumeChanged(float volume)
    {
        float vol = Mathf.Clamp01(volume);
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = vol;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(vol);
        }
    }

    public void OnSfxVolumeChanged(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = sfxVolume;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
        }
    }

    private AudioClip CreateProceduralChime(float freq, float duration, float decay)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * decay);
            float wave = 0.7f * Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
            data[i] = wave * env * 0.45f;
        }

        AudioClip clip = AudioClip.Create("Chime_" + freq, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateVictoryFanfare()
    {
        int sampleRate = 44100;
        float duration = 0.65f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        // Chuỗi nốt ngũ cung thăng hoa: C5 (523), E5 (659), G5 (784), C6 (1046)
        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
        float stepTime = duration / freqs.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            int noteIdx = Mathf.Clamp((int)(t / stepTime), 0, freqs.Length - 1);
            float freq = freqs[noteIdx];
            float noteT = t - (noteIdx * stepTime);
            float env = Mathf.Exp(-noteT * 12f);
            float wave = 0.6f * Mathf.Sin(2f * Mathf.PI * freq * t) + 0.4f * Mathf.PingPong(t * freq * 2f, 1f);
            data[i] = wave * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create("VictoryFanfare", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    #endregion

    #region Feedback & Screen Transitions

    public void ShowStatus(string message, float duration = 2.5f)
    {
        if (statusText == null) return;
        if (statusCoroutine != null) StopCoroutine(statusCoroutine);
        statusCoroutine = StartCoroutine(ShowStatusRoutine(message, duration));
    }

    private IEnumerator ShowStatusRoutine(string message, float duration)
    {
        statusText.text = message;
        yield return new WaitForSecondsRealtime(duration);
        statusText.text = "";
        statusCoroutine = null;
    }

    private IEnumerator FadeInRoutine()
    {
        if (screenFadeOverlay == null) yield break;
        screenFadeOverlay.blocksRaycasts = true;
        screenFadeOverlay.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            screenFadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        screenFadeOverlay.alpha = 0f;
        screenFadeOverlay.blocksRaycasts = false;
    }

    private IEnumerator FadeOutRoutine()
    {
        if (screenFadeOverlay == null) yield break;
        screenFadeOverlay.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            screenFadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        screenFadeOverlay.alpha = 1f;
    }

    #region Puzzle Slices Progression System (Map 1 -> Map 2 -> Map 3)

    public void SetSlicesAlpha(float a1, float a2, float a3)
    {
        EnsureSlicesFitFrameAndColor();
        if (sliceOverlay1 != null) sliceOverlay1.alpha = a1;
        if (sliceOverlay2 != null) sliceOverlay2.alpha = a2;
        if (sliceOverlay3 != null) sliceOverlay3.alpha = a3;
    }

    public void EnsureSlicesFitFrameAndColor()
    {
        SetSliceImageColor(sliceOverlay1);
        SetSliceImageColor(sliceOverlay2);
        SetSliceImageColor(sliceOverlay3);

        if (slicesContainer != null)
        {
            RectTransform scRect = slicesContainer.GetComponent<RectTransform>();
            if (scRect != null)
            {
                scRect.anchoredPosition = new Vector2(-14.5f, 30f);
                scRect.sizeDelta = new Vector2(591f, 614f);
            }
        }

        SetSliceRect(sliceOverlay1, new Vector2(-197f, 0f), new Vector2(198f, 614f));
        SetSliceRect(sliceOverlay2, new Vector2(0f, 0f), new Vector2(198f, 614f));
        SetSliceRect(sliceOverlay3, new Vector2(197f, 0f), new Vector2(198f, 614f));
    }

    public void EnsureSlicesColorBlack()
    {
        EnsureSlicesFitFrameAndColor();
    }

    private void SetSliceRect(CanvasGroup cg, Vector2 pos, Vector2 size)
    {
        if (cg != null)
        {
            RectTransform rt = cg.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = pos;
                rt.sizeDelta = size;
            }
        }
    }

    private void SetSliceImageColor(CanvasGroup cg)
    {
        if (cg != null)
        {
            Image img = cg.GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.black; // Màu đen tuyền nguyên khối (100% opaque black)
            }
        }
    }

    public IEnumerator PlaySliceRevealSequence(int mapIndex)
    {
        // Thiết lập trạng thái 3 lớp phủ theo đúng hình tham khảo:
        // - Sau Map 1: Cả 3 lớp phủ đều đen tuyền (alpha=1), sau đó Mảnh 1 xuất hiện dần (alpha -> 0)
        // - Sau Map 2: Mảnh 1 đã sáng màu (alpha=0), Mảnh 2 & 3 đen tuyền (alpha=1), Mảnh 2 xuất hiện dần (alpha -> 0)
        // - Sau Map 3: Mảnh 1 & 2 đã sáng màu (alpha=0), Mảnh 3 đen tuyền (alpha=1), Mảnh 3 xuất hiện dần (alpha -> 0), rồi bức tranh hoàn chỉnh
        if (mapIndex == 1)
        {
            SetSlicesAlpha(1f, 1f, 1f);
        }
        else if (mapIndex == 2)
        {
            SetSlicesAlpha(0f, 1f, 1f);
        }
        else
        {
            SetSlicesAlpha(0f, 0f, 1f);
        }

        if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);
        if (sliceUnlockBanner != null) sliceUnlockBanner.SetActive(false);

        // Chờ hiệu ứng mờ màn hình (Fade In) hoàn tất
        yield return new WaitForSecondsRealtime(0.5f);

        CanvasGroup targetGroup = (mapIndex == 1) ? sliceOverlay1 : ((mapIndex == 2) ? sliceOverlay2 : sliceOverlay3);
        string pieceName = (mapIndex == 1) ? "MẢNH 1: CUNG CẢNH PHÚC & QUÊ HƯƠNG HÀN QUỐC"
                         : ((mapIndex == 2) ? "MẢNH 2: HẠ ÂN & NHỊP CẦU SUM VẦY"
                                            : "MẢNH 3: THÁP RÙA & QUÊ HƯƠNG VIỆT NAM");

        if (sliceUnlockBanner != null)
        {
            sliceUnlockBanner.SetActive(true);
            if (sliceUnlockText != null) sliceUnlockText.text = $"KÝ ỨC THỨC TỈNH: {pieceName}";
        }

        ShowStatus($"Đang phục hồi mảnh ghép ký ức ({mapIndex}/3)...", 4.5f);

        // Phát chuỗi âm thanh huyền ảo thức tỉnh ký ức
        if (audioSource != null && magicChime != null)
        {
            audioSource.PlayOneShot(magicChime, sfxVolume);
        }

        // Tạo hiệu ứng xuất hiện dần 1 mảnh ảnh: Alpha giảm dần mượt mà từ 1.0 (đen nguyên khối) về 0.0 (rực rỡ sắc màu) trong 4 giây
        if (targetGroup != null)
        {
            float elapsed = 0f;
            float duration = (sliceRevealDuration > 0f) ? sliceRevealDuration : 4.0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                targetGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            targetGroup.alpha = 0f;
        }

        // Nếu là Màn 3 hoàn thành: Hiện lên bức tranh hoàn chỉnh trọn vẹn và mở khóa chiếc rương
        if (mapIndex >= 3)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            if (audioSource != null && winChime != null)
            {
                audioSource.PlayOneShot(winChime, sfxVolume);
            }
            if (sliceUnlockBanner != null)
            {
                sliceUnlockBanner.SetActive(true);
            }
            if (sliceUnlockText != null)
            {
                sliceUnlockText.text = "★ KÝ ỨC ĐÃ TRỌN VẸN! HÃY CHẠM VÀO CHIẾC RƯƠNG THỜI GIAN ★";
            }
            ShowStatus("Tuyệt vời! Bức tranh ký ức sum vầy đã hoàn thiện trọn vẹn! Hãy chạm vào chiếc rương trên bàn...", 6f);

            // Xoá khung chữ Tiếp tục / Về menu chính theo đúng yêu cầu người dùng (lúc này chưa về trang chủ)
            if (nextStageButton != null)
            {
                nextStageButton.gameObject.SetActive(false);
            }

            // Mở khóa tương tác rương thời gian
            UnlockChestInteraction();

            // Tự động mở rương và hiển thị cuốn nhật ký sau 3.0s nếu người chơi chưa bấm
            StartCoroutine(AutoOpenChestAndDiaryRoutine());
        }
        else
        {
            // Hiển thị nút Tiếp tục để chuyển màn cho Map 1 và Map 2
            if (nextStageButton != null)
            {
                currentCompletedMapIndex = mapIndex;
                nextStageButton.gameObject.SetActive(true);
                if (nextStageButtonText != null)
                {
                    if (mapIndex == 1) nextStageButtonText.text = "TIẾP TỤC: MÀN 2 (SẮC MÀU QUÊ HƯƠNG) →";
                    else if (mapIndex == 2) nextStageButtonText.text = "TIẾP TỤC: MÀN 3 (ĐỀN KÝ ỨC) →";
                }
                nextStageButton.onClick.RemoveAllListeners();
                nextStageButton.onClick.AddListener(() => OnNextStageClicked(mapIndex));
            }
        }

        // Xóa cờ đã xử lý
        PlayerPrefs.SetInt("Puzzle_RecentCompletedMap", 0);
        PlayerPrefs.Save();
    }

    public void OnDefaultNextStageClicked()
    {
        OnNextStageClicked(currentCompletedMapIndex);
    }

    public void OnNextStageClicked()
    {
        OnNextStageClicked(currentCompletedMapIndex);
    }

    public void OnNextStageClicked(int mapIndex)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        // Phát âm thanh click retro
        if (clickChime != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickChime, sfxVolume);
        }

        // Kích hoạt hiệu ứng click vào ô tiếp tục (nảy nút, lóe sáng vàng đồng) trước khi chuyển cảnh
        StartCoroutine(PlayNextStageButtonEffectRoutine(mapIndex));
    }

    private IEnumerator PlayNextStageButtonEffectRoutine(int mapIndex)
    {
        if (nextStageButton != null)
        {
            Transform btnTr = nextStageButton.transform;
            Vector3 originalScale = Vector3.one;
            Image btnImg = nextStageButton.GetComponent<Image>();
            Color originalColor = btnImg != null ? btnImg.color : new Color(0.38f, 0.20f, 0.12f, 1f);
            Color highlightColor = new Color(0.85f, 0.55f, 0.25f, 1f); // Màu vàng đồng sáng rực khi click

            // Kích hoạt thêm RegisterButtonEffect nếu có trên GameObject
            RegisterButtonEffect regEffect = nextStageButton.GetComponent<RegisterButtonEffect>();
            if (regEffect != null)
            {
                regEffect.TriggerEffect();
            }

            // Hoạt ảnh Scale Punch & Color Flash trong 0.22 giây
            float duration = 0.22f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float s;
                Color c;

                if (t < 0.35f)
                {
                    float subT = t / 0.35f;
                    s = Mathf.Lerp(1.0f, 0.90f, subT);
                    c = Color.Lerp(originalColor, highlightColor, subT);
                }
                else if (t < 0.75f)
                {
                    float subT = (t - 0.35f) / 0.40f;
                    s = Mathf.Lerp(0.90f, 1.08f, subT);
                    c = Color.Lerp(highlightColor, originalColor, subT);
                }
                else
                {
                    float subT = (t - 0.75f) / 0.25f;
                    s = Mathf.Lerp(1.08f, 1.0f, subT);
                    c = originalColor;
                }

                btnTr.localScale = originalScale * s;
                if (btnImg != null) btnImg.color = c;
                yield return null;
            }

            btnTr.localScale = originalScale;
            if (btnImg != null) btnImg.color = originalColor;
        }

        // Chuyển sang màn chơi tương ứng
        if (mapIndex == 1)
        {
            yield return StartCoroutine(TransitionToSceneRoutine("Scene_Level2"));
        }
        else if (mapIndex == 2)
        {
            yield return StartCoroutine(TransitionToSceneRoutine("Map3_Final"));
        }
        else
        {
            yield return StartCoroutine(TransitionToSceneRoutine("MainMenu"));
        }
    }

    private IEnumerator TransitionToSceneRoutine(string targetScene)
    {
        isTransitioning = true;
        if (screenFadeOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                screenFadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            screenFadeOverlay.alpha = 1f;
        }

        Time.timeScale = 1f;
        Debug.Log($"[PuzzelMenu] Đang chuyển cảnh tới Map 2 ({targetScene})...");

        try
        {
            if (Application.CanStreamedLevelBeLoaded(targetScene))
            {
                SceneManager.LoadScene(targetScene);
            }
            else if (Application.CanStreamedLevelBeLoaded($"Assets/Scenes/{targetScene}.unity"))
            {
                SceneManager.LoadScene($"Assets/Scenes/{targetScene}.unity");
            }
            else if (Application.CanStreamedLevelBeLoaded($"Assets/_Map3_Final/Scenes/{targetScene}.unity"))
            {
                SceneManager.LoadScene($"Assets/_Map3_Final/Scenes/{targetScene}.unity");
            }
            else
            {
                SceneManager.LoadScene(targetScene);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PuzzelMenu] Không thể tải scene '{targetScene}': {ex.Message}. Tải Scene_Level2 trực tiếp...");
            try
            {
                SceneManager.LoadScene("Scene_Level2");
            }
            catch
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private AudioClip CreateMagicalRevealChime()
    {
        int sampleRate = 44100;
        float duration = 3.8f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float[] freqs = new float[] { 392f, 440f, 523.25f, 587.33f, 659.25f, 783.99f, 880f, 1046.50f, 1174.66f, 1318.51f }; // G4 -> E6
        float noteDur = 0.35f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            int noteIdx = Mathf.Clamp((int)(t / noteDur), 0, freqs.Length - 1);
            float noteT = t - noteIdx * noteDur;
            float freq = freqs[noteIdx];
            float env = Mathf.Exp(-noteT * 5.5f);
            float wave = 0.7f * Mathf.Sin(2f * Mathf.PI * freq * noteT) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * noteT);
            data[i] = wave * env * 0.40f;
        }

        AudioClip clip = AudioClip.Create("MagicalReveal", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    #region Memory Chest Interaction

    public void UnlockChestInteraction()
    {
        isChestUnlocked = true;
        isChestOpened = false;
        isDiaryFloating = false;
        isDiaryOpened = false;

        if (chestButton != null)
        {
            chestButton.gameObject.SetActive(true);
            chestButton.interactable = true;
        }

        if (chestPromptBadge != null)
        {
            chestPromptBadge.SetActive(true);
        }

        if (chestGlowEffect != null)
        {
            chestGlowEffect.SetActive(true);
        }

        if (chestRaysEffect != null)
        {
            chestRaysEffect.SetActive(false);
        }

        if (chestAnimationImage != null)
        {
            chestAnimationImage.gameObject.SetActive(true);
            if (chestOpeningFrames != null && chestOpeningFrames.Length > 0)
            {
                chestAnimationImage.sprite = chestOpeningFrames[0];
            }
        }

        if (chestOpenOverlay != null)
        {
            chestOpenOverlay.SetActive(false);
        }

        if (diaryInChestButton != null)
        {
            diaryInChestButton.gameObject.SetActive(false);
        }

        if (diaryPromptBadge != null)
        {
            diaryPromptBadge.SetActive(false);
        }

        if (floatingDiaryImage != null)
        {
            floatingDiaryImage.gameObject.SetActive(false);
        }

        if (diaryContentContainer != null)
        {
            diaryContentContainer.SetActive(false);
        }

        if (diaryGlowEffect != null)
        {
            diaryGlowEffect.SetActive(false);
        }

        if (diaryRaysEffect != null)
        {
            diaryRaysEffect.SetActive(false);
        }

        if (diarySparklesEffect != null)
        {
            diarySparklesEffect.SetActive(false);
        }

        if (endingLetterModal != null)
        {
            endingLetterModal.SetActive(false);
        }

        if (finishGameButton != null)
        {
            finishGameButton.gameObject.SetActive(false);
        }

        if (sliceUnlockBanner != null)
        {
            sliceUnlockBanner.SetActive(true);
        }
        if (sliceUnlockText != null)
        {
            sliceUnlockText.text = "★ KÝ ỨC ĐÃ TRỌN VẸN! HÃY CHẠM VÀO CHIẾC RƯƠNG THỜI GIAN ★";
        }
    }

    public void OnChestClicked()
    {
        if (!isChestUnlocked || isChestOpened || isTransitioning) return;
        StartCoroutine(OpenChestSequenceRoutine());
    }

    private IEnumerator OpenChestSequenceRoutine()
    {
        isChestOpened = true;

        if (chestPromptBadge != null)
        {
            chestPromptBadge.SetActive(false);
        }
        if (chestButton != null)
        {
            chestButton.interactable = false;
        }

        // Phát âm thanh mở rương kỳ diệu
        if (chestOpenChime != null && audioSource != null)
        {
            audioSource.PlayOneShot(chestOpenChime, sfxVolume);
        }

        // Hoạt ảnh Scale Bounce nhẹ chiếc rương kết hợp với chuỗi 12 frames mở nắp
        if (chestAnimationImage != null)
        {
            chestAnimationImage.gameObject.SetActive(true);
            if (chestOpeningFrames != null && chestOpeningFrames.Length > 0)
            {
                float frameDuration = 0.09f;
                for (int i = 0; i < chestOpeningFrames.Length; i++)
                {
                    chestAnimationImage.sprite = chestOpeningFrames[i];
                    yield return new WaitForSecondsRealtime(frameDuration);
                }
                // Dừng lại ở frame cuối cùng (rương mở hoàn toàn cùng ánh sáng vàng và hạt sáng lấp lánh)
                chestAnimationImage.sprite = chestOpeningFrames[chestOpeningFrames.Length - 1];
            }
        }
        else if (chestOpenOverlay != null)
        {
            chestOpenOverlay.SetActive(true);
        }

        // Bật luồng tia sáng thần kỳ chiếu lên từ miệng chiếc rương mở
        if (chestRaysEffect != null)
        {
            chestRaysEffect.SetActive(true);
            CanvasGroup chestRaysCg = chestRaysEffect.GetComponent<CanvasGroup>();
            if (chestRaysCg != null) chestRaysCg.alpha = 0.85f;
        }

        if (sliceUnlockText != null)
        {
            sliceUnlockText.text = "★ CHIẾC RƯƠNG THỜI GIAN ĐÃ HÉ MỞ! CHẠM VÀO CUỐN NHẬT KÝ ★";
        }
        ShowStatus("Chiếc rương thời gian đã mở! Một cuốn nhật ký cổ kính đang phát sáng bên trong! Hãy chạm vào cuốn nhật ký...", 5f);

        // Kích hoạt nút tương tác cuốn nhật ký trong rương và huy hiệu chỉ dẫn
        if (diaryInChestButton != null)
        {
            diaryInChestButton.gameObject.SetActive(true);
            diaryInChestButton.interactable = true;
        }

        if (diaryPromptBadge != null)
        {
            diaryPromptBadge.SetActive(true);
        }

        // Tự động cho nhật ký bay lên và mở ra sau 2.0s nếu người chơi chưa chạm vào cuốn nhật ký
        StartCoroutine(AutoFlyDiaryRoutine());
    }

    private IEnumerator AutoOpenChestAndDiaryRoutine()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        if (isChestUnlocked && !isChestOpened && !isTransitioning)
        {
            OnChestClicked();
        }
    }

    private IEnumerator AutoFlyDiaryRoutine()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        if (isChestOpened && !isDiaryFloating && !isDiaryOpened && !isTransitioning)
        {
            OnDiaryInChestClicked();
        }
    }

    public void OnDiaryInChestClicked()
    {
        if (!isChestOpened || isDiaryFloating || isDiaryOpened || isTransitioning) return;
        StartCoroutine(FlyAndOpenDiaryRoutine());
    }

    private IEnumerator FlyAndOpenDiaryRoutine()
    {
        isDiaryFloating = true;

        if (diaryPromptBadge != null)
        {
            diaryPromptBadge.SetActive(false);
        }
        if (diaryInChestButton != null)
        {
            diaryInChestButton.gameObject.SetActive(false);
        }

        // Phát âm thanh sách bay lên kỳ diệu
        if (bookLiftChime != null && audioSource != null)
        {
            audioSource.PlayOneShot(bookLiftChime, sfxVolume);
        }

        // Khởi tạo vị trí & kích thước nhật ký ban đầu (trong rương)
        if (floatingDiaryImage != null)
        {
            floatingDiaryImage.gameObject.SetActive(true);
            if (diaryContentContainer != null) diaryContentContainer.SetActive(false);
            if (bookOpeningFrames != null && bookOpeningFrames.Length > 0)
            {
                floatingDiaryImage.sprite = bookOpeningFrames[0];
            }

            RectTransform diaryRt = floatingDiaryImage.rectTransform;
            Vector2 startPos = bookInChestPos;
            Vector2 endPos = Vector2.zero;
            Vector2 startSize = new Vector2(230f, 280f);
            Vector2 targetSize = new Vector2(460f, 560f);

            // Bật hiệu ứng vầng hào quang và các tia sáng thiên giới sau nhật ký
            if (diaryGlowEffect != null)
            {
                diaryGlowEffect.SetActive(true);
                CanvasGroup glowCg = diaryGlowEffect.GetComponent<CanvasGroup>();
                if (glowCg != null) glowCg.alpha = 0f;
            }
            if (diaryRaysEffect != null)
            {
                diaryRaysEffect.SetActive(true);
                CanvasGroup raysCg = diaryRaysEffect.GetComponent<CanvasGroup>();
                if (raysCg != null) raysCg.alpha = 0f;
            }
            if (diarySparklesEffect != null)
            {
                diarySparklesEffect.SetActive(true);
                CanvasGroup sparkCg = diarySparklesEffect.GetComponent<CanvasGroup>();
                if (sparkCg != null) sparkCg.alpha = 0f;
            }

            // GIAI ĐOẠN 1: Nhật ký từ từ bay lên, di chuyển đến chính giữa màn hình (0, 0)
            float flyDuration = 1.25f;
            float elapsed = 0f;
            while (elapsed < flyDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / flyDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // Đường bay cánh cung mềm mại
                float arcY = Mathf.Sin(t * Mathf.PI) * 140f;
                diaryRt.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothT) + new Vector2(0f, arcY);
                diaryRt.sizeDelta = Vector2.Lerp(startSize, targetSize, smoothT);

                // Độ nghiêng xoay nhẹ tạo chuyển động tự nhiên
                float tiltAngle = Mathf.Sin(t * Mathf.PI * 2f) * 6f;
                diaryRt.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);

                if (diaryGlowEffect != null)
                {
                    CanvasGroup glowCg = diaryGlowEffect.GetComponent<CanvasGroup>();
                    if (glowCg != null) glowCg.alpha = Mathf.Lerp(0f, 0.75f, t);
                }
                if (diaryRaysEffect != null)
                {
                    CanvasGroup raysCg = diaryRaysEffect.GetComponent<CanvasGroup>();
                    if (raysCg != null) raysCg.alpha = Mathf.Lerp(0f, 0.65f, t);
                }
                if (diarySparklesEffect != null)
                {
                    CanvasGroup sparkCg = diarySparklesEffect.GetComponent<CanvasGroup>();
                    if (sparkCg != null) sparkCg.alpha = Mathf.Lerp(0f, 0.80f, t);
                }

                yield return null;
            }

            diaryRt.anchoredPosition = endPos;
            diaryRt.sizeDelta = targetSize;
            diaryRt.localRotation = Quaternion.identity;

            yield return new WaitForSecondsRealtime(0.25f);

            // GIAI ĐOẠN 2: Nhật ký từ từ mở ra, hai trang giấy lật nhẹ nhàng
            if (pageFlipChime != null && audioSource != null)
            {
                audioSource.PlayOneShot(pageFlipChime, sfxVolume);
            }

            if (bookOpeningFrames != null && bookOpeningFrames.Length > 0)
            {
                float openFrameDuration = 0.10f;
                Vector2 closedSize = new Vector2(460f, 560f);
                Vector2 fullyOpenSize = new Vector2(974f, 560f);

                for (int i = 0; i < bookOpeningFrames.Length; i++)
                {
                    floatingDiaryImage.sprite = bookOpeningFrames[i];
                    float prog = (float)i / (bookOpeningFrames.Length - 1);
                    diaryRt.sizeDelta = Vector2.Lerp(closedSize, fullyOpenSize, prog);
                    yield return new WaitForSecondsRealtime(openFrameDuration);
                }

                floatingDiaryImage.sprite = bookOpeningFrames[bookOpeningFrames.Length - 1];
                diaryRt.sizeDelta = fullyOpenSize;

                // Điền và hiển thị nội dung nhật ký vào cuốn nhật ký
                EnsureDiaryContentUI();
                if (diaryContentContainer != null)
                {
                    diaryContentContainer.SetActive(true);
                    CanvasGroup contentCg = diaryContentContainer.GetComponent<CanvasGroup>();
                    if (contentCg != null)
                    {
                        StartCoroutine(FadeInDiaryContentRoutine(contentCg, 0.6f));
                    }
                }
            }

            isDiaryOpened = true;
        }

        if (sliceUnlockText != null)
        {
            sliceUnlockText.text = "★ NHẬT KÝ KÝ ỨC ĐÃ MỞ RA TRỌN VẸN ★";
        }
        ShowStatus("Cuốn nhật ký ký ức đã mở ra hai trang giấy tinh khôi, lưu giữ hành trình trọn vẹn!", 5f);

        // Hiển thị nút Sang trang / Hoàn thành bên dưới cuốn nhật ký mở
        if (finishGameButton != null)
        {
            finishGameButton.gameObject.SetActive(true);
            RectTransform finishRt = finishGameButton.GetComponent<RectTransform>();
            if (finishRt != null)
            {
                finishRt.anchoredPosition = new Vector2(0f, -325f);
                finishRt.sizeDelta = new Vector2(460f, 54f);
            }
            UpdateDiaryPageDisplay();
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút bên dưới cuốn nhật ký:
    /// - Khi ở Trang 1, 2, 3: Nhấp vào sẽ lật sang trang tiếp theo.
    /// - Khi ở Trang 4 (trang cuối): Nhấp vào sẽ hoàn thành hành trình và về menu chính.
    /// </summary>
    public void OnFinishGameButtonClicked()
    {
        if (isPageFlipping || isTransitioning) return;

        if (currentDiaryPage < TOTAL_DIARY_PAGES)
        {
            OnClickNextDiaryPage();
        }
        else
        {
            OnFinishGameClicked();
        }
    }

    public void OnFinishGameClicked()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (clickChime != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickChime, sfxVolume);
        }

        StartCoroutine(PlayFinishGameButtonEffectRoutine());
    }

    private IEnumerator PlayFinishGameButtonEffectRoutine()
    {
        if (finishGameButton != null)
        {
            Transform btnTr = finishGameButton.transform;
            Vector3 originalScale = Vector3.one;
            Image btnImg = finishGameButton.GetComponent<Image>();
            Color originalColor = btnImg != null ? btnImg.color : new Color(0.38f, 0.20f, 0.12f, 1f);
            Color highlightColor = new Color(0.95f, 0.75f, 0.35f, 1f);

            RegisterButtonEffect regEffect = finishGameButton.GetComponent<RegisterButtonEffect>();
            if (regEffect != null)
            {
                regEffect.TriggerEffect();
            }

            float duration = 0.25f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float s = 1.0f + 0.08f * Mathf.Sin(t * Mathf.PI);
                btnTr.localScale = originalScale * s;
                if (btnImg != null) btnImg.color = Color.Lerp(originalColor, highlightColor, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            btnTr.localScale = originalScale;
            if (btnImg != null) btnImg.color = originalColor;
        }

        yield return StartCoroutine(TransitionToSceneRoutine("MainMenu"));
    }

    private AudioClip CreateChestOpenChime()
    {
        int sampleRate = 44100;
        float duration = 1.4f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 987.77f, 1046.50f };

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            if (t < 0.08f)
            {
                float clickEnv = Mathf.Exp(-t * 80f);
                sample += (Mathf.Sin(2f * Mathf.PI * 1800f * t) + (UnityEngine.Random.value * 2f - 1f) * 0.4f) * clickEnv * 0.5f;
            }

            if (t >= 0.05f)
            {
                float chordT = t - 0.05f;
                for (int n = 0; n < freqs.Length; n++)
                {
                    float noteOffset = n * 0.09f;
                    if (chordT >= noteOffset)
                    {
                        float noteT = chordT - noteOffset;
                        float env = Mathf.Exp(-noteT * 3.8f);
                        sample += Mathf.Sin(2f * Mathf.PI * freqs[n] * noteT) * env * 0.22f;
                        sample += 0.3f * Mathf.Sin(4f * Mathf.PI * freqs[n] * noteT) * env * 0.15f;
                    }
                }
            }

            data[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("ChestOpenChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateBookLiftChime()
    {
        int sampleRate = 44100;
        float duration = 1.3f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float[] freqs = new float[] { 440f, 554.37f, 659.25f, 880f, 1108.73f, 1318.51f };

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            for (int n = 0; n < freqs.Length; n++)
            {
                float noteOffset = n * 0.12f;
                if (t >= noteOffset)
                {
                    float noteT = t - noteOffset;
                    float env = Mathf.Exp(-noteT * 3.5f);
                    sample += Mathf.Sin(2f * Mathf.PI * freqs[n] * noteT) * env * 0.18f;
                    sample += 0.25f * Mathf.Sin(4f * Mathf.PI * freqs[n] * noteT) * env * 0.10f;
                }
            }

            data[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("BookLiftChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreatePageFlipChime()
    {
        int sampleRate = 44100;
        float duration = 1.1f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Âm thanh loạt soạt trang giấy lật nhẹ nhàng
            if (t < 0.6f)
            {
                float rustleEnv = Mathf.Sin(t / 0.6f * Mathf.PI);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.15f;
                sample += noise * rustleEnv;
            }

            // Tiếng chuông cộng hưởng thanh tao
            float chordT = t;
            float env = Mathf.Exp(-chordT * 3.2f);
            sample += Mathf.Sin(2f * Mathf.PI * 783.99f * chordT) * env * 0.15f;
            sample += Mathf.Sin(2f * Mathf.PI * 1174.66f * chordT) * env * 0.12f;

            data[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("PageFlipChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static Font GetDiaryPixelFont()
    {
        Font font = null;
        try
        {
            font = Resources.Load<Font>("Fonts & Materials/SVN-Determination Sans");
        }
        catch { }

#if UNITY_EDITOR
        if (font == null)
        {
            try
            {
                font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.otf");
            }
            catch { }
        }
#endif

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        if (font == null)
        {
            try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        }
        return font;
    }

    /// <summary>
    /// Đảm bảo giao diện hiển thị trang nội dung nhật ký được khởi tạo đầy đủ:
    /// - Trang bên trái: Bức tranh Tết được thu nhỏ vừa vặn, căn giữa, bên dưới có dòng chữ "Chúc mừng năm mới".
    /// - Trang bên phải: Nội dung nhật ký ngày 31/01/2014 được nhích lên trên để bố cục trang cân đối và thoáng đãng.
    /// - Cơ chế lật trang sách mượt mà với 12 frame animation chuyển động từ phải sang trái.
    /// </summary>
    public void EnsureDiaryContentUI()
    {
        if (floatingDiaryImage == null) return;

        // Tự động tải Sprite bức tranh Tết nếu chưa gán
        if (diaryTetPhotoSprite == null)
        {
            try { diaryTetPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_Tet"); } catch { }
#if UNITY_EDITOR
            if (diaryTetPhotoSprite == null)
            {
                try { diaryTetPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Tet.png"); } catch { }
            }
#endif
        }

        // Tự động tải Sprite bức tranh Hội An nếu chưa gán
        if (diaryHoiAnPhotoSprite == null)
        {
            try { diaryHoiAnPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_HoiAn"); } catch { }
#if UNITY_EDITOR
            if (diaryHoiAnPhotoSprite == null)
            {
                try { diaryHoiAnPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_HoiAn.png"); } catch { }
            }
#endif
        }

        // Tự động tải Sprite bức tranh Seoul nếu chưa gán
        if (diarySeoulPhotoSprite == null)
        {
            try { diarySeoulPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_Seoul"); } catch { }
#if UNITY_EDITOR
            if (diarySeoulPhotoSprite == null)
            {
                try { diarySeoulPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Seoul.png"); } catch { }
            }
#endif
        }

        // Tự động tải Sprite bức tranh Cung điện Gyeongbokgung nếu chưa gán
        if (diaryGyeongbokgungPhotoSprite == null)
        {
            try { diaryGyeongbokgungPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_Gyeongbokgung"); } catch { }
#if UNITY_EDITOR
            if (diaryGyeongbokgungPhotoSprite == null)
            {
                try { diaryGyeongbokgungPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Gyeongbokgung.png"); } catch { }
            }
#endif
        }

        // Tự động tải Sprite bức tranh Tết Seollal nếu chưa gán
        if (diarySeollalPhotoSprite == null)
        {
            try { diarySeollalPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_Seollal"); } catch { }
#if UNITY_EDITOR
            if (diarySeollalPhotoSprite == null)
            {
                try { diarySeollalPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Seollal.png"); } catch { }
            }
#endif
        }

        // Tự động tải Sprite bức tranh Vịnh Hạ Long nếu chưa gán
        if (diaryHaLongPhotoSprite == null)
        {
            try { diaryHaLongPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_HaLong"); } catch { }
#if UNITY_EDITOR
            if (diaryHaLongPhotoSprite == null)
            {
                try { diaryHaLongPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_HaLong.png"); } catch { }
            }
#endif
        }

        // Tự động tải Sprite bức tranh Busan nếu chưa gán
        if (diaryBusanPhotoSprite == null)
        {
            try { diaryBusanPhotoSprite = Resources.Load<Sprite>("UI/PuzzelFrames/DiaryPhoto_Busan"); } catch { }
#if UNITY_EDITOR
            if (diaryBusanPhotoSprite == null)
            {
                try { diaryBusanPhotoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/PuzzelFrames/DiaryPhoto_Busan.png"); } catch { }
            }
#endif
        }

        // Tự động tải 12 frame lật trang nếu chưa gán
        if (pageFlipFrames == null || pageFlipFrames.Length == 0)
        {
            List<Sprite> pffList = new List<Sprite>();
            for (int i = 0; i < 12; i++)
            {
                string p = string.Format("Assets/UI/PuzzelFrames/PageFlipAnimation/page_flip_{0:D2}.png", i);
#if UNITY_EDITOR
                try
                {
                    Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    if (s != null) pffList.Add(s);
                }
                catch { }
#endif
            }
            if (pffList.Count > 0) pageFlipFrames = pffList.ToArray();
        }

        if (diaryContentContainer == null)
        {
            Transform existingContainer = floatingDiaryImage.transform.Find("DiaryContentContainer");
            if (existingContainer != null)
            {
                diaryContentContainer = existingContainer.gameObject;
            }
        }

        Font diaryFont = GetDiaryPixelFont();
        Color inkColor = new Color(0.18f, 0.10f, 0.06f, 1f); // Màu mực nâu đen cổ điển, nét bút viết tay trên giấy ngà vàng

        if (diaryContentContainer == null)
        {
            GameObject containerObj = new GameObject("DiaryContentContainer", typeof(RectTransform), typeof(CanvasGroup));
            containerObj.transform.SetParent(floatingDiaryImage.transform, false);
            RectTransform cRt = containerObj.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.sizeDelta = Vector2.zero;
            cRt.anchoredPosition = Vector2.zero;
            diaryContentContainer = containerObj;
        }

        // 1. TRANG BÊN TRÁI: Khung chứa ảnh Tết và dòng chữ "Chúc mừng năm mới"
        Transform leftContainerTr = diaryContentContainer.transform.Find("LeftPageContainer");
        GameObject leftContainerObj;
        if (leftContainerTr == null)
        {
            leftContainerObj = new GameObject("LeftPageContainer", typeof(RectTransform));
            leftContainerObj.transform.SetParent(diaryContentContainer.transform, false);
        }
        else
        {
            leftContainerObj = leftContainerTr.gameObject;
        }
        RectTransform leftContRt = leftContainerObj.GetComponent<RectTransform>();
        leftContRt.anchorMin = new Vector2(0.5f, 0.5f);
        leftContRt.anchorMax = new Vector2(0.5f, 0.5f);
        leftContRt.anchoredPosition = new Vector2(-205f, 0f);
        leftContRt.sizeDelta = new Vector2(320f, 440f);

        // Khung viền ảnh Polaroid hoài niệm cho bức tranh Tết
        Transform borderTr = leftContainerObj.transform.Find("PhotoBorder");
        GameObject borderObj;
        if (borderTr == null)
        {
            borderObj = new GameObject("PhotoBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderObj.transform.SetParent(leftContainerObj.transform, false);
        }
        else
        {
            borderObj = borderTr.gameObject;
        }
        RectTransform borderRt = borderObj.GetComponent<RectTransform>();
        borderRt.anchorMin = new Vector2(0.5f, 0.5f);
        borderRt.anchorMax = new Vector2(0.5f, 0.5f);
        borderRt.anchoredPosition = new Vector2(0f, 32f);
        borderRt.sizeDelta = new Vector2(242f, 253f);
        Image borderImg = borderObj.GetComponent<Image>();
        borderImg.color = new Color(0.96f, 0.94f, 0.90f, 1f); // Viền giấy trắng ngà
        borderImg.raycastTarget = false;

        // Bức tranh Tết thu nhỏ vừa vặn, căn giữa trang
        Transform photoTr = borderObj.transform.Find("LeftPagePhoto");
        GameObject photoObj;
        if (photoTr == null)
        {
            photoObj = new GameObject("LeftPagePhoto", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            photoObj.transform.SetParent(borderObj.transform, false);
        }
        else
        {
            photoObj = photoTr.gameObject;
        }
        RectTransform photoRt = photoObj.GetComponent<RectTransform>();
        photoRt.anchorMin = new Vector2(0.5f, 0.5f);
        photoRt.anchorMax = new Vector2(0.5f, 0.5f);
        photoRt.anchoredPosition = Vector2.zero;
        photoRt.sizeDelta = new Vector2(232f, 243f);
        Image photoImg = photoObj.GetComponent<Image>();
        if (diaryTetPhotoSprite != null) photoImg.sprite = diaryTetPhotoSprite;
        photoImg.preserveAspect = true;
        photoImg.color = Color.white;
        photoImg.raycastTarget = false;
        diaryLeftPhoto = photoImg;

        // Dòng chữ ghi chú bên dưới bức tranh: "Chúc mừng năm mới"
        Transform captionTr = leftContainerObj.transform.Find("LeftPhotoCaption");
        GameObject captionObj;
        if (captionTr == null)
        {
            captionObj = new GameObject("LeftPhotoCaption", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            captionObj.transform.SetParent(leftContainerObj.transform, false);
        }
        else
        {
            captionObj = captionTr.gameObject;
        }
        RectTransform captionRt = captionObj.GetComponent<RectTransform>();
        captionRt.anchorMin = new Vector2(0.5f, 0.5f);
        captionRt.anchorMax = new Vector2(0.5f, 0.5f);
        captionRt.anchoredPosition = new Vector2(0f, -125f);
        captionRt.sizeDelta = new Vector2(280f, 36f);
        Text captionTxt = captionObj.GetComponent<Text>();
        captionTxt.font = diaryFont;
        captionTxt.fontSize = 15;
        captionTxt.fontStyle = FontStyle.Italic;
        captionTxt.alignment = TextAnchor.MiddleCenter;
        captionTxt.color = inkColor;
        captionTxt.supportRichText = true;
        captionTxt.raycastTarget = false;
        captionTxt.text = "Chúc mừng năm mới";
        diaryLeftPhotoCaption = captionTxt;

        // Nút / Vùng chạm lùi trang trên trang bên trái (dành cho khi ở Trang 2)
        Transform prevBtnTr = leftContainerObj.transform.Find("PrevDiaryPageButton");
        GameObject prevBtnObj;
        if (prevBtnTr == null)
        {
            prevBtnObj = new GameObject("PrevDiaryPageButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            prevBtnObj.transform.SetParent(leftContainerObj.transform, false);
        }
        else
        {
            prevBtnObj = prevBtnTr.gameObject;
        }
        RectTransform prevBtnRt = prevBtnObj.GetComponent<RectTransform>();
        prevBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
        prevBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        prevBtnRt.anchoredPosition = Vector2.zero;
        prevBtnRt.sizeDelta = new Vector2(320f, 440f);
        Image prevBtnImg = prevBtnObj.GetComponent<Image>();
        prevBtnImg.color = new Color(1f, 1f, 1f, 0.001f);
        prevBtnImg.raycastTarget = true;
        Button prevBtn = prevBtnObj.GetComponent<Button>();
        prevBtn.onClick.RemoveAllListeners();
        prevBtn.onClick.AddListener(() => OnClickPrevDiaryPage());
        prevDiaryPageButton = prevBtn;

        // Gợi ý lật lùi trang ở góc dưới trang trái
        Transform prevPromptTr = leftContainerObj.transform.Find("PrevPagePrompt");
        GameObject prevPromptObj;
        if (prevPromptTr == null)
        {
            prevPromptObj = new GameObject("PrevPagePrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            prevPromptObj.transform.SetParent(leftContainerObj.transform, false);
        }
        else
        {
            prevPromptObj = prevPromptTr.gameObject;
        }
        RectTransform prevPromptRt = prevPromptObj.GetComponent<RectTransform>();
        prevPromptRt.anchorMin = new Vector2(0.5f, 0.5f);
        prevPromptRt.anchorMax = new Vector2(0.5f, 0.5f);
        prevPromptRt.anchoredPosition = new Vector2(-80f, -195f);
        prevPromptRt.sizeDelta = new Vector2(140f, 28f);
        Text prevPromptTxt = prevPromptObj.GetComponent<Text>();
        prevPromptTxt.font = diaryFont;
        prevPromptTxt.fontSize = 13;
        prevPromptTxt.fontStyle = FontStyle.Italic;
        prevPromptTxt.alignment = TextAnchor.MiddleLeft;
        prevPromptTxt.color = new Color(0.48f, 0.30f, 0.18f, 0.9f);
        prevPromptTxt.raycastTarget = false;
        prevPromptTxt.text = "[ ◄ Trang trước ]";
        prevPagePrompt = prevPromptObj;

        // 2. TRANG BÊN PHẢI: Văn bản nhật ký (nhích lên trên một chút để bố cục cân đối)
        Transform rightTr = diaryContentContainer.transform.Find("RightPageText");
        GameObject rightObj;
        if (rightTr == null)
        {
            rightObj = new GameObject("RightPageText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            rightObj.transform.SetParent(diaryContentContainer.transform, false);
        }
        else
        {
            rightObj = rightTr.gameObject;
        }
        RectTransform rRt = rightObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0.5f, 0.5f);
        rRt.anchorMax = new Vector2(0.5f, 0.5f);
        rRt.anchoredPosition = new Vector2(192f, 15f); // Nhích lên trên để bố cục trang cân đối hơn
        rRt.sizeDelta = new Vector2(310f, 365f);

        Text rightTxt = rightObj.GetComponent<Text>();
        rightTxt.font = diaryFont;
        rightTxt.fontSize = 14;
        rightTxt.fontStyle = FontStyle.Italic; // Nét nghiêng bút viết tay pixel
        rightTxt.lineSpacing = 1.3f;
        rightTxt.alignment = TextAnchor.UpperLeft;
        rightTxt.color = inkColor;
        rightTxt.supportRichText = true;
        rightTxt.raycastTarget = false;
        rightTxt.text = DIARY_RIGHT_PAGE_CONTENT;
        diaryRightPageText = rightTxt;

        // Nút / Vùng chạm lật sang trang sau trên trang bên phải
        Transform nextBtnTr = diaryContentContainer.transform.Find("NextDiaryPageButton");
        GameObject nextBtnObj;
        if (nextBtnTr == null)
        {
            nextBtnObj = new GameObject("NextDiaryPageButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            nextBtnObj.transform.SetParent(diaryContentContainer.transform, false);
        }
        else
        {
            nextBtnObj = nextBtnTr.gameObject;
        }
        RectTransform nextBtnRt = nextBtnObj.GetComponent<RectTransform>();
        nextBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
        nextBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        nextBtnRt.anchoredPosition = new Vector2(192f, 0f);
        nextBtnRt.sizeDelta = new Vector2(320f, 440f);
        Image nextBtnImg = nextBtnObj.GetComponent<Image>();
        nextBtnImg.color = new Color(1f, 1f, 1f, 0.001f);
        nextBtnImg.raycastTarget = true;
        Button nextBtn = nextBtnObj.GetComponent<Button>();
        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(() => OnClickNextDiaryPage());
        nextDiaryPageButton = nextBtn;

        // Gợi ý lật sang trang ở góc dưới trang phải
        Transform nextPromptTr = diaryContentContainer.transform.Find("NextPagePrompt");
        GameObject nextPromptObj;
        if (nextPromptTr == null)
        {
            nextPromptObj = new GameObject("NextPagePrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            nextPromptObj.transform.SetParent(diaryContentContainer.transform, false);
        }
        else
        {
            nextPromptObj = nextPromptTr.gameObject;
        }
        RectTransform nextPromptRt = nextPromptObj.GetComponent<RectTransform>();
        nextPromptRt.anchorMin = new Vector2(0.5f, 0.5f);
        nextPromptRt.anchorMax = new Vector2(0.5f, 0.5f);
        nextPromptRt.anchoredPosition = new Vector2(275f, -195f);
        nextPromptRt.sizeDelta = new Vector2(140f, 28f);
        Text nextPromptTxt = nextPromptObj.GetComponent<Text>();
        nextPromptTxt.font = diaryFont;
        nextPromptTxt.fontSize = 13;
        nextPromptTxt.fontStyle = FontStyle.Italic;
        nextPromptTxt.alignment = TextAnchor.MiddleRight;
        nextPromptTxt.color = new Color(0.48f, 0.30f, 0.18f, 0.9f);
        nextPromptTxt.raycastTarget = false;
        nextPromptTxt.text = "[ Trang tiếp ► ]";
        nextPagePrompt = nextPromptObj;

        // Thiết lập 2 nút mũi tên điều hướng trên thanh nhật ký (thay cho chữ "Sang trang")
        if (finishGameButton != null)
        {
            RectTransform barRt = finishGameButton.GetComponent<RectTransform>();
            if (barRt != null)
            {
                barRt.anchoredPosition = new Vector2(0f, -325f);
                barRt.sizeDelta = new Vector2(460f, 54f);
            }

            // 1. Nút mũi tên lùi trang bên trái
            Transform prevNavTr = finishGameButton.transform.Find("NavPrevButton");
            GameObject prevNavObj;
            if (prevNavTr == null)
            {
                prevNavObj = new GameObject("NavPrevButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                prevNavObj.transform.SetParent(finishGameButton.transform, false);
            }
            else
            {
                prevNavObj = prevNavTr.gameObject;
            }
            RectTransform pRt = prevNavObj.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0f, 0.5f);
            pRt.anchorMax = new Vector2(0f, 0.5f);
            pRt.pivot = new Vector2(0.5f, 0.5f);
            pRt.anchoredPosition = new Vector2(50f, 0f);
            pRt.sizeDelta = new Vector2(80f, 42f);
            Image pImg = prevNavObj.GetComponent<Image>();
            pImg.color = new Color(0.24f, 0.12f, 0.06f, 0.88f);
            Button pBtn = prevNavObj.GetComponent<Button>();
            pBtn.onClick.RemoveAllListeners();
            pBtn.onClick.AddListener(() => OnClickPrevDiaryPage());
            navPrevButton = pBtn;

            Transform pTxtTr = prevNavObj.transform.Find("Text");
            GameObject pTxtObj;
            if (pTxtTr == null)
            {
                pTxtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                pTxtObj.transform.SetParent(prevNavObj.transform, false);
            }
            else
            {
                pTxtObj = pTxtTr.gameObject;
            }
            RectTransform ptRt = pTxtObj.GetComponent<RectTransform>();
            ptRt.anchorMin = Vector2.zero;
            ptRt.anchorMax = Vector2.one;
            ptRt.offsetMin = Vector2.zero;
            ptRt.offsetMax = Vector2.zero;
            Text ptTxt = pTxtObj.GetComponent<Text>();
            ptTxt.font = diaryFont;
            ptTxt.fontSize = 24;
            ptTxt.fontStyle = FontStyle.Bold;
            ptTxt.alignment = TextAnchor.MiddleCenter;
            ptTxt.text = "◄";
            ptTxt.color = new Color(1f, 0.92f, 0.72f, 1f);
            ptTxt.raycastTarget = false;
            navPrevText = ptTxt;

            // 2. Nút mũi tên tiến trang bên phải
            Transform nextNavTr = finishGameButton.transform.Find("NavNextButton");
            GameObject nextNavObj;
            if (nextNavTr == null)
            {
                nextNavObj = new GameObject("NavNextButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                nextNavObj.transform.SetParent(finishGameButton.transform, false);
            }
            else
            {
                nextNavObj = nextNavTr.gameObject;
            }
            RectTransform nRt = nextNavObj.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(1f, 0.5f);
            nRt.anchorMax = new Vector2(1f, 0.5f);
            nRt.pivot = new Vector2(0.5f, 0.5f);
            nRt.anchoredPosition = new Vector2(-50f, 0f);
            nRt.sizeDelta = new Vector2(80f, 42f);
            Image nImg = nextNavObj.GetComponent<Image>();
            nImg.color = new Color(0.24f, 0.12f, 0.06f, 0.88f);
            Button nBtn = nextNavObj.GetComponent<Button>();
            nBtn.onClick.RemoveAllListeners();
            nBtn.onClick.AddListener(() => OnClickNextDiaryPage());
            navNextButton = nBtn;

            Transform nTxtTr = nextNavObj.transform.Find("Text");
            GameObject nTxtObj;
            if (nTxtTr == null)
            {
                nTxtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                nTxtObj.transform.SetParent(nextNavObj.transform, false);
            }
            else
            {
                nTxtObj = nTxtTr.gameObject;
            }
            RectTransform ntRt = nTxtObj.GetComponent<RectTransform>();
            ntRt.anchorMin = Vector2.zero;
            ntRt.anchorMax = Vector2.one;
            ntRt.offsetMin = Vector2.zero;
            ntRt.offsetMax = Vector2.zero;
            Text ntTxt = nTxtObj.GetComponent<Text>();
            ntTxt.font = diaryFont;
            ntTxt.fontSize = 24;
            ntTxt.fontStyle = FontStyle.Bold;
            ntTxt.alignment = TextAnchor.MiddleCenter;
            ntTxt.text = "►";
            ntTxt.color = new Color(1f, 0.92f, 0.72f, 1f);
            ntTxt.raycastTarget = false;
            navNextText = ntTxt;

            // 3. Text ở giữa thanh điều hướng
            Transform mainTxtTr = finishGameButton.transform.Find("Text");
            if (mainTxtTr != null)
            {
                navCenterText = mainTxtTr.GetComponent<Text>();
                if (navCenterText != null)
                {
                    navCenterText.font = diaryFont;
                    navCenterText.alignment = TextAnchor.MiddleCenter;
                }
            }
        }

        // Cập nhật trạng thái hiển thị theo trang hiện tại (mặc định trang 1)
        UpdateDiaryPageDisplay();

        // Đồng bộ cập nhật nội dung cho Modal kết thúc nếu có
        if (endingLetterModal != null)
        {
            Text modalLetterTxt = endingLetterModal.transform.Find("LetterCard/LetterContentBox/LetterText")?.GetComponent<Text>();
            if (modalLetterTxt == null) modalLetterTxt = endingLetterModal.GetComponentInChildren<Text>();
            if (modalLetterTxt != null)
            {
                modalLetterTxt.text = FULL_DIARY_CONTENT;
            }
        }
    }

    /// <summary>
    /// Xử lý khi người chơi chạm lật sang trang nhật ký tiếp theo.
    /// </summary>
    public void OnClickNextDiaryPage()
    {
        if (isPageFlipping || currentDiaryPage >= TOTAL_DIARY_PAGES) return;
        PlaySound(clickChime);
        StartCoroutine(FlipDiaryPageRoutine(true));
    }

    /// <summary>
    /// Xử lý khi người chơi chạm lật quay lại trang nhật ký trước đó.
    /// </summary>
    public void OnClickPrevDiaryPage()
    {
        if (isPageFlipping || currentDiaryPage <= 1) return;
        PlaySound(clickChime);
        StartCoroutine(FlipDiaryPageRoutine(false));
    }

    /// <summary>
    /// Hoạt ảnh lật trang sách mượt mà với 12 frame pixel art từ phải sang trái (hoặc ngược lại).
    /// </summary>
    private IEnumerator FlipDiaryPageRoutine(bool forward)
    {
        if (isPageFlipping) yield break;
        isPageFlipping = true;

        // Phát âm thanh lật trang nhẹ nhàng
        if (pageFlipChime != null && audioSource != null)
        {
            audioSource.PlayOneShot(pageFlipChime, sfxVolume);
        }

        // Fade out mượt nội dung trang cũ
        CanvasGroup contentCg = diaryContentContainer != null ? diaryContentContainer.GetComponent<CanvasGroup>() : null;
        if (contentCg != null)
        {
            float fadeElapsed = 0f;
            float fadeDuration = 0.10f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                contentCg.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                yield return null;
            }
            contentCg.alpha = 0f;
        }

        // Chạy lần lượt 12 frame chuyển động lật trang từ phải sang trái (hoặc ngược lại)
        if (pageFlipFrames != null && pageFlipFrames.Length > 0 && floatingDiaryImage != null)
        {
            float frameDuration = 0.045f; // ~22 FPS, tổng thời gian ~0.54s
            int totalFrames = pageFlipFrames.Length;

            if (forward)
            {
                for (int i = 0; i < totalFrames; i++)
                {
                    floatingDiaryImage.sprite = pageFlipFrames[i];
                    yield return new WaitForSecondsRealtime(frameDuration);
                }
            }
            else
            {
                for (int i = totalFrames - 1; i >= 0; i--)
                {
                    floatingDiaryImage.sprite = pageFlipFrames[i];
                    yield return new WaitForSecondsRealtime(frameDuration);
                }
            }
        }

        // Khi lật xong, đưa sprite cuốn nhật ký về trạng thái mở trọn vẹn
        if (bookOpeningFrames != null && bookOpeningFrames.Length > 0 && floatingDiaryImage != null)
        {
            floatingDiaryImage.sprite = bookOpeningFrames[bookOpeningFrames.Length - 1];
        }

        currentDiaryPage = forward ? Mathf.Min(currentDiaryPage + 1, TOTAL_DIARY_PAGES) : Mathf.Max(currentDiaryPage - 1, 1);
        UpdateDiaryPageDisplay();

        // Fade in mượt nội dung trang mới
        if (contentCg != null)
        {
            float fadeElapsed = 0f;
            float fadeDuration = 0.22f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                contentCg.alpha = Mathf.Lerp(0f, 1f, fadeElapsed / fadeDuration);
                yield return null;
            }
            contentCg.alpha = 1f;
        }

        isPageFlipping = false;
    }

    /// <summary>
    /// Cập nhật hiển thị nội dung theo trang hiện tại (Trang 1 đến 7)
    /// </summary>
    public void UpdateDiaryPageDisplay()
    {
        bool canGoPrev = (currentDiaryPage > 1);
        bool canGoNext = (currentDiaryPage < TOTAL_DIARY_PAGES);

        // Trang bên trái: Hiển thị tranh & chú thích theo từng trang
        Transform leftContainer = diaryContentContainer != null ? diaryContentContainer.transform.Find("LeftPageContainer") : null;
        if (leftContainer != null)
        {
            leftContainer.gameObject.SetActive(true);

            Transform borderTr = leftContainer.Find("PhotoBorder");
            if (borderTr != null) borderTr.gameObject.SetActive(true);

            // Bức tranh bên trái: 1 -> Tết, 2 -> Hội An, 3 -> Seoul, 4 -> Gyeongbokgung, 5 -> Tết Seollal, 6 -> Vịnh Hạ Long, 7 -> Busan
            if (diaryLeftPhoto != null)
            {
                diaryLeftPhoto.gameObject.SetActive(true);
                if (currentDiaryPage == 1) diaryLeftPhoto.sprite = diaryTetPhotoSprite;
                else if (currentDiaryPage == 2) diaryLeftPhoto.sprite = diaryHoiAnPhotoSprite;
                else if (currentDiaryPage == 3) diaryLeftPhoto.sprite = diarySeoulPhotoSprite;
                else if (currentDiaryPage == 4) diaryLeftPhoto.sprite = diaryGyeongbokgungPhotoSprite;
                else if (currentDiaryPage == 5) diaryLeftPhoto.sprite = diarySeollalPhotoSprite;
                else if (currentDiaryPage == 6) diaryLeftPhoto.sprite = diaryHaLongPhotoSprite;
                else if (currentDiaryPage == 7) diaryLeftPhoto.sprite = diaryBusanPhotoSprite;
            }

            // Dòng chữ chú thích bên dưới bức ảnh
            if (diaryLeftPhotoCaption != null)
            {
                diaryLeftPhotoCaption.gameObject.SetActive(true);
                if (currentDiaryPage == 1) diaryLeftPhotoCaption.text = DIARY_PAGE1_LEFT_CAPTION;
                else if (currentDiaryPage == 2) diaryLeftPhotoCaption.text = DIARY_PAGE2_LEFT_CAPTION;
                else if (currentDiaryPage == 3) diaryLeftPhotoCaption.text = DIARY_PAGE3_LEFT_CAPTION;
                else if (currentDiaryPage == 4) diaryLeftPhotoCaption.text = DIARY_PAGE4_LEFT_CAPTION;
                else if (currentDiaryPage == 5) diaryLeftPhotoCaption.text = DIARY_PAGE5_LEFT_CAPTION;
                else if (currentDiaryPage == 6) diaryLeftPhotoCaption.text = DIARY_PAGE6_LEFT_CAPTION;
                else if (currentDiaryPage == 7) diaryLeftPhotoCaption.text = DIARY_PAGE7_LEFT_CAPTION;
            }

            // Nút và gợi ý lùi trang ở trang trái (chỉ hiện khi ở Trang > 1)
            Transform prevBtnTr = leftContainer.Find("PrevDiaryPageButton");
            if (prevBtnTr != null) prevBtnTr.gameObject.SetActive(canGoPrev);

            Transform prevPromptTr = leftContainer.Find("PrevPagePrompt");
            if (prevPromptTr != null) prevPromptTr.gameObject.SetActive(canGoPrev);
        }
        else
        {
            if (diaryLeftPhoto != null)
            {
                diaryLeftPhoto.gameObject.SetActive(true);
                if (currentDiaryPage == 1) diaryLeftPhoto.sprite = diaryTetPhotoSprite;
                else if (currentDiaryPage == 2) diaryLeftPhoto.sprite = diaryHoiAnPhotoSprite;
                else if (currentDiaryPage == 3) diaryLeftPhoto.sprite = diarySeoulPhotoSprite;
                else if (currentDiaryPage == 4) diaryLeftPhoto.sprite = diaryGyeongbokgungPhotoSprite;
                else if (currentDiaryPage == 5) diaryLeftPhoto.sprite = diarySeollalPhotoSprite;
                else if (currentDiaryPage == 6) diaryLeftPhoto.sprite = diaryHaLongPhotoSprite;
                else if (currentDiaryPage == 7) diaryLeftPhoto.sprite = diaryBusanPhotoSprite;
            }
            if (diaryLeftPhotoCaption != null)
            {
                diaryLeftPhotoCaption.gameObject.SetActive(true);
                if (currentDiaryPage == 1) diaryLeftPhotoCaption.text = DIARY_PAGE1_LEFT_CAPTION;
                else if (currentDiaryPage == 2) diaryLeftPhotoCaption.text = DIARY_PAGE2_LEFT_CAPTION;
                else if (currentDiaryPage == 3) diaryLeftPhotoCaption.text = DIARY_PAGE3_LEFT_CAPTION;
                else if (currentDiaryPage == 4) diaryLeftPhotoCaption.text = DIARY_PAGE4_LEFT_CAPTION;
                else if (currentDiaryPage == 5) diaryLeftPhotoCaption.text = DIARY_PAGE5_LEFT_CAPTION;
                else if (currentDiaryPage == 6) diaryLeftPhotoCaption.text = DIARY_PAGE6_LEFT_CAPTION;
                else if (currentDiaryPage == 7) diaryLeftPhotoCaption.text = DIARY_PAGE7_LEFT_CAPTION;
            }
        }

        // Trang bên phải: Hiển thị nội dung nhật ký tương ứng theo từng trang
        if (diaryRightPageText != null)
        {
            if (currentDiaryPage == 1)
            {
                diaryRightPageText.fontSize = 14;
                diaryRightPageText.lineSpacing = 1.3f;
                diaryRightPageText.text = DIARY_PAGE1_RIGHT_CONTENT;
            }
            else if (currentDiaryPage == 2)
            {
                diaryRightPageText.fontSize = 13;
                diaryRightPageText.lineSpacing = 1.2f;
                diaryRightPageText.text = DIARY_PAGE2_RIGHT_CONTENT;
            }
            else if (currentDiaryPage == 3)
            {
                diaryRightPageText.fontSize = 13;
                diaryRightPageText.lineSpacing = 1.2f;
                diaryRightPageText.text = DIARY_PAGE3_RIGHT_CONTENT;
            }
            else if (currentDiaryPage == 4)
            {
                diaryRightPageText.fontSize = 13;
                diaryRightPageText.lineSpacing = 1.2f;
                diaryRightPageText.text = DIARY_PAGE4_RIGHT_CONTENT;
            }
            else if (currentDiaryPage == 5)
            {
                diaryRightPageText.fontSize = 12;
                diaryRightPageText.lineSpacing = 1.15f;
                diaryRightPageText.text = DIARY_PAGE5_RIGHT_CONTENT;
            }
            else if (currentDiaryPage == 6)
            {
                diaryRightPageText.fontSize = 12;
                diaryRightPageText.lineSpacing = 1.15f;
                diaryRightPageText.text = DIARY_PAGE6_RIGHT_CONTENT;
            }
            else if (currentDiaryPage == 7)
            {
                diaryRightPageText.fontSize = 12;
                diaryRightPageText.lineSpacing = 1.15f;
                diaryRightPageText.text = DIARY_PAGE7_RIGHT_CONTENT;
            }
        }

        // Nút và gợi ý sang trang ở trang phải (chỉ hiện khi chưa đến trang cuối: currentDiaryPage < TOTAL_DIARY_PAGES)
        if (nextDiaryPageButton != null) nextDiaryPageButton.gameObject.SetActive(canGoNext);
        if (nextPagePrompt != null) nextPagePrompt.SetActive(canGoNext);

        // Cập nhật 2 nút mũi tên điều hướng trên thanh nhật ký (Đã xóa bỏ chữ "Sang trang"):
        // - Mũi tên trái ◄: quay lại trang trước.
        // - Mũi tên phải ►: chuyển sang trang tiếp theo.
        if (navPrevButton != null)
        {
            navPrevButton.gameObject.SetActive(true);
            navPrevButton.interactable = canGoPrev;
            if (navPrevText != null)
            {
                navPrevText.color = canGoPrev ? new Color(1f, 0.92f, 0.72f, 1f) : new Color(1f, 0.92f, 0.72f, 0.25f);
            }
        }

        if (navNextButton != null)
        {
            navNextButton.gameObject.SetActive(canGoNext);
            navNextButton.interactable = canGoNext;
            if (navNextText != null)
            {
                navNextText.color = canGoNext ? new Color(1f, 0.92f, 0.72f, 1f) : new Color(1f, 0.92f, 0.72f, 0.25f);
            }
        }

        // Cập nhật text ở giữa thanh điều hướng
        Text centerTxt = navCenterText;
        if (centerTxt == null && finishGameButton != null)
        {
            centerTxt = finishGameButton.GetComponentInChildren<Text>();
        }

        if (centerTxt != null)
        {
            if (currentDiaryPage < TOTAL_DIARY_PAGES)
            {
                centerTxt.text = $"{currentDiaryPage} / {TOTAL_DIARY_PAGES}";
                centerTxt.fontSize = 15;
                centerTxt.fontStyle = FontStyle.Normal;
                centerTxt.color = new Color(0.88f, 0.76f, 0.60f, 0.92f);
            }
            else
            {
                centerTxt.text = "★ HOÀN THÀNH HÀNH TRÌNH (VỀ MENU CHÍNH) ★";
                centerTxt.fontSize = 16;
                centerTxt.fontStyle = FontStyle.Bold;
                centerTxt.color = new Color(1f, 0.92f, 0.55f, 1f);
            }
        }
    }

    private IEnumerator FadeInDiaryContentRoutine(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    #endregion

    #endregion

    #endregion
}
