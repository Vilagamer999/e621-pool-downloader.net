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

    public class PostResponse
    {
        public class File
        {
            public string url { get; set; }
            public string ext { get; set; }
        }

        public class Post
        {
            public File file { get; set; } = new File();
        }

        // posts/{id}.json 
        public Post post { get; set; } = new Post();

    }
}