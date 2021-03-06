﻿using System;
using NUnit.Framework;
using Eto.Forms;
namespace Eto.Test.UnitTests.Forms
{
	/// <summary>
	/// Tests to ensure the DataContext and DataContextChanged event act appropriately
	/// - DataContextChanged should only be fired max once per control, regardless of how or when they are constructed and added to the tree
	/// - Themed controls (visual tree) should not participate in logical tree data context
	/// </summary>
	[TestFixture]
	public class DataContextTests : TestBase
	{
		[Handler(typeof(IHandler))]
		public class CustomExpander : Expander
		{
			public new interface IHandler : Expander.IHandler { }
		}

		public class CustomExpanderHandler : Eto.Forms.ThemedControls.ThemedExpanderHandler, CustomExpander.IHandler
		{
			int dataContextChanged;
			int contentDataContextChanged;
			Panel content;

			class MyViewModel2
			{
			}

			public override Control Content
			{
				get { return content.Content; }
				set { content.Content = value; }
			}

			protected override void Initialize()
			{
				base.Initialize();

				content = new Panel();
				Control.DataContextChanged += (sender, e) => dataContextChanged++;
				content.DataContextChanged += (sender, e) => contentDataContextChanged++;

				base.Content = content;
				Assert.AreEqual(0, dataContextChanged);
				Assert.AreEqual(0, contentDataContextChanged);
				Control.DataContext = new MyViewModel2(); // this shouldn't fire data context changes for logical children
				Assert.AreEqual(1, dataContextChanged);
				Assert.AreEqual(1, contentDataContextChanged);
			}

			public override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				Assert.IsInstanceOf<MyViewModel2>(Control.DataContext);
				Assert.IsInstanceOf<MyViewModel2>(content.DataContext);

				Control.DataContext = new MyViewModel2(); // this shouldn't fire data context changes for logical children
				Assert.AreEqual(2, dataContextChanged);
				Assert.AreEqual(2, contentDataContextChanged);
				Assert.IsInstanceOf<MyViewModel2>(Control.DataContext);
				Assert.IsInstanceOf<MyViewModel2>(content.DataContext);
}

			public override void OnLoadComplete(EventArgs e)
			{
				base.OnLoadComplete(e);
				Assert.AreEqual(2, dataContextChanged);
				Assert.AreEqual(2, contentDataContextChanged);
				Assert.IsInstanceOf<MyViewModel2>(Control.DataContext);
				Assert.IsInstanceOf<MyViewModel2>(content.DataContext);
			}
		}

		static DataContextTests()
		{
			Platform.Instance.Add<CustomExpander.IHandler>(() => new CustomExpanderHandler());
		}

		public class MyViewModel { }

		[Test]
		public void DataContextChangedShouldNotFireWhenNoContext()
		{
			int dataContextChanged = 0;
			Shown(form =>
			{
				form.DataContextChanged += (sender, e) => dataContextChanged++;
				var c = new Panel();
				c.DataContextChanged += (sender, e) => dataContextChanged++;
				form.Content = c;
				Assert.AreEqual(0, dataContextChanged);
				Assert.IsNull(form.DataContext);
				Assert.IsNull(c.DataContext);
			}, () =>
			{
				Assert.AreEqual(0, dataContextChanged);
			});
		}

		[Test]
		public void DataContextChangedShouldFireAfterSet()
		{
			int dataContextChanged = 0;
			MyViewModel dataContext;
			Shown(form =>
			{
				var c = new Panel();
				c.DataContextChanged += (sender, e) => dataContextChanged++;
				c.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsInstanceOf<MyViewModel>(c.DataContext);
				Assert.AreSame(dataContext, c.DataContext);

				c.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(2, dataContextChanged);
				Assert.IsInstanceOf<MyViewModel>(c.DataContext);
				Assert.AreSame(dataContext, c.DataContext);

				form.Content = c;
				Assert.AreEqual(2, dataContextChanged);
			}, () =>
			{
				Assert.AreEqual(2, dataContextChanged);
			});
		}

