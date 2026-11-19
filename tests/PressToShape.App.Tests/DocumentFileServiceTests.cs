using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class DocumentFileServiceTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.Tests." + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best effort.
        }
    }

    private sealed class Setup
    {
        public Setup(Localizer? localizer = null, IFileStore? files = null)
        {
            Files = files ?? new FakeFileStore();
            Localizer = localizer ?? FakeLocalizer.Create();
            State = new EditorState(Document.Create("Layer 1"));
            Session = new DocumentSession(State);
            Recent = new RecentFiles(new MemorySettingsStore());
            Service = new DocumentFileService(State, Session, Recent, Files, Dialogs, Messages, Localizer, new FakeTextMeasurer(), () => FitCount++);
        }

        public Localizer Localizer { get; }

        public EditorState State { get; }

        public DocumentSession Session { get; }

        public RecentFiles Recent { get; }

        public IFileStore Files { get; }

        public FakeFileStore Fake => (FakeFileStore)Files;

        public FakeFileDialogs Dialogs { get; } = new();

        public FakeMessageDialogs Messages { get; } = new();

        public DocumentFileService Service { get; }

        public int FitCount { get; private set; }

        public void Edit() =>
            State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(State.ActiveLayerId, new RectangleShape { Width = 10, Height = 10 }));
    }

    [Fact]
    public async Task Save_WithoutAFile_AsksForOneAndWritesTheProject()
    {
        var s = new Setup();
        s.Edit();
        s.Dialogs.SaveAnswers.Enqueue("/work/drawing.p2s");

        var saved = await s.Service.SaveAsync();

        Assert.True(saved);
        Assert.Equal("Untitled.p2s", Assert.Single(s.Dialogs.SuggestedSaveNames));
        Assert.True(ProjectFile.Read(s.Fake.Files["/work/drawing.p2s"]).Succeeded);
        Assert.Equal("/work/drawing.p2s", s.Session.Path);
        Assert.False(s.Session.IsDirty);
        Assert.Equal(new[] { "/work/drawing.p2s" }, s.Recent.Paths);
    }

    [Fact]
    public async Task Save_WithAFile_WritesWithoutADialog()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        await s.Service.SaveAsync();
        s.Edit();

        var saved = await s.Service.SaveAsync();

        Assert.True(saved);
        Assert.Single(s.Dialogs.SuggestedSaveNames);
        Assert.Equal(2, s.Fake.WriteCount);
        Assert.Single(ProjectFile.Read(s.Fake.Files["/work/a.p2s"]).Document!.Layers[0].Shapes);
        Assert.False(s.Session.IsDirty);
    }

    [Fact]
    public async Task SaveAs_Cancelled_WritesNothingAndChangesNothing()
    {
        var s = new Setup();
        s.Edit();
        s.Dialogs.SaveAnswers.Enqueue(null);

        var saved = await s.Service.SaveAsAsync();

        Assert.False(saved);
        Assert.Empty(s.Fake.Files);
        Assert.True(s.Session.IsDirty);
        Assert.Null(s.Session.Path);
        Assert.Empty(s.Recent.Paths);
        Assert.Empty(s.Messages.Errors);
    }

    [Theory]
    [InlineData("storage:0123/plan")]
    [InlineData("storage:0123/plan.txt")]
    [InlineData("storage:0123/plan.p2s")]
    public async Task SaveAs_WithAReferenceFromTheSystemPicker_KeepsTheReferenceAsItIs(string reference)
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue(reference);

        var saved = await s.Service.SaveAsAsync();

        Assert.True(saved);
        Assert.True(s.Fake.Files.ContainsKey(reference));
        Assert.Equal(reference, s.Session.Path);
    }

    [Fact]
    public async Task Export_WithAReferenceFromTheSystemPicker_KeepsTheReferenceAsItIs()
    {
        var s = new Setup();
        s.Dialogs.ExportAnswers.Enqueue("storage:0123/image");

        await s.Service.ExportSvgAsync();

        Assert.True(s.Fake.Files.ContainsKey("storage:0123/image"));
    }

    [Theory]
    [InlineData("/work/drawing", "/work/drawing.p2s")]
    [InlineData("/work/drawing.p2s", "/work/drawing.p2s")]
    [InlineData("/work/DRAWING.P2S", "/work/DRAWING.P2S")]
    [InlineData("/work/drawing.txt", "/work/drawing.txt.p2s")]
    public async Task SaveAs_AddsTheEndingOnlyWhenMissing(string chosen, string expected)
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue(chosen);

        await s.Service.SaveAsAsync();

        Assert.Equal(expected, s.Session.Path);
        Assert.True(s.Fake.Files.ContainsKey(expected));
    }

    [Fact]
    public async Task SaveAs_ChangesTheTargetOfLaterSaves_AndSuggestsTheFileName()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        await s.Service.SaveAsAsync();
        s.Edit();
        s.Dialogs.SaveAnswers.Enqueue("/other/b.p2s");

        await s.Service.SaveAsAsync();
        s.Edit();
        await s.Service.SaveAsync();

        Assert.Equal("a.p2s", s.Dialogs.SuggestedSaveNames[1]);
        Assert.Equal("/other/b.p2s", s.Session.Path);
        Assert.Empty(ProjectFile.Read(s.Fake.Files["/work/a.p2s"]).Document!.Layers[0].Shapes);
        Assert.Equal(2, ProjectFile.Read(s.Fake.Files["/other/b.p2s"]).Document!.Layers[0].Shapes.Length);
        Assert.Equal(new[] { "/other/b.p2s", "/work/a.p2s" }, s.Recent.Paths);
    }

    [Fact]
    public async Task Save_WriteFailure_KeepsTheOldBytesTheDirtyStateAndShowsTheFileName()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        await s.Service.SaveAsync();
        var oldBytes = s.Fake.Files["/work/a.p2s"];
        s.Edit();
        s.Fake.WriteFailure = new IOException("disk full");

        var saved = await s.Service.SaveAsync();

        Assert.False(saved);
        Assert.Same(oldBytes, s.Fake.Files["/work/a.p2s"]);
        Assert.True(s.Session.IsDirty);
        var error = Assert.Single(s.Messages.Errors);
        Assert.Contains("“a.p2s”", error, StringComparison.Ordinal);
        Assert.DoesNotContain("disk full", error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveAs_WriteFailure_DoesNotChangeTheSessionOrTheRecentList()
    {
        var s = new Setup();
        s.Edit();
        s.Dialogs.SaveAnswers.Enqueue("/ro/a.p2s");
        s.Fake.WriteFailure = new UnauthorizedAccessException();

        var saved = await s.Service.SaveAsAsync();

        Assert.False(saved);
        Assert.Null(s.Session.Path);
        Assert.True(s.Session.IsDirty);
        Assert.Empty(s.Recent.Paths);
        Assert.Single(s.Messages.Errors);
    }

    [Fact]
    public async Task Save_DocumentEditedWhileWriting_StaysDirty()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        var task = s.Service.SaveAsAsync(); // runs up to the first real wait: the document to write is already taken
        s.Edit();
        await task;

        Assert.True(s.Session.IsDirty);
    }

    [Fact]
    public async Task Save_DialogThrows_ShowsAnErrorAndStops()
    {
        var s = new Setup();
        s.Dialogs.Failure = new InvalidOperationException("picker broke");

        var saved = await s.Service.SaveAsAsync();

        Assert.False(saved);
        Assert.Single(s.Messages.Errors);
        Assert.Empty(s.Fake.Files);
    }

    [Fact]
    public async Task Save_ErrorTexts_AreTranslated()
    {
        var german = new System.Collections.Generic.Dictionary<string, string>
        {
            ["Could not save “{0}”. {1}"] = "„{0}“ konnte nicht gespeichert werden. {1}",
            ["The file could not be written. Check the folder, the permissions and the free space."] = "Nicht schreibbar.",
        };
        var s = new Setup(FakeLocalizer.Create("de", german));
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        s.Fake.WriteFailure = new IOException();

        await s.Service.SaveAsAsync();

        Assert.Equal("„a.p2s“ konnte nicht gespeichert werden. Nicht schreibbar.", Assert.Single(s.Messages.Errors));
    }

    [Fact]
    public async Task Export_WritesAValidSvgAndLeavesSessionAndRecentAlone()
    {
        var s = new Setup();
        s.Edit();
        s.Dialogs.ExportAnswers.Enqueue("/out/picture");

        await s.Service.ExportSvgAsync();

        var svg = XDocument.Parse(Encoding.UTF8.GetString(s.Fake.Files["/out/picture.svg"]));
        Assert.Equal("svg", svg.Root!.Name.LocalName);
        Assert.Equal("Untitled.svg", Assert.Single(s.Dialogs.SuggestedExportNames));
        Assert.True(s.Session.IsDirty);
        Assert.Null(s.Session.Path);
        Assert.Empty(s.Recent.Paths);
    }

    [Fact]
    public async Task Export_DocumentWithAText_WritesItsLinesFromTheMeasurer()
    {
        var s = new Setup();
        s.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(s.State.ActiveLayerId, new TextShape { Text = "Hi\nthere", FontSize = 10 }));
        s.Dialogs.ExportAnswers.Enqueue("/out/text");

        await s.Service.ExportSvgAsync();

        XNamespace ns = "http://www.w3.org/2000/svg";
        var svg = XDocument.Parse(Encoding.UTF8.GetString(s.Fake.Files["/out/text.svg"]));
        var lines = svg.Descendants(ns + "text").Single().Elements(ns + "tspan").Select(tspan => tspan.Value);
        Assert.Equal(["Hi", "there"], lines);
    }

    [Fact]
    public async Task Export_OfASavedDocument_SuggestsItsNameAndKeepsItClean()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/drawing.p2s");
        await s.Service.SaveAsync();
        s.Dialogs.ExportAnswers.Enqueue("/out/x.SVG");

        await s.Service.ExportSvgAsync();

        Assert.Equal("drawing.svg", s.Dialogs.SuggestedExportNames[0]);
        Assert.True(s.Fake.Files.ContainsKey("/out/x.SVG"));
        Assert.False(s.Session.IsDirty);
        Assert.Equal("/work/drawing.p2s", s.Session.Path);
        Assert.Equal(new[] { "/work/drawing.p2s" }, s.Recent.Paths);
    }

    [Fact]
    public async Task Export_Cancelled_WritesNothing()
    {
        var s = new Setup();
        s.Dialogs.ExportAnswers.Enqueue(null);

        await s.Service.ExportSvgAsync();

        Assert.Empty(s.Fake.Files);
        Assert.Empty(s.Messages.Errors);
    }

    [Fact]
    public async Task Export_WriteFailure_ShowsAnErrorWithTheFileName()
    {
        var s = new Setup();
        s.Dialogs.ExportAnswers.Enqueue("/out/x.svg");
        s.Fake.WriteFailure = new IOException();

        await s.Service.ExportSvgAsync();

        Assert.Contains("“x.svg”", Assert.Single(s.Messages.Errors), StringComparison.Ordinal);
        Assert.Empty(s.Fake.Files);
    }

    [Fact]
    public async Task Export_DialogThrows_ShowsAnError()
    {
        var s = new Setup();
        s.Dialogs.Failure = new InvalidOperationException();

        await s.Service.ExportSvgAsync();

        Assert.Single(s.Messages.Errors);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var s = new Setup();

        Assert.Throws<ArgumentNullException>(() => new DocumentFileService(null!, s.Session, s.Recent, s.Files, s.Dialogs, s.Messages, s.Localizer, new FakeTextMeasurer(), () => { }));
        Assert.Throws<ArgumentNullException>(() => new DocumentFileService(s.State, s.Session, s.Recent, s.Files, s.Dialogs, s.Messages, s.Localizer, new FakeTextMeasurer(), null!));
    }

    [Fact]
    public async Task Save_SavesWhatTheDocumentHoldsRightNow_NotAnOlderState()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        s.Edit();
        s.Edit();

        await s.Service.SaveAsync();

        Assert.Equal(2, ProjectFile.Read(s.Fake.Files["/work/a.p2s"]).Document!.Layers[0].Shapes.Length);
    }

    private static byte[] ProjectWith(int shapes, string layerName = "Opened")
    {
        var document = Document.Create(layerName);
        for (var i = 0; i < shapes; i++)
        {
            document = document.AddShape(document.Layers[0].Id, new RectangleShape { X = i, Width = 5, Height = 5 });
        }

        return ProjectFile.Write(document);
    }

    // ---- unsaved changes ----

    [Fact]
    public async Task ConfirmDiscard_Clean_IsTrueWithoutAQuestion()
    {
        var s = new Setup();

        Assert.True(await s.Service.ConfirmDiscardAsync());
        Assert.Empty(s.Messages.Questions);
    }

    [Fact]
    public async Task ConfirmDiscard_DirtyAndDontSave_IsTrueWithoutWriting()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;

        Assert.True(await s.Service.ConfirmDiscardAsync());
        Assert.Equal("Untitled", Assert.Single(s.Messages.Questions));
        Assert.Empty(s.Fake.Files);
    }

    [Fact]
    public async Task ConfirmDiscard_DirtyAndCancel_IsFalseWithoutWriting()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.Cancel;

        Assert.False(await s.Service.ConfirmDiscardAsync());
        Assert.Empty(s.Fake.Files);
        Assert.True(s.Session.IsDirty);
    }

    [Fact]
    public async Task ConfirmDiscard_SaveAnswerWithAFile_SavesAndContinues()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/mine.p2s");
        await s.Service.SaveAsync();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.Save;

        Assert.True(await s.Service.ConfirmDiscardAsync());

        Assert.Equal("mine.p2s", Assert.Single(s.Messages.Questions));
        Assert.False(s.Session.IsDirty);
    }

    [Fact]
    public async Task ConfirmDiscard_SaveAnswerButTheFileDialogIsCancelled_Stops()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.Save;
        s.Dialogs.SaveAnswers.Enqueue(null);

        Assert.False(await s.Service.ConfirmDiscardAsync());
        Assert.True(s.Session.IsDirty);
    }

    [Fact]
    public async Task ConfirmDiscard_SaveAnswerButTheWriteFails_StopsAndShowsTheError()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.Save;
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        s.Fake.WriteFailure = new IOException();

        Assert.False(await s.Service.ConfirmDiscardAsync());
        Assert.Single(s.Messages.Errors);
        Assert.True(s.Session.IsDirty);
    }

    // ---- New ----

    [Fact]
    public async Task New_Clean_ReplacesTheDocumentWithoutAQuestion()
    {
        var s = new Setup();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        await s.Service.SaveAsync();

        await s.Service.NewAsync();

        Assert.Empty(s.Messages.Questions);
        var layer = Assert.Single(s.State.Document.Layers);
        Assert.Equal("Layer 1", layer.Name);
        Assert.Empty(layer.Shapes);
        Assert.Equal((1200, 800), (s.State.Document.ArtboardWidth, s.State.Document.ArtboardHeight));
        Assert.Null(s.Session.Path);
        Assert.False(s.Session.IsDirty);
        Assert.Equal(1, s.FitCount);
        Assert.Equal(layer.Id, s.State.ActiveLayerId);
    }

    [Fact]
    public async Task New_NamesTheLayerInTheCurrentLanguage()
    {
        var s = new Setup(FakeLocalizer.Create("de", new System.Collections.Generic.Dictionary<string, string> { ["Layer 1"] = "Ebene 1" }));

        await s.Service.NewAsync();

        Assert.Equal("Ebene 1", s.State.Document.Layers[0].Name);
    }

    [Fact]
    public async Task New_Dirty_ClearsTheSelection()
    {
        var s = new Setup();
        s.Edit();
        s.State.Select(s.State.Document.Layers[0].Shapes[0].Id);
        s.Messages.Answer = UnsavedChoice.DontSave;

        await s.Service.NewAsync();

        Assert.Null(s.State.SelectedShapeId);
        Assert.False(s.State.HasSelection);
    }

    [Fact]
    public async Task New_Dirty_CancelKeepsEverything()
    {
        var s = new Setup();
        s.Edit();
        var before = s.State.Document;
        s.Messages.Answer = UnsavedChoice.Cancel;

        await s.Service.NewAsync();

        Assert.Same(before, s.State.Document);
        Assert.True(s.Session.IsDirty);
        Assert.Equal(0, s.FitCount);
    }

    [Fact]
    public async Task New_Dirty_SaveAnswerSavesThenReplaces()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.Save;
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");

        await s.Service.NewAsync();

        Assert.Single(ProjectFile.Read(s.Fake.Files["/work/a.p2s"]).Document!.Layers[0].Shapes);
        Assert.Empty(s.State.Document.Layers[0].Shapes);
        Assert.Null(s.Session.Path);
    }

    // ---- Open ----

    [Fact]
    public async Task Open_ValidFile_ReplacesTheDocumentAndResetsTheSession()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;
        s.State.Select(s.State.Document.Layers[0].Shapes[0].Id);
        s.Fake.Files["/work/o.p2s"] = ProjectWith(3);
        s.Dialogs.OpenAnswers.Enqueue("/work/o.p2s");

        await s.Service.OpenAsync();

        Assert.Equal("Opened", s.State.Document.Layers[0].Name);
        Assert.Equal(3, s.State.Document.Layers[0].Shapes.Length);
        Assert.Equal("/work/o.p2s", s.Session.Path);
        Assert.False(s.Session.IsDirty);
        Assert.Null(s.State.SelectedShapeId);
        Assert.Equal(s.State.Document.Layers[0].Id, s.State.ActiveLayerId);
        Assert.Equal(new[] { "/work/o.p2s" }, s.Recent.Paths);
        Assert.Equal(1, s.FitCount);
        Assert.Empty(s.Messages.Errors);
    }

    [Fact]
    public async Task Open_Cancelled_ChangesNothing()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;
        var before = s.State.Document;
        s.Dialogs.OpenAnswers.Enqueue(null);

        await s.Service.OpenAsync();

        Assert.Same(before, s.State.Document);
        Assert.Empty(s.Messages.Errors);
        Assert.Equal(0, s.FitCount);
    }

    [Fact]
    public async Task Open_DirtyAndCancelledQuestion_DoesNotEvenShowThePicker()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.Cancel;

        await s.Service.OpenAsync();

        Assert.Equal(0, s.Dialogs.OpenCount);
    }

    [Fact]
    public async Task Open_PickerThrows_ShowsAnErrorAndChangesNothing()
    {
        var s = new Setup();
        s.Dialogs.Failure = new InvalidOperationException();
        var before = s.State.Document;

        await s.Service.OpenAsync();

        Assert.Single(s.Messages.Errors);
        Assert.Same(before, s.State.Document);
    }

    [Theory]
    [InlineData("not json at all", "This is not a PressToShape file.")]
    [InlineData("{\"format\":\"PressToShape\",\"version\":7}", "The file was made by a newer version of PressToShape.")]
    [InlineData("{\"format\":\"PressToShape\",\"version\":1,\"artboard\":{\"width\":-1,\"height\":5},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[]}]}", "The file contains invalid data.")]
    [InlineData("{\"format\":\"PressToShape\",\"version\":1,\"artboard\":{\"width\":5,\"height\":5},\"layers\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"name\":\"L\",\"visible\":true,\"locked\":false,\"shapes\":[{\"type\":\"star\",\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"x\":0,\"y\":0,\"width\":1,\"height\":1,\"fill\":null,\"stroke\":null,\"strokeWidth\":0,\"opacity\":1}]}]}", "The file contains a shape type that this version does not know.")]
    public async Task Open_BadFile_ShowsTheReasonAndChangesNothing(string content, string reason)
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;
        var before = s.State.Document;
        s.Fake.Files["/work/bad.p2s"] = Encoding.UTF8.GetBytes(content);
        s.Dialogs.OpenAnswers.Enqueue("/work/bad.p2s");

        await s.Service.OpenAsync();

        Assert.Equal($"Could not open “bad.p2s”. {reason}", Assert.Single(s.Messages.Errors));
        Assert.Same(before, s.State.Document);
        Assert.Null(s.Session.Path);
        Assert.Empty(s.Recent.Paths);
        Assert.Equal(0, s.FitCount);
    }

    [Fact]
    public async Task Open_FileTooLarge_ShowsTheReason()
    {
        var s = new Setup();
        s.Fake.ReadFailure = new InvalidDataException();
        s.Dialogs.OpenAnswers.Enqueue("/work/big.p2s");

        await s.Service.OpenAsync();

        Assert.Equal("Could not open “big.p2s”. The file is too large.", Assert.Single(s.Messages.Errors));
    }

    [Fact]
    public async Task Open_ReadFailure_ShowsTheReason()
    {
        var s = new Setup();
        s.Fake.ReadFailure = new UnauthorizedAccessException();
        s.Dialogs.OpenAnswers.Enqueue("/work/locked.p2s");

        await s.Service.OpenAsync();

        Assert.Equal("Could not open “locked.p2s”. The file could not be read.", Assert.Single(s.Messages.Errors));
    }

    [Fact]
    public async Task Open_ErrorTexts_AreTranslated()
    {
        var german = new System.Collections.Generic.Dictionary<string, string>
        {
            ["Could not open “{0}”. {1}"] = "„{0}“ konnte nicht geöffnet werden. {1}",
            ["The file was not found."] = "Nicht gefunden.",
        };
        var s = new Setup(FakeLocalizer.Create("de", german));
        s.Dialogs.OpenAnswers.Enqueue("/work/missing.p2s");

        await s.Service.OpenAsync();

        Assert.Equal("„missing.p2s“ konnte nicht geöffnet werden. Nicht gefunden.", Assert.Single(s.Messages.Errors));
    }

    // ---- Open Recent ----

    [Fact]
    public async Task OpenRecent_ExistingFile_OpensItAndMovesItToTheTop()
    {
        var s = new Setup();
        s.Fake.Files["/a.p2s"] = ProjectWith(1);
        s.Fake.Files["/b.p2s"] = ProjectWith(2);
        s.Recent.Add("/a.p2s");
        s.Recent.Add("/b.p2s");

        await s.Service.OpenRecentAsync("/a.p2s");

        Assert.Single(s.State.Document.Layers[0].Shapes);
        Assert.Equal(new[] { "/a.p2s", "/b.p2s" }, s.Recent.Paths);
        Assert.Equal("/a.p2s", s.Session.Path);
    }

    [Fact]
    public async Task OpenRecent_Dirty_AsksFirst()
    {
        var s = new Setup();
        s.Edit();
        s.Fake.Files["/a.p2s"] = ProjectWith(1);
        s.Messages.Answer = UnsavedChoice.Cancel;

        await s.Service.OpenRecentAsync("/a.p2s");

        Assert.Single(s.Messages.Questions);
        Assert.Null(s.Session.Path);
    }

    [Fact]
    public async Task OpenRecent_MissingFile_ShowsAnErrorAndRemovesTheEntry()
    {
        var s = new Setup();
        s.Recent.Add("/gone.p2s");
        s.Recent.Add("/kept.p2s");

        await s.Service.OpenRecentAsync("/gone.p2s");

        Assert.Equal("Could not open “gone.p2s”. The file was not found.", Assert.Single(s.Messages.Errors));
        Assert.Equal(new[] { "/kept.p2s" }, s.Recent.Paths);
    }

    [Fact]
    public async Task OpenRecent_DamagedFile_ShowsAnErrorButKeepsTheEntry()
    {
        var s = new Setup();
        s.Fake.Files["/broken.p2s"] = Encoding.UTF8.GetBytes("{ nope");
        s.Recent.Add("/broken.p2s");

        await s.Service.OpenRecentAsync("/broken.p2s");

        Assert.Single(s.Messages.Errors);
        Assert.Equal(new[] { "/broken.p2s" }, s.Recent.Paths);
    }

    [Fact]
    public async Task OpenRecent_BlankPath_DoesNothing()
    {
        var s = new Setup();

        await s.Service.OpenRecentAsync(" ");

        Assert.Empty(s.Messages.Errors);
        Assert.Empty(s.Messages.Questions);
    }

    // ---- with the real file store ----

    [Fact]
    public async Task SaveThenOpen_WithTheRealFileStoreInATempFolder_RoundTrips()
    {
        var path = Path.Combine(_folder, "round.p2s");
        var writer = new Setup(files: new LocalFileStore());
        writer.Edit();
        writer.Edit();
        writer.Dialogs.SaveAnswers.Enqueue(path);
        Assert.True(await writer.Service.SaveAsAsync());

        var reader = new Setup(files: new LocalFileStore());
        reader.Dialogs.OpenAnswers.Enqueue(path);
        await reader.Service.OpenAsync();

        Assert.Equal(2, reader.State.Document.Layers[0].Shapes.Length);
        Assert.Equal(writer.State.Document.Layers[0].Shapes[1].Id, reader.State.Document.Layers[0].Shapes[1].Id);
        Assert.Equal(new[] { "round.p2s" }, Directory.GetFiles(_folder).Select(Path.GetFileName).ToArray());
        Assert.False(reader.Session.IsDirty);
    }

    // ---- history ----

    [Fact]
    public async Task New_ResetsTheHistory()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;

        await s.Service.NewAsync();

        Assert.Empty(s.State.History.Steps);
        Assert.False(s.State.CanUndo);
        Assert.Same(s.State.Document, s.State.History.Current);
    }

    [Fact]
    public async Task Open_ResetsTheHistory()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;
        s.Fake.Files["/work/o.p2s"] = ProjectWith(2);
        s.Dialogs.OpenAnswers.Enqueue("/work/o.p2s");

        await s.Service.OpenAsync();

        Assert.Empty(s.State.History.Steps);
        Assert.False(s.State.CanUndo);
        Assert.Equal(2, s.State.Document.Layers[0].Shapes.Length);
    }

    [Fact]
    public async Task Open_FailedOrCancelled_KeepsTheHistory()
    {
        var s = new Setup();
        s.Edit();
        s.Messages.Answer = UnsavedChoice.DontSave;
        s.Fake.Files["/work/bad.p2s"] = Encoding.UTF8.GetBytes("nope");
        s.Dialogs.OpenAnswers.Enqueue("/work/bad.p2s");
        await s.Service.OpenAsync();
        s.Dialogs.OpenAnswers.Enqueue(null);
        await s.Service.OpenAsync();

        Assert.Single(s.State.History.Steps);
        Assert.True(s.State.CanUndo);
    }

    [Fact]
    public async Task SaveAndExport_KeepTheHistory()
    {
        var s = new Setup();
        s.Edit();
        s.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");
        s.Dialogs.ExportAnswers.Enqueue("/out/a.svg");

        await s.Service.SaveAsAsync();
        await s.Service.ExportSvgAsync();

        Assert.Single(s.State.History.Steps);
        Assert.True(s.State.CanUndo);
    }
}
