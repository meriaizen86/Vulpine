using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VulkanCore;

namespace Vulpine
{
    public class Graphics : IDisposable
    {
        static ImageSubresourceRange DefaultSubresourceRange = new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1);

        internal Context Context;
        public int Samples = 1;

        public Mesh Square { get; private set; }
        public Mesh SquareSprite { get; private set; }
        public Mesh Triangle { get; private set; }

        internal Graphics(Context context)
        {
            Context = context;

            Square = new Mesh(Context,
                new Vertex[] {
                    new Vertex( new Vector3(-0.5f, -0.5f, 0f),  new Vector3(0f, 0f, 1f),    new Vector2(0f, 0f),    Color4.White ),
                    new Vertex( new Vector3(0.5f, -0.5f, 0f),   new Vector3(0f, 0f, 1f),    new Vector2(1f, 0f),    Color4.White ),
                    new Vertex( new Vector3(0.5f, 0.5f, 0f),    new Vector3(0f, 0f, 1f),    new Vector2(1f, 1f),    Color4.White ),
                    new Vertex( new Vector3(-0.5f, 0.5f, 0f),   new Vector3(0f, 0f, 1f),    new Vector2(0f, 1f),    Color4.White ),
                },
                new int[] {
                    0, 1, 2, 2, 3, 0
                },
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            );
            SquareSprite = new Mesh(Context,
                new Vertex[] {
                    new Vertex( new Vector3(0f, 0f, 0f),    new Vector3(0f, 0f, 1f),    new Vector2(0f, 0f),    Color4.White ),
                    new Vertex( new Vector3(1f, 0f, 0f),    new Vector3(0f, 0f, 1f),    new Vector2(1f, 0f),    Color4.White ),
                    new Vertex( new Vector3(1f, 1f, 0f),    new Vector3(0f, 0f, 1f),    new Vector2(1f, 1f),    Color4.White ),
                    new Vertex( new Vector3(0f, 1f, 0f),    new Vector3(0f, 0f, 1f),    new Vector2(0f, 1f),    Color4.White ),
                },
                new int[] {
                    0, 1, 2, 2, 3, 0
                },
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 1f, 0f)
            );
            Triangle = new Mesh(Context,
                new Vertex[] {
                    new Vertex( new Vector3(-0.5f, -0.5f, 0f),  new Vector3(0f, 0f, 1f),    new Vector2(0f, 0f),    Color4.White),
                    new Vertex( new Vector3(0.5f, -0.5f, 0f),   new Vector3(0f, 0f, 1f),    new Vector2(1f, 0f),    Color4.White),
                    new Vertex( new Vector3(0f, 0.5f, 0f),    new Vector3(0f, 0f, 1f),    new Vector2(0.5f, 1f),  Color4.White),
                },
                new int[] {
                    0, 1, 2
                },
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            );
        }

        public void Dispose()
        {
            Square?.Dispose();
            Triangle?.Dispose();
        }

        public VKImage[] SwapchainImages => Context.SwapchainImages;

        /*public void Draw(int indexCount, int instances = 1, int firstIndex = 0, int firstVertex = 0, int firstInstance = 0)
        {
            CommandBuffer.CmdDrawIndexed(indexCount, instances, firstIndex, firstVertex, firstInstance);
        }*/

        /*public void SetViewport(float x, float y, float width, float height)
        {
            CommandBuffer.CmdSetViewport(new Viewport(x, y, width, height));
        }*/
        
    }
}
