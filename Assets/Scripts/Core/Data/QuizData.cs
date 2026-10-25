using System;
using System.Collections.Generic;

namespace JourneyOfMemories.Core.Data
{
    [Serializable]
    public class QuizData
    {
        public string QuizId;
        public string Title;
        public string QuestionText;
        public string OptionA;
        public string OptionB;
        public string OptionC;
        public string OptionD;
        public QuizOption CorrectOption;
        public string FeedbackCorrect;
        public string HintWrong;
        public string RewardItemId;

        public QuizData() { }

        public QuizData(
            string quizId,
            string title,
            string questionText,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            QuizOption correctOption,
            string feedbackCorrect,
            string hintWrong,
            string rewardItemId)
        {
            QuizId = quizId;
            Title = title;
            QuestionText = questionText;
            OptionA = optionA;
            OptionB = optionB;
            OptionC = optionC;
            OptionD = optionD;
            CorrectOption = correctOption;
            FeedbackCorrect = feedbackCorrect;
            HintWrong = hintWrong;
            RewardItemId = rewardItemId;
        }

        public string GetOptionText(QuizOption option)
        {
            switch (option)
            {
                case QuizOption.A: return OptionA;
                case QuizOption.B: return OptionB;
                case QuizOption.C: return OptionC;
                case QuizOption.D: return OptionD;
                default: return string.Empty;
            }
        }
    }

