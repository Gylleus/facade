# Facade

## Introduction

Facade is a procedural building generator implemented for Unity in C#.
It uses _split grammar_ for generating buildings which are read from a rules file on a specific format.
This rules file can be generated from an input image of a building facade along with a layout image
that describes the different regions of the facade. When the rules are generated in this way
textures are automatically extracted and can be used for the generation process.

An example image of a facade along with a very simple layout is provided.

To read more about the concepts the program is build upon please read the
Master's Thesis this implementation was made as part of. <Link to thesis>

## How to use

The first thing that needs to be done is to get an image of a facade and create a layout image for it.
The layout image should consist of colored rectangles where each rectangle describes a certain element on
the facade such as windows, doors or simply the wall. All elements of the same type should have the same color.
It is important that the layout is well formed and of the exact same size as the original picture or else the
generation will fail.

EXAMPLE IMAGE

To generate rules for a given facade image it is easiest to simply navigate to the __InverseRuleGeneration__ scene.
Here you can create a new GameObject with an __Input Facade__ object. The Input Facade field inside the object
should be the facade wall image as a texture. The __Facade Layout Name__ should be a path to layout image relative
to the Assets folder. See the _ExampleHouse_ object in the scene for an example. The name of your input facade
GameObject will be the name used for your building.

When pressing play in the scene you will be prompted with providing the name and depth value for all different
colored regions in the layout. When this is done the generation process will begin and if all goes well the 
resulting rules will be written to _rules.txt_ in the Assets folder. 

After generating the rules you can move to the __Preview__ scene. Here you can see how the __ExampleHouse__
object is formed and create your own house (or simply rename ExampleHouse to your chosen house name). Pressing
play will start the generation process and you should see your house.

## Comments

As this project was done as a part of a master's thesis the focus has more been on the algorithms themselves and results
rather than user friendliness. This is why the user experience might seem cumbersome, unintuitive and unstable
since it was not mainly intended for other users. Hopefully I will take the time to improve this project in the future.
But in the meantime please feel free to download and experiment.
