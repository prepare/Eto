using System;
using System.Linq;
using Eto.Drawing;
using SD = System.Drawing;
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Eto.Platform.Mac.Drawing
{

	public class RegionHandler : Region
	{
		//Gdk.Region region;
		//Gdk.Region original;
		public RegionHandler()
		{
			//this.original = region;
			//this.region = region.Copy();
		}

		public override object ControlObject
		{
			get { return null; }
		}

		public override void Exclude(Rectangle rect)
		{
			//Gdk.Region r = new Gdk.Region();
			//r.UnionWithRect(Generator.Convert(rect));
			//region.Subtract(r);
		}

		public override void Reset()
		{
			//region = original;
		}

		public override void Set(Rectangle rect)
		{
			//region.Empty();
			//region.UnionWithRect(Generator.Convert(rect));
		}
	}

	public class GraphicsHandler : MacObject<NSGraphicsContext, Graphics>, IGraphics
	{
		CGContext context;
		NSView view;
		float height;
		bool adjust;
		
		public bool Flipped
		{
			get;
			private set;
		}

		public GraphicsHandler()
		{
		}

		public GraphicsHandler(NSView view)
		{
			this.view = view;
			this.Control = NSGraphicsContext.FromWindow(view.Window);
			this.context = this.Control.GraphicsPort;
			context.SaveState();
			context.ClipToRect(view.ConvertRectToBase(view.Bounds));
			AddObserver(NSView.NSViewFrameDidChangeNotification, delegate { 
				context.RestoreState();
				context.ClipToRect(view.ConvertRectToBase(view.Bounds));
				context.SaveState();
			}, view);
			this.adjust = !view.IsFlipped;
			this.Flipped = this.adjust;
			context.InterpolationQuality = CGInterpolationQuality.High;
			context.SetAllowsSubpixelPositioning(false);
		}

		public GraphicsHandler(NSGraphicsContext gc, float height)
		{
			this.height = height;
			this.adjust = true;
			this.Flipped = true;
			this.Control = gc;
			this.context = gc.GraphicsPort;
			context.InterpolationQuality = CGInterpolationQuality.High;
			//context.ScaleCTM(1, -1);
			context.SetAllowsSubpixelPositioning(false);
		}


		public void CreateFromImage(Bitmap image)
		{
			NSImage nsimage = (NSImage)image.ControlObject;
			
			var rep = nsimage.Representations().OfType<NSBitmapImageRep>().FirstOrDefault();
			Control = NSGraphicsContext.FromBitmap(rep);
			context = Control.GraphicsPort;
			context.InterpolationQuality = CGInterpolationQuality.High;
			context.SetAllowsSubpixelPositioning(false);
		}

		public void Commit()
		{
			/*if (image != null)
			{
				Gdk.Pixbuf pb = (Gdk.Pixbuf)image.ControlObject;
				pb.GetFromDrawable(drawable, drawable.Colormap, 0, 0, 0, 0, image.Size.Width, image.Size.Height);
				gc.Dispose();
				gc = null;
				drawable.Dispose();
				drawable = null;
			}*/
		}
		
		public void Flush()
		{
			Control.FlushGraphics();
		}

		public SD.PointF TranslateView(SD.PointF point, bool halfers = false)
		{
			if (halfers) {
				point.X += 0.5F;
				point.Y += 0.5F;
			}
			if (view != null) 
			{
				if (adjust) point.Y = view.Bounds.Height - point.Y - 1;
				point = view.ConvertPointToBase(point);
			}
			else if (adjust) point.Y = this.height - point.Y - 1;
			return point;
		}
		
		public SD.RectangleF TranslateView(SD.RectangleF rect, bool halfers = false)
		{
			if (halfers) {
				rect.X += 0.5F;
				rect.Y += 0.5F;
				rect.Width -= 0.5F;
				rect.Height -= 0.5F;
			}

			if (view != null) {
				if (adjust) rect.Y = view.Bounds.Height - rect.Y - rect.Height;
				rect = view.ConvertRectToBase(rect);	
			}
			else if (adjust) rect.Y = this.height - rect.Y - rect.Height;
			return rect;
		}

		public SD.RectangleF Translate(SD.RectangleF rect, float height)
		{
			rect.Y = height - rect.Y - rect.Height;
			return rect;
		}
		
		public void DrawLine (Color color, int startx, int starty, int endx, int endy)
		{
			NSGraphicsContext.CurrentContext = this.Control;
			context.SetStrokeColorWithColor(Generator.Convert(color));
			context.SetShouldAntialias(false);
			context.SetLineCap(CGLineCap.Square);
			context.SetLineWidth(1.0F);
			context.StrokeLineSegments(new SD.PointF[] { TranslateView(new SD.PointF(startx, starty)), TranslateView(new SD.PointF(endx, endy)) });
		}

		public void DrawRectangle(Color color, int x, int y, int width, int height)
		{
			NSGraphicsContext.CurrentContext = this.Control;
			var rect = new System.Drawing.RectangleF(x, y, width, height);
			context.SetStrokeColorWithColor(Generator.Convert(color));
			context.SetShouldAntialias(false);
			context.SetLineWidth(1.0F);
			context.StrokeRect(TranslateView(rect));
		}

		public void FillRectangle(Color color, int x, int y, int width, int height)
		{
		/*	if (width == 1 || height == 1)
			{
				DrawLine(color, x, y, x+width-1, y+height-1);
				return;
			}*/
			
			NSGraphicsContext.CurrentContext = this.Control;
			context.SetFillColorWithColor(Generator.Convert(color));
			context.SetShouldAntialias(false);
			context.FillRect(TranslateView(new SD.RectangleF(x, y, width, height)));
		}

		public void DrawImage(IImage image, int x, int y)
		{
			NSGraphicsContext.CurrentContext = this.Control;

			var handler = image.Handler as IImageHandler;
			handler.DrawImage(this, x, y);
		}

		public void DrawImage(IImage image, int x, int y, int width, int height)
		{
			NSGraphicsContext.CurrentContext = this.Control;

			var handler = image.Handler as IImageHandler;
			handler.DrawImage(this, x, y, width, height);
		}

		public void DrawImage(IImage image, Rectangle source, Rectangle destination)
		{
			NSGraphicsContext.CurrentContext = this.Control;

			var handler = image.Handler as IImageHandler;
			handler.DrawImage(this, source, destination);
		}

		public void DrawIcon(Icon icon, int x, int y, int width, int height)
		{
			NSGraphicsContext.CurrentContext = this.Control;

			var nsimage = icon.ControlObject as NSImage;
			var sourceRect = Translate(new SD.RectangleF(0, 0, nsimage.Size.Width, nsimage.Size.Height), nsimage.Size.Height);
			var destRect = TranslateView(new SD.RectangleF(x, y, width, height), false);
			nsimage.Draw(destRect, sourceRect, NSCompositingOperation.SourceOver, 1);
		}

		public Region ClipRegion
		{
			get { return null; } //new RegionHandler(drawable.ClipRegion); }
			set
			{
				//gc.ClipRegion = (Gdk.Region)((RegionHandler)value).ControlObject;
			}
		}

		public void DrawText(Font font, Color color, int x, int y, string text)
		{
			NSGraphicsContext.CurrentContext = Control;
			
			var str = new NSString(text);
			var fontHandler = font.Handler as FontHandler;
			var dic = new NSMutableDictionary();
			dic.Add(NSAttributedString.ForegroundColorAttributeName, Generator.ConvertNS(color));
			dic.Add(NSAttributedString.FontAttributeName, fontHandler.GetFont());
			var size = str.StringSize(dic);
			context.SetShouldAntialias(true);
			str.DrawString(new SD.PointF(x, height - y - size.Height), dic);
		}

		public SizeF MeasureString(Font font, string text)
		{
			NSGraphicsContext.CurrentContext = Control;
			var fontHandler = font.Handler as FontHandler;
			var dic = new NSMutableDictionary();
			dic.Add(NSAttributedString.FontAttributeName, fontHandler.GetFont());
			var str = new NSString(text);
			var size = str.StringSize(dic);
			return new SizeF(size.Width, size.Height);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (Control != null) Control.FlushGraphics();
			}
			base.Dispose (disposing);
		}

	}
}
