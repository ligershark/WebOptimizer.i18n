using System.Collections.Generic;

namespace WebOptimizer.i18n
{
    /// <summary>
    /// Extension methods for <see cref="IAssetPipeline"/>.
    /// </summary>
    public static partial class AssetExtensions
    {
        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IEnumerable<IAsset> Localize<T>(this IEnumerable<IAsset> assets)
        {
            foreach (IAsset asset in assets)
            {
                asset.Localize<T>();
            }

            return assets;
        }

        /// <summary>
        /// Extension method to localizes the files in a bundle
        /// </summary>
        public static IAsset Localize<T>(this IAsset asset)
        {
            var localizer = new Localizer<T>();

            asset.Processors.Add(localizer);

            return asset;
        }

        /// <summary>
        /// Localizes the specified files.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="contentType">The content type of the response. Example: "text/css; charset=UTF-8"</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to localize.</param>
        public static IEnumerable<IAsset> LocalizeFiles<T>(this IAssetPipeline pipeline, string contentType, params string[] sourceFiles)
        {
            return pipeline.AddFiles(contentType, sourceFiles)
                           .Localize<T>();
        }
    }
}
