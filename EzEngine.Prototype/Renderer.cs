using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EzEngine.ContentManagement.Mono.Interop;
using EzEngine.ContentManagement.Mono.Interop.Models;
using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using EzEngine.Prototype.Cameras;
using EzEngine.ContentManagement.Mono.Interop.Extensions;

namespace EzEngine.Prototype;

public class Renderer : Game
{
    private GraphicsDeviceManager _graphicsDeviceManager;
    private NoclipCamera _defaultCamera;
    private List<TriangleListPrimitive> _triangleListPrimitives;
    private List<LineListPrimitive> _lineListPrimitives;
    
    private BasicEffect _defaultRenderEffect;
    private SamplerState _defaultSamplerState;
    private RasterizerState _defaultRasterizerState;

    private RenderTarget2D _sceneRenderTarget;
    private RenderTarget2D _bloomRenderTarget;
    private int _bloomRenderTargetDownscaleFactor;
    private float _bloomRenderTargetDownscaleFraction;
    private SpriteBatch _spriteBatch;

    private int _preferredBackBufferWidth;
    private int _preferredBackBufferHeight;

    private ProcessedPolyOneFileVolumeSet[] _collisionVolumeSets;

    public Renderer()
    {
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        _preferredBackBufferWidth = 1920;
        _preferredBackBufferHeight = 1080;
        Content.RootDirectory = "Content";

        _bloomRenderTargetDownscaleFactor = 8;
        _bloomRenderTargetDownscaleFraction = 1 / (float)_bloomRenderTargetDownscaleFactor;
    }

