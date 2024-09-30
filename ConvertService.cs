using System.Globalization;
using System.Text;

namespace NLPCharToNumProject
{
    public class ConvertService
    {
        public record SplitTextResult(string LeftPart, string RightPart);

        public enum NumberType
        {
            NUMBER,
            TENNER,
            HUNDRED,
            THOUSAND,
            MILLION,
        }

        public record NumberField(string ValueStr, int Value, NumberType NumberType);

        private static readonly CultureInfo TurkishCulture = new CultureInfo("tr-TR");

        public static readonly List<NumberField> NUMBERS = new List<NumberField>
        {
            new NumberField("bir", 1, NumberType.NUMBER),
            new NumberField("iki", 2, NumberType.NUMBER),
            new NumberField("üç", 3, NumberType.NUMBER),
            new NumberField("dört", 4, NumberType.NUMBER),
            new NumberField("beş", 5, NumberType.NUMBER),
            new NumberField("altı", 6, NumberType.NUMBER),
            new NumberField("yedi", 7, NumberType.NUMBER),
            new NumberField("sekiz", 8, NumberType.NUMBER),
            new NumberField("dokuz", 9, NumberType.NUMBER),
            new NumberField("on", 10, NumberType.TENNER),
            new NumberField("yirmi", 20, NumberType.TENNER),
            new NumberField("otuz", 30, NumberType.TENNER),
            new NumberField("kırk", 40, NumberType.TENNER),
            new NumberField("elli", 50, NumberType.TENNER),
            new NumberField("altmış", 60, NumberType.TENNER),
            new NumberField("yetmiş", 70, NumberType.TENNER),
            new NumberField("seksen", 80, NumberType.TENNER),
            new NumberField("doksan", 90, NumberType.TENNER),
            new NumberField("yüz", 100, NumberType.HUNDRED),
            new NumberField("bin", 1000, NumberType.THOUSAND),
            new NumberField("milyon", 1_000_000, NumberType.MILLION)
        };

