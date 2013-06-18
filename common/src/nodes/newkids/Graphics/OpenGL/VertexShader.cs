using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using VVVV.Core;

namespace VVVV.Nodes.Graphics.OpenGL
{
    public class VertexShader : IDisposable
    {
        private readonly string FVertexShaderSource = @"
#version 130

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;

in vec3 in_position;
in vec3 in_normal;

out vec3 normal;

void main(void)
{
  //works only for orthogonal modelview
  normal = (modelview_matrix * vec4(in_normal, 0)).xyz;
  
  gl_Position = projection_matrix * modelview_matrix * vec4(in_position, 1);
}";
        
        public VertexShader()
        {
            Handle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(Handle, FVertexShaderSource);
            GL.CompileShader(Handle);
            Debug.WriteLine(GL.GetShaderInfoLog(Handle));
        }
        
        public void Dispose()
        {
            GL.DeleteShader(Handle);
        }
        
        public int Handle 
        {
            get; private set;
        }
        
        [Node]
        public static VertexShader CreateVertexShader()
        {
            return new VertexShader();
        }
    }
}
