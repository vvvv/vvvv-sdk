xof 0302txt 0032
Header {
 1;
 0;
 1;
}
template Header {
 <3D82AB43-62DA-11cf-AB39-0020AF71E433>
 WORD major;
 WORD minor;
 DWORD flags;
}

template Vector {
 <3D82AB5E-62DA-11cf-AB39-0020AF71E433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template Coords2d {
 <F6F23F44-7686-11cf-8F52-0040333594A3>
 FLOAT u;
 FLOAT v;
}

template Matrix4x4 {
 <F6F23F45-7686-11cf-8F52-0040333594A3>
 array FLOAT matrix[16];
}

template ColorRGBA {
 <35FF44E0-6C7C-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}

template ColorRGB {
 <D3E16E81-7835-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template TextureFilename {
 <A42790E1-7810-11cf-8F52-0040333594A3>
 STRING filename;
}

template Material {
 <3D82AB4D-62DA-11cf-AB39-0020AF71E433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template MeshFace {
 <3D82AB5F-62DA-11cf-AB39-0020AF71E433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template MeshTextureCoords {
 <F6F23F40-7686-11cf-8F52-0040333594A3>
 DWORD nTextureCoords;
 array Coords2d textureCoords[nTextureCoords];
}

template MeshMaterialList {
 <F6F23F42-7686-11cf-8F52-0040333594A3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material]
}

template MeshNormals {
 <F6F23F43-7686-11cf-8F52-0040333594A3>
 DWORD nNormals;
 array Vector normals[nNormals];
 DWORD nFaceNormals;
 array MeshFace faceNormals[nFaceNormals];
}

template Mesh {
 <3D82AB44-62DA-11cf-AB39-0020AF71E433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

template FrameTransformMatrix {
 <F6F23F41-7686-11cf-8F52-0040333594A3>
 Matrix4x4 frameMatrix;
}

template Frame {
 <3D82AB46-62DA-11cf-AB39-0020AF71E433>
 [...]
}
template FloatKeys {
 <10DD46A9-775B-11cf-8F52-0040333594A3>
 DWORD nValues;
 array FLOAT values[nValues];
}

template TimedFloatKeys {
 <F406B180-7B3B-11cf-8F52-0040333594A3>
 DWORD time;
 FloatKeys tfkeys;
}

template AnimationKey {
 <10DD46A8-775B-11cf-8F52-0040333594A3>
 DWORD keyType;
 DWORD nKeys;
 array TimedFloatKeys keys[nKeys];
}

template AnimationOptions {
 <E2BF56C0-840F-11cf-8F52-0040333594A3>
 DWORD openclosed;
 DWORD positionquality;
}

template Animation {
 <3D82AB4F-62DA-11cf-AB39-0020AF71E433>
 [...]
}

template AnimationSet {
 <3D82AB50-62DA-11cf-AB39-0020AF71E433>
 [Animation]
}

template XSkinMeshHeader {
 <3cf169ce-ff7c-44ab-93c0-f78f62d172e2>
 WORD nMaxSkinWeightsPerVertex;
 WORD nMaxSkinWeightsPerFace;
 WORD nBones;
}

template VertexDuplicationIndices {
 <b8d65549-d7c9-4995-89cf-53a9a8b031e3>
 DWORD nIndices;
 DWORD nOriginalVertices;
 array DWORD indices[nIndices];
}

template SkinWeights {
 <6f0d123b-bad2-4167-a0d0-80224f25fabb>
 STRING transformNodeName;
 DWORD nWeights;
 array DWORD vertexIndices[nWeights];
 array FLOAT weights[nWeights];
 Matrix4x4 matrixOffset;
}
Frame cube1 {
   FrameTransformMatrix {
1.000000,0.000000,0.000000,0.000000,
0.000000,1.000000,0.000000,0.000000,
0.000000,0.000000,1.000000,0.000000,
0.000000,0.000000,0.000000,1.000000;;
 }
Mesh cube11 {
 26;
2.000889;0.000889;-0.064000;,
2.000889;2.000889;-0.064000;,
0.125938;2.000889;-0.064000;,
0.125938;0.000889;-0.064000;,
2.000889;2.000889;0.064000;,
2.000889;0.000889;0.064000;,
0.000889;0.000889;-0.064000;,
0.000889;0.000889;0.064000;,
0.000889;2.000889;0.064000;,
0.000889;2.000889;-0.064000;,
0.125938;2.000889;-2.159787;,
0.000889;2.000889;-2.159787;,
0.000889;0.000889;-2.159787;,
0.125938;0.000889;-2.159787;,
1.123769;2.000889;-2.080142;,
1.123769;2.000889;-2.159787;,
1.123769;0.000889;-2.159787;,
1.123769;0.000889;-2.080142;,
0.125938;2.000889;-2.080142;,
0.125938;0.000889;-2.080142;,
1.039004;2.000889;-2.159787;,
1.039004;2.000889;-3.286187;,
1.039004;0.000889;-3.286187;,
1.123769;2.000889;-3.286187;,
1.123769;0.000889;-3.286187;,
1.039004;0.000889;-2.159787;;

 48;
3;2,1,0;,
3;0,3,2;,
3;5,0,4;,
3;5,7,6;,
3;8,4,1;,
3;5,4,8;,
3;7,8,9;,
3;0,1,4;,
3;5,6,3;,
3;0,5,3;,
3;2,8,1;,
3;2,9,8;,
3;8,7,5;,
3;9,6,7;,
3;12,11,10;,
3;10,13,12;,
3;16,15,14;,
3;14,17,16;,
3;9,2,18;,
3;11,9,18;,
3;10,11,18;,
3;9,11,12;,
3;12,6,9;,
3;6,12,19;,
3;3,6,19;,
3;12,13,19;,
3;2,3,19;,
3;18,2,19;,
3;18,17,14;,
3;18,19,17;,
3;14,15,20;,
3;18,14,20;,
3;10,18,20;,
3;23,22,21;,
3;23,24,22;,
3;19,13,25;,
3;17,19,25;,
3;16,17,25;,
3;13,10,20;,
3;20,25,13;,
3;20,21,22;,
3;22,25,20;,
3;16,22,24;,
3;16,25,22;,
3;15,24,23;,
3;15,16,24;,
3;21,15,23;,
3;21,20,15;;
MeshMaterialList {
 1;
 48;
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0;;
Material {
 1.000000;1.000000;1.000000;1.000000;;
2.000000;
 1.000000;1.000000;1.000000;;
 0.000000;0.000000;0.000000;;
 }
}

 MeshNormals {
 26;
0.666667;-0.333333;-0.666667;,
0.408248;0.816497;-0.408248;,
0.485071;0.727607;-0.485071;,
0.301511;-0.904534;-0.301511;,
0.816497;0.408248;0.408248;,
0.267261;-0.801784;0.534522;,
-0.447214;-0.894427;0.000000;,
-0.816497;-0.408248;0.408248;,
-0.267261;0.801784;0.534522;,
-0.800000;0.600000;0.000000;,
0.000000;0.554700;-0.832050;,
-0.408248;0.816497;-0.408248;,
-0.577350;-0.577350;-0.577350;,
0.000000;-0.554700;-0.832050;,
0.666667;0.666667;0.333333;,
0.707107;0.707107;0.000000;,
0.707107;-0.707107;0.000000;,
0.333333;-0.666667;0.666667;,
0.182574;0.912871;0.365148;,
0.365148;-0.912871;0.182574;,
-0.408248;0.816497;-0.408248;,
-0.408248;0.816497;-0.408248;,
-0.577350;-0.577350;-0.577350;,
0.408248;0.408248;-0.816497;,
0.816497;-0.408248;-0.408248;,
-0.235702;-0.942809;-0.235702;;

 48;
3;2,1,0;,
3;0,3,2;,
3;5,0,4;,
3;5,7,6;,
3;8,4,1;,
3;5,4,8;,
3;7,8,9;,
3;0,1,4;,
3;5,6,3;,
3;0,5,3;,
3;2,8,1;,
3;2,9,8;,
3;8,7,5;,
3;9,6,7;,
3;12,11,10;,
3;10,13,12;,
3;16,15,14;,
3;14,17,16;,
3;9,2,18;,
3;11,9,18;,
3;10,11,18;,
3;9,11,12;,
3;12,6,9;,
3;6,12,19;,
3;3,6,19;,
3;12,13,19;,
3;2,3,19;,
3;18,2,19;,
3;18,17,14;,
3;18,19,17;,
3;14,15,20;,
3;18,14,20;,
3;10,18,20;,
3;23,22,21;,
3;23,24,22;,
3;19,13,25;,
3;17,19,25;,
3;16,17,25;,
3;13,10,20;,
3;20,25,13;,
3;20,21,22;,
3;22,25,20;,
3;16,22,24;,
3;16,25,22;,
3;15,24,23;,
3;15,16,24;,
3;21,15,23;,
3;21,20,15;;
 }
}
 }
