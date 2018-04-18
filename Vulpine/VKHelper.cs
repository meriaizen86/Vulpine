using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using VulkanCore;
using VulkanCore.Ext;
using VulkanCore.Khr;

namespace Vulpine
{
    internal static class VKHelper
    {
        static PipelineVertexInputStateCreateInfo DefaultVertexAttr;

        static VKHelper()
        {
            /* Use reflection to generate a default PipelineVertexInputStateCreateInfo from the Vertex struct later
            DefaultVertexAttr = new PipelineVertexInputStateCreateInfo(
                new[] { new VertexInputBindingDescription(0, Interop.SizeOf<Vertex>(), VertexInputRate.Vertex) },
                new[]
                {
                    new VertexInputAttributeDescription(0, 0, Format.R32G32B32SFloat, 0),  // Position.
                    new VertexInputAttributeDescription(1, 0, Format.R32G32B32SFloat, 12), // Normal.
                    new VertexInputAttributeDescription(2, 0, Format.R32G32SFloat, 24)     // TexCoord.
                }
            );*/
        }


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

        public static RenderPass CreateRenderPass(Graphics g, Texture2D depthStencil, bool clearDepthOnBeginPass)
        {
            var subpasses = new[]
            {
                new SubpassDescription(
                    new[] { new AttachmentReference(0, VulkanCore.ImageLayout.ColorAttachmentOptimal) },
                    new AttachmentReference(1, VulkanCore.ImageLayout.DepthStencilAttachmentOptimal))
            };
            var samples = g.Samples == 64 ? SampleCounts.Count64 :
                        g.Samples == 32 ? SampleCounts.Count32 :
                        g.Samples == 16 ? SampleCounts.Count16 :
                        g.Samples == 8 ? SampleCounts.Count8 :
                        g.Samples == 4 ? SampleCounts.Count4 :
                        g.Samples == 2 ? SampleCounts.Count2 :
                        SampleCounts.Count1;
            var attachments = new[]
            {
                new AttachmentDescription
                {
                    Format = g.Context.Swapchain.Format,
                    Samples = samples,
                    LoadOp = AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = VulkanCore.ImageLayout.Undefined,
                    FinalLayout = VulkanCore.ImageLayout.PresentSrcKhr
                },
                new AttachmentDescription
                {
                    Format = depthStencil.Format,
                    Samples = samples,
                    LoadOp = clearDepthOnBeginPass ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
                    StoreOp = AttachmentStoreOp.DontCare,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = VulkanCore.ImageLayout.Undefined,
                    FinalLayout = VulkanCore.ImageLayout.DepthStencilAttachmentOptimal
                }
            };

            var dependencies = new[]
            {
                new SubpassDependency
                {
                    SrcSubpass = Constant.SubpassExternal,
                    DstSubpass = 0,
                    SrcStageMask = PipelineStages.BottomOfPipe,
                    DstStageMask = PipelineStages.ColorAttachmentOutput,
                    SrcAccessMask = Accesses.MemoryRead,
                    DstAccessMask = Accesses.ColorAttachmentRead | Accesses.ColorAttachmentWrite,
                    DependencyFlags = Dependencies.ByRegion
                },
                new SubpassDependency
                {
                    SrcSubpass = 0,
                    DstSubpass = Constant.SubpassExternal,
                    SrcStageMask = PipelineStages.ColorAttachmentOutput,
                    DstStageMask = PipelineStages.BottomOfPipe,
                    SrcAccessMask = Accesses.ColorAttachmentRead | Accesses.ColorAttachmentWrite,
                    DstAccessMask = Accesses.MemoryRead,
                    DependencyFlags = Dependencies.ByRegion
                }
            };

            var createInfo = new RenderPassCreateInfo(subpasses, attachments, dependencies);
            return g.Context.Device.CreateRenderPass(createInfo);
        }

