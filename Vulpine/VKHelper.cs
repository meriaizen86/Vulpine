using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using VulkanCore;
using VulkanCore.Ext;
using VulkanCore.Khr;

namespace Vulpine
{
    internal static class VKHelper
    {
        internal static Instance CreateInstance()
        {
            // Pick surface extension for platform to create a window surface
            string surfaceExtension;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    surfaceExtension = Constant.InstanceExtension.KhrWin32Surface;
                    break;
                //case PlatformID.Unix:
                    //surfaceExtension = Constant.InstanceExtension.KhrXlibSurface;
                    //break;
                default:
                    throw new PlatformNotSupportedException("No surface extension for this platform");
            }

            // Start defining create info for this instance
            var createInfo = new InstanceCreateInfo();
#if DEBUG
            // Get properties of all available validation layers
            var availableLayers = Instance.EnumerateLayerProperties();
            // Pick all of the layers in the below array that are available
            createInfo.EnabledLayerNames = new[] { Constant.InstanceLayer.LunarGStandardValidation }
                .Where(availableLayers.Contains)
                .ToArray();
            // Set enabled extension names
            createInfo.EnabledExtensionNames = new[]
            {
                Constant.InstanceExtension.KhrSurface,
                surfaceExtension,
                Constant.InstanceExtension.ExtDebugReport // Debug report extension
            };
#else
            // Set enabled extension names
            createInfo.EnabledExtensionNames = new[]
            {
                Constant.InstanceExtension.KhrSurface,
                surfaceExtension,
            };
#endif

            return new Instance(createInfo);
        }

        internal static DebugReportCallbackExt CreateDebugReportCallback(Instance inst)
        {
#if DEBUG
            // Attach debug callback.
            var debugReportCreateInfo = new DebugReportCallbackCreateInfoExt(
                DebugReportFlagsExt.All,
                args =>
                {
                    Console.WriteLine($"[{args.Flags}][{args.LayerPrefix}] {args.Message}");
                    return args.Flags.HasFlag(DebugReportFlagsExt.Error);
                }
            );
            return inst.CreateDebugReportCallbackExt(debugReportCreateInfo);
#else
            return null;
#endif
        }

