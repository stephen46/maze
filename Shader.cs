using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace _2026_Amazing
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; }

        public Shader(string vertexPath, string fragmentPath)
        {
            // Load shader source from files
            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);

            // Create shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, vertexPath);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader, fragmentPath);

            // Create program
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);
            CheckProgramLink(Handle);

            // Cleanup
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private void CheckShaderCompile(int shader, string file)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);

            if (status == (int)All.False)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception(
                    $"Shader compile error in '{file}':\n{log}"
                );
            }
        }

        private void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);

            if (status == (int)All.False)
            {
                string log = GL.GetProgramInfoLog(program);
                throw new Exception(
                    $"Shader program link error:\n{log}"
                );
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