        public static Sampler CreateSampler(Context ctx, Filter magFilter, Filter minFilter, SamplerMipmapMode mipmapMode)
        {
            var createInfo = new SamplerCreateInfo
            {
                MagFilter = magFilter,
                MinFilter = minFilter,
                MipmapMode = mipmapMode
            };
            // We also enable anisotropic filtering. Because that feature is optional, it must be
            // checked if it is supported by the device.
            if (ctx.Features.SamplerAnisotropy)
            {
                createInfo.AnisotropyEnable = true;
                createInfo.MaxAnisotropy = ctx.Properties.Limits.MaxSamplerAnisotropy;
            }
            else
            {
                createInfo.MaxAnisotropy = 1.0f;
            }
            return ctx.Device.CreateSampler(createInfo);
        }

        public static DescriptorSetLayout CreateDescriptorSetLayout(Graphics g, DescriptorItem[] items)
        {
            var bindings = new DescriptorSetLayoutBinding[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                bindings[i] = new DescriptorSetLayoutBinding(
                    i,
                    item.Type == DescriptorItem.DescriptorType.UniformBuffer ? DescriptorType.UniformBuffer :
                        item.Type == DescriptorItem.DescriptorType.CombinedImageSampler ? DescriptorType.CombinedImageSampler :
                        DescriptorType.UniformBuffer,
                    item.Count,
                    item.Shader == DescriptorItem.ShaderType.Vertex ? ShaderStages.Vertex :
                        item.Shader == DescriptorItem.ShaderType.Fragment ? ShaderStages.Fragment :
                        ShaderStages.All
                );
            }
            return g.Context.Device.CreateDescriptorSetLayout(new DescriptorSetLayoutCreateInfo(bindings));
        }

        public static PipelineLayout CreatePipelineLayout(Graphics g, params DescriptorSetLayout[] setLayouts)
        {
            var layoutCreateInfo = new PipelineLayoutCreateInfo(setLayouts);
            return g.Context.Device.CreatePipelineLayout(layoutCreateInfo);
        }

