﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Casualty_Radar.Models;
using Casualty_Radar.Models.DataControl;
using GMap.NET;

namespace Casualty_Radar.Core {
    class GeoMapLoader {

        private readonly List<GeoMapSection> _geoMapSections; // List that contains all the GeoMapSection instances of the XML files

        private const string FILE_PATH = @"../../Resources/XML/Sections";

        public GeoMapLoader() {
            _geoMapSections = new List<GeoMapSection>();
        }

        /// <summary>
        /// Initializes the geoMap collection and fill it with all sections.
        /// All sections are loaded in dynamically and do not require manual registration.
        /// </summary>
        private void Init() {
            List<string> directory = Directory.GetFiles(FILE_PATH).Select(Path.GetFileName).ToList();
            foreach (string fileName in directory) {
                XmlReader reader = XmlReader.Create(FILE_PATH + "/" + fileName);
                if (reader.ReadToDescendant("bnd")) {
                    double minLat = double.Parse(reader.GetAttribute("minlat"), CultureInfo.InvariantCulture);
                    double minLon = double.Parse(reader.GetAttribute("minlon"), CultureInfo.InvariantCulture);
                    double maxLat = double.Parse(reader.GetAttribute("maxlat"), CultureInfo.InvariantCulture);
                    double maxLon = double.Parse(reader.GetAttribute("maxlon"), CultureInfo.InvariantCulture);
                    _geoMapSections.Add(new GeoMapSection(new PointLatLng(maxLat, maxLon), new PointLatLng(minLat, minLon), FILE_PATH + "/" + fileName));
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Retrieves boundary data from each XML file and creates a GeoMapSection instance for each of them
        /// </summary>
        /// <returns>Returns the list with all GeoMapSection instances</returns>
        public List<GeoMapSection> GetGeoMapSections() {
            if (_geoMapSections.Count == 0)
                Init();
            return _geoMapSections;
        } 
       
    }
}