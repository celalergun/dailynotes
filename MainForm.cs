using DailyNotes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notlar
{
    public partial class MainForm : Form
    {
        DailyNotesEntities _dbContext = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Database.SetInitializer(new CreateDatabaseIfNotExists<DailyNotesEntities>());
            RecreateDataBindings();

        }
        public MainForm()
        {
            InitializeComponent();
            DBHelper localDB = new DBHelper();
        }

        private void dgvMaster_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.S))
            {
                dgvMaster.EndEdit();
                dgvDetail.EndEdit();
                SaveDatabase();
            }
        }

        private void newRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem menu = sender as ToolStripItem;
            if (menu != null)
            {
                ContextMenuStrip popup = menu.Owner as ContextMenuStrip;
                if (popup != null)
                {
                    Control src = popup.SourceControl;
                    DataGridView grid = src as DataGridView;

                    if (grid == dgvMaster)
                    {
                        MasterNote master = (MasterNote)masterNoteBindingSource.AddNew();
                        masterNoteBindingSource.EndEdit();
                        _dbContext.SaveChanges();
                        _dbContext.Entry(master).GetDatabaseValues();
                    }

                    if (grid == dgvDetail)
                    {
                        DetailNote detail = (DetailNote)detailNotesBindingSource.AddNew();
                        detailNotesBindingSource.EndEdit();
                        _dbContext.SaveChanges();
                        _dbContext.Entry(detail).GetDatabaseValues();
                    }
                }
            }
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveDatabase();
        }

        private void SaveDatabase()
        {
            EndEditOnAllBindingSources();
            _dbContext.SaveChanges();
            var context = ((IObjectContextAdapter)_dbContext).ObjectContext;
            var refreshableObjects = (from entry in context.ObjectStateManager.GetObjectStateEntries(
                                                       EntityState.Added
                                                       | EntityState.Deleted
                                                       | EntityState.Modified
                                                       | EntityState.Unchanged)
                                      where entry.EntityKey != null
                                      select entry.Entity).ToList();

            context.Refresh(RefreshMode.StoreWins, refreshableObjects);
        }

        private void EndEditOnAllBindingSources()
        {
            var BindingSourcesQuery =
                from Component bindingSources in this.components.Components
                where bindingSources is BindingSource
                select bindingSources;

            foreach (BindingSource bindingSource in BindingSourcesQuery)
            {
                bindingSource.EndEdit();
            }
        }

        private void cancelChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecreateDataBindings();
        }

        private void RecreateDataBindings()
        {
            
            if (_dbContext != null)
                _dbContext.Dispose();

            _dbContext = new DailyNotesEntities();
            _dbContext.MasterNotes.Load();
   
            masterNoteBindingSource.DataSource = _dbContext.MasterNotes.Local.ToBindingList();
            masterNoteBindingSource.Sort = "CreateDate";
        }

        private void deleteRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem menu = sender as ToolStripItem;
            if (menu != null)
            {
                ContextMenuStrip popup = menu.Owner as ContextMenuStrip;
                if (popup != null)
                {
                    Control src = popup.SourceControl;
                    DataGridView grid = src as DataGridView;
                    
                    // check to see if the sender is Master
                    if (grid == dgvMaster)
                    {
                        masterNoteBindingSource.Remove(masterNoteBindingSource.Current);
                    }
                    
                    // check to see if the sender is Detail
                    if (grid == dgvDetail)
                    {
                        detailNotesBindingSource.Remove(detailNotesBindingSource.Current);
                    }

                    // 
                    _dbContext.SaveChanges();
                }
            }
        }

        private void masterNoteBindingSource_AddingNew(object sender, AddingNewEventArgs e)
        {
            MasterNote master = new MasterNote();
            master.CreateDate = DateTime.Now;
            master.Description = "Enter subject here";
            master.IsVisible = true;
            e.NewObject = master;
        }

        private void detailNotesBindingSource_AddingNew(object sender, AddingNewEventArgs e)
        {
            DetailNote detail = new DetailNote();
            detail.IsCompleted = false;
            detail.StartDate = DateTime.Now;
            detail.Description = "Enter your notes here";
            e.NewObject = detail;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                _dbContext.SaveChanges();
                return;
            }
                
            bool changesMade = _dbContext.ChangeTracker.HasChanges();
            if (changesMade)
            {
                DialogResult res = MessageBox.Show("Veritabanında kaydedilmemiş değişiklikler var. Programdan çıkmadan önce kaydetmek ister misiniz?", "Uyarı", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                    _dbContext.SaveChanges();
                else if (res == DialogResult.No)
                    RecreateDataBindings();
                else if (res == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }
    }
}
