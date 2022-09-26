using System.IO;

namespace PurenailCore.ModUtil
{
    public static class VersionUtil
    {
        private static int HashBytes(byte[] bytes)
        {
            int sum = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                int op = bytes[i];
                for (int j = 1; j < bytes.Length; j++)
                {
                    op = (op * 256) % 997;
                }
                sum = (sum + op) % 997;
            }

            return sum;
        }

        public static string ComputeVersion<T>()
        {
            var asm = typeof(T).Assembly;
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = sha1.ComputeHash(File.OpenRead(asm.Location));
            int sum = HashBytes(bytes);

            System.Version v = asm.GetName().Version;
            return $"{v.Major}.{v.Minor}.{v.Build}+{sum.ToString().PadLeft(3, '0')}";
        }
    }
}
