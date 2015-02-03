//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections;
using System.ServiceModel.Description;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.IO;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;
using System.Runtime.InteropServices;
using Logging;
using System.Diagnostics;
using GdbAdapter;
using PameNativeAdapter;
using CmdMgr;
using System.Net;
using System.Net.Cache;

namespace Pame
{
    public partial class MainForm : Form
    {
        public delegate void SettingsChangedEventHandler();
        public static event SettingsChangedEventHandler OnSettingsChanged;

        CommandManager cmdMgr = null;
        FindAndReplaceForm findForm = null;
        OutputForm outputWindow = null;
        ErrorsForm errorsWindow = null;
        DebugForm debugWindow = null;

        Compiler compiler = null;
        GdbEngine gdb = null;
        uint debugeePid = 0;
        IntPtr hWndDebugeeConsole = IntPtr.Zero;

        public MainForm()
        {
            InitializeComponent();

            lblVersionInfo.Text = Globals.FormatVersionString(Assembly.GetExecutingAssembly().GetName().Version);
            lblStatus.Text = Properties.Strings.MainForm_StatusReady;

            //Create FindAndReplaceForm
            findForm = new FindAndReplaceForm();

            //Create compiler and register callback
            compiler = new Compiler();
            compiler.OnMessageReceived += new Compiler.MessageReceivedHandler(Compiler_OnMessageReceived);
            compiler.OnCompilerFinished += new Compiler.CompilerFinishedHandler(Compiler_OnCompilerFinished);

            //Create debugger engine
            gdb = new GdbEngine();
            gdb.OnStatusChanged += new GdbEngine.StatusChangedEventHandler(Gdb_OnStatusChanged);

            //Create ErrorsWindow and register mediator callbacks
            errorsWindow = new ErrorsForm();
            errorsWindow.Show(dockPanel);
            errorsWindow.OnErrorSelected += new ErrorsForm.ErrorSelectedEventHandler(ErrorsWindow_OnErrorSelected);

            //Show output window
            outputWindow = new OutputForm();
            outputWindow.Show(dockPanel);

            //Create and show the debug window
            debugWindow = new DebugForm();
            debugWindow.Show(dockPanel);

            //Focus errorsWindow
            errorsWindow.Show();

            //Initialise event handlers for dock panel
            dockPanel.ActiveDocumentChanged += new EventHandler(dockPanel_ActiveDocumentChanged);

            timerStatusChange.Interval = 1000;
            timerStatusChange.Tick += new EventHandler(timerStatusChange_Tick);

            cmdMgr = new CommandManager();
            InitializeCommandManager();            
        }             

        public MainForm(string[] args)
            : this()
        {
            ProcessCmdLine(args);
        }

#region FORM_EVENTS
        private void MainForm_Load(object sender, EventArgs e)
        {            
            if (Properties.Settings.Default.WindowState == FormWindowState.Normal)
            {
                // Set window location
                if (Properties.Settings.Default.WindowLocation != null)
                    this.Location = Properties.Settings.Default.WindowLocation;

                // Set window size
                if (Properties.Settings.Default.WindowSize != null)
                    this.Size = Properties.Settings.Default.WindowSize;
            }

            // Set window state
            this.WindowState = Properties.Settings.Default.WindowState;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowLocation = this.Location;
            Properties.Settings.Default.WindowSize = this.Size;
            Properties.Settings.Default.WindowState = this.WindowState;

            // Save settings
            Properties.Settings.Default.Save();
        }
#endregion FORM_EVENTS

#region COMMAND_MANAGER
        private enum Commands
        {
            FileNew, FileOpen, FileSave, FileSaveAs, FileClose, FileCloseAll, FileExit,
            EditUndo, EditRedo, EditCut, EditCopy, EditPaste, EditDelete, EditFind, EditFindAndReplace, EditFindAgain,
            EditFindAgainReverse,
            ViewOutput, ViewErrors, ViewDebug,
            ProgramCompile, ProgramRun,
            DebugStart, DebugStepOver, DebugStepIn, DebugStepOut, DebugStop,
            ToolsOptions,
            HelpCheckForUpdates, HelpManual, HelpAbout
        };

