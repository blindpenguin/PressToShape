using System;

namespace PressToShape.App.Localization;

/// <summary>
/// An observable text that emits the current translation when subscribed and again after every language change.
/// It is the source of the bindings created by <see cref="TExtension"/>.
/// </summary>
internal sealed class LocalizedTextObservable : IObservable<string>
{
    private readonly ILocalizer _localizer;
    private readonly string? _context;
    private readonly string _text;

    public LocalizedTextObservable(ILocalizer localizer, string? context, string text)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _context = context;
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        void Publish() => observer.OnNext(_context is null ? _localizer.T(_text) : _localizer.TC(_context, _text));
        void OnLanguageChanged(object? sender, EventArgs e) => Publish();

        _localizer.LanguageChanged += OnLanguageChanged;
        Publish();
        return new Subscription(() => _localizer.LanguageChanged -= OnLanguageChanged);
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _dispose;

        public Subscription(Action dispose) => _dispose = dispose;

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
