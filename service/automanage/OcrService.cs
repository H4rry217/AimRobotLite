using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AimRobotLite.service.automanage {
    public class OcrService {

        public static Dictionary<string, int[]> OcrTextPos(Bitmap bitmap) {
            Dictionary<string, int[]> ocrtext = new Dictionary<string, int[]>();

            using (MemoryStream stream = new MemoryStream()) {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                var bytedata = stream.ToArray();

                using (var client = new HttpClient()) {
                    using (var content = new MultipartFormDataContent()) {

                        content.Add(new ByteArrayContent(bytedata), "image", "image.jpg");
                        var response = client.PostAsync($"{Program.Winform.textBox13.Text}/ocr", content).Result;

                        if (response.IsSuccessStatusCode) {
                            var result = response.Content.ReadAsStringAsync();

                            var jsonDocument = JsonDocument.Parse(result.Result);

                            var jsonRoot = jsonDocument.RootElement.GetProperty("result");

                            var arr = jsonRoot.EnumerateArray();
                            foreach (var textBox in arr) {
                                var boxPos = textBox[0];
                                var text = textBox[1].GetString();

                                var posArr = JsonElementToIntArray2D(boxPos);
                                int[] pos = GetBoxPosition(posArr);

                                ocrtext[text] = pos;
                            }

                        } else {
                            Console.WriteLine($"Error: {response.StatusCode}");
                        }
                    }
                }
            }

            return ocrtext;

        }

        public static object[] GetSimilarWord(string word, string[] words) {

            using (var client = new HttpClient()) {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["word"] = word;
                data["search"] = words;

                string dataString = JsonSerializer.Serialize(data);

                StringContent content = new StringContent(dataString, Encoding.UTF8, "application/json");
                var response = client.PostAsync($"{Program.Winform.textBox13.Text}/similar", content).Result;

                // 处理响应
                if (response.IsSuccessStatusCode) {
                    var result = response.Content.ReadAsStringAsync();

                    var jsonDocument = JsonDocument.Parse(result.Result);

                    return new object[] {
                        jsonDocument.RootElement.GetProperty("result").GetString(),
                        (float)jsonDocument.RootElement.GetProperty("similarity").GetDouble()
                    };
                } else {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }

            return new object[] { string.Empty, 0 };
        }

        private static int[,] JsonElementToIntArray2D(JsonElement jsonElement) {
            if (jsonElement.ValueKind == JsonValueKind.Array) {
                int rowCount = jsonElement.GetArrayLength();
                int colCount = jsonElement[0].GetArrayLength();

                int[,] intArray2D = new int[rowCount, colCount];

                for (int i = 0; i < rowCount; i++) {
                    JsonElement rowElement = jsonElement[i];

                    if (rowElement.ValueKind == JsonValueKind.Array && rowElement.GetArrayLength() == colCount) {
                        for (int j = 0; j < colCount; j++) {
                            try {
                                intArray2D[i, j] = rowElement[j].GetInt32();
                            } catch (System.FormatException e) {
                                //Console.WriteLine(e.Message);
                                //Console.WriteLine(rowElement[j]);
                                intArray2D[i, j] = -1;
                            }
                        }
                    } else {
                        return new int[,] { };
                    }
                }

                return intArray2D;
            } else {
                return new int[,] { };
            }
        }

        private static int[] GetBoxPosition(int[,] box) {
            var boxWidth = box[1, 0] - box[0, 0];
            var boxHeight = box[3, 1] - box[0, 1];

            int x = box[0, 0] + (boxWidth / 2);
            int y = box[0, 1] + (boxHeight / 2);

            return new int[] { x, y };
        }

    }
}