        public static Pipeline CreateGraphicsPipeline(Graphics g, PipelineLayout pl, string[] shaderNames, bool depthTest, bool depthWrite, bool instancing, Type instanceInfoType, BlendMode blendMode)
        {
            if (instancing && instanceInfoType == null)
                throw new NullReferenceException("Instance info type cannot be null");

            var shaderStageCreateInfos = new PipelineShaderStageCreateInfo[shaderNames.Length];
            for (var i = 0; i < shaderNames.Length; i++)
            {
                var shader = g.Context.Content.Get<ShaderModule>(shaderNames[i]);
                var shaderName = Path.GetFileNameWithoutExtension(shaderNames[i]);
                switch (Path.GetExtension(shaderName))
                {
                    case ".vert":
                        shaderStageCreateInfos[i] = new PipelineShaderStageCreateInfo(ShaderStages.Vertex, shader, "main");
                        break;
                    case ".frag":
                        shaderStageCreateInfos[i] = new PipelineShaderStageCreateInfo(ShaderStages.Fragment, shader, "main");
                        break;
                    default:
                        throw new NotImplementedException($"Unreognized shader type for file \"{shaderNames[i]}\"");
                }
                
            }
            
            var fields = typeof(Vertex).GetFields();
            var offset = 0;
            var loci = 0;
            List<VertexInputAttributeDescription> vertexAttributes = new List<VertexInputAttributeDescription>(fields.Length);
            for (var i = 0; i < fields.Length; i++)
            {
                var ftype = fields[i].FieldType;
                if (ftype == typeof(Vector3))
                {
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32G32B32SFloat, offset));
                    offset += 12;
                    loci++;
                }
                else if (ftype == typeof(Vector2))
                {
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32G32SFloat, offset));
                    offset += 8;
                    loci++;
                }
                else if (ftype == typeof(float))
                {
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32SFloat, offset));
                    offset += 4;
                    loci++;
                }
                else if (ftype == typeof(Matrix4))
                {
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32G32B32A32SFloat, offset));
                    loci++;
                    offset += 16;
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32G32B32A32SFloat, offset));
                    loci++;
                    offset += 16;
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32G32B32A32SFloat, offset));
                    loci++;
                    offset += 16;
                    vertexAttributes.Add(new VertexInputAttributeDescription(loci, 0, Format.R32G32B32A32SFloat, offset));
                    loci++;
                    offset += 16;
                }
                else throw new Exception("Field " + fields[i] + " of vertex struct is an illegal type");
            }
            var vertexFieldsLength = fields.Length;

            if (instancing)
            {
                fields = instanceInfoType.GetFields();
                offset = 0;
                for (var i = 0; i < fields.Length; i++)
                {
                    var ftype = fields[i].FieldType;
                    if (ftype == typeof(Vector3))
                    {
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32G32B32SFloat, offset));
                        loci++;
                        offset += 12;
                    }
                    else if (ftype == typeof(Vector2))
                    {
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32G32SFloat, offset));
                        loci++;
                        offset += 8;
                    }
                    else if (ftype == typeof(float))
                    {
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32SFloat, offset));
                        loci++;
                        offset += 4;
                    }
                    else if (ftype == typeof(Matrix4))
                    {
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32G32B32A32SFloat, offset));
                        loci++;
                        offset += 16;
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32G32B32A32SFloat, offset));
                        loci++;
                        offset += 16;
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32G32B32A32SFloat, offset));
                        loci++;
                        offset += 16;
                        vertexAttributes.Add(new VertexInputAttributeDescription(loci, 1, Format.R32G32B32A32SFloat, offset));
                        loci++;
                        offset += 16;
                    }
                    else throw new Exception("Field " + fields[i] + " of instance info struct is an illegal type");
                }
            }

            var vertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo(
                (instancing ? new[]
                {
                    new VertexInputBindingDescription(0, Interop.SizeOf<Vertex>(), VertexInputRate.Vertex),
                    new VertexInputBindingDescription(1, System.Runtime.InteropServices.Marshal.SizeOf(instanceInfoType), VertexInputRate.Instance)
                } :
                new[]
                {
                    new VertexInputBindingDescription(0, Interop.SizeOf<Vertex>(), VertexInputRate.Vertex)
                }),
                vertexAttributes.ToArray()
            );
            var inputAssemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo(PrimitiveTopology.TriangleList);
            var viewportStateCreateInfo = new PipelineViewportStateCreateInfo(
                new Viewport(g.ViewportPosition.X + g.ViewportSize.X, g.ViewportPosition.Y + g.ViewportSize.Y, -g.ViewportSize.X, -g.ViewportSize.Y),
                new Rect2D(g.ViewportPosition.X, g.ViewportPosition.Y, g.ViewportSize.X, g.ViewportSize.Y));
            var rasterizationStateCreateInfo = new PipelineRasterizationStateCreateInfo
            {
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModes.Back,
                FrontFace = FrontFace.Clockwise,
                LineWidth = 1.0f
            };
            var multisampleStateCreateInfo = new PipelineMultisampleStateCreateInfo
            {
                RasterizationSamples =
                        g.Samples == 64 ? SampleCounts.Count64 :
                        g.Samples == 32 ? SampleCounts.Count32 :
                        g.Samples == 16 ? SampleCounts.Count16 :
                        g.Samples == 8 ? SampleCounts.Count8 :
                        g.Samples == 4 ? SampleCounts.Count4 :
                        g.Samples == 2 ? SampleCounts.Count2 :
                        SampleCounts.Count1,
                MinSampleShading = 1.0f
            };
            var depthStencilCreateInfo = new PipelineDepthStencilStateCreateInfo
            {
                DepthTestEnable = depthTest,
                DepthWriteEnable = depthWrite,
                DepthCompareOp = CompareOp.LessOrEqual,
                Back = new StencilOpState
                {
                    FailOp = StencilOp.Keep,
                    PassOp = StencilOp.Keep,
                    CompareOp = CompareOp.Always
                },
                Front = new StencilOpState
                {
                    FailOp = StencilOp.Keep,
                    PassOp = StencilOp.Keep,
                    CompareOp = CompareOp.Always
                }
            };
            var colorBlendAttachmentState = new PipelineColorBlendAttachmentState
            {
                SrcColorBlendFactor = BlendMode.GetBlendFactor(blendMode.SrcColorFactor),
                DstColorBlendFactor = BlendMode.GetBlendFactor(blendMode.DstColorFactor),
                ColorBlendOp = BlendMode.GetBlendOp(blendMode.ColorOp),
                SrcAlphaBlendFactor = BlendMode.GetBlendFactor(blendMode.SrcAlphaFactor),
                DstAlphaBlendFactor = BlendMode.GetBlendFactor(blendMode.DstAlphaFactor),
                AlphaBlendOp = BlendMode.GetBlendOp(blendMode.AlphaOp),
                ColorWriteMask = BlendMode.GetColorWriteMask(blendMode.Mask),
                BlendEnable = true
            };
            var colorBlendStateCreateInfo = new PipelineColorBlendStateCreateInfo(
                new[] { colorBlendAttachmentState });

            var pipelineCreateInfo = new GraphicsPipelineCreateInfo(
                pl, g.Context.RenderPass, 0,
                shaderStageCreateInfos,
                inputAssemblyStateCreateInfo,
                vertexInputStateCreateInfo,
                rasterizationStateCreateInfo,
                viewportState: viewportStateCreateInfo,
                multisampleState: multisampleStateCreateInfo,
                depthStencilState: depthStencilCreateInfo,
                colorBlendState: colorBlendStateCreateInfo//,
                //dynamicState: new PipelineDynamicStateCreateInfo(DynamicState.Viewport)
            );
            return g.Context.Device.CreateGraphicsPipeline(pipelineCreateInfo);
        }

        public static DescriptorPool CreateDescriptorPool(Graphics g, DescriptorItem[] items)
        {
            var sizes = new DescriptorPoolSize[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                sizes[i] = new DescriptorPoolSize(
                    item.Type == DescriptorItem.DescriptorType.UniformBuffer ? DescriptorType.UniformBuffer :
                        item.Type == DescriptorItem.DescriptorType.CombinedImageSampler ? DescriptorType.CombinedImageSampler :
                        DescriptorType.UniformBuffer,
                    item.Count
                );
            }
            
            return g.Context.Device.CreateDescriptorPool(
                new DescriptorPoolCreateInfo(sizes.Length, sizes));
        }

        static List<Sampler> _SamplerCollection = new List<Sampler>();
        public static DescriptorSet CreateDescriptorSet(DescriptorPool pool, DescriptorSetLayout setLayout, DescriptorItem[] items, out Sampler[] samplers)
        {
            DescriptorSet descriptorSet = pool.AllocateSets(new DescriptorSetAllocateInfo(1, setLayout))[0];

            var writeDescriptorSets = new WriteDescriptorSet[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                switch (item.Type)
                {
                    case DescriptorItem.DescriptorType.UniformBuffer:
                        writeDescriptorSets[i] = new WriteDescriptorSet(descriptorSet, i, 0, item.Count, DescriptorType.UniformBuffer,
                            bufferInfo: new[] { new DescriptorBufferInfo(item.Buffer, 0, item.Buffer.Size) });
                        break;
                    case DescriptorItem.DescriptorType.CombinedImageSampler:
                        _SamplerCollection.Add(item.Sampler);
                        writeDescriptorSets[i] = new WriteDescriptorSet(descriptorSet, i, 0, 1, DescriptorType.CombinedImageSampler,
                            imageInfo: new[] { new DescriptorImageInfo(item.Sampler, item.Texture.View, VulkanCore.ImageLayout.General) });
                        break;
                    default:
                        throw new NotImplementedException($"No case for {item.Type}");
                }
            }

            pool.UpdateSets(writeDescriptorSets);

            samplers = _SamplerCollection.ToArray();
            _SamplerCollection.Clear();
            return descriptorSet;
        }
    }
}
