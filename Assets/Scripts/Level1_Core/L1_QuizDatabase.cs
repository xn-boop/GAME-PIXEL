public static class L1_QuizDatabase
{
    // 1. CÂU HỎI CHỖ CHÚ MÈO
    public static readonly L1_QuizData CatQuiz = new L1_QuizData(
        "NPC_CAT",
        "GỢI Ý TỪ CHÚ MÈO",
        "Vì sao vị cay (đặc biệt là từ ớt và tương ớt Gochujang) lại trở thành một nét đặc trưng và được ưa chuộng lâu đời trong ẩm thực truyền thống của người Hàn Quốc?",
        "Do người Hàn Quốc từ xưa chỉ thích các món ăn có nhiệt độ cao và vị gắt để kích thích thị giác khi ăn hoàng gia.",
        "Do sự kết hợp của điều kiện khí hậu (mùa đông lạnh giá), nhu cầu bảo quản thực phẩm thời xưa, và quan niệm y học cổ truyền giúp làm ấm cơ thể, giải tỏa mồ hôi.",
        "Do ảnh hưởng hoàn toàn từ phong cách ẩm thực của các nước phương Tây từ thế kỷ XIX.",
        "Do nhà nước Hàn Quốc từng ban hành sắc lệnh bắt buộc toàn dân phải ăn cay để tăng cường sức khỏe công cộng.",
        1,
        "Sự kết hợp của điều kiện khí hậu lạnh giá, nhu cầu bảo quản thực phẩm và quan niệm y học cổ truyền đã khiến vị cay trở thành linh hồn trong ẩm thực truyền thống Hàn Quốc."
    );

    // 2. CÂU HỎI MÓN ĂN CHÍNH 1: THỊT KHO TRỨNG
    public static readonly L1_QuizData ThitKhoQuiz = new L1_QuizData(
        "ITEM_THITKHO",
        "THU THẬP: THỊT KHO TRỨNG",
        "Trong một gia đình Việt, vì sao thịt kho trứng thường được nấu thành một nồi lớn để dùng trong nhiều bữa, đặc biệt vào dịp Tết ở một số vùng miền?",
        "Vì món ăn này bắt buộc phải được nấu thật nhiều thì thịt mới mềm.",
        "Vì món ăn có thể dùng nhiều bữa sau khi được bảo quản phù hợp, đồng thời gắn với sự chuẩn bị chu đáo cho gia đình.",
        "Vì trứng chỉ có thể được nấu chín khi kết hợp với thịt trong một nồi lớn.",
        "Vì đây là món ăn chỉ được phép nấu vào dịp Tết theo phong tục Việt Nam.",
        1,
        "Thịt kho trứng có thể dùng nhiều bữa khi bảo quản phù hợp, gắn liền với tình cảm gia đình sum vầy và sự chuẩn bị tươm tất cho ngày Tết."
    );

    // 3. CÂU HỎI MÓN ĂN CHÍNH 2: HŨ KIM CHI
    public static readonly L1_QuizData KimchiQuiz = new L1_QuizData(
        "ITEM_KIMCHI",
        "THU THẬP: KIM CHI",
        "Điều gì làm cho kim chi khác với một loại rau chỉ được ngâm muối thông thường?",
        "Kim chi luôn phải được nấu chín trước khi ăn.",
        "Kim chi chỉ có vị cay do sử dụng ớt bột Hàn Quốc.",
        "Quá trình lên men tạo ra những biến đổi về hương vị và đặc tính của thực phẩm.",
        "Kim chi không thể được làm từ nguyên liệu nào khác ngoài cải thảo.",
        2,
        "Quá trình lên men tự nhiên với men vi sinh tạo nên vị chua thanh, giòn ngọt và giá trị dinh dưỡng độc đáo khác biệt của kim chi."
    );

    // 4. CÂU HỎI MÓN ĂN CHÍNH 3: ĐĨA KIMBAP
    public static readonly L1_QuizData KimbapQuiz = new L1_QuizData(
        "ITEM_KIMBAP",
        "THU THẬP: KIMBAP",
        "Kimbap và cơm cuộn sushi đều có thể gồm cơm cùng các nguyên liệu cuộn trong rong biển. Muốn phân biệt hai món một cách hợp lý nhất, nên dựa vào điều gì?",
        "Mọi loại kimbap đều có thịt, còn mọi loại sushi đều có cá sống.",
        "Kimbap thuộc văn hóa ẩm thực Hàn Quốc, sushi thuộc văn hóa ẩm thực Nhật Bản; cần xét cách nêm cơm, nguyên liệu và phong cách chế biến thay vì chỉ nhìn hình dạng.",
        "Kimbap luôn được ăn nóng, còn sushi luôn phải được ăn lạnh.",
        "Hai món hoàn toàn giống nhau, chỉ khác tên gọi ở mỗi quốc gia.",
        1,
        "Kimbap dùng dầu mè nêm cơm với nhân chín phong phú của Hàn Quốc, còn sushi dùng giấm gạo đặc trưng của Nhật Bản."
    );

    // 5. CÂU HỎI TƯƠNG TÁC PHỤ 1: QUỐC KỲ VIỆT NAM
    public static readonly L1_QuizData FlagVnQuiz = new L1_QuizData(
        "CULTURE_FLAGVN",
        "KHÁM PHÁ: QUỐC KỲ VIỆT NAM",
        "Trên lá quốc kỳ nền đỏ sao vàng năm cánh của Việt Nam, ý nghĩa biểu tượng của ngôi sao vàng năm cánh được diễn giải chính xác nhất theo quan điểm lịch sử và văn hóa là gì?",
        "Năm cánh sao tượng trưng cho năm châu bốn biển, thể hiện tinh thần hội nhập quốc tế của dân tộc Việt Nam.",
        "Năm cánh sao tượng trưng cho sự đoàn kết và các tầng lớp nhân dân cùng chung sức xây dựng đất nước (bao gồm sĩ, nông, công, thương, binh).",
        "Năm cánh sao tượng trưng cho năm ngọn núi lớn linh thiêng nằm ở biên giới phía Bắc của tổ quốc.",
        "Năm cánh sao đại diện cho năm mùa trong năm theo lịch cổ truyền của người Việt xưa.",
        1,
        "Nền đỏ tượng trưng cho dòng máu anh hùng và cách mạng, trong đó ngôi sao vàng năm cánh biểu trưng cho hồn thiêng sông núi và sự đoàn kết của các tầng lớp nhân dân (sĩ, nông, công, thương, binh) trong khối đại đoàn kết dân tộc."
    );

    // 6. CÂU HỎI TƯƠNG TÁC PHỤ 2: QUỐC KỲ HÀN QUỐC (TAEGUKGI)
    public static readonly L1_QuizData FlagKrQuiz = new L1_QuizData(
        "CULTURE_FLAGKR",
        "KHÁM PHÁ: QUỐC KỲ HÀN QUỐC",
        "Quốc kỳ Hàn Quốc (Taegukgi) có nền trắng, trung tâm là vòng tròn âm dương (Taeguk) và bốn quẻ (Geon-gon-gam-ri) ở bốn góc. Triết học cốt lõi nào được thể hiện qua cấu trúc của lá cờ này?",
        "Sự cô lập và khép kín để bảo vệ bản sắc văn hóa trước các thế lực ngoại bang.",
        "Triết học Âm Dương và Ngũ hành về sự cân bằng, hài hòa, vận động không ngừng của vũ trụ và vạn vật.",
        "Khát vọng chinh phục không gian và phát triển công nghệ hiện đại của bán đảo Triều Tiên.",
        "Sự phân định rạch ròi giữa các giai cấp quý tộc và bình dân trong xã hội phong kiến xưa.",
        1,
        "Nền trắng tượng trưng cho sự tinh khiết và hòa bình; vòng tròn Taeguk ở giữa thể hiện sự cân bằng hoàn hảo giữa Âm (màu xanh) và Dương (màu đỏ); bốn quẻ ở bốn góc đại diện cho bầu trời, đất, nước và lửa, biểu trưng cho sự hài hòa và chuyển hóa không ngừng của vũ trụ."
    );

    // 7. CÂU HỎI TƯƠNG TÁC PHỤ 3: Ổ BÁNH MÌ THỊT
    public static readonly L1_QuizData BanhMiQuiz = new L1_QuizData(
        "CULTURE_BANHMI",
        "KHÁM PHÁ: BÁNH MÌ VIỆT NAM",
        "Trong quá trình hình thành, bánh mì thịt Việt Nam có nguồn gốc từ bánh baguette của Pháp. Điều gì phản ánh rõ nhất sự sáng tạo và tiếp biến văn hóa của người Việt trong món ăn này?",
        "Bánh mì Việt Nam bắt buộc phải sử dụng nguyên liệu nhập khẩu 100% từ Pháp để giữ nguyên bản hương vị gốc.",
        "Bánh mì là minh chứng cho sự giao thoa, khi lớp vỏ được cải biên cho mỏng, giòn, rỗng ruột hơn để nhồi sự kết hợp hài hòa giữa các loại thịt, chả và rau dưa chua mang đậm hương vị bản địa.",
        "Bánh mì thực chất chỉ là một cách để người Việt tiêu thụ lúa mì thay thế cho gạo trong những thời kỳ thiếu thốn lương thực.",
        "Người Việt chỉ đơn thuần sao chép lại hình dáng bánh của Pháp mà không có bất kỳ sự thay đổi nào về cấu trúc hay nhân bên trong.",
        1,
        "Bánh mì là minh chứng cho sự giao thoa, khi lớp vỏ được cải biên cho mỏng, giòn, rỗng ruột hơn để nhồi sự kết hợp hài hòa giữa các loại thịt, chả và rau dưa chua mang đậm hương vị bản địa."
    );

    // 8. CÂU HỎI TƯƠNG TÁC PHỤ 4: ĐĨA BÁNH GẠO CAY (TTEOKBOKKI)
    public static readonly L1_QuizData TteokbokkiQuiz = new L1_QuizData(
        "CULTURE_TTEOKBOKKI",
        "KHÁM PHÁ: BÁNH GẠO CAY TTEOKBOKKI",
        "Tteokbokki với nước sốt đỏ rực là một biểu tượng của ẩm thực đường phố Hàn Quốc hiện đại. Điểm mấu chốt nào tạo nên linh hồn của phiên bản này so với các loại bánh gạo xào thông thường?",
        "Vị cay ngọt đặc trưng không đến từ ớt tươi thông thường, mà nhờ sự kết hợp của tương ớt Gochujang lên men truyền thống, tạo nên sự cân bằng hương vị và màu sắc bắt mắt.",
        "Tteokbokki phải luôn được làm từ bột mì thay vì bột gạo để đảm bảo độ dai, và chỉ sử dụng phẩm màu để tạo màu đỏ.",
        "Món ăn này bắt buộc phải được ăn lạnh thì mới giữ được độ dẻo của bánh gạo và làm dịu đi vị cay nồng của nước sốt.",
        "Bánh gạo cay màu đỏ vốn là món ăn lâu đời, chỉ được phục vụ riêng cho các vị vua trong hoàng gia Hàn Quốc từ thời Joseon.",
        0,
        "Vị cay ngọt đặc trưng nhờ sự kết hợp của tương ớt Gochujang lên men truyền thống tạo nên linh hồn quyến rũ của món Tteokbokki."
    );

    public static L1_QuizData GetQuizByItemType(L1_ItemType type)
    {
        switch (type)
        {
            case L1_ItemType.ThitKho: return ThitKhoQuiz;
            case L1_ItemType.Kimchi: return KimchiQuiz;
            case L1_ItemType.Kimbap: return KimbapQuiz;
            default: return null;
        }
    }
}
