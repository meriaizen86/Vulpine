using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public enum PrimitiveCullMode
    {
        Back = CullModes.Back,
        Front = CullModes.Front,
        FrontAndBack = CullModes.FrontAndBack,
        None = CullModes.None
    }
}
