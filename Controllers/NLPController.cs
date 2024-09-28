using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace NLPCharToNumProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NLPController : ControllerBase
    {
        [HttpPost("convert")]
        public IActionResult ConvertTextToNum([FromBody] InputModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.UserText))
                return BadRequest(new { Error = "Girilen değer geçersiz." });

            // Sayı dönüştürme işlemi yapılacak yer
            string convertedText = ConvertTextToNum(input.UserText);

            var output = new OutputModel { Output = convertedText };

            return Ok(output);
        }

        private string ConvertTextToNum(string text)
        {
            var numberMappedData = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"bir", 1 },
                {"iki", 2 },
                {"üç", 3 },
                {"dört", 4 },
                {"beş", 5 },
                {"altı", 6 },
                {"yedi", 7 },
                {"sekiz", 8 },
                {"dokuz", 9 },
                {"on", 10 },
                {"yirmi", 20 },
                {"otuz", 30 },
                {"kırk", 40 },
                {"elli", 50 },
                {"altmış", 60 },
                {"yetmiş", 70 },
                {"seksen", 80 },
                {"doksan", 90 },
                {"yüz", 100 },
                {"bin", 1000 }
            };

            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int result = 0;
            int tempData = 0; // Geçici veri (toplama yapılacak veri)
            int total = 0; // Sayıların toplamı için kullanılan değişken (tamamlanmış sayılar)
            bool inNumberBlock = false; // Sayısal bir kelime içinde miyiz? (Ör: Yüz yetmiş beş )

            var tempText = new StringBuilder();

            foreach (var word in words)
            {
                string trimmedWord = word.Trim();

                if (numberMappedData.TryGetValue(trimmedWord, out int data))
                {
                    inNumberBlock = true;

                    if (data == 1000) // "bin" kelimesi için işlem
                    {
                        if (tempData == 0) tempData = 1; // "bir bin" demediysek, 1 bin olarak kabul edilir
                        result += tempData * data;
                        tempData = 0; // Bu blok tamamlandı, tempData sıfırlanır
                    }
                    else if (data == 100) // "yüz" kelimesi için işlem
                    {
                        if (tempData == 0) tempData = 1; // "bir yüz" demediysek, 1 yüz olarak kabul edilir
                        tempData *= data; // Yüz ile çarpma yapılır
                    }
                    else
                    {
                        tempData += data; // Temel sayılar toplanır (onlar, birler vs.)
                    }
                }
                else
                {
                    // Sayısal kelime bloğu bitti, toplanmış veriyi ekle
                    if (inNumberBlock)
                    {
                        total += result + tempData; // Geçici verileri toplamaya ekle
                        tempText.Append(total + " ");
                        result = 0;
                        tempData = 0;
                        total = 0;
                        inNumberBlock = false; // Sayısal blok bitti
                    }
                    tempText.Append(trimmedWord + " "); // Sayısal olmayan kelimeleri ekle
                }
            }

            // Son sayısal bloğu ekle
            if (inNumberBlock)
            {
                total += result + tempData;
                tempText.Append(total + " ");
            }

            return tempText.ToString().Trim();
        }

    }

    public class InputModel
    {
        [DefaultValue("yüz bin lira kredi kullanmak istiyorum")]
        public string UserText { get; set; }
    }

    public class OutputModel
    {
        public string Output { get; set; }
    }
}
