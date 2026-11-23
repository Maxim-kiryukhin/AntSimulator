using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AntSimulator.Core;

namespace AntSimulator.Controls;

public partial class RuleEditor : UserControl
{
    /// <summary>Текущий упорядоченный набор блоков поведения.</summary>
    public ObservableCollection<RuleBlock> Rules { get; } = new();

    public RuleEditor()
    {
        InitializeComponent();
        ActionBox.ItemsSource = RuleBlock.All;
        ActionBox.SelectedIndex = 0;
        RulesList.ItemsSource = Rules;
    }

    public string Header
    {
        get => HeaderText.Text;
        set => HeaderText.Text = value;
    }

    public void SetRules(IEnumerable<CellAction> actions)
    {
        Rules.Clear();
        foreach (CellAction action in actions)
            Rules.Add(RuleBlock.FromAction(action));
    }

    public List<CellAction> GetActions() => Rules.Select(r => r.Action).ToList();

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        if (ActionBox.SelectedItem is not RuleBlock block) return;

        int index = RulesList.SelectedIndex >= 0 ? RulesList.SelectedIndex + 1 : Rules.Count;
        Rules.Insert(index, block);
        RulesList.SelectedIndex = index;
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        int index = RulesList.SelectedIndex;
        if (index < 0) return;

        Rules.RemoveAt(index);
        if (index < Rules.Count) RulesList.SelectedIndex = index;
    }

    private void OnMoveUpClick(object sender, RoutedEventArgs e) => Move(-1);

    private void OnMoveDownClick(object sender, RoutedEventArgs e) => Move(+1);

    private void Move(int delta)
    {
        int from = RulesList.SelectedIndex;
        int to = from + delta;
        if (from < 0 || to < 0 || to >= Rules.Count) return;

        Rules.Move(from, to);
        RulesList.SelectedIndex = to;
    }
}