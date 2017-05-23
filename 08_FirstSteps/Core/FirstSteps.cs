using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{
    public class FirstSteps : RenderCanvas
    {
        private float _camAngle = 0;
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private TransformComponent _cubeTransform1;
        private TransformComponent _cubeTransform2;
        private TransformComponent _cubeTransform3;
        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(1, 1, 0.5f, 0.3f);

            // Create a scene with a cube
            // The three components: one XForm, one Material and the Mesh

            //Würfel 1
            _cubeTransform1 = new TransformComponent { Scale = new float3(1, 1, 1), Translation = new float3(0, 0, 0), Rotation = new float3(0.3f, 1, 0) };
            // var cubeTransform = new TransformComponent { Scale = new float3(1, 1, 1), Translation = new float3(0, 0, 0), Rotation = new float3(0.3f,1,0) };
            var cubeMaterial1 = new MaterialComponent
            {
                Diffuse = new MatChannelContainer { Color = new float3(1, 0.32f, 0.04f )},
                Specular = new SpecularChannelContainer { Color = float3.One, Shininess = 4 }
            };
            var cubeMesh1 = SimpleMeshes.CreateCuboid(new float3(30, 20, 20));


            //Würfel 2
            _cubeTransform2 = new TransformComponent { Scale = new float3(2, 2, 2), Rotation = new float3(0, 1, 0), Translation = new float3(0, 40, 40) };
            var cubeMaterial2 = new MaterialComponent
            {
                Diffuse = new MatChannelContainer { Color = new float3(1, 0.1f, 0.04f) },
                Specular = new SpecularChannelContainer { Color = float3.One, Shininess = 4 }
            };
            var cubeMesh2 = SimpleMeshes.CreateCuboid(new float3(30, 20, 20));



            //Würfel 3
            _cubeTransform3 = new TransformComponent { Scale = new float3(0.5f, 0.5f, 0.5f), Translation = new float3(-80, 0, 0), Rotation = new float3(0, 0, 0) };
            var cubeMaterial3 = new MaterialComponent
            {
                Diffuse = new MatChannelContainer { Color = new float3(1, 0.72f, 0.04f) },
                Specular = new SpecularChannelContainer { Color = float3.One, Shininess = 4 }
            };
            var cubeMesh3 = SimpleMeshes.CreateCuboid(new float3(30, 20, 20));



            // Assemble the cube node containing the three components Würfel 1
            var cubeNode1 = new SceneNodeContainer();
            cubeNode1.Components = new List<SceneComponentContainer>();
            cubeNode1.Components.Add(_cubeTransform1);
            cubeNode1.Components.Add(cubeMaterial1);
            cubeNode1.Components.Add(cubeMesh1);

            // Assemble the cube node containing the three components Würfel 2
            var cubeNode2 = new SceneNodeContainer();
            cubeNode2.Components = new List<SceneComponentContainer>();
            cubeNode2.Components.Add(_cubeTransform2);
            cubeNode2.Components.Add(cubeMaterial2);
            cubeNode2.Components.Add(cubeMesh2);

            // Assemble the cube node containing the three components Würfel 2
            var cubeNode3 = new SceneNodeContainer();
            cubeNode3.Components = new List<SceneComponentContainer>();
            cubeNode3.Components.Add(_cubeTransform3);
            cubeNode3.Components.Add(cubeMaterial3);
            cubeNode3.Components.Add(cubeMesh3);

            // Create the scene containing the cube as the only object
            _scene = new SceneContainer();
            _scene.Children = new List<SceneNodeContainer>();
            _scene.Children.Add(cubeNode1);
            _scene.Children.Add(cubeNode2);
            _scene.Children.Add(cubeNode3);

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            _camAngle = _camAngle + 90.0f * M.Pi / 180.0f * DeltaTime;

            //Kamera
            RC.View = float4x4.CreateTranslation(0, 0, 50) * float4x4.CreateRotationY(_camAngle);

            //Aufgabe
            _sceneRenderer.Render(RC);

            //Animation Würfel 1
            _cubeTransform1.Translation = new float3(0, 5 * M.Sin(3 * TimeSinceStart), 0);
            //Animation Würfel 2
            _cubeTransform3.Scale = new float3(M.Sin(TimeSinceStart) + 1, M.Sin(TimeSinceStart) + 1, M.Sin(TimeSinceStart) + 1);
            //Animation Würfel 3
            _cubeTransform2.Rotation = new float3(0, 0, 5 * M.Sin(TimeSinceStart));

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}