        private void InitializeCommandManager()
        {
            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileNew,
                new ExecuteHandler<Commands>(CommandHandler)),
                new object[] { toolStripButtonNew, newToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileOpen,
                new ExecuteHandler<Commands>(CommandHandler)),
                new object[] { toolStripButtonOpen, openToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileSave,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonSave, saveToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileSaveAs,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { saveAsToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileClose,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { closeToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileCloseAll,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { closeAllToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.FileExit,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { exitToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditUndo,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonUndo, undoToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditRedo,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonRedo, redoToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditCut,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonCut, cutToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditCopy,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonCopy, copyToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditPaste,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonPaste, pasteToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditDelete,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { deleteToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditFind,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { findToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditFindAndReplace,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { findAndReplaceToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditFindAgain,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { findAgainToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.EditFindAgainReverse,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { findAgainReverseToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.ViewOutput,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { outputToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.ViewErrors,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { errorsToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.ViewDebug,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { viewDebugToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.ProgramCompile,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonCompile, compileToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.ProgramRun,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonRun, runToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.DebugStart,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonDebug, debugToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.DebugStop,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonDebugStop });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.DebugStepOver,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonStepOver });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.DebugStepIn,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonStepIn });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.DebugStepOut,
                new ExecuteHandler<Commands>(CommandHandler), new UpdateHandler<Commands>(CommandUpdateHandler)),
                new object[] { toolStripButtonStepOut });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.ToolsOptions,
                new ExecuteHandler<Commands>(CommandHandler)),
                new object[] { optionsToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.HelpAbout,
                new ExecuteHandler<Commands>(CommandHandler)),
                new object[] { aboutToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.HelpCheckForUpdates,
                new ExecuteHandler<Commands>(CommandHandler)),
                new object[] { checkForUpdatesToolStripMenuItem });

            cmdMgr.Register(new CmdMgr.Command<Commands>(Commands.HelpManual,
                new ExecuteHandler<Commands>(CommandHandler)),
                new object[] { freePascalManualToolStripMenuItem });
        }

        void CommandHandler(CmdMgr.Command<MainForm.Commands> cmd)
        {
            if (cmd.CommandType == Commands.FileNew)
                FileNew();
            else if (cmd.CommandType == Commands.FileOpen)
                FileOpen();
            else if (cmd.CommandType == Commands.FileSave)
                ActiveDocument.Save();
            else if (cmd.CommandType == Commands.FileSaveAs)
                ActiveDocument.SaveAs();
            else if (cmd.CommandType == Commands.FileClose)
                ActiveDocument.Close();
            else if (cmd.CommandType == Commands.FileCloseAll)
            {
                foreach (Form f in MdiChildren)
                    f.Close();
            }
            else if (cmd.CommandType == Commands.FileExit)
                Application.Exit();
            else if (cmd.CommandType == Commands.EditUndo)
                ActiveDocument.DoEditAction(new ICSharpCode.TextEditor.Actions.Undo());
            else if (cmd.CommandType == Commands.EditRedo)
                ActiveDocument.DoEditAction(new ICSharpCode.TextEditor.Actions.Redo());
            else if (cmd.CommandType == Commands.EditCut)
                ActiveDocument.DoEditAction(new ICSharpCode.TextEditor.Actions.Cut());
            else if (cmd.CommandType == Commands.EditCopy)
                ActiveDocument.DoEditAction(new ICSharpCode.TextEditor.Actions.Copy());
            else if (cmd.CommandType == Commands.EditPaste)
                ActiveDocument.DoEditAction(new ICSharpCode.TextEditor.Actions.Paste());
            else if (cmd.CommandType == Commands.EditDelete)
                ActiveDocument.DoEditAction(new ICSharpCode.TextEditor.Actions.Delete());
            else if (cmd.CommandType == Commands.EditFind)
                findForm.ShowFor(ActiveDocument.TextEditorControl, false);
            else if (cmd.CommandType == Commands.EditFindAndReplace)
                findForm.ShowFor(ActiveDocument.TextEditorControl, true);
            else if (cmd.CommandType == Commands.EditFindAgain)
                findForm.FindNext(true, false, string.Format(Properties.Strings.FindAndReplaceForm_String1, findForm.LookFor));
            else if (cmd.CommandType == Commands.EditFindAgainReverse)
                findForm.FindNext(true, true, string.Format(Properties.Strings.FindAndReplaceForm_String1, findForm.LookFor));
            else if (cmd.CommandType == Commands.ViewOutput)
                outputWindow.DockState = outputWindow.DockState == DockState.Hidden ? DockState.Float : DockState.Hidden;
            else if (cmd.CommandType == Commands.ViewErrors)
                errorsWindow.DockState = errorsWindow.DockState == DockState.Hidden ? DockState.Float : DockState.Hidden;
            else if (cmd.CommandType == Commands.ViewDebug)
                debugWindow.DockState = debugWindow.DockState == DockState.Hidden ? DockState.Float : DockState.Hidden;
            else if (cmd.CommandType == Commands.ProgramCompile)
                StartCompile(ActiveDocument, PascalDocumentForm.ActionAfterCompilationEnum.None, true);
            else if (cmd.CommandType == Commands.ProgramRun)
                StartCompile(ActiveDocument, PascalDocumentForm.ActionAfterCompilationEnum.Execute, false);
            else if (cmd.CommandType == Commands.DebugStart)
            {
                if (gdb.Status == GdbEngine.StatusEnum.Terminated)
                    StartCompile(ActiveDocument, PascalDocumentForm.ActionAfterCompilationEnum.Debug, false);
                else
                    DebugContinue();
            }
            else if (cmd.CommandType == Commands.DebugStop)
                DebugStop();
            else if (cmd.CommandType == Commands.DebugStepOver)
                StepOver();
            else if (cmd.CommandType == Commands.DebugStepIn)
                StepIn();
            else if (cmd.CommandType == Commands.DebugStepOut)
                StepOut();
            else if (cmd.CommandType == Commands.ToolsOptions)
            {
                DialogResult res = new SettingsForm().ShowDialog();
                if (res == DialogResult.OK)
                {
                    Properties.Settings.Default.Save();
                    if (OnSettingsChanged != null)
                        OnSettingsChanged();
                }
                else
                    Properties.Settings.Default.Reload();
            }
            else if (cmd.CommandType == Commands.HelpAbout)
                new AboutBox().ShowDialog();
            else if (cmd.CommandType == Commands.HelpCheckForUpdates)
                CheckForUpdates();
            else if (cmd.CommandType == Commands.HelpManual)
            {
                Process proc = new Process();
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.FileName = Globals.GetExeDirPath() + "\\rtl.chm";
                try { proc.Start(); }
                finally { }
            }
        }

        void CommandUpdateHandler(CmdMgr.Command<MainForm.Commands> cmd)
        {
            //There is no need to update while the form is minimised. This also
            //overcomes the problem with the call to Clipboard.ContainsText(), which doesnt
            //allow the form to be restored from minimised state
            if (this.WindowState == FormWindowState.Minimized)
                return;

            if (cmd.CommandType == Commands.FileSave)
                cmd.Enabled = ActiveDocument != null && ActiveDocument.Modified;
            else if (cmd.CommandType == Commands.FileSaveAs)
                cmd.Enabled = ActiveDocument != null;
            else if (cmd.CommandType == Commands.FileClose)
                cmd.Enabled = ActiveDocument != null;
            else if (cmd.CommandType == Commands.FileCloseAll)
                cmd.Enabled = MdiChildren.Length > 0;            
            else if (cmd.CommandType == Commands.EditUndo)
                cmd.Enabled = ActiveDocument != null && ActiveDocument.TextEditorControl.Document.UndoStack.CanUndo;            
            else if (cmd.CommandType == Commands.EditRedo)
                cmd.Enabled = ActiveDocument != null && ActiveDocument.TextEditorControl.Document.UndoStack.CanRedo;            
            else if (cmd.CommandType == Commands.EditCut)
                cmd.Enabled = ActiveDocument != null && ActiveDocument.HasSomethingSelected();
            else if (cmd.CommandType == Commands.EditCopy)
                cmd.Enabled = ActiveDocument != null && ActiveDocument.HasSomethingSelected();
            else if (cmd.CommandType == Commands.EditPaste)
                cmd.Enabled = ActiveDocument != null && Clipboard.ContainsText();
            else if (cmd.CommandType == Commands.EditDelete)
                cmd.Enabled = ActiveDocument != null && ActiveDocument.HasSomethingSelected();                        
            else if (cmd.CommandType == Commands.EditFind)
                cmd.Enabled = ActiveDocument != null;
            else if (cmd.CommandType == Commands.EditFindAndReplace)
                cmd.Enabled = ActiveDocument != null;
            else if (cmd.CommandType == Commands.EditFindAgain)
                cmd.Enabled = ActiveDocument != null && findForm.LookFor != null && findForm.LookFor.Length > 0;
            else if (cmd.CommandType == Commands.EditFindAgainReverse)
                cmd.Enabled = ActiveDocument != null && findForm.LookFor != null && findForm.LookFor.Length > 0;
            else if (cmd.CommandType == Commands.ViewOutput)
                cmd.Enabled = outputWindow.DockState == DockState.Hidden;
            else if (cmd.CommandType == Commands.ViewErrors)
                cmd.Enabled = errorsWindow.DockState == DockState.Hidden;
            else if (cmd.CommandType == Commands.ViewDebug)
                cmd.Enabled = debugWindow.DockState == DockState.Hidden;
            else if (cmd.CommandType == Commands.ProgramCompile)
                cmd.Enabled = ActiveDocument != null && !compiler.Running() && gdb.Status == GdbEngine.StatusEnum.Terminated;
            else if (cmd.CommandType == Commands.ProgramRun)
                cmd.Enabled = ActiveDocument != null && !compiler.Running() && gdb.Status == GdbEngine.StatusEnum.Terminated;
            else if (cmd.CommandType == Commands.DebugStart)
                cmd.Enabled = ActiveDocument != null && !compiler.Running() && (gdb.Status == GdbEngine.StatusEnum.Ready || gdb.Status == GdbEngine.StatusEnum.Terminated);
            else if (cmd.CommandType == Commands.DebugStop)
                cmd.Enabled = gdb.Status == GdbEngine.StatusEnum.Ready || gdb.Status == GdbEngine.StatusEnum.ReplyPending;
            else if (cmd.CommandType == Commands.DebugStepIn)
                cmd.Enabled = gdb.Status == GdbEngine.StatusEnum.Ready;
            else if (cmd.CommandType == Commands.DebugStepOut)
                cmd.Enabled = gdb.Status == GdbEngine.StatusEnum.Ready;
            else if (cmd.CommandType == Commands.DebugStepOver)
                cmd.Enabled = gdb.Status == GdbEngine.StatusEnum.Ready;
        }


#endregion COMMAND_MANAGER

#region GENERAL
        private PascalDocumentForm FocusOrOpenFile(string relativeOrAbsolutePath)
        {            
            //Try to find in open files
            foreach (Form form in MdiChildren)
            {
                PascalDocumentForm existingDoc = (PascalDocumentForm)form;
                if (existingDoc.FilePath == null)
                    continue;

                
                if (!Path.IsPathRooted(relativeOrAbsolutePath))
                {
                    //Relative path

                    if (existingDoc.FileNameWithoutExtension == Path.GetFileNameWithoutExtension(relativeOrAbsolutePath))
                    {
                        existingDoc.BringToFront();
                        return existingDoc;
                    }
                }
                else
                {
                    //Absolute path

                    FileInfo f1 = new FileInfo(existingDoc.FilePath);
                    FileInfo f2 = new FileInfo(relativeOrAbsolutePath);
                    if (f1.FullName.Equals(f2.FullName))
                    {
                        existingDoc.BringToFront();
                        return existingDoc;
                    }
                }
            }

            try
            {
                //Not found, try to open as new
                if (File.Exists(relativeOrAbsolutePath))
                {
                    PascalDocumentForm newDoc = new PascalDocumentForm(relativeOrAbsolutePath);
                    ShowDocumentForm(newDoc);

                    return newDoc;
                }
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                        string.Format(Properties.Strings.MainForm_String11, relativeOrAbsolutePath, m.Message),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                Logger.Instance.Error(m.ToString());
            }

            return null;
        }

        void ErrorsWindow_OnErrorSelected(ErrorsForm form, Compiler.Message msg)
        {
            if (msg == null || msg.error == null || msg.error.file == null)
                return;

            PascalDocumentForm doc = FocusOrOpenFile(msg.error.file);
            if (doc == null)
                return;

            int line = msg.error.line > 0 ? msg.error.line - 1 : 0;
            int col = msg.error.col > 0 ? msg.error.col - 1 : 0;

            //Set caret to error position
            TextAreaControl ta = doc.TextEditorControl.ActiveTextAreaControl;
            ta.Caret.Line = line;
            ta.Caret.Column = col;
            ta.ScrollToCaret();

            if (col != 0)
            {
                //Highlight word under cursor
                LineSegment ls = ta.TextArea.Document.GetLineSegment(line);
                if (ls != null)
                {
                    TextWord word = ls.GetWord(col);
                    if (word != null)
                        ta.TextArea.SelectionManager.SetSelection(new TextLocation(col, line), new TextLocation(col + word.Length, line));
                }
            }

            //Focus to control
            doc.TextEditorControl.Focus();            
        }

        void ProcessCmdLine(string[] args)
        {
            foreach (string path in args)
            {
                if (File.Exists(path))
                    FocusOrOpenFile(path);
                else
                    MessageBox.Show(this,
                        string.Format(Properties.Strings.MainForm_String4, path),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
            }
        }

        public void OnNextInstance(string[] args)
        {
            //This method is invoked by the InstanceManager's own thread
            //we therefore need to call through the Invoke() method for safety
            Invoke(new MethodInvoker(() =>
            {
                if (WindowState == FormWindowState.Minimized)
                    WindowState = FormWindowState.Normal;

                bool oldTopMost = TopMost;
                TopMost = true;
                TopMost = oldTopMost;

                ProcessCmdLine(args);
            }));
        }

        /// <summary>Returns the currently displayed editor, or null if none are open</summary>
        private PascalDocumentForm ActiveDocument
        {
            get
            {
                if (ActiveMdiChild == null)
                    return null;

                return (PascalDocumentForm)ActiveMdiChild;
            }
        }

        private void ShowDocumentForm(PascalDocumentForm form)
        {
            
            //Register event callbacks for new document
            //
            form.TextEditorControl.ActiveTextAreaControl.TextArea.MouseDown += new MouseEventHandler(DocumentForm_TextArea_MouseDown);
            form.FormClosing += new FormClosingEventHandler(DocumentForm_FormClosing);
            form.TextEditorControl.ActiveTextAreaControl.TextArea.IconBarMargin.MouseDown += new ICSharpCode.TextEditor.MarginMouseEventHandler(OnIconBarMarginMouseDown);
            form.TextEditorControl.ActiveTextAreaControl.Caret.PositionChanged += new EventHandler(Caret_PositionChanged);

            if (dockPanel.DocumentStyle == WeifenLuo.WinFormsUI.Docking.DocumentStyle.SystemMdi)
            {
                form.MdiParent = this;
                form.Show();
            }
            else
                form.Show(dockPanel);            
        }

        void Caret_PositionChanged(object sender, EventArgs e)
        {
            ICSharpCode.TextEditor.Caret c = (ICSharpCode.TextEditor.Caret)sender;
            UpdateShowLineCol(c.Line, c.Column);
        }

        void dockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (ActiveDocument == null)
                toolStripStatusLabelCol.Text = toolStripStatusLabelLine.Text = "";
            else
                UpdateShowLineCol(ActiveDocument.TextEditorControl.ActiveTextAreaControl.Caret.Line,
                    ActiveDocument.TextEditorControl.ActiveTextAreaControl.Caret.Column);
        } 

        void UpdateShowLineCol(int line, int col)
        {
            toolStripStatusLabelLine.Text = Properties.Strings.MainForm_String19 + (line + 1);
            toolStripStatusLabelCol.Text = Properties.Strings.MainForm_String20 + (col + 1);
        }
          
        void DocumentForm_TextArea_MouseDown(object sender, MouseEventArgs e)
        {
            TextArea ta = (TextArea)sender;

            if (e.Button == MouseButtons.Right)
            {
                editToolStripMenuItem.DropDown.OwnerItem = null;
                editToolStripMenuItem.DropDown.Show(ta, e.Location);
            }
        }

        void DocumentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DocumentForm form = (DocumentForm)sender;
            if (form.Modified)
            {
                DialogResult res = MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String3, form.Title),
                    this.Text,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation);
                if (res == DialogResult.OK)
                    form.Save();
                else if (res == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void OpenFiles(string[] fns)
        {
            foreach (string fn in fns)
                FocusOrOpenFile(fn);
        }

        private void FileNew()
        {
            PascalDocumentForm form = new PascalDocumentForm();
            form.Title = RandomWordGenerator.Word(2) + ".pas";
            ShowDocumentForm(form);
        }

        private void FileOpen()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                OpenFiles(openFileDialog.FileNames);
        }

        private bool Execute(PascalDocumentForm doc)
        {
            string path = doc.FileDirectory + "\\" + doc.FileNameWithoutExtension + ".exe";
            if (!File.Exists(path))
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String1, path),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }

