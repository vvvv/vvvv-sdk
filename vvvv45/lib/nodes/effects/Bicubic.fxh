// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------

//texture size XY
float2 size_source;

//filter texture
texture TexFilter <string uiname="Filter Texture";>;

//filter sampler
sampler1D SampFilter = sampler_state
{
    Texture   = (TexFilter);     //apply a texture to the sampler
    MipFilter = linear;         //sampler states
    MinFilter = linear;
    MagFilter = linear;
    addressU = wrap;
    addressV = wrap;
};

//texture lookup
float4 tex2Dbicubic(sampler s, float2 tex)
{

//pixel size XY
  float2 pix = 1.0/size_source;

  //calc filter texture coordinates
  float2 w = tex*size_source-float2(0.5, 0.5);

  // fetch offsets and weights from filter function
  float3 hg_x = tex1D(SampFilter, w.x ).xyz;
  float3 hg_y = tex1D(SampFilter, w.y ).xyz;

  float2 e_x = {pix.x, 0};
  float2 e_y = {0, pix.y};

  // determine linear sampling coordinates
  float2 coord_source10 = tex + hg_x.x * e_x;
  float2 coord_source00 = tex - hg_x.y * e_x;
  float2 coord_source11 = coord_source10 + hg_y.x * e_y;
  float2 coord_source01 = coord_source00 + hg_y.x * e_y;
  coord_source10 = coord_source10 - hg_y.y * e_y;
  coord_source00 = coord_source00 - hg_y.y * e_y;

  // fetch four linearly interpolated inputs
  float4 tex_source00 = tex2D( s, coord_source00 );
  float4 tex_source10 = tex2D( s, coord_source10 );
  float4 tex_source01 = tex2D( s, coord_source01 );
  float4 tex_source11 = tex2D( s, coord_source11 );

  // weight along y direction
  tex_source00 = lerp( tex_source00, tex_source01, hg_y.z );
  tex_source10 = lerp( tex_source10, tex_source11, hg_y.z );

  // weight along x direction
  tex_source00 = lerp( tex_source00, tex_source10, hg_x.z );

  return tex_source00;
}

// -------------------------------------------------------------------------

