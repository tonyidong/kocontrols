using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace KO.Controls.Common
{
	public static class Utils
	{
		public static void MakeColumnHeadersInvisible(GridView gridView)
		{
			Style st = new Style(typeof(GridViewColumnHeader));
			st.Setters.Add(new Setter(GridViewColumnHeader.VisibilityProperty, Visibility.Collapsed));
			gridView.ColumnHeaderContainerStyle = st;
		}
	}
}