            string spawnExePath = Globals.GetExeDirPath() + @"\spawn.exe";
            if (!File.Exists(spawnExePath))
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String1, path),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }

            try
            {
                bool retValue = false;
                using (ScopedCreateConsole console = new ScopedCreateConsole())
                {
                    ConsoleInfo.SetConsoleFontSizeTo(console.Handle, 0, 0,
                        Properties.Settings.Default.ConsoleFont); //Set a true-type font
                    Console.Title = doc.Title;
                    Console.ForegroundColor = Properties.Settings.Default.ConsoleForegroundColor;
                    Console.BackgroundColor = Properties.Settings.Default.ConsoleBackgroundColor;
                    Win32.SetConsoleCP((uint)Properties.Settings.Default.ConsoleCodepage);
                    Win32.SetConsoleOutputCP((uint)Properties.Settings.Default.ConsoleCodepage);
                    Win32.SetWindowPos(console.Handle, Win32.SetWindowPosZ.HWND_TOP, 0, 0, 0, 0,
                        (uint)Win32.FlagsSetWindowPos.SWP_NOMOVE | (uint)Win32.FlagsSetWindowPos.SWP_NOSIZE);

                    string workingDir = System.IO.Path.GetDirectoryName(path);
                    Win32.PROCESS_INFORMATION pi;
                    Win32.STARTUPINFO si = new Win32.STARTUPINFO();
                    Win32.SECURITY_ATTRIBUTES pSec = new Win32.SECURITY_ATTRIBUTES();
                    Win32.SECURITY_ATTRIBUTES tSec = new Win32.SECURITY_ATTRIBUTES();
                    pSec.nLength = Marshal.SizeOf(pSec);
                    tSec.nLength = Marshal.SizeOf(tSec);
                    retValue = Win32.CreateProcess(spawnExePath, "\"" + spawnExePath + "\"" + " " + "\"" + path + "\"",
                        ref pSec, ref tSec, false, 0, IntPtr.Zero, workingDir, ref si, out pi);
                    if (retValue == false)
                    {
                        MessageBox.Show(this,
                        string.Format("CreateProcess() failed while trying to launch: \"{0}\"", spawnExePath),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                        return false;
                    }
                }
            }
            catch (Exception m)
            {
                string strError = string.Format(Properties.Strings.MainForm_String21, m.Message);
                MessageBox.Show(this, strError,
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Logger.Instance.Error(strError);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Method to check for new version updates
        /// </summary>
        void CheckForUpdates()
        {
            string downloadURL = null;
            Version currentVersion = null;
            Version latestVersion = null;
            bool newerExists = false;
            try
            {
                using (new ScopedCursor(Cursors.WaitCursor))
                {
                    newerExists = CheckLatestVersion(out latestVersion, out currentVersion, out downloadURL);
                }
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                            string.Format(Properties.Strings.MainForm_String17, m.Message),
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                Logger.Instance.Error(m.ToString());

                return;
            }

            if (newerExists)
            {
                if (DialogResult.Yes ==
                     MessageBox.Show(this,
                                     string.Format(Properties.Strings.MainForm_String15,
                                        Globals.FormatVersionString(currentVersion), Globals.FormatVersionString(latestVersion)),
                                     this.Text,
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process.Start(downloadURL);
                }
            }
            else
            {
                MessageBox.Show(this,
                            Properties.Strings.MainForm_String16,
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Check if the current version is the latest one or not
        /// </summary>
        /// <param name="latestVersion"></param>
        /// <param name="currentVersion"></param>
        /// <param name="downloadURL"></param>
        /// <returns></returns>
        private bool CheckLatestVersion(out Version latestVersion, out Version currentVersion, out string downloadURL)
        {
            latestVersion = null;
            downloadURL = null;

            XmlDocument xmlDoc = new XmlDocument();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Properties.Resources.VersionCheckXmlURL);
            req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0)"; //This is required due to a bug of the Π.Σ.Δ. hoster
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);
            req.Timeout = 5000;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                xmlDoc.Load(resp.GetResponseStream());
                if (resp != null)
                    resp.Close();

                latestVersion = new Version(xmlDoc.SelectSingleNode("/xml/version").InnerText);
                downloadURL = xmlDoc.SelectSingleNode("/xml/url").InnerText;
            }

            currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (currentVersion.CompareTo(latestVersion) < 0)
                return true;

            return false;
        }

        /*
        public void ChangeInputLanguage()
        {
            //Nothing to do if there is only one Input Language supported:
            if (InputLanguage.InstalledInputLanguages.Count == 1)
                return;
            
            //Get index of current Input Language
            int currentLang = InputLanguage.InstalledInputLanguages.IndexOf(InputLanguage.CurrentInputLanguage);

            //Calculate next Input Language
            InputLanguage nextLang = ++currentLang == InputLanguage.InstalledInputLanguages.Count ? InputLanguage.InstalledInputLanguages[0] : InputLanguage.InstalledInputLanguages[currentLang];

            //Change current Language to the calculated:
            ChangeInputLanguage(nextLang);
        }

        public void ChangeInputLanguage(InputLanguage InputLang)
        {
            //Check is this Language really installed. Raise exception to warn if it is not:
            if (InputLanguage.InstalledInputLanguages.IndexOf(InputLang) == -1)
                throw new ArgumentOutOfRangeException();

            //InputLAnguage changes here:
            InputLanguage.CurrentInputLanguage = InputLang;
        }
        */

#endregion GENERAL

#region COMPILER
        void Compiler_OnMessageReceived(Compiler instance, Compiler.Message msg)
        {
            //This method is invoked by the Compiler object's own thread
            //we therefore need to call through the Invoke() method for safety
            Invoke(new MethodInvoker(() =>
            {                
                outputWindow.OutputString(msg.raw + "\r\n");
                if (msg.error != null)
                    errorsWindow.AddMessage(msg);
            }));
        }

        void Compiler_OnCompilerFinished(Compiler instance, int exitCode)
        {
            Invoke(new MethodInvoker(() =>
            {
                errorsWindow.Show();

                PascalDocumentForm doc = (PascalDocumentForm)instance.Tag;
                if (doc == null)
                {
                    Logger.Instance.Error("'PascalDocumentForm.Tag' is null but should be referencing a PascalDocumentForm.");
                    return;
                }

                if (exitCode == 0) //Success
                {
                    lblStatus.Text = Properties.Strings.MainForm_StatusCompilationSucceeded;

                    doc.IsBuilt = true;
                    errorsWindow.BackColor = SystemColors.Window;
                    if (doc.ActionAfterCompilation == PascalDocumentForm.ActionAfterCompilationEnum.Execute)
                        Execute(doc);
                    else if (doc.ActionAfterCompilation == PascalDocumentForm.ActionAfterCompilationEnum.Debug)
                        DebugStart(doc);
                }
                else
                {
                    lblStatus.Text = Properties.Strings.MainForm_StatusCompilationFailed;

                    doc.IsBuilt = false;
                    errorsWindow.BackColor = Color.OrangeRed;
                }
            }));
        }

        private bool StartCompile(PascalDocumentForm doc, PascalDocumentForm.ActionAfterCompilationEnum action, bool bForce)
        {            
            string path = doc.FilePath;
            bool bWasModified = false;
            if (path == null || doc.Modified)
            {
                if (!doc.Save())
                    return false;

                path = doc.FilePath;
                bWasModified = true;
            }

            //If compilation needed
            if (bForce || !doc.IsBuilt || bWasModified)
            {
                errorsWindow.Clear();
                outputWindow.Clear();

                compiler.FpcPath = DynamicVariables.Instance.StringReplaceDynamicVariables(Properties.Settings.Default.FpcPath);
                compiler.Flags = DynamicVariables.Instance.StringReplaceDynamicVariables(Properties.Settings.Default.FpcFlags);
                compiler.FilePath = path;
                compiler.Tag = ActiveDocument;
                try
                {
                    lblStatus.Text = Properties.Strings.MainForm_StatusBuilding;

                    doc.ActionAfterCompilation = action;
                    if (!compiler.Compile())
                    {
                        MessageBox.Show(this,
                            string.Format(Properties.Strings.MainForm_String14, "\"" + Properties.Settings.Default.FpcPath + "\""),
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);

                        return false;
                    }
                }
                catch (Exception m)
                {
                    MessageBox.Show(this,
                            string.Format(Properties.Strings.MainForm_String14, "\"" + Properties.Settings.Default.FpcPath + "\" (" + m.Message + ")"),
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);

                    return false;
                }
            }
            else //Compilation wasn't needed
            {
                if (action == PascalDocumentForm.ActionAfterCompilationEnum.Execute)
                    Execute(doc);
                else if (action == PascalDocumentForm.ActionAfterCompilationEnum.Debug)
                    DebugStart(doc);
            }

            return true;
        }
#endregion COMPILER        

#region DEBUGGER

        void GdbStatusChanged(GdbEngine engine, GdbEngine.StatusEnum status)
        {
            Logger.Instance.Debug(status.ToString());
            lblStatus.Text = Properties.Strings.MainForm_StatusDebugger + " " + status.ToString();

            if (status == GdbEngine.StatusEnum.Ready && !Focused)
                this.Activate();
            else if (!Properties.Settings.Default.TopmostDebugConsole)
                timerStatusChange.Start();
        }

        void timerStatusChange_Tick(object sender, EventArgs e)
        {
            Logger.Instance.Debug("timerStatusChange_Tick");
            if (gdb == null)
                return;

            GdbEngine.StatusEnum status = gdb.Status;
            if (status == GdbEngine.StatusEnum.ReplyPending && hWndDebugeeConsole != IntPtr.Zero)             
                Win32.SetWindowPos(hWndDebugeeConsole, Win32.SetWindowPosZ.HWND_TOP, 0, 0, 0, 0,
                        (uint)Win32.FlagsSetWindowPos.SWP_NOMOVE | (uint)Win32.FlagsSetWindowPos.SWP_NOSIZE);
            
            timerStatusChange.Stop();
        }        

        /// <summary>
        /// Called whenever the user clicks on the IconBarMargin of an open document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mousepos"></param>
        /// <param name="mouseButtons"></param>
        void OnIconBarMarginMouseDown(ICSharpCode.TextEditor.AbstractMargin sender, Point mousepos, MouseButtons mouseButtons)
        {
            if (mouseButtons != MouseButtons.Left)
                return;            

            IconBarMargin marginObj = (IconBarMargin)sender;
            Rectangle viewRect = marginObj.TextArea.TextView.DrawingPosition;
            TextLocation textLocation = marginObj.TextArea.TextView.GetLogicalPosition(0, mousepos.Y - viewRect.Top);

            if (textLocation.Y >= 0 && textLocation.Y < marginObj.TextArea.Document.TotalNumberOfLines)
            {
                LineSegment l = marginObj.Document.GetLineSegment(textLocation.Y);
                string s = marginObj.Document.GetText(l);
                if (s.Trim().Length == 0)
                    return;

                GdbBreakpoint clickedBreakpoint = null;
                foreach (Bookmark mark in sender.Document.BookmarkManager.Marks)
                {
                    if (mark is GdbBreakpoint && mark.Location.Line == textLocation.Line)
                    {
                        clickedBreakpoint = mark as GdbBreakpoint;
                        break;
                    }
                }
               
                if (clickedBreakpoint != null)
                {
                    //Clicked on existing breakpoint
                    
                    if (gdb.Status == GdbEngine.StatusEnum.Ready)
                    {
                        //Try to unregister it from the GdbEngine
                        //and then remove it from the document
                        //
                        try
                        {
                            GdbAdapter.Commands.DeleteBreakpoint cmd = 
                                new GdbAdapter.Commands.DeleteBreakpoint(gdb, clickedBreakpoint.BreakpointInfo);
                            cmd.Execute();

                            sender.Document.BookmarkManager.RemoveMark(clickedBreakpoint);
                        }
                        catch (Exception m)
                        {
                            MessageBox.Show(this,
                                        "Failed to remove breakpoint: " + m.Message,
                                        this.Text,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        }
                    }
                    else if (gdb.Status == GdbEngine.StatusEnum.Terminated)
                    {
                        //Just remove it from the document
                        sender.Document.BookmarkManager.RemoveMark(clickedBreakpoint);
                    }
                }
                else
                {
                    //New breakpoint

                    if (gdb.Status == GdbEngine.StatusEnum.Ready)
                    {
                        //Try to register it with the GdbEngine
                        //and then add it to the document
                        //
                        try
                        {
                            GdbAdapter.Commands.AddBreakpoint cmd =
                                new GdbAdapter.Commands.AddBreakpoint(gdb, ActiveDocument.FileName + ":" + (textLocation.Line + 1));
                            cmd.Execute();

                            GdbBreakpoint bp = new GdbBreakpoint(sender.Document, textLocation);
                            bp.BreakpointInfo = cmd.Return;
                            sender.Document.BookmarkManager.AddMark(bp);
                        }
                        catch (Exception m)
                        {
                            MessageBox.Show(this,
                                        "Failed to add breakpoint: " + m.Message,
                                        this.Text,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                        }

                    }
                    else if (gdb.Status == GdbEngine.StatusEnum.Terminated)
                    {
                        //Just add it to the document
                        GdbBreakpoint bp = new GdbBreakpoint(sender.Document, textLocation);
                        bp.BreakpointInfo = null;
                        sender.Document.BookmarkManager.AddMark(bp);
                    }
                }
            }
        }

        /// <summary>
        /// Disassociates the BreakpointInfo structure from all breakpoints in all open documents
        /// virtually rendering the breakpoint as not being registered with the debugger
        /// </summary>
        private void CleanBreakpointsRegistration()
        {
            foreach (Form form in MdiChildren)
            {
                PascalDocumentForm doc = (PascalDocumentForm)form;
                foreach (Bookmark mark in doc.TextEditorControl.Document.BookmarkManager.Marks)
                {
                    if (mark is GdbBreakpoint)
                    {
                        GdbBreakpoint bp = (GdbBreakpoint)mark;
                        bp.BreakpointInfo = null;
                    }
                }
            }
        }

        /// <summary>
        /// Registers with the debugger all breakpoints in all open documents that currently have no
        /// associated BreakpointInfo structure
        /// </summary>
        private void RegisterBreakpoints()
        {
            if (gdb.Status != GdbEngine.StatusEnum.Ready)
            {
                Logger.Instance.Error("RegisterPendingGdbBreakpoints() aborted because the GdbEngine is not in Ready state");
                return;
            }

            foreach (Form form in MdiChildren)
            {
                PascalDocumentForm doc = (PascalDocumentForm)form;
                foreach (Bookmark mark in doc.TextEditorControl.Document.BookmarkManager.Marks)
                {
                    if (mark is GdbBreakpoint)
                    {
                        GdbBreakpoint bp = (GdbBreakpoint)mark;
                        if (bp.BreakpointInfo == null)
                        {
                            try
                            {
                                GdbAdapter.Commands.AddBreakpoint cmd =
                                    new GdbAdapter.Commands.AddBreakpoint(gdb, doc.FileName + ":" + (bp.Location.Line + 1));
                                cmd.Execute(5000);
                                bp.BreakpointInfo = cmd.Return;
                            }
                            catch (Exception m)
                            {
                                //doc.TextEditorControl.Document.BookmarkManager.RemoveMark(bp);
                                bp.IsEnabled = false;

                                Logger.Instance.Error(m);
                            }
                        }
                    }
                }
            }
        }

        private bool DebugStart(PascalDocumentForm doc)
        {
            string debugeePath = doc.FileDirectory + "\\"
                    + doc.FileNameWithoutExtension
                    + ".exe";
            if (!File.Exists(debugeePath))
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String1, debugeePath),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }

            //Start GdbEngine
            if (gdb.Status != GdbEngine.StatusEnum.Terminated)
            {                
                MessageBox.Show(this,
                    "Unexpected GdbEngine state: " + gdb.Status,
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return false;
            }

            try
            {
                gdb.GdbPath = DynamicVariables.Instance.StringReplaceDynamicVariables(Properties.Settings.Default.GdbPath);
                gdb.Run();
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String8, Properties.Settings.Default.GdbPath, m.Message),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Logger.Instance.Error(m);
                DebugStop();

                return false;
            }

            GdbAdapter.GdbEngine.SyncAdapter sa = new GdbEngine.SyncAdapter(gdb);
            sa.SendCmd("file \"" + debugeePath.Replace("\\", "\\\\") + "\"");            

            try
            {
                //Initialise GDB 
                sa.SendCmd("break main", 5000);
                sa.SendCmd("run");

                GdbAdapter.Commands.GetThreads getThreads = new GdbAdapter.Commands.GetThreads(gdb);
                getThreads.Execute(5000);

                //Retrieve PID of the debugee process through the GDB host
                if (getThreads.Return == null || getThreads.Return.Count < 1)
                    throw new Exception("Could not retrieve the PID of the debugee process");
                uint.TryParse(getThreads.Return[0].pid, out debugeePid);
                if (debugeePid <= 1)
                    throw new Exception("Invalid debugee process PID (" + debugeePid + ")");
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String8, 
                        Properties.Settings.Default.GdbPath, m.Message),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Logger.Instance.Error(m);
                DebugStop();

                return false;
            }                                 

            //Initialise the debugee process window        
            try
            {
                using (ScopedAttachConsole console = new ScopedAttachConsole(debugeePid))
                {
                    hWndDebugeeConsole = Win32.GetConsoleWindow();
                    ConsoleInfo.SetConsoleFontSizeTo(hWndDebugeeConsole, 0, 0,
                        Properties.Settings.Default.ConsoleFont); //Set a true-type font
                    Console.Title = doc.Title;
                    Console.ForegroundColor = Properties.Settings.Default.ConsoleForegroundColor;
                    Console.BackgroundColor = Properties.Settings.Default.ConsoleBackgroundColor;

                    if (Properties.Settings.Default.TopmostDebugConsole)
                    {
                        Win32.SetWindowPos(hWndDebugeeConsole, Win32.SetWindowPosZ.HWND_TOPMOST, 0, 0, 0, 0,
                            (uint)Win32.FlagsSetWindowPos.SWP_NOMOVE | (uint)Win32.FlagsSetWindowPos.SWP_NOSIZE);
                    }

                    Win32.SetConsoleCP((uint)Properties.Settings.Default.ConsoleCodepage);
                    Win32.SetConsoleOutputCP((uint)Properties.Settings.Default.ConsoleCodepage);
                }
            }
            catch (Exception m)
            {
                string strError = string.Format(Properties.Strings.MainForm_String21, m.Message);
                MessageBox.Show(this, strError,
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Logger.Instance.Error(strError);
                DebugStop();

                return false;
            }

            //Get source code position
            try
            {
                GdbAdapter.Commands.Where cmd = new GdbAdapter.Commands.Where(gdb);
                cmd.Execute(5000);
                if (cmd.Return == null || cmd.Return.file == null || !cmd.Return.line.HasValue)
                {
                    MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String12,
                        "Command could not locate source code position (Where.Return is null)"),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                    DebugStop();

                    return false;
                }
                else
                    DebugHighlight(cmd.Return.file, cmd.Return.line.Value - 1);
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String12, m.Message),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Logger.Instance.Error(m);
                DebugStop();

                return false;
            }

            //Register all pending breakpoints
            try
            {
                RegisterBreakpoints();
            }
            catch (Exception m)
            {
                Logger.Instance.Error("Exception in RegisterPendingGdbBreakpoints(): " + m);
            }

            //Activate the debug window
            debugWindow.Show();

            try
            {
                UpdateDebugWindow();
            }
            catch (Exception m)
            {
                Logger.Instance.Error(m);
            }
           
            return true;
        }

        /// <summary>
        /// Updates the local and global variables in the debug window
        /// </summary>
        void UpdateDebugWindow()
        {
            if (!Properties.Settings.Default.WatchVariables)
                return;

            List<GDBVar> gdbVars = new List<GDBVar>();

            //Retrieve global variables
            //
            GdbAdapter.Commands.GetVariables vars = new GdbAdapter.Commands.GetVariables(gdb);
            vars.Execute(5000);
            foreach (GdbAdapter.Commands.VariableInfo vi in vars.Return)
            {
                Logger.Instance.Debug("Variable: " + vi.name + ", " + vi.type + ", " + vi.file);             

                try
                {
                    GdbAdapter.Commands.GetVariableValue val = new GdbAdapter.Commands.GetVariableValue(gdb, vi);
                    val.Execute(5000);

                    GDBVar var = new GDBVar(vi, val.Return);
                    gdbVars.Add(var);
                }
                catch (Exception m)
                {
                    Logger.Instance.Error(m);
                }
            }
            
            //Retrieve local variables
            //
            GdbAdapter.Commands.GetLocalVariables localVars = new GdbAdapter.Commands.GetLocalVariables(gdb);
            localVars.Execute(5000);
            foreach (GdbAdapter.Commands.VariableInfo vi in localVars.Return)
            {
                if (vi.name == null)
                    continue;

                //We need to retrieve the variable type for each local var as it is not returned by gdb
                GdbAdapter.Commands.GetVariableType vt = new GdbAdapter.Commands.GetVariableType(gdb, vi.name);
                vt.Execute(5000);
                vi.type = vt.Return; //Set retrieved type in VariableInfo

                try
                {
                    //Retrieve parsed variable value for local variable
                    GdbAdapter.Commands.GetVariableValue val = new GdbAdapter.Commands.GetVariableValue(gdb, vi);
                    val.Execute(5000);

                    GDBVar var = new GDBVar(vi, val.Return);
                    gdbVars.Add(var);
                }
                catch (Exception m)
                {
                    Logger.Instance.Error(m);
                }
            }

            //Retrieve local arguments
            //
            GdbAdapter.Commands.GetArgs getArgs = new GdbAdapter.Commands.GetArgs(gdb);
            getArgs.Execute(5000);
            foreach (GdbAdapter.Commands.VariableInfo vi in getArgs.Return)
            {
                if (vi.name == null)
                    continue;

                //We need to retrieve the variable type for each var as it is not returned by gdb
                GdbAdapter.Commands.GetVariableType vt = new GdbAdapter.Commands.GetVariableType(gdb, vi.name);
                vt.Execute(5000);
                vi.type = vt.Return; //Set retrieved type in VariableInfo

                try
                {
                    //Retrieve parsed variable value for local variable
                    GdbAdapter.Commands.GetVariableValue val = new GdbAdapter.Commands.GetVariableValue(gdb, vi);
                    val.Execute(5000);

                    GDBVar var = new GDBVar(vi, val.Return);
                    gdbVars.Add(var);
                }
                catch (Exception m)
                {
                    Logger.Instance.Error(m);
                }
            } 

            debugWindow.VarList.UpdateVars(gdbVars.ToArray());
        }

        private void DebugStop()
        {
            debugWindow.VarList.Nodes.Clear();

            if (gdb == null || gdb.Status == GdbEngine.StatusEnum.Terminated || gdb.Status == GdbEngine.StatusEnum.Terminating)
            {
            }
            else if (gdb.Status == GdbEngine.StatusEnum.ReplyPending)
                gdb.Kill();
            else if (gdb.Status == GdbEngine.StatusEnum.Ready)
            {
                //Send quit command synchronously with a small timeout
                try
                {
                    GdbAdapter.GdbEngine.SyncAdapter sa = new GdbEngine.SyncAdapter(gdb);
                    sa.SendCmd("quit", 5000);
                    Logger.Instance.Info("Quit command completed successfully");
                }
                catch (TimeoutException)
                {
                    Logger.Instance.Warn("Quit command timed-out");
                }
                catch (Exception m)
                {
                    Logger.Instance.Error(m);
                }

                //After timeout or reply, if still not terminated, terminate forcefully
                if (gdb.Status != GdbEngine.StatusEnum.Terminated)
                {
                    Logger.Instance.Warn("Forcefully killing GdbEngince instance");
                    gdb.Kill();
                }
            }

            debugeePid = 0;
            hWndDebugeeConsole = IntPtr.Zero;

            CleanBreakpointsRegistration();

            foreach (Form form in MdiChildren)
            {
                PascalDocumentForm doc = (PascalDocumentForm)form;
                doc.TextEditorControl.ActiveTextAreaControl.Document.ReadOnly = false;
                doc.TextEditorControl.Document.BookmarkManager.RemoveMarks(bookmark => { return (bookmark is ActiveMark); });
            }
        }

        private void DebugContinue()
        {
            try
            {
                GdbAdapter.Commands.StepContinue cmd = new GdbAdapter.Commands.StepContinue(gdb);
                cmd.OnAsyncResult += new GdbAdapter.Command<GdbAdapter.Commands.StepLocation>.AsyncResultHandler(Step_OnAsyncResult);
                cmd.ExecuteAsync();                
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                        string.Format("Step command failed: {0}", m.Message),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                Logger.Instance.Error(m);
            }
        }        

        private void StepOver()
        {
            try
            {
                GdbAdapter.Commands.StepOver cmd = new GdbAdapter.Commands.StepOver(gdb);
                cmd.OnAsyncResult += new GdbAdapter.Command<GdbAdapter.Commands.StepLocation>.AsyncResultHandler(Step_OnAsyncResult);
                cmd.ExecuteAsync();
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                        string.Format("Step command failed: {0}", m.Message),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                Logger.Instance.Error(m);
            }
        }        

        private void StepIn()
        {
            try
            {
                GdbAdapter.Commands.StepIn cmd = new GdbAdapter.Commands.StepIn(gdb);
                cmd.OnAsyncResult += new GdbAdapter.Command<GdbAdapter.Commands.StepLocation>.AsyncResultHandler(Step_OnAsyncResult);
                cmd.ExecuteAsync();
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                        string.Format("Step command failed: {0}", m.Message),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                Logger.Instance.Error(m);
            }
        }

        private void StepOut()
        {
            try
            {
                GdbAdapter.Commands.StepOut cmd = new GdbAdapter.Commands.StepOut(gdb);
                cmd.OnAsyncResult += new GdbAdapter.Command<GdbAdapter.Commands.StepLocation>.AsyncResultHandler(Step_OnAsyncResult);
                cmd.ExecuteAsync();
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                        string.Format("Step command failed: {0}", m.Message),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                Logger.Instance.Error(m);
            }
        }

        void StepResult(object sender, Exception ex, GdbAdapter.Commands.StepLocation res)
        {            
            if (res == null || !res.line.HasValue)
            {
                if (ex != null)
                {
                    MessageBox.Show(this,
                        string.Format("'Step' command failed: {0}", ex.Message),
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    Logger.Instance.Error("Step command returned exception: " + ex.ToString());
                }

                Logger.Instance.Debug("StepResult() returned null data (debugee process exited?)");

                OSDMessageBox box = new OSDMessageBox();
                box.Show(this, Properties.Strings.MainForm_String18);

                DebugStop();
                return;
            }
            else
                Logger.Instance.Debug("StepResult() raw data: " + res.raw);
            
            try
            {
                //Get current position
                GdbAdapter.Commands.Where cmd = new GdbAdapter.Commands.Where(gdb);
                cmd.Execute(5000);
                
                //Highlight current line
                if (cmd.Return != null && cmd.Return.file != null && cmd.Return.line.HasValue)
                    DebugHighlight(cmd.Return.file, cmd.Return.line.Value - 1);                
            }
            catch (Exception m)
            {
                MessageBox.Show(this,
                    string.Format(Properties.Strings.MainForm_String12, m.Message),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Logger.Instance.Error(m);
                DebugStop();
            }

            try
            {
                //Update variables in debug window
                UpdateDebugWindow();
            }
            catch (Exception m)
            {
                Logger.Instance.Error(m);
            }
        }

        void DebugHighlight(string file, int line)
        {            
            PascalDocumentForm doc = FocusOrOpenFile(file);
            if (doc == null)
                return;

            //Disallow editing
            doc.TextEditorControl.ActiveTextAreaControl.Document.ReadOnly = true;

            if (line >= 0)
            {
                TextAreaControl ta = doc.TextEditorControl.ActiveTextAreaControl;
                ta.Caret.Line = line;
                ta.Caret.Column = 0;
                ta.ScrollToCaret();

                doc.TextEditorControl.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, line));
                doc.TextEditorControl.Document.CommitUpdate();

                doc.RemoveActiveMark();
                doc.SetActiveMark(line);
            }            
        }

#endregion DEBUGGER

#region GDB_ENGINE_THREAD_CALLBACKS
        void Step_OnAsyncResult(object sender, Exception ex, GdbAdapter.Commands.StepLocation res)
        {
            //Invoke StepOver_Reply() using the UI thread asynchronously to perform the application logic and 
            //return from this GdbEngine callback routine immediately. This allows
            //the GdbEngine thread to unblock and continue with other requests
            BeginInvoke(new MethodInvoker(() => { StepResult(sender, ex, res); }));
        }
        void Gdb_OnStatusChanged(GdbEngine engine, GdbEngine.StatusEnum status)
        {
            BeginInvoke(new MethodInvoker(() => { GdbStatusChanged(engine, status); }));         
        }
#endregion GDB_ENGINE_THREAD_CALLBACKS

        

    }
    
