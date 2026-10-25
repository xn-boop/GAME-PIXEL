using UnityEngine;

public static class L2_QuizDatabase
{
    public static readonly L2_QuizData CatQuiz = new L2_QuizData(
        "NPC_CAT",
        "CHÚ MÈO THÔNG THÁI",
        "Trong thiết kế trang phục truyền thống, tà áo dài truyền thống của Việt Nam và chiếc áo Jeogori (áo khoác ngắn) trong bộ Hanbok của Hàn Quốc có sự khác biệt rất rõ rệt về điểm nhấn cấu trúc ở phần cổ áo. Điểm đặc trưng nào sau đây chỉ có trên áo Jeogori của Hanbok mà không có trên Áo dài?",
        "Phần cổ vạt chéo kết hợp với dải dây vải thắt nơ dài rủ xuống ở ngực (Otgoreum).",
        "Phần cổ tàu đứng ôm sát cổ được đính nút bấm hoặc khuy bấm chéo sang bên hông.",
        "Phần đường xẻ tà cao từ eo xuống giúp người mặc dễ dàng di chuyển.",
        "Phần hoa văn thêu rồng phụng bằng chỉ vàng nổi bật trực tiếp trên vạt áo trước.",
        0
    );

    public static readonly L2_QuizData Painting1 = new L2_QuizData(
        "PAINTING_1",
        "TRANH 1: VỊNH HẠ LONG",
        "Tên gọi 'Hạ Long' gắn liền với truyền thuyết đàn rồng đáp xuống giúp người Việt đánh giặc. Theo dân gian, khu vực nơi Rồng Mẹ đáp xuống chính là Vịnh Hạ Long, vậy khu vực nơi Đàn Rồng Con đáp xuống được gọi là gì?",
        "Vịnh Bái Tử Long.",
        "Vịnh Lan Hạ.",
        "Đảo Cô Tô.",
        "Đảo Cát Bà.",
        0
    );

    public static readonly L2_QuizData Painting2 = new L2_QuizData(
        "PAINTING_2",
        "TRANH 2: CHÙA MỘT CỘT",
        "Kiến trúc biểu tượng của Chùa Một Cột được thiết kế mô phỏng hình dáng của loài hoa nào, và trong các tư liệu lịch sử thời Lý, ngôi đài chính này có tên gọi gốc là gì?",
        "Hoa súng – Đài Tịnh Độ.",
        "Hoa sen – Liên Hoa Đài.",
        "Hoa sen – Từ Bi Đài.",
        "Hoa mẫu đơn – Bảo Liên Đài.",
        1
    );

    public static readonly L2_QuizData Painting3 = new L2_QuizData(
        "PAINTING_3",
        "TRANH 3: CẦU VÀNG",
        "Công trình Cầu Vàng (Đà Nẵng) do đơn vị kiến trúc nào của Việt Nam chủ trì thiết kế, và chất liệu tạo nên bề mặt diện mạo 'rêu phong cổ kính' của đôi bàn tay khổng lồ là gì?",
        "Võ Trọng Nghĩa Architects – Đá hoa cương tự nhiên.",
        "TA Landscape Architecture – Khung thép phủ lưới xơ dừa và vữa chuyên dụng.",
        "Foster + Partners – Xi măng đúc nguyên khối.",
        "VTN Architects – Bê tông cốt thép sơn mài.",
        1
    );

    public static readonly L2_QuizData Painting4 = new L2_QuizData(
        "PAINTING_4",
        "TRANH 4: THÁP N SEOUL / NAMSAN",
        "Ban đầu khi được hoàn thành vào năm 1969, tháp truyền hình Namsan được xây dựng với mục đích chính nào sau đây trước khi trở thành một điểm tham quan du lịch nổi tiếng?",
        "Tháp đài quan sát quân sự trong thời kỳ Chiến tranh Lạnh.",
        "Tháp phát sóng đài truyền hình và radio đầu tiên của Hàn Quốc.",
        "Tượng đài kỷ niệm chiến thắng của quân đội Hàn Quốc.",
        "Trạm trung chuyển hàng không của thành phố Seoul.",
        1
    );

