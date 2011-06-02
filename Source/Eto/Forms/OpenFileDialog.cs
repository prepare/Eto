using System;

namespace Eto.Forms
{
	public interface IOpenFileDialog : IFileDialog
	{
	}
	
	public class OpenFileDialog : FileDialog
	{
		//IOpenFileDialog inner;

		public OpenFileDialog(Generator g) : base(g, typeof(IOpenFileDialog))
		{
			//inner = (IOpenFileDialog)InnerControl;
		}

	}
}
