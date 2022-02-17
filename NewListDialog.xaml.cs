using System.Windows;

namespace ToDoLi
{

    public partial class NewListDialog : Window
    {
        // Contains the new/current name of the list.
        private string _listName;

        /// <summary>
        /// Provides a new instance of the NewListDialog Window.
        /// </summary>
        public NewListDialog()
        {
            InitializeComponent();
            textBox.Focus();
        }

        /// <summary>
        /// Gets or sets the ListName. 
        /// </summary>
        public string ListName
        {
            get => _listName;
            set
            {
                _listName = value;
                textBox.Text = _listName;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == string.Empty)
            {
                MessageBox.Show("Sie haben keinen Namen festgelegt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Focus();
                return;
            }

            ListName = textBox.Text;
            DialogResult = true;
            this.Close();
        }
    }
}
