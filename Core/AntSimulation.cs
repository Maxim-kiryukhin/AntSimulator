using System;
using System.Collections.Generic;
using System.Linq;

namespace AntSimulator.Core;

/// <summary>
/// Игровая логика: поле, муравей и правила. Не зависит от WPF —
/// класс можно переиспользовать в тестах или в консольной версии.
/// </summary>
public sealed class AntSimulation
{
    private readonly bool[,] _cells; // true = чёрная клетка

    public int Width { get; }
    public int Height { get; }

    public int AntX { get; private set; }
    public int AntY { get; private set; }
    public Direction AntDirection { get; private set; } = Direction.Up;

    public long StepCount { get; private set; }
    public int BlackCount { get; private set; }

    public IReadOnlyList<CellAction> WhiteRule { get; private set; } = Array.Empty<CellAction>();
    public IReadOnlyList<CellAction> BlackRule { get; private set; } = Array.Empty<CellAction>();

    public AntSimulation(int width, int height, bool randomFill = false, int? seed = null)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;
        _cells = new bool[width, height];

        AntX = width / 2;
        AntY = height / 2;

        if (randomFill)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (random.Next(2) == 1)
                    {
                        _cells[x, y] = true;
                        BlackCount++;
                    }
        }
    }

    public void SetRules(IEnumerable<CellAction> white, IEnumerable<CellAction> black)
    {
        WhiteRule = white.ToArray();
        BlackRule = black.ToArray();
    }

    public bool IsBlack(int x, int y) => _cells[x, y];

    public void SetCell(int x, int y, bool black)
    {
        if (_cells[x, y] == black) return;
        _cells[x, y] = black;
        BlackCount += black ? 1 : -1;
    }

    public void PlaceAnt(int x, int y)
    {
        AntX = x;
        AntY = y;
    }

    /// <summary>Один шаг: выполняем все блоки правила для текущего цвета клетки.</summary>
    public void Step()
    {
        IReadOnlyList<CellAction> rule = _cells[AntX, AntY] ? BlackRule : WhiteRule;

        for (int i = 0; i < rule.Count; i++)
        {
            switch (rule[i])
            {
                case CellAction.TurnLeft:
                    Turn(-1);
                    break;
                case CellAction.TurnRight:
                    Turn(+1);
                    break;
                case CellAction.Forward:
                    MoveForward();
                    break;
                case CellAction.Repaint:
                    SetCell(AntX, AntY, !_cells[AntX, AntY]);
                    break;
            }
        }

        StepCount++;
    }

    public void Step(int count)
    {
        for (int i = 0; i < count; i++) Step();
    }

    private void Turn(int delta) =>
        AntDirection = (Direction)(((int)AntDirection + delta + 4) % 4);

    private void MoveForward()
    {
        int dx = 0, dy = 0;
        switch (AntDirection)
        {
            case Direction.Up: dy = -1; break;
            case Direction.Right: dx = +1; break;
            case Direction.Down: dy = +1; break;
            case Direction.Left: dx = -1; break;
        }

        // Поле — тор: выход за край возвращает муравья с противоположной стороны.
        AntX = (AntX + dx + Width) % Width;
        AntY = (AntY + dy + Height) % Height;
    }
}