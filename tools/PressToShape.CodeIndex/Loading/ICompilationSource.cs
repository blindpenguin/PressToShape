using System.Threading;
using System.Threading.Tasks;

namespace PressToShape.CodeIndex.Loading;

/// <summary>Supplies the compilations to analyse. The real source opens the solution with MSBuild. Tests supply in-memory compilations.</summary>
internal interface ICompilationSource
{
    Task<LoadResult> LoadAsync(CancellationToken cancellationToken);
}
