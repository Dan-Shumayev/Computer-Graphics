Why do separate vertices for each face cause flat shading?

We have seen in class that the lighting for a single vertex depends on the normal
to said vertex. This normal is the normalized average direction of all the surface
normals of the surfaces this vertex is a part of. Therefore, if each face has unique
vertices, then the normal to each vertex will be *exactly* the surface normal
of the unique surface it belongs to. Then, the colors of all vertices of a face
will be identical, and equal to the color of the face in the flat-shading model.

As we have seen in class, the lighting *for a surface* is calculated by interpolation
from the vertices comprising it. For instance, we can interpolate the colors of
the vertices (Gouraud), or the normals (Phong). However, since in our case
the normals and colors of all vertices of a face are equal, the interpolation is
a no-op, and we get the same result as if we have used flat-shading.
