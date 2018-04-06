using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using VulkanCore;

namespace Vulpine
{
    internal class Content : EasyDisposable
    {
        internal string Root = "Data";
        Dictionary<string, IDisposable> Cached = new Dictionary<string, IDisposable>();
        Context Context;

        internal Content(Context context)
        {
            Context = context;
        }

        internal T Get<T>(string name)
            where T : IDisposable
        {
            IDisposable found;
            if (Cached.TryGetValue(name, out found))
                return (T)found;

            string path = Path.Combine(Root, name);

            var type = typeof(T);
            if (type == typeof(ShaderModule))
            {
                found = ToDispose(LoadShaderModule(Context, path));
                Cached.Add(name, found);
                return (T)found;
            }
            else if (type == typeof(Texture2D))
            {
                found = ToDispose(Texture2D.FromFile(Context, path));
                Cached.Add(name, found);
                return (T)found;
            }

            throw new FileNotFoundException(path);
        }

        internal static ShaderModule LoadShaderModule(Context ctx, string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms, (int)stream.Length);
                    return ctx.Device.CreateShaderModule(new ShaderModuleCreateInfo(ms.ToArray()));
                }
            }
        }
    }
}
