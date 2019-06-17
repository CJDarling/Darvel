using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Device.Location;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace wpf_project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string vehicle = "";
        public static double distance = 0;
        public static double initialSpeed = 0;
        public static double negativeMod = 1;
        public static double weatherMod = 1;
        public static double trafficMod = 1;
        public static bool weatherCheck = false;
        public static string APIKEY ="AIzaSyARKxHW4ABiDdZaYVbZdcfc5RL7tmzcrHA";
        public MainWindow()
        {
           
            InitializeComponent();
            mapBrowser.BeginInit();
            string curDir = Directory.GetCurrentDirectory();
            string curFile = "file:///" + curDir + "/gMapsInterface.html";
            mapBrowser.Navigate(curFile);
            //mapBrowser.Navigate("http://www.google.com");
            //string webData = webclient.DownloadString("https://maps.googleapis.com/maps/api/directions/json?origin=" +originText + "&destination=" +destinationTextBox.text + "&key=AIzaSyARKxHW4ABiDdZaYVbZdcfc5RL7tmzcrHA=");
        }
        private void comboBox_SelectionChanged(object sender, EventArgs e)
        {
            vehicle = vehicleSelection.Text;
            if (vehicle == "Bike") {initialSpeed = 12; }
            else if (vehicle == "Bus") {initialSpeed = 19.1;  }
            else if (vehicle == "Car") { initialSpeed = 40.54; mpgBox.IsEnabled = true; fuelSelection.IsEnabled = true; ;  }
            else if (vehicle == "Walk") {  initialSpeed = 4.52; }
            else  initialSpeed = 1;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            double cost = 0;
            double bikeWeatherMod = 0.83;
            double busWeatherMod = 0.59;
            double carWeatherMod = 0.76;
            double result = 0;
            DateTime newtime = DateTime.Now;

            if (weatherCheckBox.IsChecked == true)
            {
                if (vehicle == "Bike") { weatherMod = bikeWeatherMod; }
                else if (vehicle == "Bus") { weatherMod = busWeatherMod; }
                else if (vehicle == "Car") { weatherMod = carWeatherMod; }
            }
            else { weatherMod = 1; }
            negativeMod = trafficMod * weatherMod;
            negativeMod = Math.Round(negativeMod, 2, MidpointRounding.AwayFromZero);

            result = distance / (initialSpeed * negativeMod);
            result = result * 60;
            result = Math.Round(result, 2, MidpointRounding.AwayFromZero);
            result_text.Content = result + " minutes";

            initialSpeed = Math.Round(initialSpeed, 2, MidpointRounding.AwayFromZero);

            trafficMod = Math.Round(trafficMod, 2, MidpointRounding.AwayFromZero);

            weatherMod = Math.Round(weatherMod, 2, MidpointRounding.AwayFromZero);

            try { TimeSpan ts = TimeSpan.FromMinutes(result); }
            catch { }
            try { newtime = DateTime.Now.AddMinutes(result); } 
            catch { }
           
            if (vehicle == "Car")
            {
                double dieselPrice = 1.2948;
                double petrolPrice = 1.1952;
                double gallontoLitre = 4.546;
                double mpgConverter = 0;
                try {mpgConverter = Convert.ToDouble(mpgBox.Text); }
                catch { mpgBox.Text = "Invalid Text"; }
                mpgConverter = mpgConverter/gallontoLitre;
                if (fuelSelection.Text == "Petrol") {cost = distance/(petrolPrice*mpgConverter);} else if (fuelSelection.Text == "Diesel") { cost = distance/(dieselPrice*mpgConverter); } else {cost= 0; }
                cost = Math.Round(cost, 2, MidpointRounding.AwayFromZero);
                if (cost != 0) { TestBlock.Text = "Cost for travel = £" + cost; } else { TestBlock.Text = "Cost couldn't be calculated"; }
            }
            if (vehicle == "Bus")
            { 
                double singleCost = (distance / 5) * 2.33;
                double dayCost = (distance / 5) * 4.92;
                singleCost = Math.Round(singleCost, 2, MidpointRounding.AwayFromZero);
                dayCost = Math.Round(dayCost, 2, MidpointRounding.AwayFromZero);
                TestBlock.Text = "Likely Cost for single ticket = £" +singleCost + "\n Likely cost for a day ticket = £" + dayCost ;}

            string timestring = Convert.ToString(newtime);
            //timestring = DateTime.ParseExact(timestring, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            arrivalTime.Content = timestring;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try { distance = Convert.ToDouble(distanceTextBox.Text); }
            catch { distanceTextBox.Text = "Please enter a valid number"; }
        }
        private void comboBox1_SelectionChanged(object sender, EventArgs e)
        {
            string traffic = comboBox1.Text;
            if (traffic == "None") { trafficMod = 1; }
            if (traffic == "Light") { trafficMod = 0.75; }
            if (traffic == "Medium") { trafficMod = 0.7; }
            if (traffic == "Heavy") { trafficMod = 0.6; }
            
        }
        public Tuple<double,double> GeoLocate()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.TryStart(false, TimeSpan.FromMilliseconds(10000));
            double latitude = 53.234859;
            double longitude = -0.538440;
            GeoCoordinate coord = watcher.Position.Location;

            
            if (coord.IsUnknown != true)
            {
                latitude = coord.Latitude;
                longitude = coord.Longitude;
                addressResolution(coord);
                watcher.Stop();
            }
            else
            {
                 TestBlock.Text = "oof";
                 coord.Latitude = latitude;
                 coord.Longitude = longitude;
                 coord.Speed = 0;
                 coord.VerticalAccuracy = 5;
                 coord.HorizontalAccuracy = 5;
                 addressResolution(coord);
            }
            
            // Begin listening for location updates.
            watcher.Start();
            
            var rTuple = new Tuple<double, double>(coord.Latitude, coord.Longitude); 
            return rTuple;
        }
        public void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //var geoCoords = GeoLocate();
            System.Net.WebClient webClient = new System.Net.WebClient();
            string searchString = "https://maps.googleapis.com/maps/api/directions/json?origin=" + originTextBox.Text + "&destination=" + destinationTextBox.Text + "&key=" + APIKEY;
            string webData = webClient.DownloadString(searchString);
            TestBlock.Text = webData;
            JObject output = JObject.Parse(webData);
            var obj = JsonConvert.DeserializeObject<RootObject>(webData);
            foreach (var distance in obj.routes)    
            {
                string strStart = " \"value\" : ";
                string strEnd = "}";
                TestBlock.Text = getBetween(webData, strStart, strEnd);
                double conversiondouble = Convert.ToDouble(TestBlock.Text);
                conversiondouble = conversiondouble / 1609.344;
                string newoutput = Convert.ToString(conversiondouble);
                distanceTextBox.Text = newoutput;
            }
            string[] myLng = new String[30];
            string[] myLat = new String[30];
            string[] myCoords = new String[30];
            foreach (var start_position in obj.routes)
            {
                int i = 0;
                myLat[i] = "lat: "+ lat;
                i++; 
            }
            foreach (var end_position in obj.routes)
            {
                int i = 0;
                myLng[i] = "lng: "+ lng;
                i++;
            }
            foreach(var end_position in obj.routes)
            {
                int i = 0;
                myCoords[i] = string.Join(myLng[i],myLat);
                i++;
            }
            TestBlock2.Text = myCoords[1];
        }
        public void addressResolution(GeoCoordinate watcher)
        { 
            CivicAddressResolver resolver = new CivicAddressResolver();
            CivicAddress address = resolver.ResolveAddress(watcher);
            TestBlock.Text = address.AddressLine1;
        }
        public class RootObject
        {
            public string error_Message { get; set; }
            public List<object> routes { get; set; }
            public string status { get; set; }
        }
        class Item
        {
            public string legs { get; set; }
            public double distance { get; set; }
        }
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }
}
