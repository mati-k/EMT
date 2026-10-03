using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EMT.Models;

namespace EMT.Views
{
    /// <summary>
    /// Editable tree of a script block (potential, trigger, effect...).
    /// </summary>
    public partial class NodeTreeEditor : UserControl
    {
        public static readonly StyledProperty<GroupNodeModel?> RootProperty =
            AvaloniaProperty.Register<NodeTreeEditor, GroupNodeModel?>(nameof(Root));

        public static readonly StyledProperty<string?> HeaderProperty =
            AvaloniaProperty.Register<NodeTreeEditor, string?>(nameof(Header));

        public GroupNodeModel? Root
        {
            get => GetValue(RootProperty);
            set => SetValue(RootProperty, value);
        }

        public string? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public NodeTreeEditor()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RootProperty)
            {
                Tree.ItemsSource = Root?.Nodes;
                UpdateHeader();
            }
            else if (change.Property == HeaderProperty)
            {
                UpdateHeader();
            }
        }

        private void UpdateHeader()
        {
            HeaderText.Text = Header ?? Root?.Name;
        }

        private static void AddValue(GroupNodeModel? group)
        {
            group?.Nodes.Add(new ValueNodeModel() { Parent = group });
        }

        private static void AddGroup(GroupNodeModel? group)
        {
            group?.Nodes.Add(new GroupNodeModel() { Parent = group });
        }

        private void AddRootValue_Click(object? sender, RoutedEventArgs e) => AddValue(Root);
        private void AddRootGroup_Click(object? sender, RoutedEventArgs e) => AddGroup(Root);
        private void AddValue_Click(object? sender, RoutedEventArgs e) => AddValue((sender as Control)?.DataContext as GroupNodeModel);
        private void AddGroup_Click(object? sender, RoutedEventArgs e) => AddGroup((sender as Control)?.DataContext as GroupNodeModel);

        private void Remove_Click(object? sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.DataContext is NodeModel node)
                node.Parent?.Nodes.Remove(node);
        }
    }
}
