using System;
using System.Collections.Generic;
using System.Xml;

namespace ToDoLi
{
    internal class ToDoList
    {

        // Contains alphanumeric characters.
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Contains AppData directory path.
        private string _directory;

        // Contains the status whether the current ToDo List is saved or not.
        private bool _isUnsaved = false;

        // Contains current list name.
        private string _name;

        /// <summary>
        /// Provides the structure for tasks.
        /// </summary>
        public struct Task
        {
            /// <summary>
            /// Gets or sets the title of the task.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the description of the task.
            /// </summary>
            public string Description { get; set; }
        }

        // Contains all tasks of the current ToDo List.
        private List<Task> _taskList = new List<Task>();

        /// <summary>
        /// Provides a new instance of the ToDoList.
        /// </summary>
        /// <param name="name"></param>
        public ToDoList(string name)
        {
            _directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ToDoLi");

#if DEBUG
            _directory = System.IO.Path.Combine(_directory, "debug");
#endif

            System.IO.Directory.CreateDirectory(_directory);

            if (name == String.Empty)
            {
                SetRandomListName();
            }
            else
            {
                Name = name;
            }

            Logger.WriteEntry($"Create new ToDoList Instance for list {Name}");

        }

        /// <summary>
        /// Provides a new instance of the ToDoList.
        /// </summary>
        public ToDoList() : this(String.Empty) { }

        /// <summary>
        /// Adds a task at the end of the task list.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns><c><see langword="true"/></c> if successful, otherwise <c><see langword="false"/></c>.</returns>
        public bool AddTask(string title, string description)
        {
            if (title == string.Empty)
            {
                Logger.WriteEntry($"Cannot add task \"{title}\" with description \"{description}\"");
                return false;
            }

            Task task = new Task();
            task.Title = title;
            task.Description = description;

            TaskList.Add(task);

            IsUnsaved = true;

            Logger.WriteEntry($"Task \"{title}\" with description \"{description}\" added.");
            return true;
        }

        /// <summary>
        /// Adds a task at the end of the task list.
        /// </summary>
        /// <param name="title"></param>
        /// <returns><c><see langword="true"/></c> if successful, otherwise <c><see langword="false"/></c>.</returns>
        public bool AddTask(string title)
        {
            return AddTask(title, "-/-");
        }

        /// <summary>
        /// Deletes the current ToDo List completly.
        /// </summary>
        public void Delete()
        {
            TaskList.Clear();

            if (Exists(_name))
            {
                System.IO.File.Delete(GetFullFilePath());
            }

            Logger.WriteEntry($"DeletedToDo List \"{Name}\".");

            SetRandomListName();
        }

        /// <summary>
        /// Determines if the specified list exists or not.
        /// </summary>
        /// <param name="listName">The name of the list</param>
        /// <returns><c><see langword="true"/></c> if the list exists, otherwise <c><see langword="false"/></c>.</returns>
        public bool Exists(string listName)
        {
            return System.IO.File.Exists(System.IO.Path.ChangeExtension(System.IO.Path.Combine(_directory, listName), "xml"));
        }

        // Returns the full file path of the current ToDo List.
        private string GetFullFilePath()
        {
            return System.IO.Path.ChangeExtension(System.IO.Path.Combine(_directory, _name), "xml");
        }

        /// <summary>
        /// Returns an array of the names of all existing lists.
        /// </summary>
        /// <returns>An array of the names of all existing lists (without file path and extension).</returns>
        public string[] GetListNames()
        {
            string[] files = System.IO.Directory.GetFiles(_directory);

            for (int i = 0; i < files.Length; i++)
            {
                if (System.IO.Path.GetExtension(files[i]) == ".xml")
                {
                    files[i] = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                }
            }

            return files;
        }

        /// <summary>
        /// Gets or sets the current status for the ToDo List. 
        /// </summary>
        public bool IsUnsaved { get => _isUnsaved; private set => _isUnsaved = value; }

        // Reads the tasks for the current ToDoList from the list.xml file and saves them in the TaskList.
        private void Load()
        {
            if (!Exists(_name))
            {
                Logger.WriteEntry($"Cannot read tasks from toto list \"{Name}\". (Does not exist)");
                return;
            }

            XmlReader xmlReader = XmlReader.Create(GetFullFilePath());
            xmlReader.ReadStartElement("tasks");

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "task")
                {
                    AddTask(xmlReader.GetAttribute("title"), xmlReader.GetAttribute("description"));
                }
            }

            xmlReader.Close();
            IsUnsaved = false;
        }

        /// <summary>
        /// Gets or sets the name for the current ToDo List.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                Logger.WriteEntry($"Set ToDo List Name to \"{Name}\".");
                _name = value;

                if (Exists(value))
                {
                    TaskList.Clear();
                    Load();
                }
            }
        }

        /// <summary>
        /// Removes the task at the specified index from the TaskList.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveTask(int index)
        {
            if (TaskList.Count - 1 >= index)
            {
                Logger.WriteEntry($"Task  \"{_taskList[index].Title}\" deleted.");
                TaskList.RemoveAt(index);
                IsUnsaved = true;
            }
        }

        /// <summary>
        /// Renames the list to the desired name if there is no list with that name already.
        /// </summary>
        /// <param name="newName"></param>
        /// <returns><c><see langword="true"/></c> if the renaming was successful, otherwise <c><see langword="false"/></c>.</returns>
        public bool Rename(string newName)
        {
            if (Exists(newName))
            {
                Logger.WriteEntry($"Cannot rename List \"{Name}\" to \"{newName}\".\"{newName}\" already exists.");
                return false;
            }

            ToDoList old = new ToDoList(Name);

            _name = newName;
            Save();
            old.Delete();

            return true;
        }

        /// <summary>
        /// Saves the entries from the TaskList as an xml file.
        /// </summary>
        async public void Save()
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Async = true;
                settings.Indent = true;
                settings.IndentChars = "\t";

                XmlWriter xmlWriter = XmlWriter.Create(GetFullFilePath(), settings);
                await xmlWriter.WriteStartDocumentAsync();
                await xmlWriter.WriteStartElementAsync(null, "tasks", null);

                foreach (var task in TaskList)
                {
                    await xmlWriter.WriteStartElementAsync(null, "task", null);
                    await xmlWriter.WriteAttributeStringAsync(null, "title", null, task.Title);
                    await xmlWriter.WriteAttributeStringAsync(null, "description", null, task.Description);
                    await xmlWriter.WriteEndElementAsync();
                }

                await xmlWriter.WriteEndElementAsync();
                await xmlWriter.WriteEndDocumentAsync();
                xmlWriter.Close();

                IsUnsaved = false;
            }
            catch (Exception ex)
            {
                Logger.WriteEntry($"Cannot save Todo List \"{Name}\". Exception: {ex.Message}");
            }
        }

        // Sets a random name for the current ToDo List.
        private void SetRandomListName()
        {
            string name = DateTime.Now.Ticks.ToString();
            bool searching = true;

            Random random = new Random();

            do
            {
                if (Exists(name))
                {
                    name += chars[random.Next(chars.Length)].ToString();
                }
                else
                {
                    searching = false;
                }

            } while (searching);

            Name = name;
        }

        /// <summary>
        /// Gets a list of tasks for the current ToDo List
        /// </summary>
        internal List<Task> TaskList { get => _taskList; }

    }
}