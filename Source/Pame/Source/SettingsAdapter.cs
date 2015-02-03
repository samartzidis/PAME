//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace Pame
{    
    public class SettingsAdapter
    {   

        [LocalizedCategoryAttribute("SettingsAdapter_String33"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String34"),
            LocalizedDescriptionAttribute("")
        ]
        public System.ConsoleColor ConsoleBackgroundColor
        {
            get { return Properties.Settings.Default.ConsoleBackgroundColor; }
            set { Properties.Settings.Default.ConsoleBackgroundColor = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String33"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String35"),
            LocalizedDescriptionAttribute("")
        ]
        public System.ConsoleColor ConsoleForegroundColor
        {
            get { return Properties.Settings.Default.ConsoleForegroundColor; }
            set { Properties.Settings.Default.ConsoleForegroundColor = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String33"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String36"),
            LocalizedDescriptionAttribute("SettingsAdapter_String37")
        ]
        public int ConsoleCodepage
        {
            get { return Properties.Settings.Default.ConsoleCodepage; }
            set { Properties.Settings.Default.ConsoleCodepage = value; }
        }
        
        [LocalizedCategoryAttribute("SettingsAdapter_String39"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String38"),
            LocalizedDescriptionAttribute("")
        ]
        public bool TopmostDebugConsole
        {
            get { return Properties.Settings.Default.TopmostDebugConsole; }
            set { Properties.Settings.Default.TopmostDebugConsole = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String39"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String41"),
            LocalizedDescriptionAttribute("")
        ]
        public bool WatchVariables
        {
            get { return Properties.Settings.Default.WatchVariables; }
            set { Properties.Settings.Default.WatchVariables = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String15"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String16"),
            LocalizedDescriptionAttribute("")          
        ]
        public bool ShowSplashScreen
        {
            get { return Properties.Settings.Default.ShowSplashScreen; }
            set { Properties.Settings.Default.ShowSplashScreen = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String15"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String17"),
            LocalizedDescriptionAttribute("SettingsAdapter_String40"),
            TypeConverter(typeof(LanguagesConverter))
        ]
        public string Language
        {
            get
            {
                return Properties.Settings.Default.Language;
            }
            set
            {
                Properties.Settings.Default.Language = value;
            }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
           LocalizedDisplayNameAttribute("SettingsAdapter_String19"),
           LocalizedDescriptionAttribute("")]
        public bool ShowSpaces
        {
            get { return Properties.Settings.Default.ShowSpaces; }
            set { Properties.Settings.Default.ShowSpaces = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
           LocalizedDisplayNameAttribute("SettingsAdapter_String20"),
           LocalizedDescriptionAttribute("")]
        public bool ShowLineNumbers
        {
            get { return Properties.Settings.Default.ShowLineNumbers; }
            set { Properties.Settings.Default.ShowLineNumbers = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
          LocalizedDisplayNameAttribute("SettingsAdapter_String21"),
          LocalizedDescriptionAttribute("")]
        public bool ShowNewlines
        {
            get { return Properties.Settings.Default.ShowNewlines; }
            set { Properties.Settings.Default.ShowNewlines = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
          LocalizedDisplayNameAttribute("SettingsAdapter_String22"),
          LocalizedDescriptionAttribute("")]
        public bool HighlightCurrentRow
        {
            get { return Properties.Settings.Default.HighlightCurrentRow; }
            set { Properties.Settings.Default.HighlightCurrentRow = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
          LocalizedDisplayNameAttribute("SettingsAdapter_String23"),
          LocalizedDescriptionAttribute("")]
        public bool HighlightMatchingBrackets
        {
            get { return Properties.Settings.Default.HighlightMatchingBrackets; }
            set { Properties.Settings.Default.HighlightMatchingBrackets = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
          LocalizedDisplayNameAttribute("SettingsAdapter_String24"),
          LocalizedDescriptionAttribute("")]
        public int TabSize
        {
            get { return Properties.Settings.Default.TabSize; }
            set { Properties.Settings.Default.TabSize = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
          LocalizedDisplayNameAttribute("SettingsAdapter_String25"),
          LocalizedDescriptionAttribute("")]
        public System.Drawing.Font Font
        {
            get { return Properties.Settings.Default.Font; }
            set { Properties.Settings.Default.Font = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String18"),
          LocalizedDisplayNameAttribute("SettingsAdapter_String42"),
          LocalizedDescriptionAttribute("")]
        public bool CodeFolding
        {
            get { return Properties.Settings.Default.CodeFolding; }
            set { Properties.Settings.Default.CodeFolding = value; }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String26"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String27"),
            LocalizedDescriptionAttribute("SettingsAdapter_String28")]
        [PathEditor.OfdParamsAttribute("Executable files (*.exe)|*.exe", "Selection")]
        [EditorAttribute(typeof(PathEditor), typeof(UITypeEditor))]
        public string FpcPath
        {
            get
            {
                return Properties.Settings.Default.FpcPath;
            }
            set
            {
                if (value == null)
                    return;

                DynamicVariables.Instance.StringCheckDynamicVariables(value);
                string expandedValue = DynamicVariables.Instance.StringReplaceDynamicVariables(value);
                if (File.Exists(expandedValue))
                    Properties.Settings.Default.FpcPath = value;
                else                    
                    throw new ArgumentException("File not found: \"" + expandedValue + "\"");             
            }
        }

        [LocalizedCategoryAttribute("SettingsAdapter_String26"),
           LocalizedDisplayNameAttribute("SettingsAdapter_String31"),
           LocalizedDescriptionAttribute("SettingsAdapter_String32")]
        public string FpcFlags
        {
            get { return Properties.Settings.Default.FpcFlags; }
            set 
            {
                DynamicVariables.Instance.StringCheckDynamicVariables(value);
                Properties.Settings.Default.FpcFlags = value; 
            }
        }        

        [LocalizedCategoryAttribute("SettingsAdapter_String39"),
            LocalizedDisplayNameAttribute("SettingsAdapter_String29"),
            LocalizedDescriptionAttribute("SettingsAdapter_String30")]
        [PathEditor.OfdParamsAttribute("Executable files (*.exe)|*.exe", "Selection")]
        [EditorAttribute(typeof(PathEditor), typeof(UITypeEditor))]
        public string GdbPath
        {
            get
            {
                return Properties.Settings.Default.GdbPath;
            }
            set
            {
                if (value == null)
                    return;

                DynamicVariables.Instance.StringCheckDynamicVariables(value);
                string expandedValue = DynamicVariables.Instance.StringReplaceDynamicVariables(value);
                if (File.Exists(expandedValue))
                    Properties.Settings.Default.GdbPath = value;
                else
                    throw new ArgumentException("File not found: \"" + expandedValue + "\"");
            }
        }

        class LocalizedDisplayNameAttribute : DisplayNameAttribute
        {
            private readonly string resourceName;
            public LocalizedDisplayNameAttribute(string resourceName)
                : base()
            {
                this.resourceName = resourceName;
            }

            public override string DisplayName
            {
                get
                {
                    return Properties.Strings.ResourceManager.GetString(resourceName);
                }
            }
        }

#region HELPER_CLASSES

        class LocalizedDescriptionAttribute : DescriptionAttribute
        {
            private readonly string resourceName;
            public LocalizedDescriptionAttribute(string resourceName)
                : base()
            { this.resourceName = resourceName; }

            public override string Description
            {
                get
                {
                    return Properties.Strings.ResourceManager.GetString(resourceName);
                }
            }
        }

        class LocalizedCategoryAttribute : CategoryAttribute
        {
            private readonly string resourceName;
            public LocalizedCategoryAttribute(string resourceName)
                : base()
            { this.resourceName = resourceName; }

            protected override string GetLocalizedString(string value)
            {
                return Properties.Strings.ResourceManager.GetString(resourceName, CultureInfo.CurrentUICulture);
            }
        }

        class PathEditor : System.Drawing.Design.UITypeEditor
        {
            //A class to hold our OpenFileDialog Settings
            public class OfdParamsAttribute : Attribute
            {
                public OfdParamsAttribute(string sFileFilter, string sDialogTitle)
                {
                    m_Filter = sFileFilter;
                    m_Title = sDialogTitle;
                }

                //The File Filter(s) of the open dialog
                private string m_Filter;
                public string Filter
                {
                    get { return m_Filter; }
                    set { m_Filter = value; }
                }

                //The Title of the open dialog
                private string m_Title;
                public string Title
                {
                    get { return m_Title; }
                    set { m_Title = value; }
                }
            }

            //The default settings for the file dialog
            private OfdParamsAttribute m_Settings = new OfdParamsAttribute("All Files (*.*)|*.*", "Open");
            public OfdParamsAttribute Settings
            {
                get { return m_Settings; }
                set { m_Settings = value; }
            }

            //Define a modal editor style and capture the settings from the property
            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(
                        ITypeDescriptorContext context)
            {
                if (context == null || context.Instance == null)
                    return base.GetEditStyle(context);

                //Retrieve our settings attribute (if one is specified)
                OfdParamsAttribute sa = (OfdParamsAttribute)context.PropertyDescriptor.Attributes[typeof(OfdParamsAttribute)];
                if (sa != null)
                    m_Settings = sa; //Store it in the editor
                return System.Drawing.Design.UITypeEditorEditStyle.Modal;
            }

            //Do the actual editing
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                if (context == null || context.Instance == null || provider == null)
                    return value;

                //Initialize the file dialog with our settings
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = m_Settings.Filter;
                dlg.CheckFileExists = true;
                dlg.Title = m_Settings.Title;

                //Find if the current value is legitimate
                string filename = (string)value;
                if (!System.IO.File.Exists(filename))
                    filename = null;

                //Preselect the existing file (if it exists)
                dlg.FileName = filename;
                //Display the dialog and change the value if confirmed
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    filename = dlg.FileName;
                }
                return filename;
            }
        }

        class LanguagesConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true; // display drop
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true; // drop-down vs combo
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> values = new List<string>();
                values.Add("Greek");
                values.Add("English");

                return new StandardValuesCollection(values.ToArray());
            }
        }
        #endregion HELPER_CLASSES
    }      
}
