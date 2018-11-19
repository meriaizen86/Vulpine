using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.SpirV
{
    public enum ExecutionMode : int
    {
        SpvExecutionModeInvocations = 0,
        SpvExecutionModeSpacingEqual = 1,
        SpvExecutionModeSpacingFractionalEven = 2,
        SpvExecutionModeSpacingFractionalOdd = 3,
        SpvExecutionModeVertexOrderCw = 4,
        SpvExecutionModeVertexOrderCcw = 5,
        SpvExecutionModePixelCenterInteger = 6,
        SpvExecutionModeOriginUpperLeft = 7,
        SpvExecutionModeOriginLowerLeft = 8,
        SpvExecutionModeEarlyFragmentTests = 9,
        SpvExecutionModePointMode = 10,
        SpvExecutionModeXfb = 11,
        SpvExecutionModeDepthReplacing = 12,
        SpvExecutionModeDepthGreater = 14,
        SpvExecutionModeDepthLess = 15,
        SpvExecutionModeDepthUnchanged = 16,
        SpvExecutionModeLocalSize = 17,
        SpvExecutionModeLocalSizeHint = 18,
        SpvExecutionModeInputPoints = 19,
        SpvExecutionModeInputLines = 20,
        SpvExecutionModeInputLinesAdjacency = 21,
        SpvExecutionModeTriangles = 22,
        SpvExecutionModeInputTrianglesAdjacency = 23,
        SpvExecutionModeQuads = 24,
        SpvExecutionModeIsolines = 25,
        SpvExecutionModeOutputVertices = 26,
        SpvExecutionModeOutputPoints = 27,
        SpvExecutionModeOutputLineStrip = 28,
        SpvExecutionModeOutputTriangleStrip = 29,
        SpvExecutionModeVecTypeHint = 30,
        SpvExecutionModeContractionOff = 31,
        SpvExecutionModeInitializer = 33,
        SpvExecutionModeFinalizer = 34,
        SpvExecutionModeSubgroupSize = 35,
        SpvExecutionModeSubgroupsPerWorkgroup = 36,
        SpvExecutionModeSubgroupsPerWorkgroupId = 37,
        SpvExecutionModeLocalSizeId = 38,
        SpvExecutionModeLocalSizeHintId = 39,
        SpvExecutionModePostDepthCoverage = 4446,
        SpvExecutionModeStencilRefReplacingEXT = 5027
    }
}
