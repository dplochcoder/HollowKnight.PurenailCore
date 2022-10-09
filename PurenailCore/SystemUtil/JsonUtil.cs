using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Reflection;

namespace PurenailCore.SystemUtil
{
    public class JsonUtil<M> where M : Mod
    {
        private static readonly Assembly asm = typeof(M).Assembly;

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

        public static T DeserializeEmbedded<T>(string embeddedResourcePath)
        {
            using StreamReader sr = new(asm.GetManifestResourceStream(embeddedResourcePath));
            using JsonTextReader jtr = new(sr);
            return SerializerHolder._js.Deserialize<T>(jtr);
        }

        public static T DeserializeFromPath<T>(string path)
        {
            using StreamReader sr = new(path);
            using JsonTextReader jtr = new(sr);
            return SerializerHolder._js.Deserialize<T>(jtr);
        }

        public static void Serialize(object o, string fileName)
        {
            using StreamWriter sw = new(File.OpenWrite(Path.Combine(Path.GetDirectoryName(asm.Location), fileName)));
            SerializerHolder._js.Serialize(sw, o);
        }

        public static void Serialize(object o, TextWriter tw)
        {
            using JsonTextWriter jtw = new(tw) { CloseOutput = false };
            SerializerHolder._js.Serialize(tw, o);
        }

        public static void RewriteJsonFile<T>(T data, string path)
        {
            File.Delete(path);
            Serialize(data, path);
        }
    }

    internal static class SerializerHolder
    {
        internal static readonly JsonSerializer _js;

        static SerializerHolder()
        {
            _js = new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            _js.Converters.Add(new StringEnumConverter());
        }
    }
}