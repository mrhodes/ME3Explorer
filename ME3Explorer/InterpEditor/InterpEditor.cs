﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using KFreonLib.MEDirectories;
using ME3Explorer.SequenceObjects;
using ME3LibWV;
using ME3Explorer.Packages;
using System.Linq;

namespace ME3Explorer.Matinee
{
    public partial class InterpEditor : WinFormsBase
    {
        public List<int> objects;

        public InterpEditor()
        {
            SText.LoadFont();
            InitializeComponent();
            timeline.Scrollbar = vScrollBar1;
            timeline.GroupList.ScrollbarH = hScrollBar1;
            timeline.GroupList.tree1 = treeView1;
            timeline.GroupList.tree2 = treeView2;
            
            objects = new List<int>();
        }

        private void openPCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "PCC Files(*.pcc)|*.pcc";
            if (d.ShowDialog() == DialogResult.OK)
            {
                LoadPCC(d.FileName);
            }
        }

        public void LoadPCC(string fileName)
        {
            try
            {
                LoadME3Package(fileName);
                RefreshCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message);
            }
        }

        public void RefreshCombo()
        {
            objects.Clear();
            for (int i = 0; i < pcc.Exports.Count; i++)
                if (pcc.Exports[i].ClassName == "InterpData")
                    objects.Add(i);
            toolStripComboBox1.Items.Clear();
            foreach (int i in objects)
                toolStripComboBox1.Items.Add("#" + i + " : " + pcc.Exports[i].ObjectName);
            if (toolStripComboBox1.Items.Count != 0)
                toolStripComboBox1.SelectedIndex = 0;
        }

        public void loadInterpData(int index)
        {
            timeline.GroupList.LoadInterpData(index, pcc as ME3Package);
            timeline.GroupList.OnCameraChanged(timeline.Camera);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
                return;
            loadInterpData(objects[n]);
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        private void loadAlternateTlkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TlkManager tm = new TlkManager();
            tm.InitTlkManager();
            tm.Show();
        }

        //for debugging purposes. not exposed to user
        private void InterpTrackScan_Click(object sender, EventArgs e)
        {
            KFreonLib.Debugging.DebugOutput.StartDebugger("Main ME3Explorer Form");
            string basepath = ME3Directory.cookedPath;
            string[] files = Directory.GetFiles(basepath, "*.pcc");
            List<string> conds = new List<string>();
            List<string> conds1 = new List<string>();

            //string property = Microsoft.VisualBasic.Interaction.InputBox("Please enter property name", "ME3 Explorer");
            string name;
            for (int f = 0; f < files.Length; f++)
            {
                string file = files[f];
                //KFreonLib.Debugging.DebugOutput.PrintLn((f + 1) + " / " + files.Length + " : " + file + " :", true);
                PCCPackage p = new PCCPackage(file, true, false, true);
                for (int i = 0; i < p.Exports.Count; i++)
                {
                    if (p.GetObjectClass(i).StartsWith("InterpTrackVisibility")) //GetObject(p.Exports[i].idxLink).StartsWith("InterpGroup"))
                    {
                        
                        List<ME3LibWV.PropertyReader.Property> props = ME3LibWV.PropertyReader.getPropList(p, p.Exports[i].Data);
                        foreach (ME3LibWV.PropertyReader.Property prop in props)
                        {
                            //KFreonLib.Debugging.DebugOutput.PrintLn(p.GetName(prop.Name));
                            if (p.GetName(prop.Name) == "VisibilityTrack")
                            {
                                int pos = 28;
                                int count = BitConverter.ToInt32(prop.raw, 24);
                                for (int j = 0; j < count; j++)
                                {
                                    List<ME3LibWV.PropertyReader.Property> p2 = ME3LibWV.PropertyReader.ReadProp(p, prop.raw, pos);
                                    for (int k = 0; k < p2.Count; k++)
                                    {
                                        name = p.GetName(p2[k].Name);
                                        if (name == "Action")
                                        {
                                            if (!conds.Contains(p.GetName(BitConverter.ToInt32(p2[k].raw, 32))))
                                            {
                                                conds.Add(p.GetName(BitConverter.ToInt32(p2[k].raw, 32)));
                                                KFreonLib.Debugging.DebugOutput.PrintLn("Action " + p.GetName(BitConverter.ToInt32(p2[k].raw, 24)) + ", " + p.GetName(BitConverter.ToInt32(p2[k].raw, 32)) + "               at: #" + i + " " + p.GetObjectPath(i + 1) + p.GetObjectClass(i + 1) + "        in: " + file.Substring(file.LastIndexOf(@"\") + 1), false);
                                            }
                                        }
                                        else if (name == "ActiveCondition")
                                        {
                                            if (!conds1.Contains(p.GetName(BitConverter.ToInt32(p2[k].raw, 32))))
                                            {
                                                conds1.Add(p.GetName(BitConverter.ToInt32(p2[k].raw, 32)));
                                                KFreonLib.Debugging.DebugOutput.PrintLn("ActiveCondition " + p.GetName(BitConverter.ToInt32(p2[k].raw, 24)) + ", " + p.GetName(BitConverter.ToInt32(p2[k].raw, 32)) + "               at: #" + i + " " + p.GetObjectPath(i + 1) + p.GetObjectClass(i + 1) + "        in: " + file.Substring(file.LastIndexOf(@"\") + 1), false);
                                            }
                                        }
                                        pos += p2[k].raw.Length;
                                    }
                                }
                            }
                            //if (p.GetName(prop.Name) == property)
                            //{
                            //    if(!conds.Contains(p.GetName(BitConverter.ToInt32(prop.raw, 32))))
                            //    {
                            //        conds.Add(p.GetName(BitConverter.ToInt32(prop.raw, 32)));
                            //        KFreonLib.Debugging.DebugOutput.PrintLn(p.GetName(BitConverter.ToInt32(prop.raw, 24)) + ", " + p.GetName(BitConverter.ToInt32(prop.raw, 32)) + "               at: #" + i + " " + p.GetObjectPath(i + 1) + p.GetObjectClass(i + 1) + "        in: " + file.Substring(file.LastIndexOf(@"\") + 1), false);
                            //    }
                            //}
                        }
                        //KFreonLib.Debugging.DebugOutput.PrintLn(i + " : " + p.GetObjectClass(i + 1) + "               at: " + p.GetObjectPath(i + 1) + "        in: " + file.Substring(file.LastIndexOf(@"\") + 1), false);
                    }
                }
                Application.DoEvents();
            }
            KFreonLib.Debugging.DebugOutput.PrintLn();
            KFreonLib.Debugging.DebugOutput.PrintLn("*****************");
            KFreonLib.Debugging.DebugOutput.PrintLn("Done");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pcc == null)
                return;
            pcc.save();
            MessageBox.Show("Done");
        }

        public override void handleUpdate(List<PackageUpdate> updates)
        {
            IEnumerable<PackageUpdate> relevantUpdates = updates.Where(x => x.change != PackageChange.Import &&
                                                                            x.change != PackageChange.ImportAdd &&
                                                                            x.change != PackageChange.Names);
            List<int> updatedExports = relevantUpdates.Select(x => x.index).ToList();
            if (updatedExports.Contains(timeline.GroupList.index))
            {
                //loaded InterpData is no longer an InterpData
                if (pcc.getExport(timeline.GroupList.index).ClassName != "InterpData")
                {
                    //?
                }
                else
                {
                    timeline.GroupList.LoadInterpData(timeline.GroupList.index, pcc as ME3Package);
                }
                updatedExports.Remove(timeline.GroupList.index);
            }
            if (updatedExports.Intersect(objects).Count() > 0)
            {
                RefreshCombo();
            }
            else
            {
                foreach (var i in updatedExports)
                {
                    if (pcc.getExport(i).ClassName == "InterpData")
                    {
                        RefreshCombo();
                        break;
                    }
                }
            }
        }
    }
}