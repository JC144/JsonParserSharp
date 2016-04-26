using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using JSonParser.WPF.ViewModels;
using JSonParser.WPF.CustomControls;
using JSonParser.SDK;

namespace JSonParser.WPF
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        #region [ Properties ]

        private static object lockMessagesViewModel = new object();
        private static MessagesViewModel _messagesViewModel;
        public static MessagesViewModel MessagesViewModel
        {
            get
            {
                lock (lockMessagesViewModel)
                {
                    if (_messagesViewModel == null)
                        _messagesViewModel = new MessagesViewModel();
                }
                return _messagesViewModel;
            }
        }

        private List<CSharpFile> _generatedClasses;
        public List<CSharpFile> GeneratedClasses
        {
            get
            {
                return _generatedClasses;
            }
            set
            {
                _generatedClasses = value;
                if (_generatedClasses != null && _generatedClasses.Any())
                    SelectedClass = _generatedClasses[0];
                RaisePropertyChanged("GeneratedClasses");
                RaisePropertyChanged("IsSaveEnable");
            }
        }

        private CSharpFile _selectedClass;
        public CSharpFile SelectedClass
        {
            get
            {
                return _selectedClass;
            }
            set
            {
                _selectedClass = value;
                RaisePropertyChanged("SelectedClass");
                RaisePropertyChanged("IsCopyEnable");
            }
        }

        private String _json;
        public String Json
        {
            get
            {
                return _json;
            }
            set
            {
                _json = value;
                RaisePropertyChanged("Json");
                RaisePropertyChanged("IsParsingAvailable");
            }
        }

        public bool IsSaveEnable
        {
            get
            {
                return GeneratedClasses != null && GeneratedClasses.Any();
            }
        }

        public bool IsCopyEnable
        {
            get
            {
                return SelectedClass !=null && !String.IsNullOrEmpty(SelectedClass.Content);
            }
        }

        public bool IsParsingAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(Json);
            }
        }

        #endregion [ Properties ]

        #region [ Constructor ]

        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        #endregion [ Constructor ]


        #region [ Interface Events ]

        private void Parse_Clicked(object sender, RoutedEventArgs e)
        {
            Parse();
        }

        private void Open_Clicked(object sender, RoutedEventArgs e)
        {
            string json = OpenJson();
            if (!String.IsNullOrEmpty(json))
            {
                Json = json;
            }
        }

        private void Paste_Clicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Clipboard.GetText()))
            {
                Json = Clipboard.GetText();
            }
        }

        private void Save_Clicked(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                if (!String.IsNullOrEmpty(folderPath))
                {
                    if (GeneratedClasses != null && GeneratedClasses.Any())
                    {
                        foreach (CSharpFile file in GeneratedClasses)
                        {
                            File.WriteAllText(Path.Combine(folderPath, file.Name), file.Content);
                        }
                    }
                    MessagesViewModel.AddMessage(new MessageModel("All files have been saved.", ToastType.Infos));
                }
            }
        }

        private void Copy_Clicked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TxtBox_CSharp.Text))
            {
                //Just some feedback for the user
                Dispatcher.Invoke(() =>
                {
                    TxtBox_CSharp.Focus();
                    TxtBox_CSharp.SelectAll();
                });

                if (SelectedClass != null && !String.IsNullOrEmpty(SelectedClass.Content))
                {
                    Clipboard.SetText(SelectedClass.Content);
                    MessagesViewModel.AddMessage(new MessageModel("Your class has been added to the clipboard.\nPress Ctrl+V to paste.", ToastType.Infos));
                }
            }
        }

        #endregion [ Interface Events ]


        #region [ Functions ]

        private string OpenJson()
        {
            string json = String.Empty;

            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "json files (*.json;*.txt)|*.json;*.txt";
            openFileDialog1.FilterIndex = 1;

            bool? fileSelected = openFileDialog1.ShowDialog();

            if (fileSelected.HasValue && fileSelected.Value)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            using (StreamReader sr = new StreamReader(myStream))
                            {
                                // Read the stream to a string, and write the string to the console.
                                json = sr.ReadToEnd();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessagesViewModel.AddMessage(new MessageModel("Error: Could not read file from disk. Original error: " + ex.Message, ToastType.Error));
                }
            }

            return json;
        }

        private void Parse()
        {
            if (!String.IsNullOrEmpty(Json))
            {
                string namespaceName = !String.IsNullOrEmpty(Txtbox_Namespace.Text) ? Txtbox_Namespace.Text : "EmptyNamespace";
                string className = !String.IsNullOrEmpty(Txtbox_Classname.Text) ? Txtbox_Classname.Text : "EmptyClassName";
                JsonObject jsonObject = null;

                try
                {
                    jsonObject = JsonParser.JsonDecode(Json) as JsonObject;
                }
                catch (Exception ex)
                {
                    MessagesViewModel.AddMessage(new MessageModel("Error: Cannot parse file: " + ex.Message, ToastType.Error));
                }

                if (jsonObject != null)
                {
                    try
                    {
                        List<CSharpFile> files = ModelCreator.GetCSharpFiles(jsonObject, namespaceName, className);
                        if (files != null && files.Any())
                        {
                            GeneratedClasses = files;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessagesViewModel.AddMessage(new MessageModel("Error: Cannot generate C# classes: " + ex.Message, ToastType.Error));
                    }
                }
            }
        }

        #endregion [ Functions ]


        #region [ RaisePropertyChanged ]

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion [ RaisePropertyChanged ]
    }
}
