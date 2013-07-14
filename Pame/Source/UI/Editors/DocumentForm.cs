//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor;
using System.IO;

namespace Pame
{           
    public partial class DocumentForm : DockContent
    {
        public delegate void ModifiedEventHandler(DocumentForm form);
        public event ModifiedEventHandler OnModifiedChanged;

        protected TextEditorControl textEditorControl;
        protected string title = null;
        protected bool modified = false;

        public DocumentForm()
        {
            InitializeComponent();
            
            Modified = false;

            //Initialise the TextEditorControl properties
            textEditorControl.IsIconBarVisible = true;
            ReloadSettings();

            //Set event handlers
            MainForm.OnSettingsChanged += new MainForm.SettingsChangedEventHandler(MainForm_OnSettingsChanged);
            textEditorControl.Document.DocumentChanged += new DocumentEventHandler(Document_DocumentChanged);            
            textEditorControl.Document.BookmarkManager.Added += new BookmarkEventHandler(BookmarkManager_Added);
            textEditorControl.Document.BookmarkManager.Removed += new BookmarkEventHandler(BookmarkManager_Removed);
        }

        void MainForm_OnSettingsChanged()
        {
            ReloadSettings();
        }                

        public DocumentForm(string path) : this()
        {
            textEditorControl.LoadFile(path, true, true);
            Title = FileName;
            Modified = false;
        }

        protected virtual void ReloadSettings()
        {
            textEditorControl.TextEditorProperties.TabIndent = Properties.Settings.Default.TabSize;
            textEditorControl.TextEditorProperties.ShowMatchingBracket = Properties.Settings.Default.HighlightMatchingBrackets;
            textEditorControl.TextEditorProperties.LineViewerStyle = Properties.Settings.Default.HighlightCurrentRow ? LineViewerStyle.FullRow : LineViewerStyle.None;
            textEditorControl.TextEditorProperties.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;
            textEditorControl.TextEditorProperties.ShowSpaces = Properties.Settings.Default.ShowSpaces;
            textEditorControl.TextEditorProperties.ShowEOLMarker = Properties.Settings.Default.ShowNewlines;
            textEditorControl.TextEditorProperties.Font = Properties.Settings.Default.Font;

            textEditorControl.Refresh();
        }

        private void Document_DocumentChanged(object sender, DocumentEventArgs e)
        {
            Modified = true;            
        }

        public bool Modified
        {
            set 
            {
                if (modified != value)
                {
                    if (value)
                        Text = title + " *";
                    else
                        Text = title;

                    if (OnModifiedChanged != null)
                        OnModifiedChanged(this);
                }

                modified = value;                
            }
            get 
            { 
                return modified; 
            }
        }

        public String Title
        {
            get 
            { 
                return title; 
            }
            set 
            { 
                title = value;
                if (modified)
                    Text = value + " *";
                else
                    Text = value;
            }
        }

        public string FileName
        {
            get { return System.IO.Path.GetFileName(textEditorControl.FileName); }
        }

