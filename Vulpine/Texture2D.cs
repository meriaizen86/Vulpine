using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    internal class TextureData
    {
        internal Mipmap[] Mipmaps { get; set; }
        internal Format Format { get; set; }

        internal class Mipmap
        {
            internal System.Drawing.Imaging.BitmapData Data { get; set; }
            internal Extent3D Extent { get; set; }
            internal int Size { get; set; }
        }
    }

    public class Texture2D : IDisposable
    {
        private Texture2D(Context ctx, Image image, DeviceMemory memory, ImageView view, Format format, Vector2I size, bool renderTarget)
        {
            Context = ctx;
            Image = new VKImage(ctx, image, format, size, view);
            Memory = memory;
            View = view;
            Format = format;
            Size = size;
            SizeF = (Vector2)Size;
            IsRenderTarget = renderTarget;
            if (IsRenderTarget)
                Image.CreateDepthStencil();
        }

        internal Context Context { get; }
        internal Format Format { get; }
        internal VKImage Image { get; }
        internal ImageView View { get; }
        internal DeviceMemory Memory { get; }
        public Vector2I Size { get; }
        internal Vector2 SizeF { get; }
        public bool IsRenderTarget { get; }
        //internal Sampler Sampler { get; }

        public void Dispose()
        {
            View.Dispose();
            Memory.Dispose();
            Image.Dispose();
        }

        public static implicit operator VKImage(Texture2D value) => value.Image;

        

        internal static Texture2D DepthStencil(Context ctx, int width, int height)
        {
            Image image = ctx.Device.CreateImage(new ImageCreateInfo
            {
                ImageType = ImageType.Image2D,
                Format = ctx.DepthStencilFormat,
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
            ImageView view = image.CreateView(new ImageViewCreateInfo(ctx.DepthStencilFormat,
                new ImageSubresourceRange(ImageAspects.Depth | ImageAspects.Stencil, 0, 1, 0, 1)));

            //var sampler = VKHelper.CreateSampler(ctx, Filter.Linear, Filter.Linear, SamplerMipmapMode.Nearest);

            return new Texture2D(ctx, image, memory, view, ctx.DepthStencilFormat, new Vector2I(width, height), false);
        }

        public static Texture2D FromWeb(Graphics g, string url)
        {
            using (var webClient = new WebClient())
            {
                using (var data = new MemoryStream(webClient.DownloadData(url)))
                {
                    return FromStream(g, data);
                }
            }
        }

        /*
        public static async Task<Texture2D> FromWebAsync(Graphics g, string url)
        {
            using (var webClient = new WebClient())
            {
                Task<byte[]> getDataTask = webClient.DownloadDataTaskAsync(new Uri(url));
                using (var data = new MemoryStream(await getDataTask))
                {
                    return FromStream(g.Context, data);
                }
            }
        }*/

        public static Texture2D FromFile(Graphics g, string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return FromStream(g, stream);
            }
        }

        public static Texture2D FromStream(Graphics g, Stream stream)
        {
            using (var img = new System.Drawing.Bitmap(stream))
            {
                return FromBitmap(g, img, new System.Drawing.Rectangle[] {
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, img.Size)
                });
            }
        }

        public static Texture2D FromBitmap(Graphics g, System.Drawing.Bitmap img, System.Drawing.Rectangle[] mips)
        {
            const int pixelDepth = 4;
            const Format pixelFormat = Format.R8G8B8A8UNorm;//Format.B8G8R8A8UNorm;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var data = new TextureData
            {
                Mipmaps = new TextureData.Mipmap[mips.Length],
                Format = pixelFormat
            };
            for (int i = 0; i < mips.Length; i++)
            {
                var mipmap = new TextureData.Mipmap
                {
                    Size = mips[i].Width * mips[i].Height * pixelDepth,
                    Extent = new Extent3D(mips[i].Width, mips[i].Height, 1)
                };
                mipmap.Data = img.LockBits(mips[i], System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                data.Mipmaps[i] = mipmap;
            }
            var tex = FromTextureData(g.Context, data);
            for (int i = 0; i < mips.Length; i++)
            {
                img.UnlockBits(data.Mipmaps[i].Data);
            }
            sw.Stop();
            Console.WriteLine($"Loaded {img.Width}x{img.Height} texture data in {sw.ElapsedMilliseconds} ms");
            return tex;
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

            unsafe
            {
                var dest = (byte*)stagingMemory.Map(0, stagingMemReq.Size);
                var src = (byte*)tex2D.Mipmaps[0].Data.Scan0;
                for (var i = 0; i < stagingMemReq.Size - 3; i += 4)
                {
                    *(dest + 2) = *(src++);
                    *(dest + 1) = *(src++);
                    *(dest) = *(src++);
                    *(dest + 3) = *(src++);
                    dest += 4;
                }
            }
            //Interop.Write(ptr, tex2D.Mipmaps[0].Data);
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

            return new Texture2D(ctx, image, memory, view, tex2D.Format, new Vector2I(tex2D.Mipmaps[0].Extent.Width, tex2D.Mipmaps[0].Extent.Height), false);
        }

        public static Texture2D RenderTarget(Graphics g, int width, int height)
        {
            const Format format = Format.B8G8R8A8UNorm;

            // Create optimal tiled target image.
            Image image = g.Context.Device.CreateImage(new ImageCreateInfo
            {
                ImageType = ImageType.Image2D,
                Format = format,
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCounts.Count1,
                Tiling = ImageTiling.Optimal,
                SharingMode = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
                Extent = new Extent3D(width, height, 1),
                Usage = ImageUsages.Sampled | ImageUsages.TransferDst | ImageUsages.TransferSrc | ImageUsages.ColorAttachment
            });
            MemoryRequirements imageMemReq = image.GetMemoryRequirements();
            int imageHeapIndex = g.Context.MemoryProperties.MemoryTypes.IndexOf(
                imageMemReq.MemoryTypeBits, MemoryProperties.DeviceLocal);
            DeviceMemory memory = g.Context.Device.AllocateMemory(new MemoryAllocateInfo(imageMemReq.Size, imageHeapIndex));
            image.BindMemory(memory);

            var subresourceRange = new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1);

            // Create image view.
            ImageView view = image.CreateView(new ImageViewCreateInfo(format, subresourceRange));

            var sampler = VKHelper.CreateSampler(g.Context, Filter.Linear, Filter.Linear, SamplerMipmapMode.Linear);

            return new Texture2D(g.Context, image, memory, view, format, new Vector2I(width, height), true);
        }
    }
}
