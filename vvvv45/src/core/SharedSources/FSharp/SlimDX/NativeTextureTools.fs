
//low level data tools for textures
module NativeTextureTools

//disable native/unsafe pointer warnings
#nowarn "9"

//use native pointers
open Microsoft.FSharp.NativeInterop
open System.Runtime.InteropServices
open SlimDX.Direct3D9

//--------------texture creation---------------------

//create texture from file path
let inline createTexFile device path =
    Texture.FromFile(device, path)

//create empty texture with given width and height
let inline createTexEmptyNoAlpha device width height =
    new Texture(device, width, height, 1, Usage.None, Format.X8R8G8B8, Pool.Managed)

//create empty texture with given width and height and alpha channel
let inline createTexEmpty device width height =
    new Texture(device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed)

//--------------low level color handling---------------------

//convert 4 bytes to uint32
let inline setARGB (a:byte) (r:byte) (g:byte) (b:byte) =
    (uint32 a <<< 24) ||| (uint32 r <<< 16) ||| (uint32 g <<< 8) ||| (uint32 b)

//get 4 bytes from an uint32
let inline getARGB (col:uint32) =
    (byte (col >>> 24), byte (col >>> 16), byte (col >>> 8), byte col)

//get channels
let inline getA (col:uint32) = byte (col >>> 24)
let inline getR (col:uint32) = byte (col >>> 16)
let inline getG (col:uint32) = byte (col >>> 8)
let inline getB (col:uint32) = byte col

//set channels
let inline setA (col:uint32) (a:byte) = (col ||| 0xFF000000u) &&& ((uint32 a) <<< 24)
let inline setR (col:uint32) (r:byte) = (col ||| 0x00FF0000u) &&& ((uint32 r) <<< 16)
let inline setG (col:uint32) (g:byte) = (col ||| 0x0000FF00u) &&& ((uint32 g) <<< 8)
let inline setB (col:uint32) (b:byte) = (col ||| 0x000000FFu) &&&  (uint32 b)

//convert native machine address to typed uint32 pointer
let inline toPointer (p:nativeint): nativeptr<uint32> = NativePtr.ofNativeInt p

//get a value at index from array as pointer
let inline getPtrVal arrayPtr i   = NativePtr.get arrayPtr i

//set a value at index in array as pointer
let inline setPtrVal arrayPtr i value = NativePtr.set arrayPtr i value

//get a value from 2D array as pointer
let inline getPtrVal2D arrayPtr (row:int) (colu:int) (width:int) = 
    NativePtr.get arrayPtr (row*width+colu)

//set a value in 2D array as pointer
let inline setPtrVal2D arrayPtr value (row:int) (colu:int) (width:int) = 
    NativePtr.set arrayPtr (row*width+colu) value

//get a value from 2D array
let inline getArrayVal2D (array:int array) (row:int) (colu:int) (width:int) = 
    uint32 array.[row*width+colu]

//set a value in 2D array
let inline setArrayVal2D (array:int array) (value:uint32) (row:int) (colu:int) (width:int) = 
    array.[row*width+colu] <- int value


//--------------generic texture fill functions---------------------


//copy texture pixels to an array
let inline copyToArray arrayPtr intSize =
    let arr = Array.zeroCreate<int> intSize
    Marshal.Copy(arrayPtr, arr, 0, intSize)
    arr


//function that fills the pixels in place with a given function
let inline fill32BitTexInPlace (tex:Texture) fillFunc =
    
    //lock the texture pixel data
    let rect = tex.LockRectangle(0, LockFlags.None)
    
    //calculate sizes
    let byteLenght = int rect.Data.Length
    let width = (rect.Pitch/4)
    let height = (byteLenght/rect.Pitch)

    //get the pointer to the data
    let data = toPointer rect.Data.DataPointer
    
    //call the given function for each pixel
    for i in 0..height-1 do
        for j in 0..width-1 do
            fillFunc data i j width height
    
    //unlock texture         
    tex.UnlockRectangle(0) |> ignore


//function that fills the pixels with a given function
let inline fill32BitTex (tex:Texture) fillFunc =
    
    //lock the texture pixel data
    let rect = tex.LockRectangle(0, LockFlags.None)  
    
    //calculate sizes
    let byteLenght = int rect.Data.Length
    let pixelCount = byteLenght/4
    let width = (rect.Pitch/4)
    let height = (byteLenght/rect.Pitch)

    //get the pointer to the data
    let data = toPointer rect.Data.DataPointer

    //copy data to array, that we can replace the data
    let oldData = copyToArray rect.Data.DataPointer pixelCount

    //call the given function for each pixel
    for i in 0..height-1 do
        for j in 0..width-1 do
            fillFunc oldData data i j width height
    
    //unlock texture      
    tex.UnlockRectangle(0) |> ignore