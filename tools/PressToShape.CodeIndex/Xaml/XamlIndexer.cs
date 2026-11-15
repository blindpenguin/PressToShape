using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Reads the XAML files of all projects, builds the solution-wide <see cref="XamlWorld"/>, finds the view locator convention of each application,
/// and resolves every file against the compilation of its project. A file that is not well-formed becomes a project error. The other files are still processed.
/// </summary>
internal sealed class XamlIndexer
{
    private const string NoRegistrationReason = "no data template is registered";

    private readonly IXamlFileSource _source;

    public XamlIndexer(IXamlFileSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    public XamlIndexResult Index(IReadOnlyList<ProjectCompilation> projects, IReadOnlySet<string> knownIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projects);
        ArgumentNullException.ThrowIfNull(knownIds);

        var problems = new List<LoadProblem>();
        var documents = new List<XamlDocument>();
        foreach (var project in projects.OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            foreach (var path in _source.List(DirectoryOf(project.ProjectPath)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = XamlReader.Read(path, project.Name, _source.Read(path));
                if (read.Document is not null)
                {
                    documents.Add(read.Document);
                }
                else if (read.Problem is not null)
                {
                    problems.Add(read.Problem);
                }
            }
        }

        if (documents.Count == 0)
        {
            return new XamlIndexResult(new Dictionary<string, XamlProjectData>(), problems);
        }

        var world = new XamlWorld(documents, AssemblyDirectories(projects));
        var conventions = DetectConventions(world, projects, knownIds);
        var result = new Dictionary<string, XamlProjectData>(StringComparer.Ordinal);
        foreach (var project in projects.OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            var own = world.Documents.Where(d => d.Project == project.Name).ToList();
            if (own.Count > 0)
            {
                result[project.Name] = ResolveProject(world, project, own, conventions, knownIds, cancellationToken);
            }
        }

        return new XamlIndexResult(result, problems);
    }

    private static XamlProjectData ResolveProject(
        XamlWorld world,
        ProjectCompilation project,
        List<XamlDocument> documents,
        Dictionary<string, ConventionDetection> conventions,
        IReadOnlySet<string> knownIds,
        CancellationToken cancellationToken)
    {
        var application = world.ApplicationFor(documents[0]);
        var resolver = new XamlResolver(world, project.Compilation, knownIds, application is null ? null : conventions[application.Path].Convention);

        var files = new List<XamlFileRecord>();
        var edges = new List<RelationshipRecord>();
        foreach (var document in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var resolution = resolver.Resolve(document);
            files.Add(AddConventionFinding(resolution.File, document, conventions));
            edges.AddRange(resolution.Relationships);
        }

        return new XamlProjectData(files.OrderBy(f => f.Id, StringComparer.Ordinal).ToList(), edges);
    }

    /// <summary>The application file gets one finding when its view locator was registered but its convention was not recognized.</summary>
    private static XamlFileRecord AddConventionFinding(XamlFileRecord file, XamlDocument document, Dictionary<string, ConventionDetection> conventions)
    {
        if (!conventions.TryGetValue(document.Path, out var detection) || detection.Convention is not null
            || detection.Reason is null || detection.Reason.StartsWith(NoRegistrationReason, StringComparison.Ordinal))
        {
            return file;
        }

        return file with { Findings = [.. file.Findings, $"The ViewLocator convention was not recognized, so no inferred View to ViewModel links were made: {detection.Reason}."] };
    }

    private static Dictionary<string, ConventionDetection> DetectConventions(XamlWorld world, IReadOnlyList<ProjectCompilation> projects, IReadOnlySet<string> knownIds)
    {
        var conventions = new Dictionary<string, ConventionDetection>(StringComparer.Ordinal);
        foreach (var application in world.Documents.Where(d => XamlNames.IsType(d.Root, "Application")))
        {
            var project = projects.First(p => p.Name == application.Project);
            var types = new XamlTypeResolver(project.Compilation, knownIds);
            conventions[application.Path] = ViewLocatorConvention.Detect(application, types, project.Compilation, knownIds);
        }

        return conventions;
    }

    private static Dictionary<string, string> AssemblyDirectories(IReadOnlyList<ProjectCompilation> projects)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var project in projects.OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            map.TryAdd(project.Compilation.AssemblyName ?? project.Name, DirectoryOf(project.ProjectPath));
        }

        return map;
    }

    private static string DirectoryOf(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash < 0 ? string.Empty : path[..slash];
    }
}