        internal static SurfaceKhr CreateSurface(Instance inst, IntPtr winHandle)
        {
            // Create surface depending on platform
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return inst.CreateWin32SurfaceKhr(new Win32SurfaceCreateInfoKhr(Process.GetCurrentProcess().Handle, winHandle));
                //case PlatformID.Unix:
                    //return inst.CreateXlibSurfaceKhr(new XlibSurfaceCreateInfoKhr(Process.GetCurrentProcess().Handle, winHandle));
                default:
                    throw new PlatformNotSupportedException("No surface extension for this platform");
            }
        }

        internal static bool GetPresentationSupport(PhysicalDevice physicalDevice, int queueFamilyIndex)
        {
            // Get whether a queue family supports presentation to the platform
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return physicalDevice.GetWin32PresentationSupportKhr(queueFamilyIndex);
                //case PlatformID.Unix:
                    //return physicalDevice.GetXlibPresentationSupportKhr(queueFamilyIndex);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static SwapchainKhr CreateSwapchain(Context ctx)
        {
            // Get the window surface capabilities
            SurfaceCapabilitiesKhr capabilities = ctx.PhysicalDevice.GetSurfaceCapabilitiesKhr(ctx.Surface);
            // Get the window surface's supported formats
            SurfaceFormatKhr[] formats = ctx.PhysicalDevice.GetSurfaceFormatsKhr(ctx.Surface);
            // Get the window surface's supported present modes
            PresentModeKhr[] presentModes = ctx.PhysicalDevice.GetSurfacePresentModesKhr(ctx.Surface);
            // If the only format available is undedined then pick 32 bit BGRA
            Format format = formats.Length == 1 && formats[0].Format == Format.Undefined
                ? Format.B8G8R8A8UNorm
                : formats[0].Format;
            // Pick a present mode, preferring mailbox
            PresentModeKhr presentMode =
                presentModes.Contains(PresentModeKhr.Mailbox) ? PresentModeKhr.Mailbox :
                presentModes.Contains(PresentModeKhr.FifoRelaxed) ? PresentModeKhr.FifoRelaxed :
                presentModes.Contains(PresentModeKhr.Fifo) ? PresentModeKhr.Fifo :
                PresentModeKhr.Immediate;
            // Finally create a swapchain with the wanted settings
            return ctx.Device.CreateSwapchainKhr(new SwapchainCreateInfoKhr(
                ctx.Surface,
                format,
                capabilities.CurrentExtent,
                capabilities.CurrentTransform,
                presentMode));
        }

        public static ImageView[] CreateImageViews(Context ctx)
        {
            var imageViews = new ImageView[ctx.SwapchainImages.Length];
            for (int i = 0; i < ctx.SwapchainImages.Length; i++)
            {
                imageViews[i] = ctx.SwapchainImages[i].CreateView(
                    new ImageViewCreateInfo(
                        ctx.Swapchain.Format,
                        new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1)
                    )
                );
            }
            return imageViews;
        }

        public static RenderPass CreateRenderPass(Context ctx, int samples)
        {
            var subpasses = new[]
            {
                new SubpassDescription(new[] { new AttachmentReference(0, VulkanCore.ImageLayout.ColorAttachmentOptimal) })
            };
            var attachments = new[]
            {
                new AttachmentDescription
                {
                    Samples =
                        samples == 64 ? SampleCounts.Count64 :
                        samples == 64 ? SampleCounts.Count32 :
                        samples == 64 ? SampleCounts.Count16 :
                        samples == 64 ? SampleCounts.Count8 :
                        samples == 64 ? SampleCounts.Count4 :
                        samples == 64 ? SampleCounts.Count2 :
                        SampleCounts.Count1,
                    Format = ctx.Swapchain.Format,
                    InitialLayout = VulkanCore.ImageLayout.Undefined,
                    FinalLayout = VulkanCore.ImageLayout.PresentSrcKhr,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare
                }
            };

            var createInfo = new RenderPassCreateInfo(subpasses, attachments);
            return ctx.Device.CreateRenderPass(createInfo);
        }

        public static PipelineLayout CreatePipelineLayout(Context ctx)
        {
            var layoutCreateInfo = new PipelineLayoutCreateInfo();
            return ctx.Device.CreatePipelineLayout(layoutCreateInfo);
        }

        public static Framebuffer[] CreateFramebuffers(Context ctx, RenderPass rp, ImageView[] iv, Vector2I size)
        {
            var framebuffers = new Framebuffer[ctx.SwapchainImages.Length];
            for (int i = 0; i < ctx.SwapchainImages.Length; i++)
            {
                framebuffers[i] = rp.CreateFramebuffer(new FramebufferCreateInfo(
                    new[] { iv[i] },
                    size.X,
                    size.Y));
            }
            return framebuffers;
        }

        public static Pipeline CreateGraphicsPipeline(Context ctx, PipelineLayout pl, RenderPass rp, Vector2I size)
        {
            ShaderModule vertexShader = ctx.Content.Get<ShaderModule>("Shader.vert.spv");
            ShaderModule fragmentShader = ctx.Content.Get<ShaderModule>("Shader.frag.spv");
            var shaderStageCreateInfos = new[]
            {
                new PipelineShaderStageCreateInfo(ShaderStages.Vertex, vertexShader, "main"),
                new PipelineShaderStageCreateInfo(ShaderStages.Fragment, fragmentShader, "main")
            };

            var vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo();
            var inputAssemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo(PrimitiveTopology.TriangleList);
            var viewportStateCreateInfo = new PipelineViewportStateCreateInfo(
                new Viewport(0, 0, size.X, size.Y),
                new Rect2D(0, 0, size.X, size.Y));
            var rasterizationStateCreateInfo = new PipelineRasterizationStateCreateInfo
            {
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModes.Back,
                FrontFace = FrontFace.CounterClockwise,
                LineWidth = 1.0f
            };
            var multisampleStateCreateInfo = new PipelineMultisampleStateCreateInfo
            {
                RasterizationSamples = SampleCounts.Count1,
                MinSampleShading = 1.0f
            };
            var colorBlendAttachmentState = new PipelineColorBlendAttachmentState
            {
                SrcColorBlendFactor = BlendFactor.One,
                DstColorBlendFactor = BlendFactor.Zero,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.Zero,
                AlphaBlendOp = BlendOp.Add,
                ColorWriteMask = ColorComponents.All
            };
            var colorBlendStateCreateInfo = new PipelineColorBlendStateCreateInfo(
                new[] { colorBlendAttachmentState });

            var pipelineCreateInfo = new GraphicsPipelineCreateInfo(
                pl, rp, 0,
                shaderStageCreateInfos,
                inputAssemblyStateCreateInfo,
                vertexInputStateCreateInfo,
                rasterizationStateCreateInfo,
                viewportState: viewportStateCreateInfo,
                multisampleState: multisampleStateCreateInfo,
                colorBlendState: colorBlendStateCreateInfo);
            return ctx.Device.CreateGraphicsPipeline(pipelineCreateInfo);
        }
    }
}
