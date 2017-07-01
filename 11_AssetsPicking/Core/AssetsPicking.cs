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
    public class AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private float _camAngle = 0;
        private TransformComponent _baseTransform;
        private TransformComponent _rightRearTransform;
        private TransformComponent _leftRearTransform;
        private TransformComponent _rightFrontTransform;
        private TransformComponent _leftFrontTransform;
        private TransformComponent _greiferTransofrm;
        private TransformComponent _roverTransform;
        private float _camAngleVelocity = 0;
        private TransformComponent _trailerTransform;
        private float _d = 20;


        SceneContainer CreateScene()
        {
            // Initialize transform components that need to be changed inside "RenderAFrame"
            _baseTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };

            // Setup the scene graph
            return new SceneContainer
            {
                Children = new List<SceneNodeContainer>
                {
                    new SceneNodeContainer
                    {
                        Components = new List<SceneComponentContainer>
                        {
                            // TRANSFROM COMPONENT
                            _baseTransform,

                            // MATERIAL COMPONENT
                            new MaterialComponent
                            {
                                Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) },
                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                            },

                            // MESH COMPONENT
                            // SimpleAssetsPickinges.CreateCuboid(new float3(10, 10, 10))
                            SimpleMeshes.CreateCuboid(new float3(10, 10, 10))
                        }
                    },
                }
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("rosimple.fus");

            //aufgreifen der private Variablen oben 
            _leftFrontTransform = _scene.Children.FindNodes(node => node.Name == "reifenVL")?.FirstOrDefault()?.GetTransform();
            _rightFrontTransform = _scene.Children.FindNodes(node => node.Name == "reifenVR")?.FirstOrDefault()?.GetTransform();
            _leftRearTransform = _scene.Children.FindNodes(node => node.Name == "ReifenHL")?.FirstOrDefault()?.GetTransform();
            _rightRearTransform = _scene.Children.FindNodes(node => node.Name == "reifenHR")?.FirstOrDefault()?.GetTransform();
            _greiferTransofrm = _scene.Children.FindNodes(node => node.Name == "greifer")?.FirstOrDefault()?.GetTransform();
            _roverTransform= _scene.Children.FindNodes(node => node.Name == "auto")?.FirstOrDefault()?.GetTransform();
            _trailerTransform = new TransformComponent { Rotation = new float3(-M.Pi / 5.7f, 0, 0), Scale = float3.One, Translation = new float3(0, 0, 10) };


            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
            _scene.Children.Add(new SceneNodeContainer
            {
                Components = new List<SceneComponentContainer>
                {
                    _trailerTransform,
                    new MaterialComponent { Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) }, Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }},
                    SimpleMeshes.CreateCuboid(new float3(2, 2, 2))
                }
            });
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // wird nicht mehr benötigt _baseTransform.Rotation = new float3(0, M.MinAngle(TimeSinceStart), 0);


            // Clear the backbuffer
            //kamera 
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            RC.View = float4x4.CreateRotationX(-M.Pi / 7.3f) * float4x4.CreateRotationY(M.Pi - _trailerTransform.Rotation.y) * float4x4.CreateTranslation(-_trailerTransform.Translation.x, -6, -_trailerTransform.Translation.z);



            //Steuerung ohne Tractor Trailor Methode 
            float posVel = Input.Keyboard.WSAxis * DeltaTime;
            float rotVel = Input.Keyboard.ADAxis * DeltaTime;
            float newRot = _roverTransform.Rotation.y + rotVel;
            _roverTransform.Rotation = new float3(0, newRot, 0);

       //Teilweise aus Vorlesung Räder hat leider nicht funktioniert und die Steuerung ist etwas seltsam.
            float3 pBalt = _trailerTransform.Translation;

            float3 pAneu = _roverTransform.Translation + new float3(-10 * posVel * M.Sin(newRot), 0, -10* posVel);
            _roverTransform.Translation = pAneu;

            float3 pBneu = pAneu + (float3.Normalize(pBalt - pAneu) * _d);
            _trailerTransform.Translation = pBneu;

            _trailerTransform.Rotation = new float3(0, (float)System.Math.Atan2(float3.Normalize(pBalt - pAneu).x, float3.Normalize(pBalt - pAneu).z), 0);

           
            // Setup the camera 
            RC.View = float4x4.CreateRotationX(-M.Pi / 7.3f) * float4x4.CreateRotationY(M.Pi - _trailerTransform.Rotation.y) * float4x4.CreateTranslation(-_trailerTransform.Translation.x, -6, -_trailerTransform.Translation.z);



            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
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