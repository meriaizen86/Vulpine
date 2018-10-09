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
        Dictionary<Tuple<Type, string>, IDisposable> Cached = new Dictionary<Tuple<Type, string>, IDisposable>();
        Context Context;

        internal Content(Context context)
        {
            Context = context;
        }

        public T Get<T>(string name)
            where T : IDisposable
        {
            var type = typeof(T);
            var path = Path.GetFullPath(name);
            var typePath = new Tuple<Type, string>(type, path);
            IDisposable found;
            if (Cached.TryGetValue(typePath, out found))
                return (T)found;
            
            if (type == typeof(ShaderModule))
            {
                found = ToDispose(LoadShaderModule(Context, path));
                Cached.Add(typePath, found);
                return (T)found;
            }
            else if (type == typeof(Texture2D))
            {
                found = ToDispose(Texture2D.FromFile(Context.Graphics, path));
                Cached.Add(typePath, found);
                return (T)found;
            }
            else if (type == typeof(System.Drawing.Image))
            {
                found = ToDispose(System.Drawing.Image.FromFile(path));
                Cached.Add(typePath, found);
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
