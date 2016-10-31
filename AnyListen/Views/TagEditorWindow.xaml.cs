using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AnyListen.Music.Track;
using AnyListen.ViewModels;
using Microsoft.Win32;
using TagLib;

namespace AnyListen.Views
{
    /// <summary>
    /// Interaction logic for TagEditorWindow.xaml
    /// </summary>
    public partial class TagEditorWindow
    {
        public TagEditorWindow(LocalTrack track)
        {
            DataContext = new TagEditorViewModel(track, this);
            InitializeComponent();
        }
    }
}
