using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Tests;

/// <summary>The smallest shell: only what the base class gives every layout, with fake files, dialogs and settings. For testing the parts of a layout.</summary>
internal sealed class TestShell : EditorShellViewModel
{
    public TestShell(ILocalizer? localizer = null)
        : this(localizer ?? FakeLocalizer.Create(), new FakeFileStore(), new FakeFileDialogs(), new FakeMessageDialogs())
    {
    }

    private TestShell(ILocalizer localizer, FakeFileStore files, FakeFileDialogs dialogs, FakeMessageDialogs messages)
        : base(localizer, TestFileServices.Create(files, dialogs, messages))
    {
        LocalizerForTests = localizer;
        Files = files;
        Dialogs = dialogs;
        Messages = messages;
    }

    /// <summary>The localizer the shell was made with.</summary>
    public ILocalizer LocalizerForTests { get; }

    /// <summary>The commands behind the file items of a menu, which the base class keeps protected.</summary>
    public FileMenuCommands FileCommandsForTests => FileCommands;

    /// <summary>The commands behind the edit items of a menu, which the base class keeps protected.</summary>
    public EditMenuCommands EditCommandsForTests => EditCommands;

    public FakeFileStore Files { get; }

    public FakeFileDialogs Dialogs { get; }

    public FakeMessageDialogs Messages { get; }

    /// <summary>Draws a rectangle on the active layer as one history step, without selecting it.</summary>
    public RectangleShape AddRectangle(double x = 100, double y = 100, double width = 200, double height = 100)
    {
        var shape = new RectangleShape { X = x, Y = y, Width = width, Height = height };
        State.Edit(HistoryStepType.DrawRectangle, document => document.AddShape(State.ActiveLayerId, shape));
        return shape;
    }

    /// <summary>Draws a rectangle and selects it.</summary>
    public RectangleShape AddAndSelectRectangle(double x = 100, double y = 100, double width = 200, double height = 100)
    {
        var shape = AddRectangle(x, y, width, height);
        State.SelectOnly(shape.Id);
        return shape;
    }
}
