float3 attractor3d (
  float3 v, // vertex
  float3 c, // center
  float  p, // power
  float  s, // strenght
  float  r  // radius
) 
{
  if(length(v-c)<r)
  {
    v += (v-c)*s*(pow((length(v-c)/r),p)*sign(length(v-c)/r)/(length(v-c)/r)-1);
  }
  return v;
}
