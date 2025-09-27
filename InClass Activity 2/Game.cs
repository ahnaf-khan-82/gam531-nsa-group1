using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace WindowEngine
{
    public class Game : IDisposable
    {
        private GameWindow _window;
        private int _shaderProgram;
        private int _vao;
        private int _vbo;
        private int _ebo;
        private int _texture;

        private float scale = 1.0f;

        private TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        private TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear;
        private TextureMagFilter magFilter = TextureMagFilter.Linear;

        private readonly float[] vertices = new float[]
        {
    -0.5f, -0.5f, 0f,   0f, 0f,
     0.5f, -0.5f, 0f,   4f, 0f,
     0.5f,  0.5f, 0f,   4f, 4f,
    -0.5f,  0.5f, 0f,   0f, 4f
        };


        private readonly uint[] indices = new uint[]
        {
            0, 1, 2,
            2, 3, 0
        };

        private readonly string VertexShaderSource = @"
#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

uniform float uScale;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPosition.xy * uScale, aPosition.z, 1.0);
    TexCoord = aTexCoord;
}";

        private readonly string FragmentShaderSource = @"
#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, TexCoord);
}";

        public Game()
        {
            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Mipmaps - Quad Example",
                Flags = ContextFlags.ForwardCompatible
            };

            var gameSettings = new GameWindowSettings()
            {
                UpdateFrequency = 60.0
            };

            _window = new GameWindow(gameSettings, nativeSettings);
            _window.Load += OnLoad;
            _window.RenderFrame += OnRenderFrame;
            _window.UpdateFrame += OnUpdateFrame;
            _window.Resize += OnResize;
        }

        public void Run() => _window.Run();

        private void OnLoad()
        {
            GL.ClearColor(Color.Black);

            int vert = CompileShader(ShaderType.VertexShader, VertexShaderSource);
            int frag = CompileShader(ShaderType.FragmentShader, FragmentShaderSource);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vert);
            GL.AttachShader(_shaderProgram, frag);
            GL.LinkProgram(_shaderProgram);
            CheckProgram(_shaderProgram);

            GL.DeleteShader(vert);
            GL.DeleteShader(frag);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string texturePath = Path.Combine(baseDir, "Assets", "mipmaps.png");
            _texture = LoadTexture(texturePath);

            GL.UseProgram(_shaderProgram);
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "uTexture"), 0);
        }

        private void OnUpdateFrame(FrameEventArgs e)
        {
            if (_window.IsKeyDown(Keys.Escape))
                _window.Close();

            if (_window.IsKeyDown(Keys.Up)) scale += 0.01f;
            if (_window.IsKeyDown(Keys.Down)) scale -= 0.01f;
            scale = MathF.Max(0.01f, scale);

            GL.UseProgram(_shaderProgram);
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "uScale"), scale);

            if (_window.IsKeyPressed(Keys.D1)) SetWrapMode(TextureWrapMode.Repeat);
            if (_window.IsKeyPressed(Keys.D2)) SetWrapMode(TextureWrapMode.MirroredRepeat);
            if (_window.IsKeyPressed(Keys.D3)) SetWrapMode(TextureWrapMode.ClampToEdge);
            if (_window.IsKeyPressed(Keys.D4)) SetWrapMode(TextureWrapMode.ClampToBorder);

            // Minification filter shortcuts
            if (_window.IsKeyPressed(Keys.F1)) SetFilter(TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Linear);
            if (_window.IsKeyPressed(Keys.F2)) SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            if (_window.IsKeyPressed(Keys.F3)) SetFilter(TextureMinFilter.NearestMipmapLinear, TextureMagFilter.Linear);
            if (_window.IsKeyPressed(Keys.F4)) SetFilter(TextureMinFilter.LinearMipmapNearest, TextureMagFilter.Linear);

        }

        private void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            _window.SwapBuffers();
        }

        private void OnResize(ResizeEventArgs e) => GL.Viewport(0, 0, e.Width, e.Height);

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) throw new Exception(GL.GetShaderInfoLog(shader));
            return shader;
        }

        private void CheckProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0) throw new Exception(GL.GetProgramInfoLog(program));
        }

        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}");

            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            using (Bitmap bitmap = new Bitmap(path))
            {
                var data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );


                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    bitmap.Width,
                    bitmap.Height,
                    0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0
                );

                bitmap.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return textureID;
        }

        private void SetWrapMode(TextureWrapMode mode)
        {
            wrapMode = mode;
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
        }

        private void SetFilter(TextureMinFilter min, TextureMagFilter mag)
        {
            minFilter = min;
            magFilter = mag;
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteTexture(_texture);
            GL.DeleteProgram(_shaderProgram);
            _window.Dispose();
        }
    }
}
