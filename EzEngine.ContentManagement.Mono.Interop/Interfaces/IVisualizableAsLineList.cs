using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EzEngine.ContentManagement.Mono.Interop.Interfaces;

public interface IVisualizableAsLineList
{
    LineListPrimitive GetLineListVisualization(GraphicsDevice graphicsDevice, Color? colourOverride = null);
}