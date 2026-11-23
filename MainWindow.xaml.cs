using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AntSimulator.Core;

namespace AntSimulator;

public partial class MainWindow : Window
{
    private const int MinFieldSize = 5;
    private const int MaxFieldSize = 600;

    // Цвета формата Bgr32 (0x00RRGGBB)
    private const int WhiteColor = 0xFFFFFF;
    private const int BlackColor = 0x000000;
    private const int AntColor = 0xE53935; // муравей подсвечен красным

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };

    private AntSimulation? _simulation;
    private WriteableBitmap? _bitmap;
    private int[] _pixels = Array.Empty<int>();

    private bool _isPainting;
    private bool _paintValue;

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += OnTimerTick;

        // Классический муравей Лэнгтона как стартовый набор правил.
        WhiteRuleEditor.SetRules(new[] { CellAction.TurnRight, CellAction.Repaint, CellAction.Forward });
        BlackRuleEditor.SetRules(new[] { CellAction.TurnLeft, CellAction.Repaint, CellAction.Forward });

        Generate();
    }

    // ---------- Кнопки управления ----------

    private void OnGenerateClick(object sender, RoutedEventArgs e) => Generate();

    private void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (_simulation is null) return;

        var white = WhiteRuleEditor.GetActions();
        var black = BlackRuleEditor.GetActions();

        if (white.Count == 0 && black.Count == 0)
        {
            MessageBox.Show("Добавьте хотя бы один блок поведения, иначе муравей будет стоять на месте.",
                            "Правила не заданы", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _simulation.SetRules(white, black);
        _timer.Start();
        UpdateControls();
    }

    private void OnPauseClick(object sender, RoutedEventArgs e) => Pause();

    private void OnStepClick(object sender, RoutedEventArgs e)
    {
        if (_simulation is null) return;

        _simulation.SetRules(WhiteRuleEditor.GetActions(), BlackRuleEditor.GetActions());
        _simulation.Step();
        Render();
        UpdateStatus();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_simulation is null) return;

        int steps = Math.Max(1, (int)SpeedSlider.Value);
        _simulation.Step(steps);
        Render();
        UpdateStatus();
    }

    // ---------- Создание поля ----------

    private void Generate()
    {
        if (!TryReadSize(out int width, out int height)) return;

        Pause();

        _simulation = new AntSimulation(width, height, RandomFillBox.IsChecked == true);
        _simulation.SetRules(WhiteRuleEditor.GetActions(), BlackRuleEditor.GetActions());

        _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        _pixels = new int[width * height];
        FieldImage.Source = _bitmap;

        Render();
        UpdateStatus();
        UpdateControls();
    }

    private bool TryReadSize(out int width, out int height)
    {
        width = height = 0;

        if (!int.TryParse(WidthBox.Text, out width) || !int.TryParse(HeightBox.Text, out height))
        {
            MessageBox.Show("Ширина и высота должны быть целыми числами.",
                            "Некорректный размер", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (width < MinFieldSize || height < MinFieldSize || width > MaxFieldSize || height > MaxFieldSize)
        {
            MessageBox.Show($"Размеры поля должны быть от {MinFieldSize} до {MaxFieldSize} клеток.",
                            "Некорректный размер", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void Pause()
    {
        _timer.Stop();
        _isPainting = false;
        UpdateControls();
    }

    // ---------- Отрисовка ----------

    private void Render()
    {
        if (_simulation is null || _bitmap is null) return;

        int w = _simulation.Width;
        int h = _simulation.Height;

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
                _pixels[row + x] = _simulation.IsBlack(x, y) ? BlackColor : WhiteColor;
        }

        _pixels[_simulation.AntY * w + _simulation.AntX] = AntColor;

        _bitmap.WritePixels(new Int32Rect(0, 0, w, h), _pixels, w * 4, 0);
    }

    private void UpdateStatus()
    {
        if (_simulation is null)
        {
            StatusText.Text = "Поле не создано";
            return;
        }

        StatusText.Text =
            $"Шаг: {_simulation.StepCount}    " +
            $"Муравей: ({_simulation.AntX}, {_simulation.AntY})    " +
            $"Направление: {DirectionName(_simulation.AntDirection)}    " +
            $"Чёрных клеток: {_simulation.BlackCount}";
    }

    private static string DirectionName(Direction direction) => direction switch
    {
        Direction.Up => "вверх",
        Direction.Right => "вправо",
        Direction.Down => "вниз",
        Direction.Left => "влево",
        _ => "?"
    };

    private void UpdateControls()
    {
        bool running = _timer.IsEnabled;
        bool hasField = _simulation is not null;

        StartButton.IsEnabled = hasField && !running;
        StepButton.IsEnabled = hasField && !running;
        PauseButton.IsEnabled = running;

        FieldPanel.IsEnabled = !running;
        RulesPanel.IsEnabled = !running;
    }

    // ---------- Ручное редактирование поля мышью ----------

    private void OnFieldMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_simulation is null || _timer.IsEnabled) return;
        if (!TryGetCell(e.GetPosition(FieldImage), out int x, out int y)) return;

        _paintValue = !_simulation.IsBlack(x, y);
        _simulation.SetCell(x, y, _paintValue);
        _isPainting = true;
        FieldImage.CaptureMouse();

        Render();
        UpdateStatus();
    }

    private void OnFieldMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPainting || _simulation is null) return;
        if (!TryGetCell(e.GetPosition(FieldImage), out int x, out int y)) return;

        _simulation.SetCell(x, y, _paintValue);
        Render();
        UpdateStatus();
    }

    private void OnFieldMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isPainting = false;
        FieldImage.ReleaseMouseCapture();
    }

    private void OnFieldMouseLeave(object sender, MouseEventArgs e) => _isPainting = false;

    private void OnFieldMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_simulation is null || _timer.IsEnabled) return;
        if (!TryGetCell(e.GetPosition(FieldImage), out int x, out int y)) return;

        _simulation.PlaceAnt(x, y);
        Render();
        UpdateStatus();
    }

    /// <summary>Перевод координат мыши в координаты клетки с учётом Stretch="Uniform".</summary>
    private bool TryGetCell(Point position, out int x, out int y)
    {
        x = y = 0;
        if (_simulation is null) return false;

        double controlWidth = FieldImage.ActualWidth;
        double controlHeight = FieldImage.ActualHeight;
        if (controlWidth <= 0 || controlHeight <= 0) return false;

        double scale = Math.Min(controlWidth / _simulation.Width, controlHeight / _simulation.Height);
        double drawnWidth = _simulation.Width * scale;
        double drawnHeight = _simulation.Height * scale;
        double offsetX = (controlWidth - drawnWidth) / 2;
        double offsetY = (controlHeight - drawnHeight) / 2;

        double fx = (position.X - offsetX) / scale;
        double fy = (position.Y - offsetY) / scale;

        if (fx < 0 || fy < 0 || fx >= _simulation.Width || fy >= _simulation.Height) return false;

        x = (int)fx;
        y = (int)fy;
        return true;
    }
}