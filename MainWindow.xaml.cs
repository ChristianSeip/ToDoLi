using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToDoLi
{
    public partial class MainWindow : Window
    {

        // Contains the current instance of ToDoList
        private ToDoList _todo = new ToDoList();

        // Contains instance of Config
        private Config _config;

        public MainWindow()
        {
            InitializeComponent();

            _config = new Config();
            this.Height = _config.WindowHeight;
            this.Width = _config.WindowWidth;
            this.Left = _config.WindowPosX;
            this.Top = _config.WindowPosY;
        }

        // Calls the NewListDialog and checks the inputs for validity to create a new todo list.
        private void AddList()
        {
            if (!SaveChanges())
            {
                return;
            }

            NewListDialog newListDialog = new NewListDialog();
            newListDialog.Owner = this;
            newListDialog.ShowDialog();

            if (newListDialog.DialogResult == true)
            {
                if (newListDialog.ListName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
                {
                    MessageBox.Show("Der gewählte Name enthält leider ungültige Zeichen. Bitte wählen Sie einen anderen Namen.", "Ungültiger Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.WriteEntry($"Liste kann nicht angelegt werden. Ungültige Zeichen in \"{newListDialog.ListName}\".");
                    AddList();
                    return;
                }

                if (_todo.Exists(newListDialog.ListName))
                {
                    MessageBox.Show($"Es existiert bereits eine Liste mit dem Namen {newListDialog.ListName}. Bitte wählen Sie einen anderen Namen.", "Liste existiert bereits", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.WriteEntry($"Liste kann nicht angelegt werden. Es existiert bereits eine Liste mit dem Namen \"{newListDialog.ListName}\".");
                    AddList();
                    return;
                }

                ToDoList todo = new ToDoList(newListDialog.ListName);
                todo.Save();

                LoadLists();
                listBoxLists.SelectedItem = todo.Name;
            }
        }

        // Calls the TaskEditor dialog and checks the inputs for validity to add a new task to the current todo list.
        private void AddTask()
        {
            if (listBoxLists.SelectedIndex == -1)
            {
                return;
            }

            TaskEditor taskEditor = new TaskEditor();
            taskEditor.Owner = this;
            taskEditor.ShowDialog();

            if (taskEditor.DialogResult == true)
            {
                if (!_todo.AddTask(taskEditor.TaskTitle, taskEditor.TaskDescription))
                {
                    MessageBox.Show("Beim Hinzufügen der Aufgabe ist ein unerwarteter Fehler aufgetreten. Bitte versuche es erneut.", "Unerwarteter Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadTasks();
            }
        }

        // Deletes the selected task from the current todo list
        private void DeleteTask()
        {
            if (listBoxTasks.SelectedIndex == -1)
            {
                return;
            }

            _todo.RemoveTask(listBoxTasks.SelectedIndex);
            LoadTasks();
        }

        // Calls the TaskEditor dialog and checks the inputs for validity to edit the selected task.
        private void EditTask()
        {
            if (listBoxLists.SelectedIndex == -1)
            {
                return;
            }

            string currentTitle = listBoxTasks.SelectedItem.ToString();
            string currentDescription = _todo.TaskList.ElementAt(listBoxTasks.SelectedIndex).Description.ToString();

            Logger.WriteEntry($"Rufe Editor für Aufgabe \"{currentTitle}\" auf.");
            TaskEditor taskEditor = new TaskEditor(currentTitle, currentDescription);
            taskEditor.Owner = this;
            taskEditor.ShowDialog();

            if (taskEditor.DialogResult == true)
            {
                Logger.WriteEntry($"Ändere Aufgabe \"{currentTitle}\" Beschreibung: \"{currentDescription}\" in \"{taskEditor.TaskTitle}\" Beschreibung: \"{taskEditor.TaskDescription}\".");
                _todo.AddTask(taskEditor.TaskTitle, taskEditor.TaskDescription);
                _todo.TaskList.RemoveAt(listBoxTasks.SelectedIndex);
                LoadTasks();
            }
        }

        // Loads the list of todo lists.
        private void LoadLists()
        {

            Logger.WriteEntry("Lade ToDo-Listen.");

            listBoxLists.Items.Clear();

            string[] files = _todo.GetListNames();

            foreach (string file in files)
            {
                listBoxLists.Items.Add(file);
            }
        }

        // Loads the task list for the selected todo list.
        private void LoadTasks()
        {
            Logger.WriteEntry($"Lade Aufgaben für Liste \"{_todo.Name}\"");
            menuEditTask.IsEnabled = false;
            menuDeleteTask.IsEnabled = false;

            listBoxTasks.Items.Clear();

            foreach (var task in _todo.TaskList)
            {
                listBoxTasks.Items.Add(((ToDoList.Task)task).Title);
            }
        }

        // If the list has been changed, the user will be asked if he wants to the changes.
        private bool SaveChanges()
        {
            if (_todo.IsUnsaved)
            {
                var msgBoxResult = MessageBox.Show($"Es gibt ungespeicherte Änderungen an der Liste {_todo.Name}. Möchten Sie die Änderungen speichern?", "Änderungen speichern?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (msgBoxResult == MessageBoxResult.Yes)
                {
                    _todo.Save();
                    return true;
                }
                else if (msgBoxResult == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        private void LabelAddList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AddList();
        }

        private void LabelAddTask_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AddTask();
        }

        private void LabelRemoveList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (listBoxLists.SelectedIndex == -1)
            {
                return;
            }

            _todo.Delete();
            LoadLists();
            listBoxLists.SelectedIndex = -1;
        }

        private void LabelRemoveTask_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DeleteTask();
        }

        private void ListBoxLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            menuRenameList.IsEnabled = listBoxLists.SelectedIndex > -1;
            menuDeleteList.IsEnabled = listBoxLists.SelectedIndex > -1;
            menuAddTask.IsEnabled = listBoxLists.SelectedIndex > -1;

            if (listBoxLists.SelectedIndex == -1 || listBoxLists.SelectedItem.ToString() == _todo.Name)
            {
                return;
            }

            if (!SaveChanges())
            {
                listBoxLists.SelectedItem = _todo.Name;
                return;
            }

            if (listBoxLists.SelectedIndex < _todo.GetListNames().Count())
            {
                _todo = new ToDoList(listBoxLists.SelectedItem.ToString());
                LoadTasks();
            }
            else
            {
                AddList();
            }
        }

        private void ListBoxTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxTasks.SelectedIndex != -1)
            {
                menuEditTask.IsEnabled = true;
                menuDeleteTask.IsEnabled = true;
                textBlock.Text = _todo.TaskList.ElementAt(listBoxTasks.SelectedIndex).Description.ToString();
            }
        }

        private void MenuAddList_Click(object sender, RoutedEventArgs e)
        {
            AddList();
        }

        private void MenuAddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTask();
        }

        private void MenuClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuDeleteList_Click(object sender, RoutedEventArgs e)
        {
            _todo.Delete();
            LoadLists();
            listBoxLists.SelectedIndex = -1;
        }

        private void MenuDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            DeleteTask();
        }

        private void MenuEditTask_Click(object sender, RoutedEventArgs e)
        {
            EditTask();
        }

        private void MenuRenameList_Click(object sender, RoutedEventArgs e)
        {
            NewListDialog newListDialog = new NewListDialog();
            newListDialog.Owner = this;
            newListDialog.ShowDialog();

            if (newListDialog.DialogResult == true)
            {
                if (newListDialog.ListName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
                {
                    MessageBox.Show("Der gewählte Name enthält leider ungültige Zeichen. Bitte wählen Sie einen anderen Namen.", "Ungültiger Name", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.WriteEntry($"Liste kann nicht geändert werden. Ungültige Zeichen in \"{newListDialog.ListName}\".");
                    AddList();
                    return;
                }

                if (_todo.Exists(newListDialog.ListName))
                {
                    MessageBox.Show($"Es existiert bereits eine Liste mit dem Namen {newListDialog.ListName}. Bitte wählen Sie einen anderen Namen.", "Liste existiert bereits", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.WriteEntry($"Liste kann nicht angelegt werden. Eine Liste mit dem Namen  \"{newListDialog.ListName}\" existiert bereits.");
                    AddList();
                    return;
                }

                _todo.Rename(newListDialog.ListName);

                LoadLists();
                listBoxLists.SelectedItem = _todo.Name;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _config.WindowHeight = this.Height;
            _config.WindowWidth = this.Width;
            _config.WindowPosX = this.Left;
            _config.WindowPosY = this.Top;
            _config.Save();

            if (!SaveChanges())
            {
                Logger.WriteEntry("Beende Application.");
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLists();
            Logger.RemoveOldLogs();
            Logger.WriteEntry("Application geladen.");
        }

    }
}