using System.Windows;

namespace ToDoLi
{
    public partial class TaskEditor : Window
    {
        // Contains the task description.
        private string _taskDescription;

        // Contains the task title.
        private string _taskTitle;

        /// <summary>
        /// Provides a new instance of the TaskEditor Window.
        /// </summary>
        public TaskEditor() : this(string.Empty, string.Empty) { }

        /// <summary>
        /// Provides a new instance of the TaskEditor Window with prefilled textboxes. 
        /// </summary>
        /// <param name="taskTitle"></param>
        /// <param name="taskDescription"></param>
        public TaskEditor(string taskTitle, string taskDescription)
        {
            InitializeComponent();
            textBoxTitle.Text = taskTitle;
            textBoxDescription.Text = taskDescription;
        }

        /// <summary>
        /// Gets the task description. 
        /// </summary>
        public string TaskDescription { get => _taskDescription; private set => _taskDescription = value; }

        /// <summary>
        /// Gets the task title.
        /// </summary>
        public string TaskTitle { get => _taskTitle; private set => _taskTitle = value; }

        // Close window on cancel.
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        //  Checks the form and closes the window OR displays an error message, if something is invalid.
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {

            if (textBoxTitle.Text == string.Empty)
            {
                MessageBox.Show("Sie haben keinen Namen für die Aufgabe festgelegt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                textBoxTitle.Focus();
                return;
            }

            TaskTitle = textBoxTitle.Text;
            TaskDescription = textBoxDescription.Text == string.Empty ? "-/-" : textBoxDescription.Text;

            this.DialogResult = true;
            this.Close();
        }
    }
}
