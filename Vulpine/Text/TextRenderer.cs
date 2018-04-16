using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine.Text
{
    public class TextRenderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct CharTransform
        {
            public Matrix4 Transform;
        }

        PipelineController Pipeline;
        VKBuffer Instances;
        VKBuffer UTextTransform;

        public TextRenderer(Graphics g, int maxCharacters = 256)
        {
            Instances = VKBuffer.InstanceInfo<CharTransform>(g, maxCharacters);
            
        }

        public void Build()
        {

        }

        public void Dispose()
        {
            Pipeline?.Dispose();
        }
    }
}
