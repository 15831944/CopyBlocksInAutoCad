using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.AutoCAD.ApplicationServices;

[assembly: CommandClass(typeof(CopyBlocks.MyCommands))]

namespace CopyBlocks
{

    public partial class SelectBlock : Form
    {
        public SelectBlock()
        {
            InitializeComponent();
        }

        private void SelectBlock_Load(object sender, EventArgs e)
        {
            Editor ed =
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = ed.Document.Database;
            try
            {
                using (Transaction tr =
                                   db.TransactionManager.StartTransaction())
                {
                    // Find the "TEST" block in the current drawing
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId,
                                                             OpenMode.ForRead);



                    foreach (ObjectId objId in bt)
                    {
                        BlockTableRecord btr;
                        btr = tr.GetObject(objId, OpenMode.ForRead) as BlockTableRecord;
                        if (btr.Name != "*Model_Space" && btr.Name != "*Paper_Space" && btr.Name != "*Paper_Space0")
                            cmb1.Items.Add(btr.Name);
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                MyCommands mc = new MyCommands();
                SelectBlock b = new SelectBlock();
                if (cmb1.Text.Equals(""))
                {
                    System.Windows.Forms.MessageBox.Show("Не избравте блок!");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Префрлетесе во AutoCad и изберете точка за вметнување на блокот " + cmb1.Text);
                    mc.Names(cmb1.Text);
                }
            }
        }
    }
}
