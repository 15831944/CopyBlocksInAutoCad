// (C) Copyright 2017 by Microsoft 
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(CopyBlocks.MyCommands))]

namespace CopyBlocks
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        [CommandMethod("CopyBlockByName")]
        static public void CopyBlock()
        {
            SelectBlock sb = new SelectBlock();
            sb.Show();
        }

        string name = "";
        public void Names(string n)
        {
            name = n;
            BlockJig();
        }

        [CommandMethod("BlockJig")]
        public void BlockJig()
        {
            Editor ed =
                    Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = ed.Document.Database;
            using (DocumentLock acLckDoc = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                try
                {
                    using (Transaction tr =
                                       db.TransactionManager.StartTransaction())
                    {
                        // Find the "TEST" block in the current drawing
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId,
                                                                 OpenMode.ForRead);
                        BlockTableRecord block = (BlockTableRecord)tr.GetObject(
                                                     bt[name], OpenMode.ForRead);

                        // Create the Jig and ask the user to place the block
                        //----------------------------------------------------
                        MyBlockJig blockJig = new MyBlockJig();
                        Point3d point;
                        PromptResult res = blockJig.DragMe(block.ObjectId,
                                                            out point);
                        if (res.Status == PromptStatus.OK)
                        {
                            // Now we need to do the usual steps to place
                            //   the block insert at the position where the user
                            //   did the click

                            BlockTableRecord curSpace =
                              (BlockTableRecord)tr.GetObject(
                                               db.CurrentSpaceId, OpenMode.ForWrite);
                            BlockReference insert = new BlockReference(point,
                                                                     block.ObjectId);
                            curSpace.AppendEntity(insert);
                            tr.AddNewlyCreatedDBObject(insert, true);
                        }
                        tr.Commit();
                    } // using
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage(ex.ToString());
                }
            }
        } // BlockJig()

    } // class BlockJigCmds


    // This Jig will show the given block during the dragging.
    //=================================================================
    public class MyBlockJig : DrawJig
    {
        public Point3d _point;
        private ObjectId _blockId = ObjectId.Null;

        // Shows the block until the user clicks a point.
        // The 1st parameter is the Id of the block definition.
        // The 2nd is the clicked point.
        //---------------------------------------------------------------
        public PromptResult DragMe(ObjectId i_blockId, out Point3d o_pnt)
        {
            _blockId = i_blockId;
            Editor ed =
                    Application.DocumentManager.MdiActiveDocument.Editor;

            PromptResult jigRes = ed.Drag(this);
            o_pnt = _point;
            return jigRes;
        }


        // Need to override this method.
        // Updating the current position of the block.
        //--------------------------------------------------------------
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpts = new JigPromptPointOptions();
            jigOpts.UserInputControls =
                                (UserInputControls.Accept3dCoordinates |
                                 UserInputControls.NullResponseAccepted);
            jigOpts.Message = "Select a point:";
            PromptPointResult jigRes = prompts.AcquirePoint(jigOpts);

            Point3d pt = jigRes.Value;
            if (pt == _point)
                return SamplerStatus.NoChange;

            _point = pt;
            if (jigRes.Status == PromptStatus.OK)
                return SamplerStatus.OK;

            return SamplerStatus.Cancel;
        }


        // Need to override this method.
        // We are showing our block in its current position here.
        //--------------------------------------------------------------
        protected override bool WorldDraw(
                      Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            BlockReference inMemoryBlockInsert =
                                     new BlockReference(_point, _blockId);
            draw.Geometry.Draw(inMemoryBlockInsert);

            inMemoryBlockInsert.Dispose();

            return true;
        } // WorldDraw()

    }

}
