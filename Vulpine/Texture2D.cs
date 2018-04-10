using System;
using System.Linq;
using System.IO;

using VulkanCore;

namespace Vulpine
{
    internal class TextureData
    {
        internal Mipmap[] Mipmaps { get; set; }
        internal Format Format { get; set; }

        internal class Mipmap
        {
            internal byte[] Data { get; set; }
            internal Extent3D Extent { get; set; }
            internal int Size { get; set; }
        }
    }

    public class Texture2D : IDisposable
    {
        private Texture2D(Image image, DeviceMemory memory, ImageView view, Format format, Sampler sampler)
        {
            Image = image;
            Memory = memory;
            View = view;
            Format = format;
            Sampler = sampler;
        }

        internal Format Format { get; }
        internal Image Image { get; }
        internal ImageView View { get; }
        internal DeviceMemory Memory { get; }
        internal Sampler Sampler { get; }

        public void Dispose()
        {
            View.Dispose();
            Memory.Dispose();
            Image.Dispose();
        }

        public static implicit operator Image(Texture2D value) => value.Image;

        internal static Texture2D DepthStencil(Context ctx, int width, int height)
        {
            Format[] validFormats =
            {
                Format.D32SFloatS8UInt,
                Format.D32SFloat,
                Format.D24UNormS8UInt,
                Format.D16UNormS8UInt,
                Format.D16UNorm
            };

            Format? potentialFormat = validFormats.FirstOrDefault(
                validFormat =>
                {
                    FormatProperties formatProps = ctx.PhysicalDevice.GetFormatProperties(validFormat);
                    return (formatProps.OptimalTilingFeatures & FormatFeatures.DepthStencilAttachment) > 0;
                });

            if (!potentialFormat.HasValue)
                throw new InvalidOperationException("Required depth stencil format not supported.");

            Format format = potentialFormat.Value;

            Image image = ctx.Device.CreateImage(new ImageCreateInfo
            {
                ImageType = ImageType.Image2D,
                Format = format,
                Extent = new Extent3D(width, height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCounts.Count1,
                Tiling = ImageTiling.Optimal,
                Usage = ImageUsages.DepthStencilAttachment | ImageUsages.TransferSrc
            });
            MemoryRequirements memReq = image.GetMemoryRequirements();
            int heapIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                memReq.MemoryTypeBits, MemoryProperties.DeviceLocal);
            DeviceMemory memory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(memReq.Size, heapIndex));
            image.BindMemory(memory);
            ImageView view = image.CreateView(new ImageViewCreateInfo(format,
                new ImageSubresourceRange(ImageAspects.Depth | ImageAspects.Stencil, 0, 1, 0, 1)));

            var sampler = VKHelper.CreateSampler(ctx, Filter.Linear, Filter.Linear, SamplerMipmapMode.Linear);

            return new Texture2D(image, memory, view, format, sampler);
        }

