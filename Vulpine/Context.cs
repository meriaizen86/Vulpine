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
        PhysicalDeviceProperties Properties;
        internal Queue GraphicsQueue;
        internal Queue ComputeQueue;
        internal Queue PresentQueue;
        internal CommandPool GraphicsCommandPool;
        internal CommandPool ComputeCommandPool;
        internal Semaphore ImageAvailableSemaphore;
        internal Semaphore RenderingFinishedSemaphore;
        internal SwapchainKhr Swapchain;
        internal Image[] SwapchainImages;
        internal CommandBuffer[] CommandBuffers;
        internal List<PipelineController> Pipelines = new List<PipelineController>();

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
                new[] { Constant.DeviceExtension.KhrSwapchain },
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

            Swapchain = ToDispose(VKHelper.CreateSwapchain(this));
            SwapchainImages = Swapchain.GetImages();

            CommandBuffers = GraphicsCommandPool.AllocateBuffers(
                new CommandBufferAllocateInfo(CommandBufferLevel.Primary, SwapchainImages.Length)
            );
        }

        internal void RecordCommandBuffers()
        {
            var subresourceRange = new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1);
            for (int i = 0; i < CommandBuffers.Length; i++)
            {
                CommandBuffer cmdBuffer = CommandBuffers[i];
                cmdBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.SimultaneousUse));

                if (PresentQueue != GraphicsQueue)
                {
                    var barrierFromPresentToDraw = new ImageMemoryBarrier(
                        SwapchainImages[i], subresourceRange,
                        Accesses.MemoryRead, Accesses.ColorAttachmentWrite,
                        ImageLayout.Undefined, ImageLayout.PresentSrcKhr,
                        PresentQueue.FamilyIndex, GraphicsQueue.FamilyIndex);

                    cmdBuffer.CmdPipelineBarrier(
                        PipelineStages.ColorAttachmentOutput,
                        PipelineStages.ColorAttachmentOutput,
                        imageMemoryBarriers: new[] { barrierFromPresentToDraw });
                }

                Window.RecordCommandBuffer(cmdBuffer, i);

                if (PresentQueue != GraphicsQueue)
                {
                    var barrierFromDrawToPresent = new ImageMemoryBarrier(
                        SwapchainImages[i], subresourceRange,
                        Accesses.ColorAttachmentWrite, Accesses.MemoryRead,
                        ImageLayout.PresentSrcKhr, ImageLayout.PresentSrcKhr,
                        GraphicsQueue.FamilyIndex, PresentQueue.FamilyIndex);

                    cmdBuffer.CmdPipelineBarrier(
                        PipelineStages.ColorAttachmentOutput,
                        PipelineStages.BottomOfPipe,
                        imageMemoryBarriers: new[] { barrierFromDrawToPresent });
                }

                cmdBuffer.End();
            }
        }

        public override void Dispose()
        {
            Pipelines.Dispose();

            base.Dispose();
        }
    }
}
