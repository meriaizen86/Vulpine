using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public struct BlendMode
    {
        public static BlendMode Alpha = new BlendMode(
            Factor.SrcAlpha, Factor.OneMinusSrcAlpha, Op.Add,
            Factor.SrcAlpha, Factor.OneMinusSrcAlpha, Op.Add
        );

        public enum Factor
        {
            One,
            Zero,
            SrcAlpha,
            SrcColor,
            OneMinusSrcAlpha,
            OneMinusSrcColor,
            DstAlpha,
            DstColor,
            OneMinusDstAlpha,
            OneMinusDstColor,
        }

        public enum Op
        {
            Add,
            Subtract,
            SubtractReverse,
            Min,
            Max,
        }

        public enum WriteMask
        {
            All,
            R,
            G,
            B,
            A
        }

        public Factor SrcColorFactor;
        public Factor DstColorFactor;
        public Op ColorOp;

        public Factor SrcAlphaFactor;
        public Factor DstAlphaFactor;
        public Op AlphaOp;

        public WriteMask Mask;

        public BlendMode(Factor srcColor, Factor dstColor, Op colorOp, Factor srcAlpha, Factor dstAlpha, Op alphaOp, WriteMask mask = WriteMask.All)
        {
            SrcColorFactor = srcColor;
            DstColorFactor = dstColor;
            ColorOp = colorOp;
            SrcAlphaFactor = srcAlpha;
            DstAlphaFactor = dstAlpha;
            AlphaOp = alphaOp;
            Mask = mask;
        }

        internal static BlendFactor GetBlendFactor(Factor factor)
        {
            switch (factor)
            {
                case Factor.One:
                    return BlendFactor.One;
                case Factor.Zero:
                    return BlendFactor.Zero;
                case Factor.SrcAlpha:
                    return BlendFactor.SrcAlpha;
                case Factor.SrcColor:
                    return BlendFactor.SrcColor;
                case Factor.OneMinusSrcAlpha:
                    return BlendFactor.OneMinusSrcAlpha;
                case Factor.OneMinusSrcColor:
                    return BlendFactor.OneMinusSrcColor;
                case Factor.DstAlpha:
                    return BlendFactor.DstAlpha;
                case Factor.DstColor:
                    return BlendFactor.DstColor;
                case Factor.OneMinusDstAlpha:
                    return BlendFactor.OneMinusDstAlpha;
                case Factor.OneMinusDstColor:
                    return BlendFactor.OneMinusDstColor;
                default:
                    return BlendFactor.One;
            }
        }

        internal static BlendOp GetBlendOp(Op op)
        {
            switch (op)
            {
                case Op.Add:
                    return BlendOp.Add;
                case Op.Subtract:
                    return BlendOp.Subtract;
                case Op.SubtractReverse:
                    return BlendOp.ReverseSubtract;
                case Op.Min:
                    return BlendOp.Min;
                case Op.Max:
                    return BlendOp.Max;
                default:
                    return BlendOp.Add;
            }
        }

        internal static ColorComponents GetColorWriteMask(WriteMask mask)
        {
            return mask == WriteMask.All ? ColorComponents.All :
                mask == WriteMask.R ? ColorComponents.R :
                mask == WriteMask.G ? ColorComponents.G :
                mask == WriteMask.B ? ColorComponents.B :
                mask == WriteMask.A ? ColorComponents.A :
                ColorComponents.All;
        }
    }
}
