using System.Collections.Generic;
using System.Linq;

namespace AntSimulator.Core;

/// <summary>Блок правила + его человекочитаемое название для интерфейса.</summary>
public sealed class RuleBlock
{
    public CellAction Action { get; }
    public string Title { get; }

    private RuleBlock(CellAction action, string title)
    {
        Action = action;
        Title = title;
    }

    public static readonly RuleBlock TurnLeft = new(CellAction.TurnLeft, "Повернуть налево");
    public static readonly RuleBlock TurnRight = new(CellAction.TurnRight, "Повернуть направо");
    public static readonly RuleBlock Forward = new(CellAction.Forward, "Идти прямо");
    public static readonly RuleBlock Repaint = new(CellAction.Repaint, "Перекрасить клетку");

    public static IReadOnlyList<RuleBlock> All { get; } =
        new[] { TurnLeft, TurnRight, Forward, Repaint };

    public static RuleBlock FromAction(CellAction action) => All.First(b => b.Action == action);

    public override string ToString() => Title;
}