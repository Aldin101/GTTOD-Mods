using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace WaterFixPreloader;
public class WaterFixPreloader
{
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };
    public static void Patch(AssemblyDefinition assembly)
    {
        var waterVolumeType = assembly.MainModule.Types.First(t => t.Name == "WaterVolume");

        var onDestroyMethod = new MethodDefinition("OnDestroy", MethodAttributes.Public | MethodAttributes.HideBySig, assembly.MainModule.TypeSystem.Void);

        var il = onDestroyMethod.Body.GetILProcessor();

        il.Emit(OpCodes.Ret);

        waterVolumeType.Methods.Add(onDestroyMethod);

        assembly.Write("Assembly-CSharp.dll");
    }
}
