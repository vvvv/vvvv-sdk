using System;
using OpenTK.Graphics.OpenGL;
using VVVV.Core;

namespace VVVV.Nodes.Graphics.OpenGL
{
    public class FragmentShader : IDisposable
    {
        private readonly string FFragmentShaderSource = @"
#version 130

precision highp float;

const vec3 ambient = vec3(0.1, 0.1, 0.1);
const vec3 lightVecNormalized = normalize(vec3(0.5, 0.5, 2.0));
const vec3 lightColor = vec3(0.9, 0.9, 0.7);

in vec3 normal;

out vec4 out_frag_color;

void main(void)
{
  float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);
  out_frag_color = vec4(ambient + diffuse * lightColor, 1.0);
}";
        
        public FragmentShader()
        {
            Handle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(Handle, FFragmentShaderSource);
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
        public static FragmentShader CreateFragmentShader()
        {
            return new FragmentShader();
        }
    }
}
