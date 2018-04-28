# RULES FOR GRAMMAR

'//' Indicates a comment

 Axes are local to shapes, meaning Z will always mean depth, x width and y height of shape regardless of rotation.
 (X,Y,Z) Indicates a rule that works on one dimensions at a time. (XYZ) indicates a rule that can work in multiple dimensions simultaneously.
 Rule types:

 split(X,Y,Z) splits the left shapes into the given shapes on the right
 repeat(X,Y,Z) works as the split rule but repeats a pattern as many types as there are space
 decompose(XYZ) hollows out a cube and leaves the surfaces as thin cubes representing the facades. Simulates going down in dimensionality.
 protrude(XYZ) rescales a shape and moves it accordingly to the new size. Used for example to protrude a pillar/balcony from a facade wall
 replace(cube,cylinder,hexagon) changes the base-shape of a given shape if cubes are not desired as the final result.

 The second parameter of a rule '{  }' specifies the result and size of resulting shapes. 
 If the size ends in 'N' it is a relative size that will be scaled to fit the parent shapes size.
 Inside '[ ]' constraint are declared that are used to telling when a rule can and cannot be invoked.

