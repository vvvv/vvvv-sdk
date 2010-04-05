module VVVV.SlimDXUtils

open SlimDX
open VVVV.Utils.VMath

let inline Mat2SDXMat (m:Matrix4x4) = new Matrix(M11 = float32 m.m11, M12 = float32 m.m12, M13 = float32 m.m13, M14 = float32 m.m14,
                                                 M21 = float32 m.m21, M22 = float32 m.m22, M23 = float32 m.m23, M24 = float32 m.m24,
                                                 M31 = float32 m.m31, M32 = float32 m.m32, M33 = float32 m.m33, M34 = float32 m.m34,
                                                 M41 = float32 m.m41, M42 = float32 m.m42, M43 = float32 m.m43, M44 = float32 m.m44)

