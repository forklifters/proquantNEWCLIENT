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
	public partial class SettingSpecific : ContentPage
	{
        Settings _setting;
		public SettingSpecific (Settings setting)
		{
			InitializeComponent ();

            Start();
		}

        public void Start()
        {
            Description.Text = _setting.description;
            OrigionalValue.Text = $"{_setting.uom} {_setting.original}";
            NewValue.Placeholder = _setting.value;
        }

        private void UpdateClicked(object sender, EventArgs e)
        {
            //do checks for max and min.
            _setting.value = NewValue.Text;
            //send message to last page.
        }
    }
}