    public static readonly L2_QuizData Painting5 = new L2_QuizData(
        "PAINTING_5",
        "TRANH 5: THÁP CHEOMSEONGDAE",
        "Tháp Cheomseongdae được ghép từ chính xác 365 khối đá, tượng trưng cho 365 ngày trong năm. Kiến trúc này có bao nhiêu tầng đá tính từ đế lên đến đỉnh tháp, tượng trưng cho điều gì trong thiên văn cổ đại?",
        "24 tầng – tượng trưng cho 24 tiết khí trong năm.",
        "27 tầng – tượng trưng cho Nữ vương Seondeok (triều đại thứ 27 của Silla).",
        "28 tầng – tượng trưng cho 28 chòm sao (Nhị thập bát tú).",
        "30 tầng – tượng trưng cho số ngày trong một tháng âm lịch.",
        2
    );

    public static readonly L2_QuizData Painting6 = new L2_QuizData(
        "PAINTING_6",
        "TRANH 6: CUNG ĐIỆN GYEONGBOKGUNG",
        "Tên gọi 'Gyeongbokgung' (Cảnh Phúc Cung) mang ý nghĩa 'Nguyện cầu cho nhà vua và triều đại mới được hưởng phúc lớn'. Tên gọi này do danh thần nổi tiếng nào của triều đại Joseon đặt dựa trên một bài thơ trong Kinh Thi?",
        "Jeong Do-jeon (Zheng Daochuan / Trịnh Đạo Truyền).",
        "Yi Hwang (Lý Hoàng / Toegye).",
        "Yi I (Lý Y / Yulgok).",
        "Sejong Đại đế.",
        0
    );

    // Áo Dài Việt Nam
    public static readonly L2_QuizData AoDaiQuiz = new L2_QuizData(
        "ITEM_AODAI",
        "THU THẬP: ÁO DÀI VIỆT NAM",
        "Hạ Ân thấy mẹ mặc áo dài trong một bức ảnh gia đình. Kết luận nào thể hiện cách hiểu phù hợp nhất về giá trị văn hóa của áo dài ngày nay?",
        "Áo dài chỉ có giá trị khi được mặc đúng một kiểu dáng cổ xưa duy nhất.",
        "Áo dài là trang phục chỉ dành cho phụ nữ và không có ý nghĩa nào ngoài thẩm mỹ.",
        "Áo dài là biểu tượng văn hóa Việt Nam, vừa mang giá trị nhận diện và thẩm mỹ, vừa có thể biến đổi theo thời đại và hoàn cảnh sử dụng.",
        "Mặc áo dài là điều kiện bắt buộc để một người được xem là hiểu văn hóa Việt Nam.",
        2
    );

    // Hanbok Hàn Quốc
    public static readonly L2_QuizData HanbokQuiz = new L2_QuizData(
        "ITEM_HANBOK",
        "THU THẬP: HANBOK HÀN QUỐC",
        "Hạ Ân muốn xác định bộ trang phục của bố trong ảnh là Hanbok nam. Tổ hợp đặc điểm nào phù hợp nhất với trang phục truyền thống nam Hàn Quốc?",
        "Jeogori kết hợp với Baji (quần), có thể đi cùng áo choàng Durumagi.",
        "Jeogori kết hợp với Chima (váy) là cấu tạo cơ bản bắt buộc của mọi Hanbok nam.",
        "Áo dài xẻ tà kết hợp với quần trắng và khăn đóng.",
        "Kimono kết hợp với Hakama và dây Obi.",
        0
    );

