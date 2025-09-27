# Step 6: Mipmap Exercise Answers

**1. How do mipmaps help performance?**  

Mipmaps help improve performance because they create smaller versions of a texture in advance. When a texture is drawn far away or very small on the screen, OpenGL can use one of these smaller versions instead of the full-size texture. Using smaller textures means the graphics card has to do less work, which makes rendering faster and reduces flickering or visual glitches.

**2. Compare different minification filters and describe what looks different.**  

Different minification filters affect how the texture looks when it is scaled down. The `NearestMipmapNearest` filter can look blocky or pixelated because it picks the closest mipmap level and the nearest texel. `LinearMipmapNearest` is a bit smoother, since it samples texels linearly within a single mipmap level, but it can still have noticeable jumps between levels. `NearestMipmapLinear` samples between two mipmap levels and is smoother, though slightly blurry. Finally, `LinearMipmapLinear` (trilinear filtering) blends both between texels and between mipmap levels, producing the smoothest and most natural appearance. In the program, pressing F1–F4 lets you test these filters live.

**3. What happens if you don’t call `GL.GenerateMipmap()`?**  

If `GL.GenerateMipmap()` is not called, the GPU only has the original full-size texture. When the texture is scaled down or viewed from far away, it can look blocky, flickery, or noisy because there are no smaller versions to sample from. Mipmaps prevent these problems by giving the GPU the appropriate resolution for the distance, improving both visual quality and performance.