    public static class DefaultQuizDatabase
    {
        public static readonly Dictionary<string, QuizData> Quizzes = new Dictionary<string, QuizData>
        {
            {
                "Q01",
                new QuizData(
                    "Q01",
                    "Thịt kho trứng",
                    "Trong một gia đình Việt, vì sao thịt kho trứng thường được nấu thành một nồi lớn để dùng trong nhiều bữa, đặc biệt vào dịp Tết ở một số vùng miền?",
                    "Vì món ăn này bắt buộc phải được nấu thật nhiều thì thịt mới mềm.",
                    "Vì món ăn có thể dùng nhiều bữa sau khi được bảo quản phù hợp, đồng thời gắn với sự chuẩn bị chu đáo cho gia đình.",
                    "Vì trứng chỉ có thể được nấu chín khi kết hợp với thịt trong một nồi lớn.",
                    "Vì đây là món ăn chỉ được phép nấu vào dịp Tết theo phong tục Việt Nam.",
                    QuizOption.B,
                    "Món ăn gắn với sự chuẩn bị chu đáo cho gia đình và có thể dùng nhiều bữa khi được bảo quản phù hợp.",
                    "Hãy nghĩ đến cách gia đình chuẩn bị bữa ăn, không phải một quy định bắt buộc về lượng nấu.",
                    "Food_ThitKhoTrung"
                )
            },
            {
                "Q02",
                new QuizData(
                    "Q02",
                    "Hũ kim chi",
                    "Điều gì làm cho kim chi khác với một loại rau chỉ được ngâm muối thông thường?",
                    "Kim chi luôn phải được nấu chín trước khi ăn.",
                    "Kim chi chỉ có vị cay do sử dụng ớt bột Hàn Quốc.",
                    "Quá trình lên men tạo ra những biến đổi về hương vị và đặc tính của thực phẩm.",
                    "Kim chi không thể được làm từ nguyên liệu nào khác ngoài cải thảo.",
                    QuizOption.C,
                    "Điểm cần chú ý là quá trình lên men làm thay đổi hương vị và đặc tính của thực phẩm.",
                    "Đừng chỉ nhìn màu sắc hoặc độ cay; hãy nghĩ tới quá trình diễn ra trong hũ.",
                    "Food_Kimchi"
                )
            },
            {
                "Q03",
                new QuizData(
                    "Q03",
                    "Đĩa kimbap",
                    "Kimbap và cơm cuộn sushi đều có thể gồm cơm cùng các nguyên liệu cuộn trong rong biển. Muốn phân biệt hai món một cách hợp lý nhất, nên dựa vào điều gì?",
                    "Mọi loại kimbap đều có thịt, còn mọi loại sushi đều có cá sống.",
                    "Kimbap thuộc văn hóa ẩm thực Hàn Quốc, sushi thuộc văn hóa ẩm thực Nhật Bản; cần xét cách nêm cơm, nguyên liệu và phong cách chế biến thay vì chỉ nhìn hình dạng.",
                    "Kimbap luôn được ăn nóng, còn sushi luôn phải được ăn lạnh.",
                    "Hai món hoàn toàn giống nhau, chỉ khác tên gọi ở mỗi quốc gia.",
                    QuizOption.B,
                    "Hình dáng gần giống chưa đủ để kết luận hai món như nhau. Cần xét bối cảnh ẩm thực và cách chế biến.",
                    "Chú ý những lựa chọn dùng từ tuyệt đối như 'mọi loại' hoặc 'luôn'.",
                    "Food_ComCuon"
                )
            },
            {
                "Q04",
                new QuizData(
                    "Q04",
                    "Áo dài Việt Nam",
                    "Hạ Ân thấy mẹ mặc áo dài trong một bức ảnh gia đình. Kết luận nào thể hiện cách hiểu phù hợp nhất về giá trị văn hóa của áo dài ngày nay?",
                    "Áo dài chỉ có giá trị khi được mặc đúng một kiểu dáng cổ xưa duy nhất.",
                    "Áo dài là trang phục chỉ dành cho phụ nữ và không có ý nghĩa nào ngoài thẩm mỹ.",
                    "Áo dài là biểu tượng văn hóa Việt Nam, vừa mang giá trị nhận diện và thẩm mỹ, vừa có thể biến đổi theo thời đại và hoàn cảnh sử dụng.",
                    "Mặc áo dài là điều kiện bắt buộc để một người được xem là hiểu văn hóa Việt Nam.",
                    QuizOption.C,
                    "Một biểu tượng văn hóa có thể giữ ý nghĩa nhận diện và vẫn thay đổi theo thời đại, hoàn cảnh.",
                    "Giá trị truyền thống không có nghĩa là mọi cách thể hiện phải bất biến.",
                    "Clothing_AoDai"
                )
            },
            {
                "Q05",
                new QuizData(
                    "Q05",
                    "Hanbok Hàn Quốc",
                    "Hạ Ân muốn xác định bộ trang phục của bố trong ảnh là Hanbok nam. Tổ hợp đặc điểm nào phù hợp nhất với trang phục truyền thống nam Hàn Quốc?",
                    "Jeogori kết hợp với Baji (quần), có thể đi cùng áo choàng Durumagi.",
                    "Jeogori kết hợp với Chima (váy) là cấu tạo cơ bản bắt buộc của mọi Hanbok nam.",
                    "Áo dài xẻ tà kết hợp với quần trắng và khăn đóng.",
                    "Kimono kết hợp với Hakama và dây Obi.",
                    QuizOption.A,
                    "Trong câu hỏi này, tổ hợp phù hợp là Jeogori và Baji, có thể kèm Durumagi.",
                    "Quan sát phần quần trong ảnh của bố và đối chiếu tên thành phần.",
                    "Clothing_Hanbok"
                )
            },
            {
                "Q06",
                new QuizData(
                    "Q06",
                    "Nửa ngọc hoa sen",
                    "Trong nhiều cách diễn giải văn hóa Việt Nam, hoa sen thường gắn với sự thanh khiết. Cách giải thích nào hợp lý nhất khi hình ảnh hoa sen được dùng trên một món đồ lưu niệm của gia đình Hạ Ân?",
                    "Hoa sen chỉ có một ý nghĩa cố định và được mọi người Việt Nam hiểu giống hệt nhau.",
                    "Hoa sen có thể gợi sự thanh khiết, sức sống và khả năng vươn lên; ý nghĩa cụ thể còn phụ thuộc vào bối cảnh và người sử dụng biểu tượng.",
                    "Hoa sen chứng minh rằng mọi truyền thống Việt Nam đều bắt nguồn từ một tôn giáo duy nhất.",
                    "Hoa sen chỉ là họa tiết trang trí, không thể mang ý nghĩa văn hóa.",
                    QuizOption.B,
                    "Biểu tượng có thể gợi nhiều ý nghĩa. Trong món kỷ vật này, câu chuyện của gia đình giúp xác định ý nghĩa cụ thể.",
                    "Một biểu tượng không buộc mọi người phải có cùng một cách hiểu.",
                    "Gem_LotusHalf"
                )
            },
            {
                "Q07",
                new QuizData(
                    "Q07",
                    "Nửa ngọc Mugunghwa",
                    "Mugunghwa (hoa dâm bụt Syriac, Rose of Sharon) là quốc hoa của Hàn Quốc. Khi đặt biểu tượng này cạnh hoa sen trên viên ngọc của Hạ Ân, cách hiểu nào phù hợp nhất với chủ đề câu chuyện?",
                    "Hai biểu tượng phải được phân định rõ ràng vì một người chỉ có thể thuộc về một quốc gia.",
                    "Biểu tượng Mugunghwa làm mất đi ý nghĩa của hoa sen khi hai hình ảnh xuất hiện cùng nhau.",
                    "Mỗi biểu tượng có lịch sử và ý nghĩa riêng, nhưng việc đặt chúng cạnh nhau có thể thể hiện sự kết nối mà không xóa bỏ bản sắc của mỗi bên.",
                    "Hai loài hoa có cùng nguồn gốc và luôn mang một ý nghĩa giống hệt nhau.",
                    QuizOption.C,
                    "Hai hình ảnh cùng xuất hiện để nói về sự kết nối. Mỗi bên vẫn giữ câu chuyện và ý nghĩa của mình.",
                    "Hãy liên hệ với mâm cơm và bức ảnh gia đình đã được khôi phục.",
                    "Gem_MugunghwaHalf"
                )
            },
            {
                "Q_Cat",
                new QuizData(
                    "Q_Cat",
                    "Văn hóa ăn cay của người Hàn Quốc",
                    "Vì sao vị cay (đặc biệt là từ ớt và tương ớt Gochujang) lại trở thành một nét đặc trưng và được ưa chuộng lâu đời trong ẩm thực truyền thống của người Hàn Quốc?",
                    "Do người Hàn Quốc từ xưa chỉ thích các món ăn có nhiệt độ cao và vị gắt để kích thích thị giác khi ăn hoàng gia.",
                    "Do sự kết hợp của điều kiện khí hậu (mùa đông lạnh giá), nhu cầu bảo quản thực phẩm thời xưa, và quan niệm y học cổ truyền giúp làm ấm cơ thể, giải tỏa mồ hôi.",
                    "Do ảnh hưởng hoàn toàn từ phong cách ẩm thực của các nước phương Tây từ thế kỷ XIX.",
                    "Do nhà nước Hàn Quốc từng ban hành sắc lệnh bắt buộc toàn dân phải ăn cay để tăng cường sức khỏe công cộng.",
                    QuizOption.B,
                    "Vị cay trong ẩm thực Hàn Quốc gắn liền với sự thích ứng tự nhiên (giúp làm ấm cơ thể trong mùa đông lạnh), vai trò lịch sử của ớt trong việc bảo quản/tạo hương vị, và triết lý cân bằng cơ thể của y học Á Đông.",
                    "Chưa đúng, hãy thử lại nhé! Hãy nghĩ về điều kiện khí hậu và thói quen sinh hoạt truyền thống.",
                    null
                )
            },
            {
                "Q_BanhMi",
                new QuizData(
                    "Q_BanhMi",
                    "Ổ bánh mì thịt",
                    "Trong quá trình hình thành, bánh mì thịt Việt Nam có nguồn gốc từ bánh baguette của Pháp. Điều gì phản ánh rõ nhất sự sáng tạo và tiếp biến văn hóa của người Việt trong món ăn này?",
                    "Bánh mì Việt Nam bắt buộc phải sử dụng nguyên liệu nhập khẩu 100% từ Pháp để giữ nguyên bản hương vị gốc.",
                    "Bánh mì là minh chứng cho sự giao thoa, khi lớp vỏ được cải biên cho mỏng, giòn, rỗng ruột hơn để nhồi sự kết hợp hài hòa giữa các loại thịt, chả và rau chua mang đậm hương vị bản địa.",
                    "Bánh mì thực chất chỉ là một cách để người Việt tiêu thụ lúa mì thay thế cho gạo trong những thời kỳ thiếu thốn lương thực.",
                    "Người Việt chỉ đơn thuần sao chép lại hình dáng bánh của Pháp mà không có bất kỳ sự thay đổi nào về cấu trúc hay nhân bên trong.",
                    QuizOption.B,
                    "Bánh mì Việt Nam là minh chứng sáng tạo cho sự giao thoa văn hóa, biến tấu vỏ giòn rỗng ruột kết hợp nhân thịt, chả và rau chua bản địa.",
                    "Chưa đúng, hãy thử lại nhé! Hãy chú ý sự cải biên cấu trúc vỏ bánh và phần nhân mang hương vị bản địa.",
                    "Food_BanhMi"
                )
            },
            {
                "Q_Tteokbokki",
                new QuizData(
                    "Q_Tteokbokki",
                    "Đĩa bánh gạo cay (Tteokbokki)",
                    "Tteokbokki với nước sốt đỏ rực là một biểu tượng của ẩm thực đường phố Hàn Quốc hiện đại. Điểm mấu chốt nào tạo nên linh hồn của phiên bản này so với các loại bánh gạo xào thông thường?",
                    "Vị cay ngọt đặc trưng không đến từ ớt tươi thông thường, mà nhờ sự kết hợp của tương ớt Gochujang lên men truyền thống, tạo nên sự cân bằng hương vị và màu sắc bắt mắt.",
                    "Tteokbokki phải luôn được làm từ bột mì thay vì bột gạo để đảm bảo độ dai, và chỉ sử dụng phẩm màu để tạo màu đỏ.",
                    "Món ăn này bắt buộc phải được ăn lạnh thì mới giữ được độ dẻo của bánh gạo và làm dịu đi vị cay nồng của nước sốt.",
                    "Bánh gạo cay màu đỏ vốn là món ăn lâu đời, chỉ được phục vụ riêng cho các vị vua trong hoàng gia Hàn Quốc từ thời cổ đại.",
                    QuizOption.A,
                    "Vị cay ngọt đặc trưng và màu sắc bắt mắt của Tteokbokki đến từ tương ớt Gochujang lên men truyền thống.",
                    "Chưa đúng, hãy thử lại nhé! Hãy chú ý loại gia vị lên men truyền thống tạo nên linh hồn món ăn.",
                    "Food_Tteokbokki"
                )
            }
        };
    }
}
