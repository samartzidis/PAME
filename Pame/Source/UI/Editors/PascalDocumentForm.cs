//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Drawing;
using System.Windows.Forms;

namespace Pame
{
    public class PascalDocumentForm : DocumentForm
    {        
        protected bool isBuilt = false;
        public enum ActionAfterCompilationEnum { None, Execute, Debug };
        private ActionAfterCompilationEnum actionAfterCompilation = ActionAfterCompilationEnum.None;
        private Timer timer;
        private System.ComponentModel.IContainer components;

        public PascalDocumentForm()
            : base()
        {
            Init();
        }

        public PascalDocumentForm(string path)
            : base(path)
        {
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            HighlightingManager.Manager.AddSyntaxModeFileProvider(new FileSyntaxModeProvider(Globals.GetExeDirPath()));
            textEditorControl.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("Pascal");            
            textEditorControl.Document.FoldingManager.FoldingStrategy = new PascalFoldingStrategy();

            MainForm.OnSettingsChanged += new MainForm.SettingsChangedEventHandler(MainForm_OnSettingsChanged);

            textEditorControl.Document.DocumentChanged += new DocumentEventHandler(Document_DocumentChanged);            
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);

            LoadSettings();
        }

        void MainForm_OnSettingsChanged()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            PascalFoldingStrategy fs = (PascalFoldingStrategy)textEditorControl.Document.FoldingManager.FoldingStrategy;
            if (fs != null)
            {
                fs.Enabled = Properties.Settings.Default.CodeFolding;
                textEditorControl.Document.FoldingManager.UpdateFoldings(null, null);
            }
        }
        
        void timer_Tick(object sender, EventArgs e)
        {
            textEditorControl.Document.FoldingManager.UpdateFoldings(null, null);
            timer.Stop();
        }

        void Document_DocumentChanged(object sender, DocumentEventArgs e)
        {
            if (timer.Enabled)
                timer.Stop();
            timer.Start();
        }

        public bool IsBuilt
        {
            get { return isBuilt; }
            set { isBuilt = value; }
        }

        public ActionAfterCompilationEnum ActionAfterCompilation
        {
            get { return actionAfterCompilation; }
            set { actionAfterCompilation = value; }
        }        

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // PascalDocumentForm
            // 
            this.ClientSize = new System.Drawing.Size(398, 352);
            this.Name = "PascalDocumentForm";
            this.ResumeLayout(false);

        }
    }  
 
    class PascalFoldingStrategy : IFoldingStrategy
    {
        protected bool bEnabled = true;

        #region IFoldingStrategy Members

        public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
        {
            if (!Enabled)
                return null;

            List<FoldMarker> list = new List<FoldMarker>();
                        
            Stack<FoldMarkerInfo> stack = new Stack<FoldMarkerInfo>();
            for (int line = 0; line < document.TotalNumberOfLines; line++)
            {
                foreach (TextWord word in document.GetLineSegment(line).Words)
                {
                    string strWord = word.Word.ToLower();

                    if (strWord.Equals("begin") || strWord.Equals("case") || strWord.Equals("repeat"))
                    {
                        FoldMarkerInfo fmi = new FoldMarkerInfo();
                        fmi.startCol = word.Offset;
                        fmi.startLine = line;
                        stack.Push(fmi);
                    }
                    else if (strWord.Equals("end") || strWord.Equals("end.") || strWord.Equals("until"))
                    {
                        if (stack.Count > 0)
                        {
                            FoldMarkerInfo fmi = stack.Pop();
                            fmi.endLine = line;
                            fmi.endCol = word.Offset + word.Length;

                            FoldMarker marker = new FoldMarker(document,
                                fmi.startLine, fmi.startCol, fmi.endLine, fmi.endCol);
                            marker.FoldType = FoldType.MemberBody;
                            list.Add(marker);
                        }
                    }
                }
            }

            return list;
 
        }

        public bool Enabled
        {
            get { return bEnabled; }
            set { bEnabled = value; }
        }

        class FoldMarkerInfo
        {
            public int startLine; public int startCol;
            public int endLine; public int endCol;
        }

        #endregion
    }
}
