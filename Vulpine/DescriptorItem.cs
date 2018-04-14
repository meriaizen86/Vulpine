using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public struct DescriptorItem
    {
        public enum DescriptorType
        {
            UniformBuffer,
            CombinedImageSampler
        }
        public enum ShaderType
        {
            Vertex,
            Fragment
        }

        public enum SamplerFilter
        {
            Nearest,
            Linear,
            Cubic
        }

        public DescriptorType Type { get; private set; }
        public ShaderType Shader { get; private set; }
        public VKBuffer Buffer { get; private set; }
        public Texture2D Texture { get; private set; }
        internal Sampler Sampler { get; private set; }
        //public int Count => Type == DescriptorType.UniformBuffer ? Buffer.Count : 1;
        public int Count => 1;

        public static DescriptorItem UniformBuffer(ShaderType shaderType, VKBuffer buffer)
        {
            var item = new DescriptorItem();
            item.Type = DescriptorType.UniformBuffer;
            item.Shader = shaderType;
            item.Buffer = buffer;
            return item;
        }

        public static DescriptorItem CombinedImageSampler(ShaderType shaderType, Texture2D texture, SamplerFilter minFilter, SamplerFilter magFilter, SamplerFilter mipmapFilter = SamplerFilter.Nearest)
        {
            var item = new DescriptorItem();
            item.Type = DescriptorType.CombinedImageSampler;
            item.Shader = shaderType;
            item.Texture = texture;
            item.Sampler = VKHelper.CreateSampler(
                texture.Context,
                minFilter == SamplerFilter.Nearest ? Filter.Nearest :
                    minFilter == SamplerFilter.Cubic ? Filter.CubicImg :
                    Filter.Linear,
                magFilter == SamplerFilter.Nearest ? Filter.Nearest :
                    magFilter == SamplerFilter.Cubic ? Filter.CubicImg :
                    Filter.Linear,
                mipmapFilter == SamplerFilter.Linear ? SamplerMipmapMode.Linear :
                    SamplerMipmapMode.Nearest
            );
            return item;
        }
    }
}
