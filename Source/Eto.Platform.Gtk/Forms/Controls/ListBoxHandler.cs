using System;
using Eto.Forms;
using System.Linq;
using System.Collections.Generic;

namespace Eto.Platform.GtkSharp
{
	public class ListBoxHandler : GtkControl<Gtk.ScrolledWindow, ListBox>, IListBox
	{
		Gtk.TreeView tree;
		Gtk.TreeStore store;


		public ListBoxHandler()
		{
			store = new Gtk.TreeStore(typeof(string));

			Control = new Gtk.ScrolledWindow();
			Control.ShadowType = Gtk.ShadowType.In;
			tree = new Gtk.TreeView(store);
			Control.Add(tree);

			tree.AppendColumn("Data", new Gtk.CellRendererText(), "text", 0);
			tree.HeadersVisible = false;
			tree.Selection.Changed += Selection_Changed;
			tree.RowActivated += new Gtk.RowActivatedHandler(tree_RowActivated);
		}

		public override void Focus()
		{
			tree.GrabFocus();
		}

		#region IListControl Members

		public void AddRange (IEnumerable<object> collection)
		{
			foreach (object o in collection)
				AddItem(o);
		}
		
		public void AddItem(object item)
		{
			store.AppendValues(item);
		}

		public void RemoveItem(object item)
		{
			Gtk.TreePath path = new Gtk.TreePath();
			path.AppendIndex(((ListBox)Widget).Items.IndexOf(item));
			Gtk.TreeIter iter;
			store.GetIter(out iter, path);
			store.Remove(ref iter);
		}

		public int SelectedIndex
		{
			get
			{
				Gtk.TreeIter iter;
				
				if (tree.Selection != null && tree.Selection.GetSelected(out iter))
				{
					object val = store.GetValue(iter, 0);
					if (val != null)
					{
						return ((ListBox)Widget).Items.IndexOf(val);
					}
				}
				
				return -1;
			}
			set
			{
				if (value == -1) {
					if (tree.Selection != null) tree.Selection.UnselectAll ();
					return;
				}
				Gtk.TreePath path = new Gtk.TreePath();
				path.AppendIndex(value);
				Gtk.TreeViewColumn focus_column = tree.Columns[0];
				
				tree.SetCursor(path, focus_column, false);
			}
		}

		public void RemoveAll()
		{
			store.Clear();
		}

		#endregion

		private void Selection_Changed(object sender, EventArgs e)
		{
			Widget.OnSelectedIndexChanged(EventArgs.Empty);
		}

		private void tree_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Widget.OnActivated(EventArgs.Empty);
		}
	}
}
