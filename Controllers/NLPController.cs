using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace NLPCharToNumProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NLPController : ControllerBase
    {
        private readonly ConvertService _convertService;
        public NLPController()
        {
            _convertService = new ConvertService();
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            var testValues = new List<string>{
            "Yüz bin lira kredi kullanmak istiyorum",
            "Bugün yirmi sekiz yaşıma girdim",
            "Elli altı bin lira kredi alıp üç yılda geri ödeyeceğim",
            "Seksen yedi bin iki yüz on altı lira borcum var",
            "Bin yirmi dört lira eksiğim kaldı",
            "Yarın saat onaltıda geleceğim",
            "Dokuzyüzelli beş lira fiyat var",
            };

            var result = testValues.Select(x => new
            {
                userText = x,
                ConvertedText = _convertService.ConvertTextToNum(x)
            });

            return Ok(result);
        }

        [HttpPost("convert")]
        public IActionResult ConvertText([FromBody] InputModel input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.UserText))
                return BadRequest(new { Error = "Girilen değer geçersiz." });

            string convertedText = _convertService.ConvertTextToNum(input.UserText);

            var output = new OutputModel { Output = convertedText };

            return Ok(output);
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
