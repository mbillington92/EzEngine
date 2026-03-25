using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EzEngine.ContentManagement.Mono.Interop.Models.Renderables;

public class TriangleListPrimitive
{
    private readonly VertexBuffer _vertexBuffer;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly VertexPositionColorTexture[] _vertexData;
    private readonly BasicEffect _renderEffect;

    public TriangleListPrimitive(GraphicsDevice graphicsDevice, Vector3[] vertexPositions, Color[] vertexColours, Vector2[] textureCoordinates, string textureName)
    {
        _graphicsDevice = graphicsDevice;

        //_vertexPositionColours = new VertexPositionColor[vertexPositions.Length];
        _vertexData = new VertexPositionColorTexture[vertexPositions.Length];
        for (var i = 0; i < vertexPositions.Length; i++)
        {
            _vertexData[i].Position = vertexPositions[i];
            _vertexData[i].Color = vertexColours[i];
            _vertexData[i].TextureCoordinate = textureCoordinates[i];
        }

        _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), vertexPositions.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData<VertexPositionColorTexture>(_vertexData);

        _renderEffect = new BasicEffect(_graphicsDevice)
        {
            LightingEnabled = false,
            //AmbientLightColor = new Vector3(1.0F, 0.0F, 0.0F);
            VertexColorEnabled = true,
            World = Matrix.Identity
        };

        var texturePath = $"{Directory.GetCurrentDirectory()}\\Textures\\{textureName}.png";
        //var imageData = Image.FromStream(File.OpenRead(texturePath), false, false);
        //var imageData = Image.FromFile(texturePath);
        //var texture = new Texture2D(_graphicsDevice, imageData.Width, imageData.Height, true, SurfaceFormat.Color);
        //texture.SetData()

        var texture = Texture2D.FromFile(_graphicsDevice, texturePath);

        _renderEffect.Texture = texture;
        _renderEffect.TextureEnabled = true;
    }

    public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
    {
        foreach (var pass in _renderEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }

        _renderEffect.View = viewMatrix;
        _renderEffect.Projection = projectionMatrix;

        _graphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(
            PrimitiveType.TriangleList, _vertexData, 0, _vertexData.Length / 3);
    }
}