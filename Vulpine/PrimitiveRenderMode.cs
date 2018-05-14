using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public enum PrimitiveRenderMode
    {
        Fill = PolygonMode.Fill,
        Lines = PolygonMode.Line,
        Points = PolygonMode.Point
    }
}
