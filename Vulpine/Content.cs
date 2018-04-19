using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using VulkanCore;

namespace Vulpine
{
    public class Content : EasyDisposable
    {
        Dictionary<string, IDisposable> Cached = new Dictionary<string, IDisposable>();
        Context Context;

        internal Content(Context context)
        {
            Context = context;
        }

        public T Get<T>(string name)
            where T : IDisposable
        {
            string path = Path.GetFullPath(name);
            IDisposable found;
            if (Cached.TryGetValue(path, out found))
                return (T)found;

            var type = typeof(T);
            if (type == typeof(ShaderModule))
            {
                found = ToDispose(LoadShaderModule(Context, path));
                Cached.Add(path, found);
                return (T)found;
            }
            else if (type == typeof(Texture2D))
            {
                found = ToDispose(Texture2D.FromFile(Context, path));
                Cached.Add(path, found);
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
