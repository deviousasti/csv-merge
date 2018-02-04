using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace csv_merge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MergerViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel = (MergerViewModel)this.DataContext;

            this.Drop += ViewModel.OnDragDrop;

            //ViewModel.AddPaths(@"C:\Source\designs");
        }

    }
}
