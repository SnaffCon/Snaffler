namespace SnaffCore.Config
{
    public partial class Config
    {
        public string[] ExtSkipList { get; set; } =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".tiff",
            ".tif",
            ".psd",
            ".xcf",
            ".ttf",
            ".lock",
            ".css",
            ".less"
        };
    }
}