        internal static Texture2D FromFile(Context ctx, string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return FromStream(ctx, stream);
            }
        }

        internal static Texture2D FromStream(Context ctx, Stream stream)
        {
            const int numberOfMipmapLevels = 1;
            const int pixelDepth = 4;
            const Format pixelFormat = Format.R8G8B8A8UNorm;//Format.B8G8R8A8UNorm;

            using (var img = new System.Drawing.Bitmap(stream))
            {
                var bytes = new Byte[img.Width * img.Height * pixelDepth];
                var n = 0;
                for (var y = 0; y < img.Height; y++)
                {
                    for (var x = 0; x < img.Width; x++)
                    {
                        var px = img.GetPixel(x, y);
                        bytes[n] = px.R;
                        bytes[n + 1] = px.G;
                        bytes[n + 2] = px.B;
                        bytes[n + 3] = px.A;
                        n += 4;
                    }
                }

                var data = new TextureData
                {
                    Mipmaps = new TextureData.Mipmap[numberOfMipmapLevels],
                    Format = pixelFormat
                };

                for (int i = 0; i < numberOfMipmapLevels; i++)
                {
                    var mipmap = new TextureData.Mipmap
                    {
                        Size = img.Width * img.Height * pixelDepth,
                        Extent = new Extent3D(img.Width, img.Height, 1)
                    };
                    mipmap.Data = bytes;
                    data.Mipmaps[i] = mipmap;
                }

                return Texture2D.FromTextureData(ctx, data);
            }
        }

        internal static Texture2D FromTextureData(Context ctx, TextureData tex2D)
        {
            var stagingBuffer = ctx.Device.CreateBuffer(
                new BufferCreateInfo(tex2D.Mipmaps[0].Size, BufferUsages.TransferSrc));
            MemoryRequirements stagingMemReq = stagingBuffer.GetMemoryRequirements();
            int heapIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                stagingMemReq.MemoryTypeBits, MemoryProperties.HostVisible);
            DeviceMemory stagingMemory = ctx.Device.AllocateMemory(
                new MemoryAllocateInfo(stagingMemReq.Size, heapIndex));
            stagingBuffer.BindMemory(stagingMemory);

            IntPtr ptr = stagingMemory.Map(0, stagingMemReq.Size);
            Interop.Write(ptr, tex2D.Mipmaps[0].Data);
            stagingMemory.Unmap();

            // Setup buffer copy regions for each mip level.
            var bufferCopyRegions = new BufferImageCopy[tex2D.Mipmaps.Length];
            int offset = 0;
            for (int i = 0; i < bufferCopyRegions.Length; i++)
            {
                bufferCopyRegions = new[]
                {
                    new BufferImageCopy
                    {
                        ImageSubresource = new ImageSubresourceLayers(ImageAspects.Color, i, 0, 1),
                        ImageExtent = tex2D.Mipmaps[0].Extent,
                        BufferOffset = offset
                    }
                };
                offset += tex2D.Mipmaps[i].Size;
            }

            // Create optimal tiled target image.
            Image image = ctx.Device.CreateImage(new ImageCreateInfo
            {
                ImageType = ImageType.Image2D,
                Format = tex2D.Format,
                MipLevels = tex2D.Mipmaps.Length,
                ArrayLayers = 1,
                Samples = SampleCounts.Count1,
                Tiling = ImageTiling.Optimal,
                SharingMode = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
                Extent = tex2D.Mipmaps[0].Extent,
                Usage = ImageUsages.Sampled | ImageUsages.TransferDst
            });
            MemoryRequirements imageMemReq = image.GetMemoryRequirements();
            int imageHeapIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                imageMemReq.MemoryTypeBits, MemoryProperties.DeviceLocal);
            DeviceMemory memory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(imageMemReq.Size, imageHeapIndex));
            image.BindMemory(memory);

            var subresourceRange = new ImageSubresourceRange(ImageAspects.Color, 0, tex2D.Mipmaps.Length, 0, 1);

            // Copy the data from staging buffers to device local buffers.
            CommandBuffer cmdBuffer = ctx.GraphicsCommandPool.AllocateBuffers(new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1))[0];
            cmdBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
            cmdBuffer.CmdPipelineBarrier(PipelineStages.TopOfPipe, PipelineStages.Transfer,
                imageMemoryBarriers: new[]
                {
                    new ImageMemoryBarrier(
                        image, subresourceRange,
                        0, Accesses.TransferWrite,
                        ImageLayout.Undefined, ImageLayout.TransferDstOptimal)
                });
            cmdBuffer.CmdCopyBufferToImage(stagingBuffer, image, ImageLayout.TransferDstOptimal, bufferCopyRegions);
            cmdBuffer.CmdPipelineBarrier(PipelineStages.Transfer, PipelineStages.FragmentShader,
                imageMemoryBarriers: new[]
                {
                    new ImageMemoryBarrier(
                        image, subresourceRange,
                        Accesses.TransferWrite, Accesses.ShaderRead,
                        ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal)
                });
            cmdBuffer.End();

            // Submit.
            Fence fence = ctx.Device.CreateFence();
            ctx.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { cmdBuffer }), fence);
            fence.Wait();

            // Cleanup staging resources.
            fence.Dispose();
            stagingMemory.Dispose();
            stagingBuffer.Dispose();

            // Create image view.
            ImageView view = image.CreateView(new ImageViewCreateInfo(tex2D.Format, subresourceRange));

            var sampler = VKHelper.CreateSampler(ctx, Filter.Linear, Filter.Linear, SamplerMipmapMode.Linear);

            return new Texture2D(image, memory, view, tex2D.Format, sampler);
        }
    }
}
