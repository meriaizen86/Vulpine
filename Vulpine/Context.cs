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

        public Context(GameWindow window)
        {
            Window = window;
            Instance = ToDispose(VKHelper.CreateInstance());
            DebugReportCallback = ToDispose(VKHelper.CreateDebugReportCallback(Instance));
            Surface = ToDispose(VKHelper.CreateSurface(Instance, Window.Handle));


            int graphicsQueueFamilyIndex = -1;
            int computeQueueFamilyIndex = -1;
            int presentQueueFamilyIndex = -1;
            foreach (PhysicalDevice physicalDevice in Instance.EnumeratePhysicalDevices())
            {
                QueueFamilyProperties[] queueFamilyProperties = physicalDevice.GetQueueFamilyProperties();
                for (int i = 0; i < queueFamilyProperties.Length; i++)
                {
                    if (queueFamilyProperties[i].QueueFlags.HasFlag(Queues.Graphics))
                    {
                        if (graphicsQueueFamilyIndex == -1) graphicsQueueFamilyIndex = i;
                        if (computeQueueFamilyIndex == -1) computeQueueFamilyIndex = i;

                        if (physicalDevice.GetSurfaceSupportKhr(i, Surface) &&
                            VKHelper.GetPresentationSupport(physicalDevice, i))
                        {
                            presentQueueFamilyIndex = i;
                        }

                        if (graphicsQueueFamilyIndex != -1 &&
                            computeQueueFamilyIndex != -1 &&
                            presentQueueFamilyIndex != -1)
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
            bool sameGraphicsAndPresent = graphicsQueueFamilyIndex == presentQueueFamilyIndex;
            var queueCreateInfos = new DeviceQueueCreateInfo[sameGraphicsAndPresent ? 1 : 2];
            queueCreateInfos[0] = new DeviceQueueCreateInfo(graphicsQueueFamilyIndex, 1, 1.0f);
            if (!sameGraphicsAndPresent)
                queueCreateInfos[1] = new DeviceQueueCreateInfo(presentQueueFamilyIndex, 1, 1.0f);

            var deviceCreateInfo = new DeviceCreateInfo(
                queueCreateInfos,
                new[] { Constant.DeviceExtension.KhrSwapchain, Constant.DeviceExtension.KhrMaintenance1 },
                Features);
            Device = PhysicalDevice.CreateDevice(deviceCreateInfo);

            // Get queue(s).
            GraphicsQueue = Device.GetQueue(graphicsQueueFamilyIndex);
            ComputeQueue = computeQueueFamilyIndex == graphicsQueueFamilyIndex
                ? GraphicsQueue
                : Device.GetQueue(computeQueueFamilyIndex);
            PresentQueue = presentQueueFamilyIndex == graphicsQueueFamilyIndex
                ? GraphicsQueue
                : Device.GetQueue(presentQueueFamilyIndex);

            // Create command pool(s).
            GraphicsCommandPool = ToDispose(Device.CreateCommandPool(new CommandPoolCreateInfo(graphicsQueueFamilyIndex)));
            ComputeCommandPool = ToDispose(Device.CreateCommandPool(new CommandPoolCreateInfo(computeQueueFamilyIndex)));

            Content = new Content(this);

            ImageAvailableSemaphore = ToDispose(Device.CreateSemaphore());
            RenderingFinishedSemaphore = ToDispose(Device.CreateSemaphore());
            //CommandBufferSemaphore = ToDispose(Device.CreateSemaphore());

            Swapchain = ToDispose(VKHelper.CreateSwapchain(this));
            CacheSwapchainImages();

            Graphics = ToDispose(new Graphics(this));
            Graphics.ViewportPosition = Vector2I.Zero;
            Graphics.ViewportSize = Window.Size;
            Graphics.Build();
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
                SwapchainImages[i] = new VKImage { Image = imgs[i] };
        }
    }
}
