using System.Windows;
using TrialWpfApp.Models;

namespace TrialWpfApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new ViewModel(SampleData.Data);
        DataContext = vm;
    }
}
