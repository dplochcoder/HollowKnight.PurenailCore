using System.Collections.Generic;
using System.IO;

namespace PurenailCore.ModUtil;

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

    private static int HashFile(string path)
    {
        var sha1 = System.Security.Cryptography.SHA1.Create();
        var bytes = sha1.ComputeHash(File.OpenRead(path));
        return HashBytes(bytes);
    }

    public static string ComputeVersion<T>() => ComputeVersion<T>(new());

    public static string ComputeVersion<T>(List<string> extraFiles)
    {
        var asm = typeof(T).Assembly;
        int sum = HashFile(asm.Location);

        foreach (var f in extraFiles ?? new()) sum += HashFile(f);
        sum %= 997;

        System.Version v = asm.GetName().Version;
        return $"{v.Major}.{v.Minor}.{v.Build}+{sum.ToString().PadLeft(3, '0')}";
    }
}
