using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Chat.Logic
{
    public static class StringExtensions
    {
        public static string GetMd5String(this string value)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

                return bytes.Aggregate(string.Empty, (current, @byte) => current + @byte.ToString("x2"));
            }
        }
    }
}