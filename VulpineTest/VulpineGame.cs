using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulpine;

namespace VulpineTest
{
    class VulpineGame : GameWindow
    {
        PipelineController Pipeline;

        public VulpineGame() : base("Vulpine Test", new Vector2I(800, 600))
        {

        }
        

        protected override void OnInit()
        {
            base.OnInit();

            Pipeline = new PipelineController(this);
        }

        protected override void OnBuildPipeline()
        {
            base.OnBuildPipeline();

            Pipeline.Build();
        }

        protected override void OnLoad()
        {
            base.OnLoad();


        }

        protected override void OnUpdate(int tick)
        {
            base.OnUpdate(tick);

            if (tick % 15 == 0)
                Title = $"FPS: {ActualFPS}";
        }

        protected override void OnRecordCommandBuffer()
        {
            base.OnRecordCommandBuffer();


            Graphics.BeginPass(Pipeline);
            Graphics.Draw(3);
            Graphics.EndPass();
        }
    }
}
