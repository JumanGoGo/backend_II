using System.Globalization;

namespace backend_II.Utils
{
    public static class DataUtils
    {
        public static string null_string(this object? value, string defaultValue = "")
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            return value.ToString() ?? defaultValue;
        }

        public static int null_int(this object? value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public static bool null_bool(this object? value, bool defaultValue = false)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            var str = value.ToString()?.Trim();
            if (bool.TryParse(str, out var result))
            {
                return result;
            }
            if (str == "1") return true;
            if (str == "0") return false;
            return defaultValue;
        }

        public static decimal null_decimal(this object? value, decimal defaultValue = 0m)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public static double null_double(this object? value, double defaultValue = 0.0)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public static DateTime null_datetime(this object? value, DateTime defaultValue = default)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public static Guid null_guid(this object? value, Guid defaultValue = default)
        {
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }
            if (value is Guid guid)
            {
                return guid;
            }
            if (Guid.TryParse(value.ToString(), out var result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
