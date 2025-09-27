# Step 6: Mipmap Exercise Answers

**1. How do mipmaps help performance?**  

Mipmaps improve performance by creating smaller versions of a texture. When the texture appears far away or small on the screen, OpenGL can use a smaller version instead of the full-size one. This makes rendering faster and reduces flickering or visual glitches.

**2. Compare different minification filters and describe what looks different.**  

Minification filters change how a texture looks when it is made smaller. `NearestMipmapNearest` looks blocky or pixelated. `LinearMipmapNearest` is smoother but can still jump between levels. `NearestMipmapLinear` is a bit blurry but more even. `LinearMipmapLinear` is the smoothest and looks natural. Pressing F1–F4 in the program lets you see these differences.

**3. What happens if you don’t call `GL.GenerateMipmap()`?**  

If `GL.GenerateMipmap()` is not called, the GPU only has the original full-size texture. When the texture is scaled down or viewed from far away, it can look blocky, flickery, or noisy because there are no smaller versions to sample from. Mipmaps prevent these problems by giving the GPU the appropriate resolution for the distance, improving both visual quality and performance.