		[Test]
		public void DataContextChangedShouldFireForThemedControl()
		{
			int dataContextChanged = 0;
			MyViewModel dataContext = null;
			Panel c = null;
			Shown(form =>
			{
				c = new Panel();
				c.DataContextChanged += (sender, e) => dataContextChanged++;
				form.Content = new CustomExpander { Content = c };
				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsInstanceOf<MyViewModel>(c.DataContext);
				Assert.IsInstanceOf<MyViewModel>(form.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
				Assert.AreSame(dataContext, form.Content.DataContext);
				return form;
			}, form =>
			{
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsInstanceOf<MyViewModel>(c?.DataContext);
				Assert.IsInstanceOf<MyViewModel>(form.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
				Assert.AreSame(dataContext, form.Content.DataContext);
			});
		}

		[Test]
		public void DataContextChangedShouldFireWhenSettingContentAfterLoaded()
		{
			int dataContextChanged = 0;
			int contentDataContextChanged = 0;
			MyViewModel dataContext = null;
			Shown(form =>
			{
				form.DataContextChanged += (sender, e) => dataContextChanged++;
				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsInstanceOf<MyViewModel>(form.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
				return form;
			}, form =>
			{
				var c = new Panel();
				c.DataContextChanged += (sender, e) => contentDataContextChanged++;
				form.Content = c;
				Assert.AreEqual(1, contentDataContextChanged);
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsInstanceOf<MyViewModel>(c.DataContext);
				Assert.IsInstanceOf<MyViewModel>(form.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);

				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(2, contentDataContextChanged);
				Assert.AreEqual(2, dataContextChanged);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
			});
		}

		[Test]
		public void DataContextChangedShouldFireWhenSettingContentAfterLoadedWithThemedControl()
		{
			int dataContextChanged = 0;
			int contentDataContextChanged = 0;
			MyViewModel dataContext = null;
			Shown(form =>
			{
				form.DataContextChanged += (sender, e) => dataContextChanged++;
				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.AreSame(dataContext, form.DataContext);
				return form;
			}, form =>
			{
				var c = new Panel();
				c.DataContextChanged += (sender, e) => contentDataContextChanged++;
				form.Content = new CustomExpander { Content = c };
				Assert.AreEqual(1, contentDataContextChanged);
				Assert.AreEqual(1, dataContextChanged);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);

				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(2, contentDataContextChanged);
				Assert.AreEqual(2, dataContextChanged);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
			});
		}

		[Test]
		public void DataContextChangedShouldFireForChildWithCustomModel()
		{
			int dataContextChanged = 0;
			int childDataContextChanged = 0;
			MyViewModel dataContext;
			MyViewModel childDataContext;
			Shown(form =>
			{
				var container = new Panel();
				container.DataContextChanged += (sender, e) => dataContextChanged++;
				container.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.AreSame(dataContext, container.DataContext);

				var child = new Panel();
				child.DataContextChanged += (sender, e) => childDataContextChanged++;
				child.DataContext = childDataContext = new MyViewModel();
				container.Content = child;
				form.Content = container;

				Assert.AreEqual(1, childDataContextChanged);
				Assert.AreSame(dataContext, container.DataContext);
				Assert.AreSame(childDataContext, child.DataContext);
			}, () =>
			{
				Assert.AreEqual(1, dataContextChanged);
				Assert.AreEqual(1, childDataContextChanged);
			});
		}

		[Test]
		public void DataContextChangeShouldFireForControlInStackLayout()
		{
			int dataContextChanged = 0;
			MyViewModel dataContext = null;
			Panel c = null;
			Shown(form =>
			{
				c = new Panel();
				c.DataContextChanged += (sender, e) => 
					dataContextChanged++;

				form.Content = new StackLayout
				{
					Items = { c }
				};
				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsNotNull(c.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
				return form;
			}, form =>
			{
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsNotNull(c.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
			});
		}

		[Test]
		public void DataContextChangeShouldFireForControlInTableLayout()
		{
			int dataContextChanged = 0;
			MyViewModel dataContext = null;
			Panel c = null;
			Shown(form =>
			{
				c = new Panel();
				c.DataContextChanged += (sender, e) => dataContextChanged++;

				form.Content = new TableLayout
				{
					Rows = { c }
				};
				form.DataContext = dataContext = new MyViewModel();
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsNotNull(c.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
				return form;
			}, form =>
			{
				Assert.AreEqual(1, dataContextChanged);
				Assert.IsNotNull(c.DataContext);
				Assert.AreSame(dataContext, c.DataContext);
				Assert.AreSame(dataContext, form.DataContext);
			});
		}
	}
}

