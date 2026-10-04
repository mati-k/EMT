using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using EMT.Models;
using EMT.Services;
using System;
using System.Collections.Generic;

namespace EMT.Views
{
    /// <summary>
    /// Image of a sprite by its gfx name. Animated sprites are played while the image is on screen.
    /// </summary>
    public class GfxImage : Image
    {
        public static readonly StyledProperty<string?> GfxNameProperty =
            AvaloniaProperty.Register<GfxImage, string?>(nameof(GfxName));

        public string? GfxName
        {
            get => GetValue(GfxNameProperty);
            set => SetValue(GfxNameProperty, value);
        }

        protected override Type StyleKeyOverride => typeof(Image);

        private IReadOnlyList<IImage> _frames = [];
        private GfxSprite? _sprite;
        private DispatcherTimer? _timer;
        private int _frame;
        private int _pauseTicks;

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == GfxNameProperty)
                LoadFrames();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            UpdateTimer();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            StopTimer();
        }

        private void LoadFrames()
        {
            var gfxService = Ioc.Default.GetService<IGfxService>();
            _sprite = gfxService?.GetSprite(GfxName);
            _frames = gfxService?.GetFrames(GfxName) ?? [];
            _frame = 0;
            _pauseTicks = 0;

            Source = _frames.Count > 0 ? _frames[0] : null;
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            StopTimer();

            if (_sprite == null || !_sprite.IsAnimated || _frames.Count <= 1 || VisualRoot == null)
                return;

            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1 / _sprite.Fps), DispatcherPriority.Render, (_, _) => NextFrame());
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer?.Stop();
            _timer = null;
        }

        private void NextFrame()
        {
            if (_pauseTicks > 0)
            {
                _pauseTicks--;
                return;
            }

            _frame = (_frame + 1) % _frames.Count;
            Source = _frames[_frame];

            // Game holds the last frame for pause_on_loop seconds before starting over
            if (_frame == _frames.Count - 1 && _sprite != null)
                _pauseTicks = (int)Math.Round(_sprite.PauseOnLoop * _sprite.Fps);
        }
    }
}
