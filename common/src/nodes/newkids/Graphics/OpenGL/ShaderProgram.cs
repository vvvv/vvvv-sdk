using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using VVVV.Core;

namespace VVVV.Nodes.Graphics.OpenGL
{
    public class ShaderProgram
    {
        public ShaderProgram(VertexShader vertexShader, FragmentShader fragmentShader)
        {
            Handle = GL.CreateProgram();
            if (vertexShader != null) GL.AttachShader(Handle, vertexShader.Handle);
            if (fragmentShader != null) GL.AttachShader(Handle, fragmentShader.Handle);
            GL.LinkProgram(Handle);
            Debug.WriteLine(GL.GetProgramInfoLog(Handle));
            
            ProjectionMatrixLocation = GL.GetUniformLocation(Handle, "projection_matrix");
            ModelviewMatrixLocation = GL.GetUniformLocation(Handle, "modelview_matrix");
        }
        
        public int Handle
        {
            get; private set;
        }
        
        public int ProjectionMatrixLocation
        {
            get; private set;
        }
        
        public int ModelviewMatrixLocation
        {
            get; private set;
        }
        
        [Node]
        public static ShaderProgram CreateShaderProgram(VertexShader vertexShader, FragmentShader fragmentShader)
        {
            return new ShaderProgram(vertexShader, fragmentShader);
        }
    }
}
