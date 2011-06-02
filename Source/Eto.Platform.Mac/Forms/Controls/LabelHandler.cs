using System;
using System.Reflection;
using SD = System.Drawing;
using Eto.Forms;
using Eto.Drawing;
using MonoMac.AppKit;
using Eto.Platform.Mac.Drawing;
using MonoMac.Foundation;

namespace Eto.Platform.Mac
{
	public class LabelHandler : MacView<NSTextField, Label>, ILabel
	{
		Font font;
		
		class MyTextFieldCell : NSTextFieldCell
		{
			public LabelHandler Handler { get; set; }

			public override SD.RectangleF TitleRectForBounds (SD.RectangleF theRect)
			{
				SD.RectangleF titleFrame = base.TitleRectForBounds (theRect);
				var titleSize = this.CellSizeForBounds (theRect);
				//var titleSize = this.AttributedStringValue.BoundingRect(theRect.Size, (NSStringDrawingOptions)0).Size;
				//NSString str = new NSString(this.AttributedStringValue.Value);
				//var attributes = new NSMutableDictionary();
				//dic.Add();
				//SD.SizeF titleSize = str.BoundingRectWithSize(theRect.Size, (NSStringDrawingOptions)0, attributes);
				switch (Handler.VerticalAlign) {
				case VerticalAlign.Middle:
					titleFrame.Y = theRect.Y + (theRect.Height - titleSize.Height) / 2.0F;
					break;
				case VerticalAlign.Top:
					// do nothing!
					break;
				case VerticalAlign.Bottom:
					titleFrame.Y = theRect.Y + (theRect.Height - titleSize.Height);
					break;
				}
				return titleFrame;
			}
			
			public override void DrawInteriorWithFrame (SD.RectangleF cellFrame, NSView inView)
			{
				var titleRect = this.TitleRectForBounds (cellFrame);
				this.AttributedStringValue.DrawString (titleRect);
			}
		}
		
		public LabelHandler ()
		{
			Control = new NSTextField ();
			Control.Cell = new MyTextFieldCell{ Handler = this };
			//Control.Cell.Wraps = true;
			//Control.SetFrameSize(new SD.SizeF(0, 0));
			Control.DrawsBackground = false;
			Control.Bordered = false;
			Control.Bezeled = false;
			Control.Editable = false;
			Control.Selectable = false;
			Control.Alignment = NSTextAlignment.Left;
			Control.Cell.UsesSingleLineMode = false;
			Control.Cell.LineBreakMode = NSLineBreakMode.ByWordWrapping;
			//var ps = new NSMutableParagraphStyle();
			//ps.SetLineBreakMode(NSLineBreakMode.ByWordWrapping);
			//Control.DefaultParagraphStyle = ps;
			//Control.VerticallyResizable = true;
		}
		/*
		public override Size PositionOffset {
			get {
				return LabelPositionOffset;
			}
		}*/
		
		public WrapMode Wrap {
			get {
				if (Control.Cell.UsesSingleLineMode)
					return WrapMode.None;
				else if (Control.Cell.LineBreakMode == NSLineBreakMode.ByWordWrapping)
					return WrapMode.Word;
				else //if (Control.Cell.LineBreakMode == NSLineBreakMode.ByWordWrapping)
					return WrapMode.Character;
			}
			set {
				switch (value) {
				case WrapMode.None:
					Control.Cell.UsesSingleLineMode = true;
					Control.Cell.LineBreakMode = NSLineBreakMode.Clipping;
					break;
				case WrapMode.Word:
					Control.Cell.UsesSingleLineMode = false;
					Control.Cell.LineBreakMode = NSLineBreakMode.ByWordWrapping;
					break;
				case WrapMode.Character:
					Control.Cell.UsesSingleLineMode = false;
					Control.Cell.LineBreakMode = NSLineBreakMode.CharWrapping;
					break;
				default:
					throw new NotSupportedException ();
				}
			}
		}
		
		public string Text {
			get {
				return Control.StringValue;
			}
			set {
				Control.StringValue = value;
			}
		}
		
		public HorizontalAlign HorizontalAlign {
			get {
				switch (Control.Alignment) {
				case NSTextAlignment.Center:
					return HorizontalAlign.Center;
				case NSTextAlignment.Right:
					return HorizontalAlign.Right;
				default:
				case NSTextAlignment.Left:
					return HorizontalAlign.Left;
				}
			}
			set {
				switch (value) {
				case HorizontalAlign.Center:
					Control.Alignment = NSTextAlignment.Center;
					break;
				case HorizontalAlign.Right:
					Control.Alignment = NSTextAlignment.Right;
					break;
				case HorizontalAlign.Left:
					Control.Alignment = NSTextAlignment.Left;
					break;
				}
			}
		}
		
		public Eto.Drawing.Font Font {
			get {
				return font;
			}
			set {
				font = value;
				if (font != null)
					Control.Font = ((FontHandler)font.Handler).GetFont ();
				else
					Control.Font = NSFont.LabelFontOfSize (NSFont.LabelFontSize);
			}
		}
		
		public VerticalAlign VerticalAlign { get; set; }

	}
}
