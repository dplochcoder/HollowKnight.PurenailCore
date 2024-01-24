using Modding;
using Newtonsoft.Json;
using RandomizerCore.Json;
using System.IO;
using System.Reflection;

namespace PurenailCore.SystemUtil;

public class JsonUtil<M> where M : Mod
{
    private static readonly Assembly asm = typeof(M).Assembly;

    private static readonly JsonSerializer serializer = JsonUtil.GetNonLogicSerializer();

    public static string InferGitRoot()
    {
        string path = Directory.GetCurrentDirectory();
        var info = Directory.GetParent(path);
        while (info != null)
        {
            if (Directory.Exists(Path.Combine(info.FullName, ".git")))
            {
                return info.FullName;
            }
            info = Directory.GetParent(info.FullName);
        }
        return path;
    }

    public static T DeserializeEmbedded<T>(string embeddedResourcePath) where T : class => JsonUtil.DeserializeFromEmbeddedResource<T>(asm, embeddedResourcePath);

    public static T DeserializeFromPath<T>(string path) where T : class => JsonUtil.DeserializeFromFile<T>(path);

    public static T DeserializeFromString<T>(string data) where T : class => serializer.DeserializeFromString<T>(data);

    public static void Serialize(object o, string fileName) 
    {
        using StreamWriter sw = new(File.OpenWrite(Path.Combine(Path.GetDirectoryName(asm.Location), fileName)));
        serializer.Serialize(sw, o);
    }

    public static void Serialize(object o, TextWriter tw) => serializer.Serialize(tw, o);

    public static void RewriteJsonFile<T>(T data, string path)
    {
        File.Delete(path);
        Serialize(data, path);
    }
}
