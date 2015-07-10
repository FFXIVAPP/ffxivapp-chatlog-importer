// FFXIVAPP.ChatLog.Importer
// MainWindow.xaml.cs
// 
// Copyright © 2007 - 2015 Ryan Wilson - All Rights Reserved
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are met: 
// 
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer. 
//  * Redistributions in binary form must reproduce the above copyright 
//    notice, this list of conditions and the following disclaimer in the 
//    documentation and/or other materials provided with the distribution. 
//  * Neither the name of SyndicatedLife nor the names of its contributors may 
//    be used to endorse or promote products derived from this software 
//    without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.Win32;

namespace FFXIVAPP.ChatLog.Importer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StringToBrushConverter _stb = new StringToBrushConverter();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FFXIVAPP", "Logs"),
                Multiselect = false,
                Filter = "XML Files (*.xml)|*.xml"
            };
            openFileDialog.FileOk += delegate
            {
                var document = XDocument.Load(openFileDialog.FileName);
                var dictionary = new Dictionary<int, List<string>>();
                var lineCount = 0;
                foreach (var xElement in document.Descendants()
                                                 .Elements("Entry"))
                {
                    var xKey = (string) xElement.Attribute("Key");
                    var xLine = (string) xElement.Element("Line");
                    var xTimeStamp = (string) xElement.Element("TimeStamp");
                    if (String.IsNullOrWhiteSpace(xKey) || String.IsNullOrWhiteSpace(xLine) || String.IsNullOrWhiteSpace(xTimeStamp))
                    {
                        continue;
                    }
                    dictionary.Add(lineCount++, new List<string>()
                    {
                        xKey,
                        xLine,
                        xTimeStamp
                    });
                }
                var paragraphs = new List<Paragraph>();
                var colors = new[]
                {
                    "", ""
                };
                foreach (var data in dictionary.Select(kvp => kvp.Value))
                {
                    var timeStampColor = _stb.Convert(String.IsNullOrWhiteSpace(colors[0]) ? "#FFFFFFFF" : colors[0]);
                    var lineColor = _stb.Convert(String.IsNullOrWhiteSpace(colors[1]) ? "#FFFFFFFF" : colors[1]);
                    var paraGraph = new Paragraph();
                    var timeStamp = new Span(new Run(data[2]))
                    {
                        Foreground = (Brush) timeStampColor,
                        FontWeight = FontWeights.Bold
                    };
                    var coloredLine = new Span(new Run(data[1]))
                    {
                        Foreground = (Brush) lineColor
                    };
                    paraGraph.Inlines.Add(timeStamp);
                    if (!String.IsNullOrWhiteSpace(data[0]))
                    {
                        var playerColor = _stb.Convert("#FFFF00FF");
                        var playerLine = new Span(new Run("[" + data[0] + "] "))
                        {
                            Foreground = (Brush) playerColor
                        };
                        paraGraph.Inlines.Add(playerLine);
                    }
                    paraGraph.Inlines.Add(coloredLine);
                    paragraphs.Add(paraGraph);
                }
                _FDR.Document.Blocks.AddRange(paragraphs);
            };
            openFileDialog.ShowDialog();
        }
    }
}
