using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace csv_merge
{
    public class FileEntry
    {
        public string Name { get; set; }

        public int Rows { get; set; }

        public string Path { get; set; }

        public override string ToString() => $"{Name} [{Path}]";

    }

    public enum CombineMode
    {
        Union,
        Join,
        Total,
    }

    public class Column
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public bool Key { get; set; }

        public CombineMode Combine { get; set; }

        public string Separator { get; set; }

        public override string ToString() => Name;
    }

    public class MergerViewModel : INotifyPropertyChanged
    {

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties

        public const string FileExtension = ".csv";

        public StringComparer Comparer { get; set; } = StringComparer.InvariantCultureIgnoreCase;

        public ObservableCollection<FileEntry> Entries { get; set; } = new ObservableCollection<FileEntry>();

        public ObservableCollection<Column> Columns { get; set; } = new ObservableCollection<Column>();

        public string[] ColumnNames => Columns.Select(c => c.Name).ToArray();

        FileEntry selectedItem;
        public FileEntry SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value)
                    return;

                selectedItem = value;
                OnPropertyChanged();
                UpdateHeaders();
            }
        }

        private string _selectedColumns;
        public string SelectedColumns
        {
            get => _selectedColumns;
            set
            {
                if (_selectedColumns == value)
                    return;

                _selectedColumns = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand MergeCommand { get; set; }

        public RelayCommand ConsolidateCommand { get; set; }

        public Column SourceColumn { get; } = new Column() { Name = "(Source)", Combine = CombineMode.Union };

        #endregion

        #region Events

        public void OnDragDrop(object sender, DragEventArgs e)
        {
            var formats = e.Data.GetFormats();
            var format = formats.Where(f => f == "FileDrop" || f == "FileNameW").FirstOrDefault();

            if (string.IsNullOrEmpty(format))
                return;

            var files = (e.Data.GetData(format) as string[]) ?? new string[] { };
            AddPaths(files);
        }

        public void OnDropDown(object sender, EventArgs e)
        {
            UpdateHeaders();
        }

        public void AddPaths(params string[] files)
        {
            List<FileEntry> all =
                    files.SelectMany(f => Directory.Exists(f) ?
                            Directory.EnumerateFiles(f, $"*{FileExtension}", System.IO.SearchOption.AllDirectories) :
                            new string[] { f })
                            .Where(file => !Entries.Any(f => f.Path == file))
                            .Select(f => new FileEntry { Path = f, Name = Path.GetFileNameWithoutExtension(f), Rows = Lines(f) })
                            .Where(r => r.Rows > 0)
                            .ToList();

            all.ForEach(Entries.Add);
            UpdateHeaders();
            MergeCommand.CanExecuteChange();
            ConsolidateCommand.CanExecuteChange();
        }

        #endregion

        #region Constructor

        public MergerViewModel()
        {
            this.MergeCommand = new RelayCommand(Merge, CanMerge);
            this.ConsolidateCommand = new RelayCommand(Consolidate, CanMerge);
            this.SelectedColumns = "Drag and drop a folder/file onto the window";
        }
        #endregion

        #region Merge

        protected int Lines(string f)
        {
            try { return File.ReadAllLines(f).Length - 1; }
            catch (Exception e)
            {
                if (MessageBox.Show($"Could not load file: {e.Message}\n\nRetry?", "Fail", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    return Lines(f);
            }

            return 0;
        }

        string[] Strings_Total = new string[] { "sum", "quantity", "qty", "total" };
        string[] Strings_Combine = new string[] { "designator", "reference", "ref", "serial", "source" };

        protected void UpdateHeaders()
        {
            var entry = SelectedItem ?? Entries.LastOrDefault();
            if (entry == null)
                return;

            try
            {
                using (var reader = GetReader(entry))
                {
                    Columns.Clear();

                    var currentHeaders = reader.ReadFields();


                    currentHeaders //.Except(Headers, StringComparer.InvariantCultureIgnoreCase)
                    .Select((c, i) => new Column
                    {
                        Index = i + 1,
                        Name = c,
                        Key = i == 0,
                        Combine =
                            Strings_Total.Contains(c, Comparer) ? CombineMode.Total :
                            Strings_Combine.Contains(c, Comparer) ? CombineMode.Join :
                            CombineMode.Union
                    })
                    .StartWith(SourceColumn)
                    .ToList()
                    .ForEach(Columns.Add);
                }

                SelectedColumns = string.Join(", ", Columns.Skip(1));

            }
            catch
            { }
        }


        public bool CanMerge()
        {
            return Entries.Count > 0 && Columns.Count > 0;
        }

        public void Merge()
        {
            Save("Merged", MergeFiles(Entries));
        }

        public void Consolidate()
        {
            var keys = Columns.Where(c => c.Key).Select(c => c.Index).ToArray();
            var range = Enumerable.Range(0, Columns.Count);
            var groups = MergeFiles(Entries).Skip(1).GroupBy(row => string.Join("\n", keys.Select(i => row[i])), Comparer);
            var lines = groups.Select(group =>
                range.Select(i => Combine(group.Select(row => row[i]), Columns[i])).ToArray()
            );

            Save("Consolidated", lines.StartWith(ColumnNames));
        }

        string Combine(IEnumerable<string> values, Column column)
        {
            switch (column.Combine)
            {

                case CombineMode.Join:
                    return string.Join(column.Separator ?? "; ", values);

                case CombineMode.Total:
                    return values.Select(v => int.TryParse(v, out int result) ? result : 0).Sum().ToString();

                case CombineMode.Union:
                default:
                    return string.Join(column.Separator ?? ", ", values.Distinct(Comparer));
            }
        }

        private void Save(string defaultFilename, IEnumerable<string[]> content)
        {
            var dialog = new SaveFileDialog()
            {
                FileName = defaultFilename,
                DefaultExt = FileExtension,
                Filter = $"CSV Files(*{FileExtension})|*{FileExtension}"
            };

            if (!dialog.ShowDialog().GetValueOrDefault())
                return;

            var lines = content.Select(arr => string.Join(",", arr.Select(s => $"\"{s}\"")));

            try
            {
                File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public IEnumerable<string[]> MergeFiles(IEnumerable<FileEntry> files)
        {
            if (Columns.Count == 0)
                yield break;

            yield return ColumnNames;

            var headers = ColumnNames.Skip(1).ToArray();
            var headerLength = headers.Length;

            foreach (var entry in files)
            {
                TextFieldParser reader = GetReader(entry);

                try
                {
                    string[] currentHeaders = null;

                    try
                    {
                        currentHeaders = reader.ReadFields();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Unable to read file: {e.Message}");
                    }

                    if (currentHeaders == null || currentHeaders.Length == 0)
                    {
                        MessageBox.Show($"Could not find any usable headers in:\n{entry}");
                        continue;
                    }

                    var lookup = currentHeaders.ToLookup(c => c).Where(c => c.Count() > 1);

                    if (lookup.Any(l => string.IsNullOrWhiteSpace(l.Key)))
                    {
                        MessageBox.Show($"Warning: Empty column in {entry.Name}, skipping");
                        continue;
                    }

                    if (lookup.Any())
                    {
                        MessageBox.Show($"Duplicate headers in {entry.Name}: " + string.Join("\n", lookup.Select(c => $"'{c.Key}'")));
                        continue;
                    }

                    var columnOrder = currentHeaders.Select(Tuple.Create<string, int>).ToDictionary(t => t.Item1, t => t.Item2, Comparer);
                    var mapping = headers.Select(h => columnOrder.ContainsKey(h) ? columnOrder[h] : -1).ToArray();

                    while (!reader.EndOfData)
                    {
                        var fields = reader.ReadFields();
                        var ordered =
                            Enumerable.Range(1, headerLength)
                                      .Select(i => mapping[i - 1])
                                      .Select(i => i >= 0 && i < fields.Length ? fields[i]?.Replace("\"", "\"\"") : String.Empty)
                                      .StartWith(entry.Name);

                        yield return ordered.ToArray();
                    }

                }
                finally
                {
                    reader.Dispose();
                }

            }

        }


        static TextFieldParser GetReader(FileEntry file)
        {
            return new TextFieldParser(file.Path) { Delimiters = new string[] { "," }, TextFieldType = FieldType.Delimited, TrimWhiteSpace = true };
        }

        #endregion
    }
}
