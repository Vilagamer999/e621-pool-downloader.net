namespace Pooldl
{
    internal class GetJson
    {
        public string name { get; set; }
        public int[] post_ids { get; set; }
        public string creator_name { get; set; }
        public bool success { get; set; }
    }

    internal class Post
    {
        public string url { get; set; }
        public string ext { get; set; }

    }

    //public class File
    //{
    //    public int width { get; set; }
    //    public int height { get; set; }
    //    public string ext { get; set; }
    //    public int size { get; set; }
    //    public string md5 { get; set; }
    //    public string url { get; set; }
    //}

    public class Sample
    {
        public bool has { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string url { get; set; }
    }
}