        public string ConvertTextToNum(string remainingText, string convertedText = "", long total = 0)
        {
            remainingText = remainingText.Trim();

            var convertedTextSb = new StringBuilder(convertedText);

            // kalan bir şey yoksa convert edilmiş halini dön
            if (string.IsNullOrEmpty(remainingText))
            {
                if (total > 0)
                {
                    convertedTextSb.Append(total);
                }

                return convertedTextSb.ToString().Trim();
            }

            var splittedText = SplitText(remainingText);
            if (!string.IsNullOrEmpty(splittedText?.LeftPart))
            {
                if (total > 0)
                {
                    convertedTextSb.Append(total + " ");
                }

                convertedTextSb.Append(splittedText.LeftPart + " ");
                return ConvertTextToNum(splittedText.RightPart, convertedTextSb.ToString());
            }

            var firstNumber = GetNumber(remainingText);

            if (firstNumber is null)
            {
                if (total > 0)
                    convertedTextSb.Append(total + " " + remainingText);
                else
                    convertedTextSb.Append(remainingText);

                return convertedTextSb.ToString().Trim();
            }

            var firstNumberType = firstNumber.NumberType;

            // firstNumber'ı text içerisinden çıkart
            remainingText = GetRemainingText(remainingText, firstNumber);

            if (total > 0 && (firstNumberType == NumberType.HUNDRED || firstNumberType == NumberType.THOUSAND || firstNumberType == NumberType.MILLION))
            {
                return ConvertTextToNum(remainingText, convertedTextSb.ToString(), total * firstNumber.Value);
            }

            var secondNumber = GetNumber(remainingText);

            if (total > 0 && secondNumber is null && (firstNumberType == NumberType.NUMBER || firstNumberType == NumberType.TENNER))
            {
                return ConvertTextToNum(remainingText, convertedTextSb.ToString(), total + firstNumber.Value);
            }

            // ikinci değer yoksa ya da aynı tip gelirse yüz yüz bin bin gibi düşünebiliriz.
            if (secondNumber is null || firstNumber.NumberType == secondNumber.NumberType)
            {
                convertedTextSb.Append(firstNumber.Value + " ");
                return ConvertTextToNum(remainingText, convertedTextSb.ToString());
            }

            var secondNumberType = secondNumber.NumberType;

            if (
                // KURAL: rakamdan sonra onluk gelirse
                (firstNumberType == NumberType.NUMBER && secondNumberType == NumberType.TENNER)
                // KURAL: rakam 1 ise ve sonraki kelime on, yüz ya da bin ise
                || (firstNumber.Value == 1 && (secondNumberType == NumberType.TENNER || secondNumberType == NumberType.HUNDRED || secondNumberType == NumberType.THOUSAND))
                // KURAL: onluktan sonra yüzlük gelirse
                || (firstNumber.NumberType == NumberType.TENNER && secondNumberType == NumberType.HUNDRED)
                // KURAL: binden sonra milyon denilemez
                || (firstNumber.NumberType == NumberType.THOUSAND && secondNumberType == NumberType.MILLION)
            )
            {
                convertedTextSb.Append(firstNumber.Value + " ");
                return ConvertTextToNum(remainingText, convertedTextSb.ToString());
            }

            // Toplama işlemleri
            if (
                 (firstNumberType == NumberType.TENNER && secondNumberType == NumberType.NUMBER)
                 || (firstNumberType == NumberType.HUNDRED && (secondNumberType == NumberType.NUMBER || secondNumberType == NumberType.TENNER))
                 || (firstNumberType == NumberType.THOUSAND &&
                    (secondNumberType == NumberType.NUMBER || secondNumberType == NumberType.TENNER || secondNumberType == NumberType.HUNDRED))
                 || (firstNumberType == NumberType.MILLION)
               )
            {
                remainingText = GetRemainingText(remainingText, secondNumber);
                return ConvertTextToNum(remainingText, convertedTextSb.ToString(), total + (firstNumber.Value + secondNumber.Value));
            }

            // Çarpma işlemleri
            if (
                (firstNumberType == NumberType.NUMBER)
                || (firstNumberType == NumberType.TENNER
                    && (secondNumberType == NumberType.HUNDRED
                    || secondNumberType == NumberType.THOUSAND
                    || secondNumberType == NumberType.MILLION))
                || (firstNumberType == NumberType.HUNDRED && (secondNumberType == NumberType.THOUSAND || secondNumberType == NumberType.MILLION))
               )
            {
                remainingText = GetRemainingText(remainingText, secondNumber);
                return ConvertTextToNum(remainingText, convertedTextSb.ToString(), total + (firstNumber.Value * secondNumber.Value));
            }

            return ConvertTextToNum(remainingText, convertedTextSb.ToString(), total);
        }

        private static NumberField? GetNumber(string text)
        {
            if (string.IsNullOrEmpty(text)) return default;

            text = text.TrimStart();

            return NUMBERS.FirstOrDefault(number => text.StartsWith(number.ValueStr, true, TurkishCulture));
        }

        private string GetRemainingText(string text, NumberField? numberField)
        {
            if (string.IsNullOrEmpty(text) || numberField is null) return text;

            text = text.TrimStart();

            if (text.StartsWith(numberField.ValueStr, true, TurkishCulture))
            {
                int index = FindIndex(text, numberField.ValueStr);
                if (index >= 0)
                {
                    return text.Remove(index, numberField.ValueStr.Length);
                }
            }

            return text;
        }

        private int FindIndex(string text, string value)
        {
            for (int i = 0; i <= text.Length - value.Length; i++)
            {
                if (string.Compare(text.Substring(i, value.Length), value, TurkishCulture, CompareOptions.IgnoreCase) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private SplitTextResult? SplitText(string text)
        {
            if (string.IsNullOrEmpty(text)) return default;

            text = text.TrimStart();

            int firstIndex = NUMBERS
               .Select(number => FindIndex(text, number.ValueStr))
               .Where(index => index >= 0)
               .DefaultIfEmpty(-1)
               .Min();

            if (firstIndex >= 0)
            {
                return new SplitTextResult(text[..firstIndex].Trim(), text[firstIndex..].Trim());
            }

            return default;
        }
    }
}
