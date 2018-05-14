using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public enum PrimitiveType : int
    {
        Lines = PrimitiveTopology.LineList,
        LineStrip = PrimitiveTopology.LineStrip,
        Patches = PrimitiveTopology.PatchList,
        Points = PrimitiveTopology.PointList,
        TriangleFan = PrimitiveTopology.TriangleFan,
        Triangles = PrimitiveTopology.TriangleList,
        TriangleStrip = PrimitiveTopology.TriangleStrip
    }
}
