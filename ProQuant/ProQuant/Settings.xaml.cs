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
        List<SettingsObject> UpdatedSettings;
        bool firstLoad = true;

		public Settings (Connection cnx, List<SettingsObject> settings, string header)
		{
			InitializeComponent ();
            Maincnx = cnx;
            Header = header;
            UpdatedSettings = settings;
            updateList(settings);
            firstLoad = false;
            

		}

        public void updateList(List<SettingsObject> settings)
        {
            //if settings Text = settings, if materials Text = materials.
            Label jobHeader = new Label
            {
                Text = Header,
                TextColor = Color.Red,
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

            searchbar.SearchButtonPressed += Searchbar_SearchButtonPressed;
            searchbar.TextChanged += Searchbar_TextChanged;

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
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    jobHeader,
                    searchbar,
                    listView
                }
            };
        }

        private void Searchbar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchbar.Text))
            {
                listView.ItemsSource = UpdatedSettings;
            }
        }

        private async void Searchbar_SearchButtonPressed(object sender, EventArgs e)
        {
            string search = "";
            if (!string.IsNullOrWhiteSpace(searchbar.Text))
            {
                search = searchbar.Text.ToLower();
            }
            else {
                return;
            }
            
            List<SettingsObject> settingsList = UpdatedSettings;

            if (!string.IsNullOrWhiteSpace(search))
            {
                try
                {
                    var results = settingsList.Where(item => item.description.ToLower().Contains(search));
                    var results2 = settingsList.Where(item => item.key.ToLower().Contains(search));

                    List<SettingsObject> resultsList = results.ToList();
                    List<SettingsObject> resultsList2 = results2.ToList();
                    resultsList.Concat(resultsList2);

                    resultsList.Sort((componentA, componentB) => componentA.description.CompareTo(componentB.description));

                    listView.ItemsSource = resultsList;
                }catch(Exception ex)
                {
                    LoginPage.SendError("ST01", "Error occured whilst filtering items inside the try/catch of Searchbar_SearchButtonPressed()", ex.Message);
                    await DisplayAlert("Error", "An error occured whilst trying to search with your input\n\nError code: ST01\n\n If this keeps happening please call the office.","Ok");
                    searchbar.Text = "";
                    return;
                }
                
            }
            



        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SettingsObject settings = e.SelectedItem as SettingsObject;
            await gotoSpecificPage(settings);
        }

        private async Task gotoSpecificPage(SettingsObject settings)
        {
            SettingSpecific settingSpecific = new SettingSpecific(settings, Maincnx, Header);
            await Navigation.PushAsync(settingSpecific);

            MessagingCenter.Subscribe<SettingSpecific, SettingsObject>(settingSpecific, "send", (sender, arg) =>
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

        protected override void OnAppearing()
        {
            if (firstLoad == false)
            {
                updateList(UpdatedSettings);
            }
            base.OnAppearing();
        }
    }
    
}