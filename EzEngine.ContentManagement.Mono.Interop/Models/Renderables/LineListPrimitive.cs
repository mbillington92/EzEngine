using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EzEngine.ContentManagement.Mono.Interop.Models.Renderables;

public class LineListPrimitive
{
    private readonly VertexBuffer _vertexBuffer;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly VertexPositionColor[] _vertexData;
    private readonly BasicEffect _renderEffect;

    public LineListPrimitive(GraphicsDevice graphicsDevice, Vector3[] vertexPositions, Color[] vertexColours)
    {
        _graphicsDevice = graphicsDevice;

        _vertexData = new VertexPositionColor[vertexPositions.Length];
        for (var i = 0; i < vertexPositions.Length; i++)
        {
            _vertexData[i].Position = vertexPositions[i];
            _vertexData[i].Color = vertexColours[i];
        }
        _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), vertexPositions.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData<VertexPositionColor>(_vertexData);

        _renderEffect = new BasicEffect(_graphicsDevice)
        {
            LightingEnabled = false,
            VertexColorEnabled = true,
            TextureEnabled = false,
            World = Matrix.Identity
        };
    }

    public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
    {
        foreach (var pass in _renderEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }

        _renderEffect.View = viewMatrix;
        _renderEffect.Projection = projectionMatrix;

        _graphicsDevice.DrawUserPrimitives<VertexPositionColor>(
            PrimitiveType.LineList, _vertexData, 0, _vertexData.Length / 2);
    }
}