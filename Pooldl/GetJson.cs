using static System.Formats.Asn1.AsnWriter;
using System.Collections.Generic;
using System;

namespace Pooldl
{
    internal class Pools
    {
        public string name { get; set; }
        public int[] post_ids { get; set; }
        public string creator_name { get; set; }
    }

    public class File
    {
        public int width { get; set; }
        public int height { get; set; }
        public string ext { get; set; }
        public int size { get; set; }
        public string md5 { get; set; }
        public string url { get; set; }
    }
}