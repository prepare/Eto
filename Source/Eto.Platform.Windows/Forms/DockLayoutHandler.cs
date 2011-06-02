using System;
using SD = System.Drawing;
using SWF = System.Windows.Forms;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.Platform.Windows
{
	public class DockLayoutHandler : WindowsLayout<object, DockLayout>, IDockLayout
	{
		Control child;
		Padding padding;
		
		public DockLayoutHandler()
		{
			padding = DockLayout.DefaultPadding;
		}
		
		
		public override object Control {
			get {
				return Widget.Container.ContainerObject;
			}
			protected set {
				base.Control = value;
			}
		}
		
		public Padding Padding {
			get {
				return padding;
			}
			set {
				padding = value;
				if (child != null)
				{
					SWF.Control c = (SWF.Control)child.ControlObject;
					c.Margin = Generator.Convert(padding);
				}
			}
		}

		public void Add(Control child)
		{
			SWF.Control childControl = (SWF.Control)child.ControlObject;
			childControl.Dock = SWF.DockStyle.Fill;
			childControl.Margin = Generator.Convert(padding);
			SWF.Control parent = (SWF.Control)Widget.Container.ContainerObject;
			parent.Controls.Add(childControl);
			this.child = child;
		}

		public void Remove(Control child)
		{
			SWF.Control parent = (SWF.Control)Widget.Container.ControlObject;
			parent.Controls.Remove((SWF.Control)child.ControlObject);
			this.child = null;
		}
	}
}
