using System;

namespace Vulpine.SpirV
{
    public enum ExecutionModel : int
    {
        Vertex = 0,
        TessellationControl = 1,
        TessellationEvaluation = 2,
        Geometry = 3,
        Fragment = 4,
        GLCompute = 5,
        Kernel = 6
    }
}
