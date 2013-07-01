using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spinnaker.Sample.WinForms.ViewModels
{
	/// <summary>
	/// This view model is a bindable collection of Customers. It is simply responsible for loading and exposing a
	/// collection of Customers which are also view models themselves.
	/// 
	/// In a real application, this view model make queries against an application's data layer to load its Customers.
	/// </summary>
    public class CustomersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<Customer> customers = new List<Customer>();
        private Random random = new Random();
        private string filterText;
		private const int NumSampleProfiles = 120;

        public CustomersViewModel()
        {
        }

		/// <summary>
		/// Pretends to load a collection of customers from persistent storage like a database or a web service.
		/// Since this sample doesn't do any real persistence, this method creates a random set of customers that
		/// look like they were loaded from storage.
		/// </summary>
        public void LoadCustomers()
        {
            customers.Clear();
			int numCustomers = 100 + random.Next(100);
            int profileIndex = ((int)(random.NextDouble() * (double)(NumSampleProfiles + 1)));
            for (int i = 0; i < numCustomers; i++)
            {
                string firstName = GetRandomItem(firstNames);
                string lastName = GetRandomItem(lastNames);
                customers.Add(new Customer()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = firstName + "." + lastName + "@gmail.com",
                    Balance = Math.Round((decimal)(random.NextDouble() * 10000.0), 2),
                    Address = ((int)(random.NextDouble() * 10000.0)).ToString() + " " + GetRandomStreetName(),
                    ProfilePicture = "img/profiles/ProfilePicture" + ((++profileIndex) % (NumSampleProfiles + 1)).ToString("0000") + ".jpg"
                });
            }

            foreach (Customer customer in customers)
                customer.ClearDirty();

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Customers"));
        }

		/// <summary>
		/// Delete a customer from the collection. In a real application, this method would probably also
		/// delete the customer from persistent storage.
		/// </summary>
        public void Delete(string indexArg)
        {
            int index = -1;
            if (Int32.TryParse(indexArg, out index))
            {
                customers.RemoveAt(index);
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Customers"));
            }
        }

		/// <summary>
		/// Applies the current filter text against the collection. All this does is raise a PropertyChanged event,
		/// since the actual work of filtering is simply a byproduct of iterating through the collection.
		/// </summary>
        public void Filter()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Customers"));
        }

		/// <summary>
		/// Gets or sets the filter text that is used to filter the collection of customers that are exposed.
		/// This won't cause anything to happen until Filter() is called, or something causes a PropertyChanged
		/// event to fire against the Customers collection, which will trigger the GUI to iterate the collection.
		/// </summary>
        public string FilterText
        {
            get { return filterText; }
            set
            {
                if (String.IsNullOrEmpty(value) || value.Trim().Length == 0)
                    filterText = null;
                else
                    filterText = value.ToLower();
            }
        }

		/// <summary>
		/// The collection of customers that is exposed to the GUI (and to anyone else who is interested).
		/// </summary>
        public IEnumerable<Customer> Customers
        {
            get
            {
				// We could obviously implement a far more efficient filtering strategy,
				// but we do this to keep things simple for the sake of clarity in a sample application.
				// You can be as creative as you like in ViewModels - Spinnaker is concerned only with
				// binding to your INotifyPropertyChanged properties. You have complete freedom in generating
				// and consuming those values.
                foreach (Customer customer in customers)
                {
                    if (String.IsNullOrEmpty(filterText) ||
                        (customer.FirstName.ToLower().Contains(filterText) ||
                        customer.LastName.ToLower().Contains(filterText) ||
                        customer.Address.ToLower().Contains(filterText) ||
                        customer.Email.ToLower().Contains(filterText) ||
                        customer.BalanceDisplay.Contains(filterText)))
                        yield return customer;
                }
            }
        }

        private string GetRandomStreetName()
        {
            string name = GetRandomItem(streetNames1);
            if (random.NextDouble() <= 0.5)
                name += " " + GetRandomItem(streetNames2);
            name += " " + GetRandomItem(streetNames3);
            return name;
        }

        private string GetRandomItem(string[] items)
        {
            int index = items.Length;
            while (index == items.Length)
                index = random.Next(items.Length);
            return items[index];
        }

        private string[] streetNames1 = {
            "Amber",
            "Auburn",
            "Birch",
            "Blue",
            "Blue",
            "Bright",
            "Broad",
            "Burning",
            "Calm",
            "Cinder",
            "Clear",
            "Cold",
            "Colonial",
            "Cotton",
            "Cozy",
            "Crimson",
            "Crystal",
            "Dewy",
            "Dusty",
            "Easy",
            "Emerald",
            "Fallen",
            "Foggy",
            "Gentle",
            "Golden",
            "Grand",
            "Green",
            "Happy",
            "Harvest",
            "Hazy",
            "Heather",
            "Hidden",
            "High",
            "Honey",
            "Honey",
            "Indian",
            "Iron",
            "Ivory",
            "Jagged",
            "Lazy",
            "Little",
            "Lone",
            "Lonely",
            "Long",
            "Lost",
            "Merry",
            "Middle",
            "Misty",
            "Noble",
            "Old",
            "Orange",
            "Pearl",
            "Pied",
            "Pleasant",
            "Quaking",
            "Quiet",
            "Red",
            "Rocky",
            "Rose",
            "Rough",
            "Round",
            "Rustic",
            "Sandy",
            "Shady",
            "Silent",
            "Silver",
            "Sleepy",
            "Small",
            "Stony",
            "Sunny",
            "Sweet",
            "Tawny",
            "Thunder",
            "Twin",
            "Umber",
            "Velvet",
            "White",
            "Windy",
            "Wishing",
            "Big",
            "Strong",
            "Hush",
            "Turning",
            "Still",
            "Cool",
            "Quaint",
            "Big",
            "Tender"
        };

        private string[] streetNames2 = {
            "Acorn",
            "Anchor",
            "Apple",
            "Autumn",
            "Axe",
            "Barn",
            "Beacon",
            "Bear",
            "Beaver",
            "Berry",
            "Bird",
            "Blossom",
            "Bluff",
            "Branch",
            "Bridge",
            "Brook",
            "Butterfly",
            "Butternut",
            "Castle",
            "Chestnut",
            "Cider",
            "Cloud",
            "Cottage",
            "Creek",
            "Crow",
            "Dale",
            "Deer",
            "Diamond",
            "Dove",
            "Elk",
            "Elm",
            "Embers",
            "Fawn",
            "Feather",
            "Flower",
            "Forest",
            "Fox",
            "Gate",
            "Goat",
            "Goose",
            "Grove",
            "Harbor",
            "Hickory",
            "Hills",
            "Holly",
            "Horse",
            "Island",
            "Lake",
            "Leaf",
            "Log",
            "Maple",
            "Mill",
            "Mountain",
            "Nectar",
            "Oak",
            "Panda",
            "Peach",
            "Pebble",
            "Pine",
            "Pioneer",
            "Pond",
            "Pony",
            "Prairie",
            "Quail",
            "Rabbit",
            "Rise",
            "River",
            "Robin",
            "Rock",
            "Shadow",
            "Sky",
            "Spring",
            "Stone",
            "Swan",
            "Snake",
            "Timber",
            "Treasure",
            "Turtle",
            "View",
            "Wagon",
            "Willow",
            "Zephyr",
            "Nest",
            "Lamb",
            "Squirrel",
            "Nut",
            "Pumpkin"
        };

        private string[] streetNames3 = {
            "Acres",
            "Alcove",
            "Arbor",
            "Avenue",
            "Bank",
            "Bayou",
            "Bend",
            "Bluff",
            "Byway",
            "Canyon",
            "Chase",
            "Circle",
            "Corner",
            "Court",
            "Cove",
            "Crest",
            "Crest",
            "Dale",
            "Dell",
            "Drive",
            "Edge",
            "Estates",
            "Falls",
            "Farms",
            "Field",
            "Flats",
            "Gardens",
            "Gate",
            "Glade",
            "Glen",
            "Grove",
            "Haven",
            "Heights",
            "Highlands",
            "Hollow",
            "Isle",
            "Jetty",
            "Journey",
            "Knoll",
            "Lagoon",
            "Landing",
            "Lane",
            "Ledge",
            "Manor",
            "Meadow",
            "Mews",
            "Niche",
            "Nook",
            "Orchard",
            "Park",
            "Path",
            "Pike",
            "Place",
            "Point",
            "Promenade",
            "Quay",
            "Race",
            "Ridge",
            "Round",
            "Run",
            "Shoal",
            "Stead",
            "Stroll",
            "Summit",
            "Swale",
            "Terrace",
            "Trace",
            "Trail",
            "Trek",
            "Vale",
            "View",
            "Valley",
            "Villa",
            "Vista",
            "Way",
            "Woods",
            "Twist",
            "Turn",
            "Road",
            "Street"
        };

        private string[] firstNames = {
            "John",
            "Fred",
            "Joe",
            "Jim",
            "Nancy",
            "Phil",
            "Steve",
            "Clay",
            "Mike",
            "Mark",
            "Eric",
            "Dave",
            "Justin",
            "Patrick",
            "Qui",
            "Bob",
            "Henry",
            "Jane",
            "Janet",
            "Lisa",
            "Patty",
            "Greg",
            "Peter",
            "Ryan",
            "Sean",
            "Kyle",
            "Scott",
            "Wolfgang",
            "Dennis",
            "Pol",
            "Phung",
            "Kristine",
            "Christie",
            "Hans",
            "Franz",
            "Fritz",
            "Himmel",
            "Karl",
            "Arnold",
            "Doky",
            "Jason",
            "Morn",
            "Min",
            "Angus",
            "Teddy",
            "Ronald",
            "Gary",
            "Tim",
            "Timothy",
            "Dustin",
            "Rusty",
            "Hudson",
            "Woodrow",
            "Lillian",
            "Pamela",
            "Pam",
            "Abbey",
            "Abelard",
            "Aba",
            "Adele",
            "Adeline",
            "Alessa",
            "Alexis",
            "Alphonse",
            "Alise",
            "Alonzo",
            "Baldhart",
            "Baldric",
            "Baltasar",
            "Baldwin",
            "Bamey",
            "Barrett",
            "Barney",
            "Bargin",
            "Bernadette",
            "Bernard",
            "Bess",
            "Berty",
            "Bing",
            "Brenda",
            "Brewster",
            "Broderick",
            "Bronson",
            "Budd",
            "Caden",
            "Carra",
            "Casper",
            "Caden",
            "Cheryl",
            "Chriselda",
            "Connie",
            "Corrado",
            "Dietta",
            "Dirk",
            "Dolph",
            "Durin",
            "Ebba",
            "Edsil",
            "Griswald"
        };

        private string[] lastNames = {
            "Fowler",
            "Heeg",
            "Telling",
            "Velasquez",
            "McClaran",
            "McDougald",
            "McGee",
            "McCourner",
            "McCloud",
            "Sanchez",
            "Phillips",
            "Blaydes",
            "Hiite",
            "Huff",
            "Kaye",
            "Su",
            "Kwai",
            "Stevens",
            "Jobs",
            "Smith",
            "Brown",
            "Speaker",
            "Porter",
            "Prine",
            "Mozart",
            "McEvers",
            "Aachen",
            "Adler",
            "Amsel",
            "Austerlitz",
            "Bach",
            "Bader",
            "Bauer",
            "Barth",
            "Beckenbauer",
            "Dorfmeister",
            "Dechsler",
            "Eichel",
            "Engel",
            "Faber",
            "Eiffel",
            "Eberhardt",
            "Fassbinder",
            "Faust",
            "Fiedler",
            "Fischer",
            "Foerster",
            "Frankfurter",
            "Frei",
            "Fuchs",
            "Fruehauf",
            "Fuhrmann",
            "Gersten",
            "Gottlieb",
            "Hahn",
            "Herrmann",
            "Hertz",
            "Hoch",
            "Kirsch",
            "Holtzmann",
            "Klein",
            "Koch",
            "Kluge",
            "Kohl",
            "Kuefer",
            "Krause",
            "Koertig",
            "Mahler",
            "Maier",
            "Meister",
            "Nacht",
            "Metzger",
            "Mueller",
            "Nadel",
            "Oster",
            "Osterhagen",
            "Allaway",
            "Aitken",
            "Armstrong",
            "Breckenridge",
            "Burns",
            "Calhoun",
            "Campbell",
            "Carr",
            "Coburn",
            "Coutts",
            "Cowden",
            "Craig",
            "Crawford",
            "Croft",
            "Cruickshank",
            "Cummins",
            "Cunningham",
            "Darrow",
            "David",
            "Davis",
            "Donahughe",
            "Donne",
            "Drummond",
            "Duff",
            "Duncan",
            "Eads",
            "Fairbairn",
            "Faulkner",
            "Ferguson",
            "Finley",
            "Forney",
            "Gibb",
            "Graham",
            "Grant",
            "Grieve",
            "Hambledon",
            "Hamilton",
            "Hendry",
            "Hepburn",
            "Holme",
            "Holmes",
            "Kelly",
            "Kerr",
            "Kidd",
            "Lithgow",
            "Logan",
            "Low",
            "Lowe",
            "MacKenny",
            "MacPharlain",
            "MacNeil",
            "MacLeod",
            "MacLean",
            "Magee",
            "Maguire",
            "McAlister",
            "McCracker",
            "McCune",
            "McDonald"            
        };
    }
}
