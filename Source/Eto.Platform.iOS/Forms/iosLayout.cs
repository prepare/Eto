using System;
using Eto.Forms;
using SD = System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Eto.Platform.iOS.Forms
{
	public interface IiosLayout {
		object LayoutObject { get; }		
	}
	
	public abstract class iosLayout<T, W> : iosObject<T, W>, ILayout, IiosLayout
		where T: NSObject
		where W: Layout
	{
		public virtual object LayoutObject
		{
			get { return null; }
		}

	}
}
