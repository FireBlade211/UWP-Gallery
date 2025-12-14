using System;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.ViewManagement;

namespace UWPGallery.ControlPages
{

    public sealed partial class ContactCardPage : Page
    {
        public ContactCardPage()
        {
            InitializeComponent();
        }

        private async void standardShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactManager.IsShowContactCardSupported())
            {
                //var contact = new Contact
                //{
                //    Addresses = {
                //        new ContactAddress
                //        {
                //            StreetAddress = "10 UWP Way, Redmond",
                //            Kind = ContactAddressKind.Work,
                //            Country = "United States",
                //            Description = "This is a sample description",
                //            Locality = "Sample Locality",
                //            Region = "WA",
                //            PostalCode = "12345"
                //        }
                //    },
                //    ConnectedServiceAccounts =
                //    {
                //        new ContactConnectedServiceAccount
                //        {
                //            ServiceName = "Sample Connected Service",
                //            Id = "Test Id"
                //        }
                //    },
                //    DataSuppliers =
                //    {
                //        "Microsoft",
                //        "Sample data supplier"
                //    },
                //    DisplayPictureUserUpdateTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)),
                //    FirstName = "John",
                //    HonorificNamePrefix = "Mr.",
                //    HonorificNameSuffix = "Jr.",
                //    LastName = "UWP",
                //    Emails =
                //    {
                //        new ContactEmail
                //        {
                //            Address = "test@uwp.com",
                //            Kind = ContactEmailKind.Personal,
                //            Description = "Email description goes here"
                //        }
                //    },
                //    //Fields =
                //    //{
                //    //    new ContactField("Test Field", "Sample value...", ContactFieldType.Website, ContactFieldCategory.Work)
                //    //},
                //    Id = "Sample Id",
                //    ImportantDates =
                //    {
                //        new ContactDate
                //        {
                //            Day = 29,
                //            Month = 7,
                //            Year = 2015,
                //            Kind = ContactDateKind.Birthday,
                //            Description = "Windows 10 Release"
                //        }
                //    },
                //    JobInfo =
                //    {
                //        new ContactJobInfo
                //        {
                //            CompanyAddress = "Test Address",
                //            CompanyName = "Company Name",
                //            CompanyYomiName = "Company Yomi Name",
                //            Department = "UWP Department",
                //            Description = "Sample Description",
                //            Manager = "John Microsoft",
                //            Office = "Sample office",
                //            Title = "Sample Job Name"
                //        }
                //    },
                //    SourceDisplayPicture = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/logo.png")),
                //    Name = "Mr. John UWP Jr.",
                //    Nickname = "Mr. UWP",
                //    Notes = "Some notes...",
                //    Phones =
                //    {
                //        new ContactPhone
                //        {
                //            Description = "Work Phone Number",
                //            Kind = ContactPhoneKind.Work,
                //            Number = "123 456 789"
                //        }
                //    },
                //    Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Branding/background.png")),
                //    Websites =
                //    {
                //        new ContactWebsite
                //        {
                //            Uri = new Uri("https://microsoft.com/"),
                //            Description = "Microsoft Website"
                //        }
                //    },
                //    SignificantOthers =
                //    {
                //        new ContactSignificantOther
                //        {
                //            Name = "Mrs. UWP",
                //            Relationship = ContactRelationship.Partner,
                //            Description = "Sample description"
                //        }
                //    },
                //    YomiFamilyName = "Sample Yomi Family Name",
                //    YomiGivenName = "Sample Yomi Given Name"
                //};

                var picker = new ContactPicker();

                Contact contact = await picker.PickContactAsync();

                if (contact != null)
                {
                    ContactManager.ShowContactCard(contact, standardShowButton
                        .TransformToVisual(null)
                        .TransformBounds(new Rect(0, 0, standardShowButton.ActualWidth, standardShowButton.ActualHeight)),
                        Placement.Default);
                }
            }
            else
            {
                new Flyout
                {
                    Content = new TextBlock
                    {
                        Text = "ShowContactCard is not supported on this device.",
                        Style = (Style)Application.Current.Resources["BaseTextBlockStyle"]
                    },
                    Placement = FlyoutPlacementMode.Top
                }.ShowAt(standardShowButton);
            }
        }

        private async void fullShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (await ContactManager.IsShowFullContactCardSupportedAsync())
            {
                var picker = new ContactPicker();

                Contact contact = await picker.PickContactAsync();

                if (contact != null)
                {
                    var options = new FullContactCardOptions();
                    options.DesiredRemainingView = ViewSizePreference.UseLess;

                    ContactManager.ShowFullContactCard(contact, options);
                }
            }
            else
            {
                new Flyout
                {
                    Content = new TextBlock
                    {
                        Text = "ShowContactCard is not supported on this device.",
                        Style = (Style)Application.Current.Resources["BaseTextBlockStyle"]
                    },
                    Placement = FlyoutPlacementMode.Top
                }.ShowAt(standardShowButton);
            }
        }
    }
}