    // Câu 8: Nón Lá (Item_NonLa) - Nón Bài Thơ Xứ Huế
    public static readonly L2_QuizData NonLaQuiz = new L2_QuizData(
        "Quiz_NonLa",
        "THU THẬP: NÓN LÁ",
        "Trong các hộp đồ lưu niệm gia đình, Hạ Ân tìm thấy một chiếc nón lá truyền thống. Khi đưa nón lên trước ánh sáng, cô nhận ra những hình ảnh bài thơ và danh thắng Việt Nam ẩn hiện tinh tế giữa các lớp lá. Chiếc nón này thuộc loại nón lá nổi tiếng nào và đặc điểm chế tác của nó phản ánh điều gì?",
        "Nón Thúng – Được đan từ tre gầy, chủ yếu dùng cho sinh hoạt lao động nông nghiệp hàng ngày.",
        "Nón Bài Thơ (Xứ Huế) – Sử dụng kỹ thuật chèn hình/thơ giữa hai lớp lá, thể hiện sự tinh tế và tâm hồn thi văn của người Việt.",
        "Nón Quai Thao – Loại nón rộng vành có quai tết chỉ ngũ sắc, dành riêng cho nghệ nhân hát Quan họ.",
        "Nón Ngựa – Loại nón đội khi cưỡi ngựa của giới quan lại phong kiến miền Trung.",
        1,
        "Nón Bài Thơ nổi tiếng của xứ Huế được tạo ra bằng cách cắt tỉa hình ảnh hoặc các câu thơ bằng giấy bản rồi ép khéo léo giữa hai lớp lá nón. Khi soi lên ánh sáng, hình ảnh và dòng thơ mới hiện rõ, tạo nên giá trị nghệ thuật độc đáo."
    );

    // Câu 9: Mũ Gat (Item_MuGat) - Mũ Gat Thời Joseon
    public static readonly L2_QuizData MuGatQuiz = new L2_QuizData(
        "Quiz_MuGat",
        "THU THẬP: MŨ GAT",
        "Hạ Ân nhớ lại cô từng thấy người cha Hàn Quốc của mình đội một chiếc mũ hình trụ cao, vành rộng, màu đen mịn và có thể nhìn xuyên thấu nhẹ. Đây là chiếc mũ Gat (갓) truyền thống. Trong xã hội Hàn Quốc thời Joseon, chiếc mũ này mang giá trị văn hóa nổi bật nào?",
        "Lớp mũ bảo hộ chuyên dụng dành cho binh lính khi đi tuần tra ban đêm.",
        "Mũ che nắng dành cho tầng lớp nông dân lao động ngoài đồng ruộng.",
        "Biểu tượng thể hiện địa vị xã hội, sự tươm tất và phong thái tri thức của tầng lớp quý tộc / nho sĩ (Yangban).",
        "Mũ nghi lễ bắt buộc dành cho chú rể trong ngày cưới, bất kể tầng lớp xuất thân.",
        2,
        "Mũ Gat (đặc biệt là Heunglip - mũ Gat đen làm từ lông đuôi ngựa hoặc tre chẻ sợi siêu nhỏ) là phụ kiện không thể thiếu của các quý tộc Nho sĩ thời Joseon mỗi khi ra khỏi nhà, tượng trưng cho lễ nghi, tri thức và phẩm cấp xã hội."
    );

    public static L2_QuizData GetPaintingQuiz(int index)
    {
        switch (index)
        {
            case 1: return Painting1;
            case 2: return Painting2;
            case 3: return Painting3;
            case 4: return Painting4;
            case 5: return Painting5;
            case 6: return Painting6;
            default: return Painting1;
        }
    }

    public static L2_QuizData GetQuizByItemType(L2_ItemType itemType)
    {
        switch (itemType)
        {
            case L2_ItemType.AoDai:
                return AoDaiQuiz;
            case L2_ItemType.NonLa:
                return NonLaQuiz;
            case L2_ItemType.Hanbok:
                return HanbokQuiz;
            case L2_ItemType.MuGat:
                return MuGatQuiz;
            default:
                return AoDaiQuiz;
        }
    }
}
