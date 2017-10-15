using Aga.Controls.Tree;
using MsaSQLEditor;
using MsaSQLEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ExportQueriesPlugin
{
    public class Main : IPlugin2
    {
        public string Name => "Export Queries";
        private IPluginContext2 _Context;
        private ToolStripMenuItem _ExportQueriesButton;
        private ISystemWindow _QueriesWindow;

        public void Initialize(IPluginContext2 context)
        {
            _Context = context;
            _ExportQueriesButton = (_Context.MainMenu.Items["MMTools"] as ToolStripDropDownButton)
                .DropDownItems.Add("Export Selected Queries") as ToolStripMenuItem;
            _QueriesWindow = _Context.SystemWindows
                .Where<ISystemWindow>(w => w.Text == "Queries")
                .FirstOrDefault();

            _ExportQueriesButton.Click += _ExportQueriesButton_Click;
        }

        private void _ExportQueriesButton_Click(object sender, EventArgs e)
        {
            var list = (_QueriesWindow as Form)
                .Controls["TreeList"] as TreeViewAdv;

            var toExport = list.SelectedNodes
                .Select<TreeNodeAdv, string>(n => n.ToString())
                .ToArray<string>();

            using (FolderBrowserDialog fileDialog = new FolderBrowserDialog())
            {
                fileDialog.ShowNewFolderButton = true;

                if(fileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string query in toExport)
                    {
                        string sql = GetQueryPatchedSQL(query);

                        File.WriteAllText(Path.Combine(fileDialog.SelectedPath, query + ".sql"), sql);
                    }
                }
            }
        }

        private string GetQueryPatchedSQL(string query)
        {
            return ((dynamic)_Context.Database)
                    .GetQueryElement(query).GetQueryPatchedSQL();
        }

        private string GetQueryFullTextSQL(string query)
        {
            return ((dynamic)_Context.Database)
                    .GetQueryElement(query).GetQueryFullTextSQL();
        }

        public void Unload()
        {
            _ExportQueriesButton.Dispose();
            _Context = null;
        }
    }
}
