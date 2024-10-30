using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Mono.CSharp;

namespace WorkshopScripts
{
    public static class MonoCompiler
    {
        private static readonly HashSet<string> StdLib =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                {"mscorlib", "System.Core", "System", "System.Xml"};

        private static readonly HashSet<string> compiledAssemblies = new HashSet<string>();

        // Mimicked from https://github.com/kkdevs/Patchwork/blob/master/Patchwork/MonoScript.cs#L124
        public static Assembly Compile(Dictionary<string, byte[]> sources, TextWriter logger = null, string dllName = "CompiledScripts")
        {
            ReportPrinter reporter = logger == null ? new ConsoleReportPrinter() : new StreamReportPrinter(logger);
            Location.Reset();

            compiledAssemblies.Add(dllName);

            var ctx = CreateContext(reporter);
            ctx.Settings.SourceFiles.Clear();

            foreach (var source in sources)
            {
                var sourceFile = new SourceFile(source.Key, source.Key, ctx.Settings.SourceFiles.Count, file =>
                    new SeekableStreamReader(new MemoryStream(source.Value), Encoding.UTF8));
                ctx.Settings.SourceFiles.Add(sourceFile);
            }

            var container = new ModuleContainer(ctx);

            RootContext.ToplevelTypes = container;
            Location.Initialize(ctx.Settings.SourceFiles);

            var session = new ParserSession { UseJayGlobalArrays = true, LocatedTokens = new LocatedToken[15000] };
            container.EnableRedefinition();

            foreach (var sourceFile in ctx.Settings.SourceFiles)
            {
                var stream = sourceFile.GetInputStream(sourceFile);
                var source = new CompilationSourceFile(container, sourceFile);
                source.EnableRedefinition();
                container.AddTypeContainer(source);
                var parser = new CSharpParser(stream, source, session);
                parser.parse();
            }

            var ass = new AssemblyDefinitionDynamic(container, $"{dllName}.dll", $"{dllName}.dll");
            container.SetDeclaringAssembly(ass);

            var importer = new ReflectionImporter(container, ctx.BuiltinTypes)
            {
                IgnoreCompilerGeneratedField = true,
                IgnorePrivateMembers = false
            };
            ass.Importer = importer;

            var loader = new DynamicLoader(importer, ctx);
            ImportAppdomainAssemblies(a => importer.ImportAssembly(a, container.GlobalRootNamespace));

            loader.LoadReferences(container);
            ass.Create(AppDomain.CurrentDomain, AssemblyBuilderAccess.RunAndSave);
            container.CreateContainer();
            loader.LoadModules(ass, container.GlobalRootNamespace);
            container.InitializePredefinedTypes();
            container.Define();

            if (ctx.Report.Errors > 0)
            {
                UnityEngine.Debug.LogError("Found errors! Aborting compilation...");
                return null;
            }

            try
            {
                ass.Resolve();
                ass.Emit();
                container.CloseContainer();
                ass.EmbedResources();

                string outputPath = Path.Combine("WorkshopModDllCache", $"{dllName}.dll");
                ass.Builder.Save($"{dllName}.dll");

                if (File.Exists(outputPath))
                {
                    File.Move(outputPath, outputPath + UnityEngine.Random.Range(1, 10000000) + ".fordeletion");
                }

                File.Move($"{dllName}.dll", outputPath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to compile because {e}");
                return null;
            }

            return ass.Builder;
        }

        private static AssemblyName ParseName(string fullName)
        {
            try
            {
                return new AssemblyName(fullName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void ImportAppdomainAssemblies(Action<Assembly> import)
        {
            // In some cases there could be multiple versions of the same assembly loaded
            // In that case we decide to simply load only the latest one as it's easiest to handle
            var dedupedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => new { ass = a, name = ParseName(a.FullName) })
                .Where(a => a.name != null)
                .GroupBy(a => a.name.Name)
                .Select(g => g.OrderByDescending(a => a.name.Version).First());
            foreach (var ass in dedupedAssemblies)
            {
                if (StdLib.Contains(ass.name.Name) || compiledAssemblies.Contains(ass.name.Name))
                    continue;
                import(ass.ass);
            }
        }

        private static CompilerContext CreateContext(ReportPrinter reportPrinter)
        {
            var settings = new CompilerSettings
            {
                Version = LanguageVersion.Experimental,
                GenerateDebugInfo = false,
                StdLib = true,
                Target = Mono.CSharp.Target.Library
            };

            return new CompilerContext(settings, reportPrinter);
        }
    }
}
