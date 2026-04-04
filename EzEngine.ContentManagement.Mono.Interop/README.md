# EzEngine.ContentManagement.Mono.Interop

The primary purpose of this library is converting the contents of PolyOne files to a more useful complex object structure, specifically for usage with projects using MonoGame. This is done in an "opinionated" way, i.e. expecting the resulting data to be used in specific ways.

## Features

### Geometry Conversion

Raw primitives (aka layers) have their data converted from simple type arrays to more useful MonoGame objects, for example VertsX, VertsY and Z custom vertex property arrays are converted to a single array of MonoGame Vector3. Custom properties are made accessible via dictionary for performance.

### Shared Level & Model Format (WIP)

The same complex object structure is used for both levels/maps and models, reducing the amount of learning. Levels can contain models, but models can also contain models, which can save development time and improve runtime performance depending on usage.

### Collision Detection 

A form of Bounding Volume Hierarchy (BVH) is automatically generated, at the level of both each placed level and model, and then each collision volume defined in the respective file. Collision volumes consist of vertically-oriented triangular prisms, which can be sloped at the top or bottom.

### Transformations

Each model placed in a level or other model is represented by a single triangle. The first vertex represents the uppermost left corner of the model's geometry. The model's Z rotation and X scale are set implicity by the angle and length of the triangle along its local X axis from the first to second vertex, as a multiple of the model's original width. The Y scale is similarly set by the distance from the first to third vertex in the triangle. Z scale is set manually via custom vertex property read from the first vertex. 
Unique to this system is the ability to skew models up or down on the X or Y axis based on distance from the first vertex, using a custom property on the second vertex for skewing on the X axis, and the third vertex for skewing on the Y axis.

### Vertex Lighting (WIP)

This library offers vertex lighting without the use of shaders. It allows:
- Coloured ambient lighting
- Coloured directional lighting
- Coloured point lights with configurable radius (linear falloff calculation)
- This all works as expected with existing vertex colours set on models and level geometry
There are no limits to the amount of colours that can be mixed, but as this is implemented without shaders overbrightening is not supported.

### Out-of-the-box Tools (WIP)

Also included are: 
- Primitive classes which can easily be fed data from PolyOne files processed by this library
- Maths helpers for calculating the angle between two points, and others such as vertex normal calculations and squared distance with multiple overloads
- Multiple types of ready-made cameras (WIP) including an RTS camera and a first-person noclip camera with mouselook

## Symbolic Naming (WIP)

Primitives/layers in PolyOne files can be named to trigger specific behaviour in this library.
- "Models": This library will attempt to place a model at the root or first vertex of each triangle in the primitive.
- "Volumes": Builds triangular prisms out of each triangle in the primitive, if the required custom vertex properties are assigned to the layer (see below).
- "PointLights": Places a light at the root vertex of each triangle, with its colour set to the colour of the same vertex.
- "Dummy": Used to override the location that is determined to be the uppermost left corner of the model's geometry

## Custom Properties

Custom properties allow PolyOne files to contain data specific to this library and anything relevant to any project you might want. They can be stored at the file, primitive/layer and vertex level. Some custom vertex properties are only read or needed from the first vertex of each triangle.

### Supported File-Level Custom Properties

- DirectionalLightR, DirectionalLightG, DirectionalLightB
- DirectionalLightVectorX, DirectionalLightVectorY, DirectionalLightVectorZ (WIP)
- AmbientLightR, AmbientLightG, AmbientLightB
- StartPositionX, StartPositionY, StartPositonZ

### Supported Layer-Level Custom Properties

- TriSplitPasses: How many times to split all triangles on the primitive/layer, results in better lighting at the expense of performance. Values over 4 are not recommended due to extreme cost of calculating lighting.

### Supported Vertex-Level Custom Properties

- Z: A fundamental custom property necessary since PolyOne was originally made for 2D games.
- ZTop: Only processed in primitives named "Volumes", determines the Z position of each vertex on the top triangle of each collidable vertically-oriented triangular prism.
- PointLightFalloffDistance: Determines the linear falloff distance of point lights in primitives named "PointLights", only read on the first vertex of each triangle
- ModelName: Determines which model to place for a given triangle, only read on the first vertex of each. Ignored on all primitives/layers not named "Models"

### Planned Layer-Level Custom Properties

- TextureScaleX, TextureScaleY

### Planned File-Level Custom Properties

- TextureScaleX, TextureScaleY
- TextureReplacementFrom, TextureReplacementTo: Pipe-separated lists of texture names to replace
