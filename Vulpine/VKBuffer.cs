using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class VKBuffer : IDisposable
    {
        private VKBuffer(Context ctx, VulkanCore.Buffer buffer, DeviceMemory memory, Fence fence, int count, long size, bool writeUsingStagingBuffer = false, VulkanCore.Buffer stagingBuffer = null, DeviceMemory stagingMemory = null, CommandBuffer stagingCommandBuffer = null)
        {
            Context = ctx;
            Buffer = buffer;
            Memory = memory;
            Count = count;
            Size = size;
            WriteUsingStagingBuffer = writeUsingStagingBuffer;
            StagingBuffer = stagingBuffer;
            StagingMemory = stagingMemory;
            StagingCommandBuffer = stagingCommandBuffer;
            Fence = fence;
        }

        Context Context;
        internal VulkanCore.Buffer Buffer { get; }
        VulkanCore.Buffer StagingBuffer;
        DeviceMemory StagingMemory;
        internal DeviceMemory Memory { get; }
        internal int Count { get; }
        internal long Size { get; }
        bool WriteUsingStagingBuffer;
        internal Fence Fence;
        internal CommandBuffer StagingCommandBuffer;

        public void Dispose()
        {
            Fence?.Dispose();
            Memory.Dispose();
            Buffer.Dispose();
            if (WriteUsingStagingBuffer)
            {
                StagingMemory?.Dispose();
                StagingBuffer?.Dispose();
                StagingCommandBuffer?.Dispose();
            }
        }

        public static implicit operator VulkanCore.Buffer(VKBuffer value) => value.Buffer;

        /*internal static VKBuffer DynamicUniform<T>(Context ctx, int count) where T : struct
        {
            long size = Interop.SizeOf<T>() * count;

            VulkanCore.Buffer buffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.UniformBuffer));
            MemoryRequirements memoryRequirements = buffer.GetMemoryRequirements();
            // We require host visible memory so we can map it and write to it directly.
            // We require host coherent memory so that writes are visible to the GPU right after unmapping it.
            int memoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                memoryRequirements.MemoryTypeBits,
                MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
            DeviceMemory memory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(memoryRequirements.Size, memoryTypeIndex));
            buffer.BindMemory(memory);

            return new VKBuffer(ctx, buffer, memory, null, count, size);
        }*/

        public enum UniformUsageHint
        {
            ModifiedOften,
            ModifiedRarely
        }

        public static VKBuffer UniformBuffer<T>(Graphics g, int count, UniformUsageHint usage = UniformUsageHint.ModifiedRarely) where T : struct
        {
            long size = count * Interop.SizeOf<T>();

            if (usage == UniformUsageHint.ModifiedRarely)
            {
                // Create a staging buffer that is writable by host.
                var stagingBuffer = g.Context.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.TransferSrc));
                MemoryRequirements stagingReq = stagingBuffer.GetMemoryRequirements();
                int stagingMemoryTypeIndex = g.Context.MemoryProperties.MemoryTypes.IndexOf(
                    stagingReq.MemoryTypeBits,
                    MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
                DeviceMemory stagingMemory = g.Context.Device.AllocateMemory(new MemoryAllocateInfo(stagingReq.Size, stagingMemoryTypeIndex));
                stagingBuffer.BindMemory(stagingMemory);

                // Create a device local buffer where the vertex data will be copied and which will be used for rendering.
                VulkanCore.Buffer buffer = g.Context.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.UniformBuffer | BufferUsages.TransferDst));
                MemoryRequirements req = buffer.GetMemoryRequirements();
                int memoryTypeIndex = g.Context.MemoryProperties.MemoryTypes.IndexOf(
                    req.MemoryTypeBits,
                    MemoryProperties.DeviceLocal);
                DeviceMemory memory = g.Context.Device.AllocateMemory(new MemoryAllocateInfo(req.Size, memoryTypeIndex));
                buffer.BindMemory(memory);

                return new VKBuffer(g.Context, buffer, memory, g.Context.Device.CreateFence(), count, size, true, stagingBuffer, stagingMemory, g.Context.GraphicsCommandPool.AllocateBuffers(new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1))[0]);
            }
            else
            {
                VulkanCore.Buffer buffer = g.Context.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.UniformBuffer));
                MemoryRequirements memoryRequirements = buffer.GetMemoryRequirements();
                // We require host visible memory so we can map it and write to it directly.
                // We require host coherent memory so that writes are visible to the GPU right after unmapping it.
                int memoryTypeIndex = g.Context.MemoryProperties.MemoryTypes.IndexOf(
                    memoryRequirements.MemoryTypeBits,
                    MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
                DeviceMemory memory = g.Context.Device.AllocateMemory(new MemoryAllocateInfo(memoryRequirements.Size, memoryTypeIndex));
                buffer.BindMemory(memory);

                return new VKBuffer(g.Context, buffer, memory, null, count, size);
            }
        }

        internal static VKBuffer Index(Context ctx, int[] indices)
        {
            long size = indices.Length * sizeof(int);

            // Create staging buffer.
            VulkanCore.Buffer stagingBuffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.TransferSrc));
            MemoryRequirements stagingReq = stagingBuffer.GetMemoryRequirements();
            int stagingMemoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                stagingReq.MemoryTypeBits,
                MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
            DeviceMemory stagingMemory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(stagingReq.Size, stagingMemoryTypeIndex));
            IntPtr indexPtr = stagingMemory.Map(0, stagingReq.Size);
            Interop.Write(indexPtr, indices);
            stagingMemory.Unmap();
            stagingBuffer.BindMemory(stagingMemory);

            // Create a device local buffer.
            VulkanCore.Buffer buffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.IndexBuffer | BufferUsages.TransferDst));
            MemoryRequirements req = buffer.GetMemoryRequirements();
            int memoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                req.MemoryTypeBits,
                MemoryProperties.DeviceLocal);
            DeviceMemory memory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(req.Size, memoryTypeIndex));
            buffer.BindMemory(memory);

            // Copy the data from staging buffer to device local buffer.
            CommandBuffer cmdBuffer = ctx.GraphicsCommandPool.AllocateBuffers(new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1))[0];
            cmdBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
            cmdBuffer.CmdCopyBuffer(stagingBuffer, buffer, new BufferCopy(size));
            cmdBuffer.End();

            // Submit.
            Fence fence = ctx.Device.CreateFence();
            ctx.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { cmdBuffer }), fence);
            fence.Wait();

            // Cleanup.
            fence.Dispose();
            cmdBuffer.Dispose();
            stagingBuffer.Dispose();
            stagingMemory.Dispose();

            return new VKBuffer(ctx, buffer, memory, null, indices.Length, size);
        }

        internal static VKBuffer Vertex<T>(Context ctx, T[] vertices) where T : struct
        {
            long size = vertices.Length * Interop.SizeOf<T>();

            // Create a staging buffer that is writable by host.
            VulkanCore.Buffer stagingBuffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.TransferSrc));
            MemoryRequirements stagingReq = stagingBuffer.GetMemoryRequirements();
            int stagingMemoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                stagingReq.MemoryTypeBits,
                MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
            DeviceMemory stagingMemory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(stagingReq.Size, stagingMemoryTypeIndex));
            IntPtr vertexPtr = stagingMemory.Map(0, stagingReq.Size);
            Interop.Write(vertexPtr, vertices);
            stagingMemory.Unmap();
            stagingBuffer.BindMemory(stagingMemory);

            // Create a device local buffer where the vertex data will be copied and which will be used for rendering.
            VulkanCore.Buffer buffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.VertexBuffer | BufferUsages.TransferDst));
            MemoryRequirements req = buffer.GetMemoryRequirements();
            int memoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                req.MemoryTypeBits,
                MemoryProperties.DeviceLocal);
            DeviceMemory memory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(req.Size, memoryTypeIndex));
            buffer.BindMemory(memory);

            // Copy the data from staging buffers to device local buffers.
            CommandBuffer cmdBuffer = ctx.GraphicsCommandPool.AllocateBuffers(new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1))[0];
            cmdBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
            cmdBuffer.CmdCopyBuffer(stagingBuffer, buffer, new BufferCopy(size));
            cmdBuffer.End();

            // Submit.
            Fence fence = ctx.Device.CreateFence();
            ctx.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { cmdBuffer }), fence);
            fence.Wait();

            // Cleanup.
            fence.Dispose();
            cmdBuffer.Dispose();
            stagingBuffer.Dispose();
            stagingMemory.Dispose();

            return new VKBuffer(ctx, buffer, memory, null, vertices.Length, size);
        }

        public static VKBuffer InstanceInfo<T>(Graphics g, int count) where T : struct
        {
            long size = count * Interop.SizeOf<T>();

            // Create a staging buffer that is writable by host.
            var stagingBuffer = g.Context.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.TransferSrc));
            MemoryRequirements stagingReq = stagingBuffer.GetMemoryRequirements();
            int stagingMemoryTypeIndex = g.Context.MemoryProperties.MemoryTypes.IndexOf(
                stagingReq.MemoryTypeBits,
                MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
            DeviceMemory stagingMemory = g.Context.Device.AllocateMemory(new MemoryAllocateInfo(stagingReq.Size, stagingMemoryTypeIndex));
            stagingBuffer.BindMemory(stagingMemory);

            // Create a device local buffer where the vertex data will be copied and which will be used for rendering.
            VulkanCore.Buffer buffer = g.Context.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.VertexBuffer | BufferUsages.TransferDst));
            MemoryRequirements req = buffer.GetMemoryRequirements();
            int memoryTypeIndex = g.Context.MemoryProperties.MemoryTypes.IndexOf(
                req.MemoryTypeBits,
                MemoryProperties.DeviceLocal);
            DeviceMemory memory = g.Context.Device.AllocateMemory(new MemoryAllocateInfo(req.Size, memoryTypeIndex));
            buffer.BindMemory(memory);

            return new VKBuffer(g.Context, buffer, memory, g.Context.Device.CreateFence(), count, size, true, stagingBuffer, stagingMemory, g.Context.GraphicsCommandPool.AllocateBuffers(new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1))[0]);
        }

        internal static VKBuffer Storage<T>(Context ctx, T[] data) where T : struct
        {
            long size = data.Length * Interop.SizeOf<T>();

            // Create a staging buffer that is writable by host.
            VulkanCore.Buffer stagingBuffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.TransferSrc));
            MemoryRequirements stagingReq = stagingBuffer.GetMemoryRequirements();
            int stagingMemoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                stagingReq.MemoryTypeBits,
                MemoryProperties.HostVisible | MemoryProperties.HostCoherent);
            DeviceMemory stagingMemory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(stagingReq.Size, stagingMemoryTypeIndex));
            IntPtr vertexPtr = stagingMemory.Map(0, stagingReq.Size);
            Interop.Write(vertexPtr, data);
            stagingMemory.Unmap();
            stagingBuffer.BindMemory(stagingMemory);

            // Create a device local buffer where the data will be copied.
            VulkanCore.Buffer buffer = ctx.Device.CreateBuffer(new BufferCreateInfo(size, BufferUsages.VertexBuffer | BufferUsages.StorageBuffer | BufferUsages.TransferDst));
            MemoryRequirements req = buffer.GetMemoryRequirements();
            int memoryTypeIndex = ctx.MemoryProperties.MemoryTypes.IndexOf(
                req.MemoryTypeBits,
                MemoryProperties.DeviceLocal);
            DeviceMemory memory = ctx.Device.AllocateMemory(new MemoryAllocateInfo(req.Size, memoryTypeIndex));
            buffer.BindMemory(memory);

            // Copy the data from staging buffers to device local buffers.
            CommandBuffer cmdBuffer = ctx.GraphicsCommandPool.AllocateBuffers(new CommandBufferAllocateInfo(CommandBufferLevel.Primary, 1))[0];
            cmdBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
            cmdBuffer.CmdCopyBuffer(stagingBuffer, buffer, new BufferCopy(size));
            cmdBuffer.End();

            // Submit.
            Fence fence = ctx.Device.CreateFence();
            ctx.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { cmdBuffer }), fence);
            fence.Wait();

            // Cleanup.
            fence.Dispose();
            cmdBuffer.Dispose();
            stagingBuffer.Dispose();
            stagingMemory.Dispose();

            return new VKBuffer(ctx, buffer, memory, null, data.Length, size);
        }

        public void Write<T>(ref T value) where T : struct
        {
            if (WriteUsingStagingBuffer)
            {
                MemoryRequirements stagingReq = StagingBuffer.GetMemoryRequirements();
                IntPtr vertexPtr = StagingMemory.Map(0, stagingReq.Size);
                Interop.Write(vertexPtr, ref value);
                StagingMemory.Unmap();

                // Copy the data from staging buffers to device local buffers.
                StagingCommandBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
                StagingCommandBuffer.CmdCopyBuffer(StagingBuffer, Buffer, new BufferCopy(Size));
                StagingCommandBuffer.End();

                // Submit.
                Context.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { StagingCommandBuffer }), Fence);
                Fence.Wait();
                Fence.Reset();
                StagingCommandBuffer.Reset();

                return;
            }
            IntPtr ptr = Memory.Map(0, Interop.SizeOf<T>());
            Interop.Write(ptr, ref value);
            Memory.Unmap();
        }

        public void Write<T>(T[] values, long pos = 0) where T : struct
        {
            var size = Interop.SizeOf<T>();
            var count = values.Length;
            if (WriteUsingStagingBuffer)
            {
                MemoryRequirements stagingReq = StagingBuffer.GetMemoryRequirements();
                var mapSize = Math.Min(count * size, stagingReq.Size - pos * size);
                IntPtr vertexPtr = StagingMemory.Map(pos * size, mapSize);
                Interop.Write(vertexPtr, values);
                StagingMemory.Unmap();

                // Copy the data from staging buffers to device local buffers.
                StagingCommandBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
                StagingCommandBuffer.CmdCopyBuffer(StagingBuffer, Buffer, new BufferCopy(mapSize, pos * size, pos * size));
                StagingCommandBuffer.End();

                // Submit.
                Context.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { StagingCommandBuffer }), Fence);
                Fence.Wait();
                Fence.Reset();
                StagingCommandBuffer.Reset();

                return;
            }

           
            IntPtr ptr = Memory.Map(pos * size, count * size);
            Interop.Write(ptr, values);
            Memory.Unmap();
        }

        public void Write<T>(IList<T> values, long pos = 0) where T : struct
        {
            var size = Interop.SizeOf<T>();
            var count = values.Count;
            if (WriteUsingStagingBuffer)
            {
                MemoryRequirements stagingReq = StagingBuffer.GetMemoryRequirements();
                var mapSize = Math.Min(count * size, stagingReq.Size - pos * size);
                IntPtr vertexPtr = StagingMemory.Map(pos * size, mapSize);
                for (var i = 0; i < count; i++)
                {
                    var value = values[i];
                    Interop.Write(vertexPtr + i * size, ref value);
                }
                StagingMemory.Unmap();

                // Copy the data from staging buffers to device local buffers.
                StagingCommandBuffer.Begin(new CommandBufferBeginInfo(CommandBufferUsages.OneTimeSubmit));
                StagingCommandBuffer.CmdCopyBuffer(StagingBuffer, Buffer, new BufferCopy(mapSize, pos * size, pos * size));
                StagingCommandBuffer.End();

                // Submit.
                Context.GraphicsQueue.Submit(new SubmitInfo(commandBuffers: new[] { StagingCommandBuffer }), Fence);
                Fence.Wait();
                Fence.Reset();
                StagingCommandBuffer.Reset();

                return;
            }


            IntPtr ptr = Memory.Map(pos * size, count * size);
            for (var i = 0; i < count; i++)
            {
                var value = values[i];
                Interop.Write(ptr + i * size, ref value);
            }
            Memory.Unmap();
        }
    }
}
