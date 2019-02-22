﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using StringTheory.Analysis;

namespace StringTheory.UI
{
    public sealed class HomePage : ITabPage
    {
        public event Action CloseRequested;

        public ICommand OpenDumpCommand { get; }
        public ICommand AttachToProcessCommand { get; }
        public ICommand ShowAboutCommand { get; }

        public DrawingBrush IconDrawingBrush { get; }

        public string HeaderText => "Home";
        public bool CanClose => false;

        public HomePage(MainWindow mainWindow)
        {
            OpenDumpCommand = new DelegateCommand(OpenDump);
            AttachToProcessCommand = new DelegateCommand(() => new AttachToProcessWindow(mainWindow) { Owner = mainWindow }.ShowDialog());
            ShowAboutCommand = new DelegateCommand(() => new AboutWindow { Owner = mainWindow }.Show());

            IconDrawingBrush = (DrawingBrush) Application.Current.FindResource("HomeIconBrush");

            void OpenDump()
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Dump files (*.dmp)|*.dmp|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var dumpFilePath = openFileDialog.FileName;

                    OpenDumpFile(dumpFilePath);
                }
            }

            void OpenDumpFile(string dumpFilePath)
            {
                var operation = new LoadingOperation(
                    (progressCallback, token) =>
                    {
                        var analyzer = new HeapAnalyzer(dumpFilePath);

                        var summary = analyzer.GetStringSummary(progressCallback, token);

                        var description = $"All strings in {dumpFilePath}";

                        return new StringListPage(mainWindow, summary, analyzer, Path.GetFileNameWithoutExtension(dumpFilePath), description);
                    });

                mainWindow.AddTab(new LoadingTabPage(Path.GetFileNameWithoutExtension(dumpFilePath), StringListPage.IconDrawingBrush, operation));
            }
        }
    }
}