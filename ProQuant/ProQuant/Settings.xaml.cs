using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProQuant
{ 

	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Settings : ContentPage
	{
        Connection Maincnx;

		public Settings (Connection cnx, List<Settings> settings)
		{
			InitializeComponent ();
            Maincnx = cnx;

		}
	}
}