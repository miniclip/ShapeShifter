namespace Miniclip.ShapeShifter
{
    static class SharedInfo
    {
        internal static readonly string SHAPESHIFTER_NAME = "ShapeShifter";

        internal static readonly string ExternalAssetsFolder = "external";
        internal static readonly string InternalAssetsFolder = "internal";

        internal static ShapeShifterConfiguration Configuration { get; private set; }

        public static void SetConfiguration(ShapeShifterConfiguration configuration) => Configuration = configuration;
    }
}