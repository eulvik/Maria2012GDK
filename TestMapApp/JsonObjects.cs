using System.Collections.Generic;
namespace TestMapApp
{
    public class Position
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class PlaceName
    {
        public string Adm1 { get; set; }
        public string Adm2 { get; set; }
        public string Adm3 { get; set; }
        public object City { get; set; }
        public string Country { get; set; }
        public double Distance { get; set; }
        public string FeatureClass { get; set; }
        public string FeatureCode { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
    }

    public class Match
    {
        public int MatchInfo { get; set; }
        public PlaceName PlaceName { get; set; }
    }

    public class RootObject
    {
        public object Facets { get; set; }
        public List<Match> Matches { get; set; }
        public int Status { get; set; }
        public int TotalServerMatchCount { get; set; }
    }
}