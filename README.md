# EZEngine

This project was initially conceived as a rebuild of a first-person shooter I was making with [ENIGMA](https://enigma-dev.org/docs/Wiki/Main_Page). It's now transcended that purpose and aims to be a game engine which bring together years of work on [PolyOne3](https://mbillington.itch.io/polyone3-pro), the aforementioned FPS, and a more specialized 3D model and level builder tool which was also made with ENIGMA.

## Purpose

Why "EZEngine"? The engine has several goals:
1. Provide the groundwork for anyone to use PolyOne3 files, primarily in 3D games developed with MonoGame, while leaving options open for alternative uses for those willing to dive deeply into their own integrations.
2. Test the viability of the previous point.
3. Create a basic first-person experience to demonstrate the usefulness of PolyOne3 files.
4. Offer an easy-to-use engine without sacrificing too much on flexibility.
5. Provide an environment to explore levels/maps and models made in a specialized, separate editor.

## Usage

The development lifecycle of EzEngine is intended to work as follows:
1. Use either [PolyOne3 Pro](https://mbillington.itch.io/polyone3-pro) or the yet-to-be added, free PolyOne3 3D Development Kit (P13DDK) to build models and levels from scratch or out of existing models.
2. Load the relevant JSV files using EzEngine.ContentManagement
3. Use the loaded data in your own project either directly or with the provided classes in EzEngine.ContentManagement.Mono.Interop

## Q&A

> How far along in development is EZEngine?

A: This is a very early version, so I would recommend considering it for novelty use only at the moment until everything is more fleshed out. If it's plainly evident that something is missing then there's a chance there's already plans to add it in the future.


> Will there be Interop libraries for other frameworks or libraries, and base libraries for other languages?

A: There are no plans for that at this time, presently only MonoGame support is expected.


> Why does this ignore the content management pipeline built into MonoGame?

A: MonoGame was new to me when I started this project and I wanted to get it off the ground as quickly as possible. If there is a way to access PolyOne3 models and levels via MonoGame, it's likely I will set this up in the future.


> What IS currently supported?

A: Check out the readme for the ContentManagement.Mono.Interop library for a decent rundown on what's already possible and hints of what's in progress or being considered.


> Why not just use Unity/Godot/Pure MonoGame/any other engine?

A: One of the goals of EZEngine is to strike a sweet spot between of ease of use and flexibility. For beginners to programming or basic use, the ease of use is as much if not more so, planned to be achieved through PolyOne3 and P13DDK rather than primarily writing code using this library directly. Key milestones on this would be to make it possible to create a first-person walking simulator with its own identity these tools. From there branching out to other first-person games with more interaction and features, such as horror games, before finally building up to fully-fledged first-person shooters would be ideal next steps, followed finally by branching out to other genres such as real-time strategy.


> What license is EZEngine under?

A: A specific license has yet to be selected, but plans are to choose one which allows for usage in commercial products... Assuming the licenses of any dependencies don't prevent that. Consider that MonoGame is under [Microsoft Public License.](https://monogame.net/). Like the MonoGame framework it depends on, EZEngine is free. A donation link may be provided in the future, but even then you should consider donating to the MonoGame Foundation first, as EZEngine would be completely unusable in its absence.


> Can I use the included maps/models/textures in my own public projects?

A: You are strongly advised NOT to use the included textures, these were included with the free [Cube 3D FPS Engine](http://cubeengine.com/cube.php).


> What dependencies does EZEngine have?

A:
- MonoGame
- ServiceStack.Text
- .NET 8
There's probably more that I can't remember right now, will be added when recalled.


> Will PolyOne3 or P13DDK ever be open source?

A: P13DDK, possibly. I have no plans to make PolyOne3 Pro open source as I believe this undermines the versatility of the tool, the amount of time and work that has gone into it and its viability as a commercial product. There is already a free edition of PolyOne3, although this lacks the capabilities to create models and levels supported by EZEngine.


> Can I get in contact? I need to report a bug, suggest an idea or tell you about some of your code I've reviewed and which could use some improvement.

A: I'm not open to this right now, but may provide options in the future.


> Can I contribute to the development of EZEngine?

A: This is my first GitHub project or project of any kind with publicly available source code. I don't currently intend to open the project to other contributors, but this may change in the future. Likely I would open a smaller repository to outside contributors first to get a feel for what to expect.