#region HELPER_CLASSES

    public class ScopedAttachConsole : IDisposable
    {
        bool bAttached = false;

        public ScopedAttachConsole(uint pid)
        {
            if (!Win32.AttachConsole(pid))
                throw new Exception("Attach console to process with PID: " + (int)pid + " failed");
            bAttached = true;
        }

        protected void FreeConsole()
        {
            if (bAttached)
            {
                Win32.FreeConsole();
                bAttached = false;
            }
        }

        ~ScopedAttachConsole()
        {
            FreeConsole();
        }

        #region IDisposable Members

        public void Dispose()
        {
            FreeConsole();
        }

        #endregion
    }

    public class ScopedCreateConsole : IDisposable
    {
        private static IntPtr hWnd = IntPtr.Zero;

        public ScopedCreateConsole()
        {
            if (hWnd != IntPtr.Zero)
                throw new Exception("A second console window cannot be allocated to the same process");

            if (!Win32.AllocConsole())
                throw new Exception("AllocConsole() failed");

            hWnd = Win32.GetConsoleWindow();            
        }

        ~ScopedCreateConsole()
        {
            Dispose();
        }

        public IntPtr Handle
        {
            get { return hWnd; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (hWnd != IntPtr.Zero)
            {
                if (Win32.FreeConsole())
                    hWnd = IntPtr.Zero;
            }
        }

        #endregion
    }

    public class GdbBreakpoint : ICSharpCode.TextEditor.Document.Bookmark
    {
        protected GdbAdapter.Commands.BreakpoitInfo bi = null;

        public GdbBreakpoint(ICSharpCode.TextEditor.Document.IDocument d, TextLocation l)
            : base(d, l)
        {
        }

        public GdbAdapter.Commands.BreakpoitInfo BreakpointInfo
        {
            get { return bi; }
            set { bi = value; }
        }

        public override void Draw(ICSharpCode.TextEditor.IconBarMargin margin, System.Drawing.Graphics g, System.Drawing.Point p)
        {
            margin.DrawBreakpoint(g, p.Y, IsEnabled, true);
        }

        public override bool Click(Control parent, MouseEventArgs e)
        {
            return false;
        }
    }
#endregion HELPER_CLASSES
}
