using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EzEngine.ContentManagement.Mono.Interop;
using EzEngine.ContentManagement.Mono.Interop.Models;
using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using EzEngine.Prototype.Cameras;

namespace EzEngine.Prototype;

public class Renderer : Game
{
    private GraphicsDeviceManager _graphicsDeviceManager;
    private NoclipCamera _defaultCamera;
    private List<TriangleListPrimitive> _triangleListPrimitives;
    private List<LineListPrimitive> _lineListPrimitives;
    private BasicEffect _defaultRenderEffect;

    private ProcessedPolyOneFileVolumeSet[] _collisionVolumeSets;

    public Renderer()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        _graphicsDeviceManager.PreferredBackBufferWidth = 1920;
        _graphicsDeviceManager.PreferredBackBufferHeight = 1080;
        _graphicsDeviceManager.ApplyChanges();

        _triangleListPrimitives = [];
        _lineListPrimitives = [];
        _defaultCamera = new NoclipCamera(GraphicsDevice);

        var collisionVolumeSets = new List<ProcessedPolyOneFileVolumeSet>();

        var currentDirectory = Directory.GetCurrentDirectory();
        var level = Converters.LoadConvertedPolyOneFile($"{currentDirectory}\\Maps\\map_test.jsv");
        level.RemoveOffset();
        foreach (var volumeSet in level.Volumes)
        {
            collisionVolumeSets.Add(volumeSet);
            //_lineListPrimitives.Add(volumeSet.GetLineListPrimitive(GraphicsDevice));
            _lineListPrimitives.Add(volumeSet.OverallAxisAlignedBoundingBox.GetVisualization(GraphicsDevice));
            _lineListPrimitives.AddRange(volumeSet.AxisAlignedBoundingBoxes.Select(x => x.GetVisualization(GraphicsDevice)));
        }
        foreach (var levelPrimitive in level.PrimitiveGroups[0].Primitives)
        {
            if (levelPrimitive.VertexPositions.Length > 0)
            {
                if (levelPrimitive.Name == "PointLights")
                {

                }
                else if (levelPrimitive.Name == "Volumes")
                {
                }
                else if (levelPrimitive.Name == "Models")
                {
                    for (var i = 0; i < levelPrimitive.VertexPositions.Length; i += 3)
                    {
                        levelPrimitive.VertexPositions[i+1].Z += levelPrimitive.VertexPositions[i].Z;
                        levelPrimitive.VertexPositions[i+2].Z += levelPrimitive.VertexPositions[i].Z;
                    }
                    levelPrimitive.VertexSurfaceNormals = Helpers.CalculateSurfaceNormals(levelPrimitive.VertexPositions);
                    var modelName = levelPrimitive.CustomVertexProperties["ModelName"].Values;
                    for (var i = 0; i < levelPrimitive.VertexPositions.Length; i += 3)
                    {
                        var model = Converters.LoadConvertedPolyOneFile($"{currentDirectory}\\Models\\{modelName[i]}.jsv");
                        model.RemoveOffset();
                        model.ApplyTransformation(
                            levelPrimitive.VertexPositions[i],
                            levelPrimitive.VertexPositions[i + 1],
                            levelPrimitive.VertexPositions[i + 2]
                        );
                        foreach (var volumeSet in model.Volumes)
                        {
                            collisionVolumeSets.Add(volumeSet);
                            //_lineListPrimitives.Add(volumeSet.GetLineListPrimitive(GraphicsDevice));
                            _lineListPrimitives.Add(volumeSet.OverallAxisAlignedBoundingBox.GetVisualization(GraphicsDevice));
                            _lineListPrimitives.AddRange(volumeSet.AxisAlignedBoundingBoxes.Select(x => x.GetVisualization(GraphicsDevice)));
                        }

                        foreach (var modelPrimitive in model.PrimitiveGroups[0].Primitives)
                        {
                            modelPrimitive.CalculateLighting([], level.DirectionalLightVector, level.DirectionalLightColour.Value, new Color(0.15F, 0.175F, 0.2F));

                            _triangleListPrimitives.Add(new TriangleListPrimitive(GraphicsDevice,
                                modelPrimitive.VertexPositions, //newVertexPositions,
                                modelPrimitive.LitVertexColours,
                                modelPrimitive.VertexTextureCoordinates,
                                modelPrimitive.TextureName));
                        }
                    }
                }
                else
                {
                    levelPrimitive.CalculateLighting([], level.DirectionalLightVector, level.DirectionalLightColour.Value, new Color(0.15F, 0.175F, 0.2F));

                    _triangleListPrimitives.Add(new TriangleListPrimitive(GraphicsDevice,
                        levelPrimitive.VertexPositions,
                        levelPrimitive.LitVertexColours,
                        levelPrimitive.VertexTextureCoordinates,
                        levelPrimitive.TextureName));

                    /*
                    var centroids = levelPrimitive.GetCentroids(levelPrimitive.VertexPositions);
                    var relativeNormals = new List<Vector3>();
                    for (int j = 0; j < levelPrimitive.VertexPositions.Length; j += 3)
                    {
                        relativeNormals.Add(centroids[j]);
                        relativeNormals.Add(centroids[j] + levelPrimitive.VertexSurfaceNormals![j] * 16);
                    }
                    var normalLineColours = levelPrimitive.VertexPositions
                        .Select(x => new Color(1.0F, 1.0F, 0.0F)).ToArray();

                    _lineListPrimitives.Add(new LineListPrimitive(GraphicsDevice,
                        relativeNormals.ToArray(), normalLineColours));
                    */
                }
            }
        }
        _collisionVolumeSets = [.. collisionVolumeSets];

        _defaultRenderEffect = new BasicEffect(GraphicsDevice)
        {
            //_defaultRenderEffect.Texture = myTexture;
            LightingEnabled = false,
            TextureEnabled = false,
            VertexColorEnabled = true,
            View = _defaultCamera.ViewMatrix,
            Projection = _defaultCamera.ProjectionMatrix,
            World = Matrix.Identity
        };

        GraphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            FillMode = FillMode.Solid
        };
        GraphicsDevice.SamplerStates[0] = new SamplerState
        {
            Filter = TextureFilter.Anisotropic,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MaxAnisotropy = 8,
            MaxMipLevel = 8,
            MipMapLevelOfDetailBias = 0
        };

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        if (this.IsActive)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            _defaultCamera.Update(mouseState, keyboardState, _collisionVolumeSets);

            base.Update(gameTime);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        foreach (var pass in _defaultRenderEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
        foreach (var primitive in _triangleListPrimitives)
        {
            primitive.Draw(_defaultCamera.ViewMatrix, _defaultCamera.ProjectionMatrix);
        }
        foreach (var primitive in _lineListPrimitives)
        {
            primitive.Draw(_defaultCamera.ViewMatrix, _defaultCamera.ProjectionMatrix);
        }

        base.Draw(gameTime);
    }

    private void DrawTerrain(Model model)
    {
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
                effect.World = Matrix.Identity;

                // Use the matrices provided by the game camera
                effect.View = _defaultCamera.ViewMatrix;
                effect.Projection = _defaultCamera.ProjectionMatrix;
            }
            mesh.Draw();
        }
    }
}