using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vulpine
{
    public class MeshRenderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MeshRenderInfo
        {
            public Vector3 Translation;
            public Vector3 Scale;
            public Matrix4 Rotation;
            public Vector3 Velocity;
            public Vector2 TextureLeftTop;
            public Vector2 TextureRightBottom;
            

            public override string ToString()
            {
                return $"[MeshInfo Translation={Translation} Scale={Scale} Rotation={Rotation} Velocity={Velocity} TextureLeftTop={TextureLeftTop} TextureTopRight={TextureRightBottom}]";
            }
        }

        public struct MeshInfo
        {
            public MeshRenderInfo RenderInfo;
            public Mesh Mesh;
        }

        public int MaxInstances { get; private set; }
        public int MaxUniqueMeshes { get; private set; }
        Graphics Graphics;
        Texture2D Texture;
        Dictionary<VKImage, CommandBufferController> CBuffer = new Dictionary<VKImage, CommandBufferController>();
        PipelineController Pipeline;
        VKBuffer[] Instances;
        VKBuffer UProjection;
        VKBuffer UTime;
        int[] Count;
        MeshInfo[] TempInstances;
        MeshRenderInfo[] InstanceCopyBuffer;
        Mesh[] Meshes;

        public Matrix4 Projection = Matrix4.Identity;

        public BlendMode BlendMode
        {
            get
            {
                return Pipeline.BlendMode;
            }
            set
            {
                Pipeline.BlendMode = value;
            }
        }

        public Vector2 ViewportPos
        {
            get
            {
                return Pipeline.ViewportPos;
            }
            set
            {
                Pipeline.ViewportPos = value;
            }
        }

        public Vector2 ViewportSize
        {
            get
            {
                return Pipeline.ViewportSize;
            }
            set
            {
                Pipeline.ViewportSize = value;
            }
        }

        public MeshRenderer(Graphics g, Texture2D tex, string vertexShader, string fragmentShader, int maxInstances = 1024, int maxUniqueMeshes = 8)
        {
            MaxInstances = maxInstances;
            MaxUniqueMeshes = maxUniqueMeshes;
            Graphics = g;
            Texture = tex;
            Instances = new VKBuffer[maxUniqueMeshes];
            Count = new int[maxUniqueMeshes];
            Meshes = new Mesh[maxUniqueMeshes];
            for (var i = 0; i < maxUniqueMeshes; i++)
                Instances[i] = VKBuffer.InstanceInfo<MeshRenderInfo>(g, maxInstances);
            TempInstances = new MeshInfo[maxInstances];
            InstanceCopyBuffer = new MeshRenderInfo[maxInstances];
            UProjection = VKBuffer.UniformBuffer<Matrix4>(g, 1);
            UTime = VKBuffer.UniformBuffer<float>(g, 1);

            Pipeline = new PipelineController(Graphics);
            Pipeline.ClearDepthOnBeginPass = true;
            Pipeline.DepthTest = false;
            Pipeline.DepthWrite = false;
            Pipeline.BlendMode = BlendMode.AlphaPremultiplied;
            Pipeline.Instancing = true;
            Pipeline.InstanceInfoType = typeof(MeshRenderInfo);
            Pipeline.Shaders = new[] { vertexShader, fragmentShader };
            Pipeline.DescriptorItems = new[] {
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UProjection),
                DescriptorItem.UniformBuffer(DescriptorItem.ShaderType.Vertex, UTime),
                DescriptorItem.CombinedImageSampler(DescriptorItem.ShaderType.Fragment, tex, DescriptorItem.SamplerFilter.Nearest, DescriptorItem.SamplerFilter.Nearest)
            };
        }

        public void BuildPipeline()
        {
            Pipeline.Build();
        }

        public void AddImage(VKImage image)
        {
            var cb = new CommandBufferController(Graphics, image);
            CBuffer.Add(image, cb);
        }

        public void RemoveImage(VKImage image)
        {
            CBuffer[image].Dispose();
            CBuffer.Remove(image);
        }

        public enum MeshInfoSortMode
        {
            DontSort,
            SortCopy,
            SortOriginal
        }
        public void SetMeshInfo(MeshInfo[] instances, int count, MeshInfoSortMode sortMode = MeshInfoSortMode.SortOriginal)
        {
            switch (sortMode)
            {
                case MeshInfoSortMode.DontSort:
                    TempInstances = instances;
                    break;
                case MeshInfoSortMode.SortCopy:
                    instances.CopyTo(TempInstances, 0);
                    TempInstances.OrderBy(info => info.Mesh.ID);
                    break;
                case MeshInfoSortMode.SortOriginal:
                    TempInstances = instances;
                    TempInstances.OrderBy(info => info.Mesh.ID);
                    break;
            }

            for (var i = 0; i < MaxUniqueMeshes; i++)
                Count[i] = 0;

            var instanceI = 0;
            var copyI = 0;
            Mesh lastMesh = null;
            for (var i = 0; i < count; i++)
            {
                var info = TempInstances[i];

                if (info.Mesh != lastMesh)
                {
                    if (lastMesh != null)
                    {
                        Meshes[instanceI] = lastMesh;
                        Instances[instanceI].Write(InstanceCopyBuffer.Take(copyI).ToArray());
                        Count[instanceI] = copyI;
                        instanceI++;

                        copyI = 0;
                    }
                    lastMesh = info.Mesh;
                }

                InstanceCopyBuffer[copyI++] = info.RenderInfo;
            }
            Meshes[instanceI] = lastMesh;
            Instances[instanceI].Write(InstanceCopyBuffer.Take(copyI).ToArray());
            Count[instanceI] = copyI;
        }

        public void Draw(VKImage image, float tick)
        {
            UProjection.Write(ref Projection);
            UTime.Write(ref tick);

            var cb = CBuffer[image];
            cb.Begin();
            cb.BeginPass(Pipeline);
            for (var i = 0; i < MaxUniqueMeshes; i++)
            {
                var count = Count[i];
                if (count > 0)
                {
                    cb.Draw(Meshes[i], Instances[i], count);
                }
            }
            cb.EndPass();
            cb.End();

            cb.Submit(true);
            cb.Reset();
        }

        public void Dispose()
        {
            CBuffer?.Values?.DisposeRange();
            Pipeline?.Dispose();
            Instances?.DisposeRange();
            UProjection?.Dispose();
            UTime?.Dispose();
        }

        public MeshInfo CreateMeshInfo(Mesh mesh, Sprite.Sprite sprite, Vector3 translation, Vector3 scale, Matrix4 rotation, Vector3 velocity)
        {
            return new MeshInfo
            {
                Mesh = mesh,
                RenderInfo = new MeshRenderInfo
                {
                    Translation = translation,
                    TextureLeftTop = sprite.LeftTop / Texture.SizeF,
                    TextureRightBottom = sprite.RightBottom / Texture.SizeF,
                    Scale = scale,
                    Rotation = rotation,
                    Velocity = velocity
                }
            };
        }
    }
}
