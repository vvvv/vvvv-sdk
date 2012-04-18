This is a fork from Microsoft.Cci.ReflectionEmitter project located at
http://ccimetadata.codeplex.com/

Changes:
- Mapper.cs:129-141
  Added a hack to workaround issues described here
  - http://connect.microsoft.com/VisualStudio/feedback/details/98403/unable-to-generate-code-creating-new-generic-object-of-instanciated-typebuilder-using-reflection
  - http://social.msdn.microsoft.com/Forums/en/netfxbcl/thread/ad002ff8-d821-4b23-b96c-b390820f9cdf
  The MethodInfo returned from TypeBuilder.GetMethod contains wrong type arguments,
  a type check will therefor fail. To work around this a skipTypeCheck parameter was 
  added to ReflectionMapper.GetMethodFrom.