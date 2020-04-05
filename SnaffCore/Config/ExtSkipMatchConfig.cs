namespace SnaffCore.Config
{
    public partial class Options
    {
        [Nett.TomlIgnore]
        public string[] ExtSkipList { get; set; } =
        {
            ".jpg",
            ".jpeg",
            ".jpe",
            ".jif",
            ".jfif",
            ".jfi",
            ".webp",
            ".ico",
            ".psd",
            ".png",
            ".gif",
            ".bmp",
            ".tiff",
            ".tif",
            ".otf",
            ".eps",
            ".xcf",
            ".ttf",
            ".lock",
            ".css",
            ".less"
        };
    }
}