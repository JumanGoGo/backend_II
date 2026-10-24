namespace backend_II.Utils
{
    public static class PasswordUtils
    {
        public static bool VerifyPassword(string providedPassword, string storedHash)
        {
            return string.Equals(providedPassword, storedHash, StringComparison.Ordinal);
        }
    }
}
