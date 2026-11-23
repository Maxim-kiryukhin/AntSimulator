using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntSimulator.Core;

/// <summary>Доступные блоки поведения муравья.</summary>
public enum CellAction
{
    TurnLeft,   // повернуть налево
    TurnRight,  // повернуть направо
    Forward,    // идти прямо на одну клетку
    Repaint     // перекрасить текущую клетку
}