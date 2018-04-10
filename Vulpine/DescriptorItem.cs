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

        public DescriptorType Type { get; private set; }
        public ShaderType Shader { get; private set; }
        public VKBuffer Buffer { get; private set; }
        public Texture2D Texture { get; private set; }
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

        public static DescriptorItem CombinedImageSampler(ShaderType shaderType, Texture2D texture)
        {
            var item = new DescriptorItem();
            item.Type = DescriptorType.CombinedImageSampler;
            item.Shader = shaderType;
            item.Texture = texture;
            return item;
        }
    }
}
