using System;
using System.Linq;
using System.Windows.Input;

namespace csv_merge
{
    public class RelayCommand : ICommand
    {
        protected Func<bool> CanExecuteFunc { get; }

        protected Action ExecuteFunc { get; }

        public RelayCommand(Action executeFunc, Func<bool> canExecuteFunc)
        {
            CanExecuteFunc = canExecuteFunc;
            ExecuteFunc = executeFunc;
        }

        public bool CanExecute(object parameter) => CanExecuteFunc();

        public void Execute(object parameter) => ExecuteFunc();

        public event EventHandler CanExecuteChanged;
        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(sender, e);
        }

        public void CanExecuteChange()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }


    }
}