    protected override void Initialize()
    {
        _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, _preferredBackBufferWidth, _preferredBackBufferHeight,
            false, SurfaceFormat.Color, DepthFormat.Depth16);
        _bloomRenderTarget = new RenderTarget2D(GraphicsDevice, _preferredBackBufferWidth / _bloomRenderTargetDownscaleFactor, _preferredBackBufferHeight / _bloomRenderTargetDownscaleFactor);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _graphicsDeviceManager.PreferredBackBufferWidth = _preferredBackBufferWidth;
        _graphicsDeviceManager.PreferredBackBufferHeight = _preferredBackBufferHeight;
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
            //_lineListPrimitives.Add(volumeSet.GetLineListVisualization(GraphicsDevice));
            //_lineListPrimitives.Add(volumeSet.OverallAxisAlignedBoundingBox.GetLineListVisualization(GraphicsDevice));
            //_lineListPrimitives.AddRange(volumeSet.AxisAlignedBoundingBoxes.Select(x => x.GetLineListVisualization(GraphicsDevice)));
        }
        var models = new List<ProcessedPolyOneFile?>();
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
                            //.Add(volumeSet.GetLineListVisualization(GraphicsDevice));
                            //_lineListPrimitives.Add(volumeSet.OverallAxisAlignedBoundingBox.GetLineListVisualization(GraphicsDevice));
                            //_lineListPrimitives.AddRange(volumeSet.AxisAlignedBoundingBoxes.Select(x => x.GetLineListVisualization(GraphicsDevice)));
                        }
                        models.Add(model);
                    }
                }
            }
        }
        _collisionVolumeSets = [.. collisionVolumeSets];

        var lightVector = new Vector3(0.0F, 0.0F, 0.0F);

        //_lineListPrimitives.Add(lightVector.GetLineListVisualization(GraphicsDevice, level.DirectionalLightVector));

        /*
        _lineListPrimitives.AddRange(level.PrimitiveGroups
            .SelectMany(x => x.Primitives)
            .Where(x => x.Name != "PointLights" && x.Name != "Volumes" && x.Name != "Models")
            .Select(x => x.VertexPositions.GetLineListVisualization(GraphicsDevice, level.DirectionalLightVector * 4.0F)));
        */

        _lineListPrimitives.AddRange(level.PointLights.Select(x => x.GetLineListVisualization(GraphicsDevice)));

        level.CalculateLighting();
        _triangleListPrimitives.AddRange(level.PrimitiveGroups
            .SelectMany(x => x.Primitives)
            .Where(x => x.Name != "PointLights" && x.Name != "Volumes" && x.Name != "Models")
            .Select(x => new TriangleListPrimitive(GraphicsDevice, x.VertexPositions, x.LitVertexColours, x.VertexTextureCoordinates, x.TextureName)));

        var allVolumes = level.Volumes.ToList();
        allVolumes.AddRange(models.SelectMany(x => x.Volumes));
        var allVolumesArray = allVolumes.ToArray();

        foreach (var model in models)
        {
            model.CalculateLighting(allVolumesArray, level.PointLights);

            _triangleListPrimitives.AddRange(model.PrimitiveGroups
                .SelectMany(x => x.Primitives)
                .Select(x => new TriangleListPrimitive(GraphicsDevice, x.VertexPositions, x.LitVertexColours, x.VertexTextureCoordinates, x.TextureName)));

            /*
            _lineListPrimitives.AddRange(model.PrimitiveGroups
                .SelectMany(x => x.Primitives)
                .Where(x => x.Name != "PointLights" && x.Name != "Volumes" && x.Name != "Models")
                .Select(x => x.VertexPositions.GetLineListVisualization(GraphicsDevice, level.DirectionalLightVector * 4.0F)));
            */
        }
        

        _defaultRenderEffect = new BasicEffect(GraphicsDevice)
        {
            LightingEnabled = false,
            TextureEnabled = false,
            VertexColorEnabled = true,
            View = _defaultCamera.ViewMatrix,
            Projection = _defaultCamera.ProjectionMatrix,
            World = Matrix.Identity
        };
        _defaultRasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            FillMode = FillMode.Solid,
            DepthBias = 0f,
            MultiSampleAntiAlias = true,
            ScissorTestEnable = false,
            SlopeScaleDepthBias = 0f,
            DepthClipEnable = true
        };
        GraphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            FillMode = FillMode.Solid
        };
        _defaultSamplerState = new SamplerState
        {
            Filter = TextureFilter.Anisotropic,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MaxAnisotropy = 8,
            MaxMipLevel = 8,
            MipMapLevelOfDetailBias = 0
        };
        GraphicsDevice.SamplerStates[0] = _defaultSamplerState;

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsActive)
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
        GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
        GraphicsDevice.Clear(Color.Black);

        GraphicsDevice.RasterizerState = _defaultRasterizerState;
        GraphicsDevice.SamplerStates[0] = _defaultSamplerState;

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        GraphicsDevice.BlendState = BlendState.Opaque;

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

        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, GraphicsDevice.SamplerStates[0]);
        _spriteBatch.Draw(_sceneRenderTarget, 
            Vector2.Zero, 
            new Rectangle(0, 0, _preferredBackBufferWidth, _preferredBackBufferHeight), 
            Color.White);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(_bloomRenderTarget);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, GraphicsDevice.SamplerStates[0]);
        _spriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, new Rectangle(0, 0, _preferredBackBufferWidth, _preferredBackBufferHeight), Color.White,
            0F, Vector2.Zero, new Vector2(_bloomRenderTargetDownscaleFraction, _bloomRenderTargetDownscaleFraction), SpriteEffects.None, 0F);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);

        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, GraphicsDevice.SamplerStates[0]);
        _spriteBatch.Draw(_sceneRenderTarget, 
            Vector2.Zero, 
            new Rectangle(0, 0, _preferredBackBufferWidth, _preferredBackBufferHeight), 
            Color.White);
        _spriteBatch.End();

        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
        _spriteBatch.Draw(_bloomRenderTarget, Vector2.Zero, new Rectangle(0, 0, _preferredBackBufferWidth, _preferredBackBufferHeight), Color.Gray,
            0F, Vector2.Zero, new Vector2(_bloomRenderTargetDownscaleFactor, _bloomRenderTargetDownscaleFactor), SpriteEffects.None, 0F);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}