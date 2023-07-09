using System;
using System.Globalization;
using System.Windows.Data;

namespace RonbunMatome
{
    public enum Month
    {
        None = 0,
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

    /// <summary>
    /// Month 列挙型を、月の数値を表す文字列に変換する（例： Month.January → "1"）。
    /// ただし Month.None は空文字列に変換される。
    /// 逆変換の際には数値の文字列だけでなく、月を表す数値や、英語名（例："january"）あるいはその3文字略称（例："jan"）も受付可能。
    /// </summary>
    public class MonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Month 列挙型以外は空文字列に変換する。
            if (value is not Month)
            {
                return string.Empty;
            }

            return Convert((Month)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int)
            {
                return ConvertBack(@int);
            }

            return ConvertBack((string)value);
        }

        public static string Convert(Month value)
        {
            if (value == Month.None)
            {
                return string.Empty;
            }

            return ((int)value).ToString();
        }

        /// <summary>
        /// Month 列挙型を、英語名の3文字略称（例："jan"）に変換する。
        /// </summary>
        /// <param name="value">変換したい Month 列挙型</param>
        /// <returns>英語名の3文字略称（例："jan"）</returns>
        public static string ConvertToAbbreviatedName(Month value)
        {
            return value switch
            {
                Month.January => "jan",
                Month.February => "feb",
                Month.March => "mar",
                Month.April => "apr",
                Month.May => "may",
                Month.June => "jun",
                Month.July => "jul",
                Month.August => "aug",
                Month.September => "sep",
                Month.October => "oct",
                Month.November => "nov",
                Month.December => "dec",
                _ => ""
            };
        }

        public static Month ConvertBack(int value)
        {
            return value switch
            {
                1 => Month.January,
                2 => Month.February,
                3 => Month.March,
                4 => Month.April,
                5 => Month.May,
                6 => Month.June,
                7 => Month.July,
                8 => Month.August,
                9 => Month.September,
                10 => Month.October,
                11 => Month.November,
                12 => Month.December,
                _ => Month.None
            };
        }

        public static Month ConvertBack(string value)
        {
            // 文字列が数値に変換可能なら、数値に変換してから符号化
            if (int.TryParse(value, out int intValue))
            {
                return ConvertBack(intValue);
            }

            string lowerdValue = value.ToLower();

            if (lowerdValue.Length < 3)
            {
                return Month.None;
            }

            // 文字列の最初の3文字で分類する。
            return lowerdValue[..3] switch
            {
                "jan" => Month.January,
                "feb" => Month.February,
                "mar" => Month.March,
                "apr" => Month.April,
                "may" => Month.May,
                "jun" => Month.June,
                "jul" => Month.July,
                "aug" => Month.August,
                "sep" => Month.September,
                "oct" => Month.October,
                "nov" => Month.November,
                "dec" => Month.December,
                _ => Month.None
            };
        }
    }
}
