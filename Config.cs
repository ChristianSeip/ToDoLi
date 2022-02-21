using Microsoft.Win32;
using System;

namespace ToDoLi
{
    internal class Config
    {

        private RegistryKey _registryKey;

        // Contains the value for MainWindow.Height
        private double _windowHeight = 450;

        // Contains the value for MainWindow.Width
        private double _windowWidth = 800;

        // Contains the value for MainWindow.Left
        private double _windowPosX = 0.0;

        // Contains the value for MainWindow.Top
        private double _windowPosY = 0.0;

        // Contains the language key for localisation
        private string _language = "en-GB";

        public Config()
        {
            _registryKey = Registry.CurrentUser.OpenSubKey("Software", true);
            _registryKey.CreateSubKey("ToDoLi");
            _registryKey.OpenSubKey("ToDoLi");
            Load();
        }

        /// <summary>
        /// Gets or sets the value of the WindowHeight property.
        /// </summary>
        /// <remarks>
        /// Minimum value: 300
        /// </remarks>
        public double WindowHeight
        {
            get => _windowHeight;
            set
            {
                if (value >= 300)
                {
                    _windowWidth = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the WindowWidth property.
        /// </summary>
        /// <remarks>
        /// Minimum value: 500
        /// </remarks>
        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (value >= 500)
                {
                    _windowWidth = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the WindowPositionX property.
        /// </summary>
        public double WindowPosX { get => _windowPosX; set => _windowPosX = value; }

        /// <summary>
        /// Gets or sets the value of the WindowPositionY property.
        /// </summary>
        public double WindowPosY { get => _windowPosY; set => _windowPosY = value; }

        /// <summary>
        /// Gets or sets the value of the Language property.
        /// </summary>
        public string Language { get => _language; set => _language = value; }

        /// <summary>
        /// Saves the current configuration.
        /// </summary>
        public void Save()
        {
            try
            {
                _registryKey.SetValue("width", WindowWidth);
                _registryKey.SetValue("height", WindowHeight);
                _registryKey.SetValue("posX", WindowPosX);
                _registryKey.SetValue("posY", WindowPosY);
                _registryKey.SetValue("language", Language);
            }
            catch (Exception ex)
            {
                Logger.WriteEntry($"Cannot save app config to registry. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the configuration from the saved file i.e config.xml.
        /// </summary>
        public void Load()
        {
            try
            {
                if (_registryKey.GetValue("width") == null)
                {
                    _registryKey.SetValue("width", WindowWidth);
                }

                if (_registryKey.GetValue("height") == null)
                {
                    _registryKey.SetValue("height", WindowHeight);
                }

                if (_registryKey.GetValue("posX") == null)
                {
                    _registryKey.SetValue("posX", (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - (WindowWidth / 2));
                }

                if (_registryKey.GetValue("posY") == null)
                {
                    _registryKey.SetValue("posY", (System.Windows.SystemParameters.PrimaryScreenHeight / 2) - (WindowHeight / 2));
                }

                if (_registryKey.GetValue("language") == null)
                {
                    _registryKey.SetValue("language", "en-GB");
                }

                WindowHeight = Convert.ToDouble(_registryKey.GetValue("height").ToString());
                WindowWidth = Convert.ToDouble(_registryKey.GetValue("width").ToString());
                WindowPosX = Convert.ToDouble(_registryKey.GetValue("posX").ToString());
                WindowPosY = Convert.ToDouble(_registryKey.GetValue("posY").ToString());
                Language = _registryKey.GetValue("language").ToString();
            }
            catch (Exception ex)
            {
                Logger.WriteEntry($"Cannot load app config from registry. Error: {ex.Message}");
            }
        }
    }
}
