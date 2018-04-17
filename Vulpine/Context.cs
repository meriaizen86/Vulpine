using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;
using VulkanCore.Khr;
using VulkanCore.Ext;

namespace Vulpine
{
    internal class Context : EasyDisposable
    {
        Instance Instance;
        internal Content Content;
        DebugReportCallbackExt DebugReportCallback;
        internal SurfaceKhr Surface;
        GameWindow Window;
        internal Device Device;
        internal PhysicalDevice PhysicalDevice;
        internal PhysicalDeviceMemoryProperties MemoryProperties;
        internal PhysicalDeviceFeatures Features;
        internal PhysicalDeviceProperties Properties;
        internal Queue GraphicsQueue;
        internal Queue ComputeQueue;
        internal Queue PresentQueue;
        internal CommandPool GraphicsCommandPool;
        internal CommandPool ComputeCommandPool;
        internal Semaphore ImageAvailableSemaphore;
        internal Semaphore RenderingFinishedSemaphore;
        //internal Semaphore CommandBufferSemaphore;
        internal SwapchainKhr Swapchain;
        internal VKImage[] SwapchainImages;
        internal List<PipelineController> Pipelines = new List<PipelineController>();
        internal Graphics Graphics;
        internal RenderPass RenderPass;
        internal Texture2D DepthStencil;
        int GraphicsQueueFamilyIndex = -1;
        int ComputeQueueFamilyIndex = -1;
        int PresentQueueFamilyIndex = -1;

        public Context(GameWindow window)
        {
            Window = window;
            Instance = ToDispose(VKHelper.CreateInstance());
            DebugReportCallback = ToDispose(VKHelper.CreateDebugReportCallback(Instance));
            Surface = ToDispose(VKHelper.CreateSurface(Instance, Window.Handle));
            
            foreach (PhysicalDevice physicalDevice in Instance.EnumeratePhysicalDevices())
            {
                QueueFamilyProperties[] queueFamilyProperties = physicalDevice.GetQueueFamilyProperties();
                for (int i = 0; i < queueFamilyProperties.Length; i++)
                {
                    if (queueFamilyProperties[i].QueueFlags.HasFlag(Queues.Graphics))
                    {
                        if (GraphicsQueueFamilyIndex == -1) GraphicsQueueFamilyIndex = i;
                        if (ComputeQueueFamilyIndex == -1) ComputeQueueFamilyIndex = i;

                        if (physicalDevice.GetSurfaceSupportKhr(i, Surface) &&
                            VKHelper.GetPresentationSupport(physicalDevice, i))
                        {
                            PresentQueueFamilyIndex = i;
                        }

                        if (GraphicsQueueFamilyIndex != -1 &&
                            ComputeQueueFamilyIndex != -1 &&
                            PresentQueueFamilyIndex != -1)
                        {
                            PhysicalDevice = physicalDevice;
                            break;
                        }
                    }
                }
                if (PhysicalDevice != null) break;
            }

            if (PhysicalDevice == null)
                throw new InvalidOperationException("No suitable physical device found.");

            // Store memory properties of the physical device.
            MemoryProperties = PhysicalDevice.GetMemoryProperties();
            Features = PhysicalDevice.GetFeatures();
            Properties = PhysicalDevice.GetProperties();

            // Create a logical device.
            bool sameGraphicsAndPresent = GraphicsQueueFamilyIndex == PresentQueueFamilyIndex;
            var queueCreateInfos = new DeviceQueueCreateInfo[sameGraphicsAndPresent ? 1 : 2];
            queueCreateInfos[0] = new DeviceQueueCreateInfo(GraphicsQueueFamilyIndex, 1, 1.0f);
            if (!sameGraphicsAndPresent)
                queueCreateInfos[1] = new DeviceQueueCreateInfo(PresentQueueFamilyIndex, 1, 1.0f);

            var deviceCreateInfo = new DeviceCreateInfo(
                queueCreateInfos,
                new[] { Constant.DeviceExtension.KhrSwapchain, Constant.DeviceExtension.KhrMaintenance1 },
                Features);
            Device = PhysicalDevice.CreateDevice(deviceCreateInfo);

            // Get queue(s).
            GraphicsQueue = Device.GetQueue(GraphicsQueueFamilyIndex);
            ComputeQueue = ComputeQueueFamilyIndex == GraphicsQueueFamilyIndex
                ? GraphicsQueue
                : Device.GetQueue(ComputeQueueFamilyIndex);
            PresentQueue = PresentQueueFamilyIndex == GraphicsQueueFamilyIndex
                ? GraphicsQueue
                : Device.GetQueue(PresentQueueFamilyIndex);

            Content = new Content(this);

            GraphicsCommandPool = ToDispose(Device.CreateCommandPool(new CommandPoolCreateInfo(GraphicsQueueFamilyIndex)));
            ComputeCommandPool = ToDispose(Device.CreateCommandPool(new CommandPoolCreateInfo(ComputeQueueFamilyIndex)));

            Graphics = ToDispose(new Graphics(this));
            Graphics.ViewportPosition = Vector2I.Zero;
            Graphics.ViewportSize = Window.Size;

            Build();
        }

        internal void Build()
        {
            ComputeCommandPool?.Reset();
            ComputeCommandPool?.Reset();

            ImageAvailableSemaphore?.Dispose();
            RenderingFinishedSemaphore?.Dispose();
            Swapchain?.Dispose();

            ImageAvailableSemaphore = ToDispose(Device.CreateSemaphore());
            RenderingFinishedSemaphore = ToDispose(Device.CreateSemaphore());

            Swapchain = ToDispose(VKHelper.CreateSwapchain(this));
            CacheSwapchainImages();

            DepthStencil = Texture2D.DepthStencil(this, Window.Width, Window.Height);
            RenderPass = VKHelper.CreateRenderPass(Graphics, DepthStencil, Graphics.ClearDepthOnBeginPass);
            foreach (var img in SwapchainImages)
                img.CreateFrameBuffer(RenderPass, DepthStencil);
        }

        public override void Dispose()
        {
            Pipelines.DisposeRange();

            base.Dispose();
        }

        internal void CacheSwapchainImages()
        {
            var imgs = Swapchain.GetImages();
            SwapchainImages = new VKImage[imgs.Length];
            for (var i = 0; i < imgs.Length; i++)
                SwapchainImages[i] = new VKImage(this, imgs[i], Swapchain.Format, Window.Size);
        }
    }
}
