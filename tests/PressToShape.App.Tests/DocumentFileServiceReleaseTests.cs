using System.Threading.Tasks;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using System.IO;
using Xunit;

namespace PressToShape.App.Tests;

// The file service tells the store which picked files it will not use again (the registry of the Android picker frees their handles).
public sealed class DocumentFileServiceReleaseTests
{
    private const string A = "storage:a/a.p2s";
    private const string B = "storage:b/b.p2s";
    private const string C = "storage:c/c.p2s";

    private sealed class Setup
    {
        public Setup()
        {
            State = new EditorState(Document.Create("Layer 1"));
            Session = new DocumentSession(State);
            Service = new DocumentFileService(State, Session, new RecentFiles(new MemorySettingsStore()), Store, Dialogs, Messages, FakeLocalizer.Create(), new FakeTextMeasurer(), () => { });
        }

        public EditorState State { get; }

        public DocumentSession Session { get; }

        public FakeFileStore Store { get; } = new();

        public FakeFileDialogs Dialogs { get; } = new();

        public FakeMessageDialogs Messages { get; } = new();

        public DocumentFileService Service { get; }

        public void AddProject(string path) => Store.Files[path] = ProjectFile.Write(Document.Create("Other"));

        public async Task OpenAsync(string path)
        {
            AddProject(path);
            Dialogs.OpenAnswers.Enqueue(path);
            await Service.OpenAsync();
        }
    }

    [Fact]
    public async Task Open_FirstFile_ReleasesNothing()
    {
        var s = new Setup();

        await s.OpenAsync(A);

        Assert.Equal(A, s.Session.Path);
        Assert.Empty(s.Store.Released);
    }

    [Fact]
    public async Task Open_AnotherFile_ReleasesThePreviousOne()
    {
        var s = new Setup();
        await s.OpenAsync(A);

        await s.OpenAsync(B);

        Assert.Equal(B, s.Session.Path);
        Assert.Equal([A], s.Store.Released);
    }

    [Fact]
    public async Task Open_TheCurrentFileAgain_ReleasesNothing()
    {
        var s = new Setup();
        await s.OpenAsync(A);

        await s.OpenAsync(A);

        Assert.Empty(s.Store.Released);
    }

    [Fact]
    public async Task Open_FileThatCannotBeRead_ReleasesThePickedOneAndKeepsTheCurrent()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Dialogs.OpenAnswers.Enqueue(B);
        s.Store.ReadFailure = new IOException("broken");

        await s.Service.OpenAsync();

        Assert.Equal(A, s.Session.Path);
        Assert.Equal([B], s.Store.Released);
    }

    [Fact]
    public async Task Open_NotAProjectFile_ReleasesThePickedOne()
    {
        var s = new Setup();
        s.Store.Files[B] = [1, 2, 3];
        s.Dialogs.OpenAnswers.Enqueue(B);

        await s.Service.OpenAsync();

        Assert.Null(s.Session.Path);
        Assert.Equal([B], s.Store.Released);
    }

    [Fact]
    public async Task Open_TheCurrentFileThatCannotBeRead_DoesNotReleaseIt()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Store.ReadFailure = new IOException("broken");

        await s.Service.OpenRecentAsync(A);

        Assert.Empty(s.Store.Released);
    }

    [Fact]
    public async Task SaveAs_AnotherFile_ReleasesThePreviousOne()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Dialogs.SaveAnswers.Enqueue(B);

        var saved = await s.Service.SaveAsAsync();

        Assert.True(saved);
        Assert.Equal(B, s.Session.Path);
        Assert.Equal([A], s.Store.Released);
    }

    [Fact]
    public async Task SaveAs_TheCurrentFile_ReleasesNothingAndSaveStillWorks()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Dialogs.SaveAnswers.Enqueue(A);

        await s.Service.SaveAsAsync();
        var saved = await s.Service.SaveAsync();

        Assert.True(saved);
        Assert.Empty(s.Store.Released);
    }

    [Fact]
    public async Task SaveAs_WriteFails_ReleasesThePickedFileAndKeepsTheCurrent()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Dialogs.SaveAnswers.Enqueue(C);
        s.Store.WriteFailure = new IOException("full");

        var saved = await s.Service.SaveAsAsync();

        Assert.False(saved);
        Assert.Equal(A, s.Session.Path);
        Assert.Equal([C], s.Store.Released);
    }

    [Fact]
    public async Task Save_CurrentFileWriteFails_DoesNotReleaseIt()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Store.WriteFailure = new IOException("full");

        await s.Service.SaveAsync();

        Assert.Empty(s.Store.Released);
    }

    [Fact]
    public async Task New_AfterOpeningAFile_ReleasesIt()
    {
        var s = new Setup();
        await s.OpenAsync(A);

        await s.Service.NewAsync();

        Assert.Null(s.Session.Path);
        Assert.Equal([A], s.Store.Released);
    }

    [Fact]
    public async Task New_WithoutAFile_ReleasesNothing()
    {
        var s = new Setup();

        await s.Service.NewAsync();

        Assert.Empty(s.Store.Released);
    }

    [Fact]
    public async Task ExportSvg_ReleasesTheChosenFileAndKeepsTheCurrent()
    {
        var s = new Setup();
        await s.OpenAsync(A);
        s.Dialogs.ExportAnswers.Enqueue("storage:x/x.svg");

        await s.Service.ExportSvgAsync();

        Assert.Equal(A, s.Session.Path);
        Assert.Equal(["storage:x/x.svg"], s.Store.Released);
    }

    [Fact]
    public async Task ExportSvg_WriteFails_StillReleasesTheChosenFile()
    {
        var s = new Setup();
        s.Dialogs.ExportAnswers.Enqueue("storage:x/x.svg");
        s.Store.WriteFailure = new IOException("full");

        await s.Service.ExportSvgAsync();

        Assert.Equal(["storage:x/x.svg"], s.Store.Released);
    }

    [Fact]
    public async Task ExportSvg_Cancelled_ReleasesNothing()
    {
        var s = new Setup();

        await s.Service.ExportSvgAsync();

        Assert.Empty(s.Store.Released);
    }
}