        public string FileNameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(textEditorControl.FileName); }
        }

        public string FileDirectory
        {
            get { return System.IO.Path.GetDirectoryName(textEditorControl.FileName); }
        }

        public string FileExtension
        {
            get { return System.IO.Path.GetExtension(textEditorControl.FileName); }
        }

        public string FilePath
        {
            get { return textEditorControl.FileName; }
        }

        public bool Save()
        {
            if (string.IsNullOrEmpty(FilePath))
                return SaveAs();

            textEditorControl.SaveFile(FilePath);
            Modified = false;
            return true;
        }

        public bool SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = textEditorControl.FileName != null ? textEditorControl.FileName : Title;
            saveFileDialog.DefaultExt = ".pas";
            saveFileDialog.Filter = "Pascal files (*.pas)|*.pas|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                textEditorControl.SaveFile(saveFileDialog.FileName);                                
                Title = FileName;
                Modified = false;
                
                // The syntax highlighting strategy doesn't change
                // automatically, so do it manually.
                //textEditorControl.Document.HighlightingStrategy =
                //    HighlightingStrategyFactory.CreateHighlightingStrategyForFile(textEditorControl.FileName);

                return true;
            }

            return false;
        }        

        public TextEditorControl TextEditorControl
        {
            get { return textEditorControl; }
        }

        /// <summary>Performs an action encapsulated in IEditAction.</summary>
        /// <remarks>
        /// There is an implementation of IEditAction for every action that 
        /// the user can invoke using a shortcut key (arrow keys, Ctrl+X, etc.)
        /// The editor control doesn't provide a public funciton to perform one
        /// of these actions directly, so I wrote DoEditAction() based on the
        /// code in TextArea.ExecuteDialogKey(). You can call ExecuteDialogKey
        /// directly, but it is more fragile because it takes a Keys value (e.g.
        /// Keys.Left) instead of the action to perform.
        /// <para/>
        /// Clipboard commands could also be done by calling methods in
        /// editor.ActiveTextAreaControl.TextArea.ClipboardHandler.
        /// </remarks>
        public void DoEditAction(ICSharpCode.TextEditor.Actions.IEditAction action)
        {
            if (textEditorControl != null && action != null)
            {
                var area = textEditorControl.ActiveTextAreaControl.TextArea;
                textEditorControl.BeginUpdate();
                try
                {
                    lock (textEditorControl.Document)
                    {
                        action.Execute(area);
                        if (area.SelectionManager.HasSomethingSelected && area.AutoClearSelection /*&& caretchanged*/)
                        {
                            if (area.Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal)
                            {
                                area.SelectionManager.ClearSelection();
                            }
                        }
                    }
                }
                finally
                {
                    textEditorControl.EndUpdate();
                    area.Caret.UpdateCaretPosition();
                }
            }
        }

        public bool HasSomethingSelected()
        {
            return textEditorControl.ActiveTextAreaControl.TextArea.SelectionManager.HasSomethingSelected;
        }

        public void SetActiveMark(int line)
        {
            ActiveMark mark = new ActiveMark(textEditorControl.Document, new TextLocation(0, line));
            textEditorControl.Document.BookmarkManager.AddMark(mark);
            textEditorControl.ActiveTextAreaControl.ScrollTo(line + 10);
        }

        public void RemoveActiveMark()
        {
            BookmarkManager mgr = textEditorControl.Document.BookmarkManager;

            List<Bookmark> activeMarks = new List<Bookmark>();
            foreach (Bookmark mark in mgr.Marks)
                if (mark is ActiveMark)
                    activeMarks.Add(mark);

            foreach (Bookmark mark in activeMarks)
                mgr.RemoveMark(mark);
        }

        void BookmarkManager_Removed(object sender, BookmarkEventArgs e)
        {
            textEditorControl.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, e.Bookmark.LineNumber));
            textEditorControl.Document.CommitUpdate();
        }

        void BookmarkManager_Added(object sender, BookmarkEventArgs e)
        {
            textEditorControl.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, e.Bookmark.LineNumber));
            textEditorControl.Document.CommitUpdate();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentForm));
            this.textEditorControl = new ICSharpCode.TextEditor.TextEditorControl();
            this.SuspendLayout();
            // 
            // textEditorControl
            // 
            this.textEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditorControl.IsReadOnly = false;
            this.textEditorControl.Location = new System.Drawing.Point(0, 0);
            this.textEditorControl.Name = "textEditorControl";
            this.textEditorControl.Size = new System.Drawing.Size(398, 352);
            this.textEditorControl.TabIndex = 0;
            // 
            // DocumentForm
            // 
            this.ClientSize = new System.Drawing.Size(398, 352);
            this.Controls.Add(this.textEditorControl);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DocumentForm";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.ResumeLayout(false);

        }
    }

#region HELPER_CLASSES
    public class NonClickableBookmark : ICSharpCode.TextEditor.Document.Bookmark
    {
        public NonClickableBookmark(ICSharpCode.TextEditor.Document.IDocument d, TextLocation l)
            : base(d, l)
        {
        }

        public override bool Click(Control parent, MouseEventArgs e)
        {
            return false;
        }
    }

    public class ActiveMark : ICSharpCode.TextEditor.Document.Bookmark
    {
        public ActiveMark(ICSharpCode.TextEditor.Document.IDocument d, TextLocation l)
            : base(d, l)
        {
        }

        public override void Draw(IconBarMargin margin, System.Drawing.Graphics g, System.Drawing.Point p)
        {
            margin.DrawArrow(g, p.Y);
        }

        public override bool Click(Control parent, MouseEventArgs e)
        {
            return false;
        }
    }
    
#endregion HELPER_CLASSES
}
