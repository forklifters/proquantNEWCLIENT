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
        SearchBar searchbar;
        string searchBarText;
        ListView listView;
        string Header;
        List<Settings> UpdatedSettings;

		public Settings (Connection cnx, List<Settings> settings, string header)
		{
			InitializeComponent ();
            Maincnx = cnx;
            UpdatedSettings = settings;
            updateList(settings);
            Header = header;

		}

        public void updateList(List<Settings> settings)
        {
            //if settings Text = settings, if materials Text = materials.
            Label jobHeader = new Label
            {
                Text = Header,
                TextColor = Color.Black,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            //Create event handlers for text changed and also search button pressed.
            searchbar = new SearchBar()
            {
                Placeholder = "Search:",
                Text = searchBarText,
                CancelButtonColor = Color.Red
            };

            listView = new ListView()
            {
                HasUnevenRows = true,
                BackgroundColor = Color.White,
                ItemsSource = settings,
                SeparatorColor = Color.Red,

                ItemTemplate = new DataTemplate(() =>
                {
                    Label Setting = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        FontAttributes = FontAttributes.Bold
                    };

                    Setting.SetBinding(Label.TextProperty, "description");

                    Label UOM = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                    };

                    UOM.SetBinding(Label.TextProperty, "uom");

                    Label OValue = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                    };
                    OValue.SetBinding(Label.TextProperty, "origional");

                    Label NValue = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                    };
                    NValue.SetBinding(Label.TextProperty, "value");

                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = new Thickness(10, 15, 10, 15),
                            Orientation = StackOrientation.Horizontal,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Children =
                            {
                                new StackLayout
                                {
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    Children =
                                    {
                                        Setting
                                    }
                                },

                                new StackLayout
                                {
                                    HorizontalOptions = LayoutOptions.End,
                                    Children =
                                    {
                                        UOM,
                                        NValue
                                    }   
                                }     
                            }
                        }
                    };

                })
            };

            listView.ItemSelected += ListView_ItemSelected;

            this.Padding = new Thickness(10, 20, 10, 5);

            this.Content = new StackLayout
            {
                Children =
                {
                    jobHeader,
                    searchbar,
                    listView
                }
            };
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Settings settings = e.SelectedItem as Settings;
            await gotoSpecificPage(settings);

        }

        private async Task gotoSpecificPage(Settings settings)
        {
            SettingSpecific settingSpecific = new SettingSpecific(settings);
            await Navigation.PushAsync(settingSpecific);

            MessagingCenter.Subscribe<SettingSpecific, Settings>(settingSpecific, "send", (sender, arg) =>
            {
                for (int i = 0; i < UpdatedSettings.Count; i++)
                {
                    //check the last key to check it covers it.
                    if(UpdatedSettings[i].key == arg.key)
                    {
                        UpdatedSettings[i] = arg;
                    }
                }
            });
        }
    }
    
}