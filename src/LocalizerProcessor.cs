using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Net.Http.Headers;

namespace WebOptimizer.i18n
{
    /// <summary>
    /// Compiles Sass files
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public class LocalizerProcessor<T> : IProcessor
    {
        private IStringLocalizer _stringProvider;

        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context)
        {
            IRequestCultureFeature cf = context.Features.Get<IRequestCultureFeature>();

            if (cf == null)
            {
                throw new InvalidOperationException("No UI culture found.  Did you forget to add UseRequestLocalization?");
            }

            return cf.RequestCulture.UICulture.TwoLetterISOLanguageName;
        }

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext config)
        {
            _stringProvider = config.HttpContext.RequestServices.GetService<IStringLocalizer<T>>();
            var content = new Dictionary<string, byte[]>();

            foreach (string route in config.Content.Keys)
            {
                content[route] = Localize(config.Content[route].AsString()).AsByteArray();
            }

            if (config.Content.Keys.Any())
            {
                config.HttpContext.Response.Headers[HeaderNames.Vary] = "Accept-Language";
            }

            config.Content = content;

            return Task.CompletedTask;
        }

        private string Localize(string document)
        {
            const char beginArgChar = '{';
            const char endArgChar = '}';

            StringBuilder sb = new StringBuilder();
            int potentialArgBegin = -1;

            int pos = 0;
            int len = document.Length;
            char ch = '\x0';

            while (true)
            {

                while (pos < len)
                {
                    ch = document[pos];
                    pos++;

                    //Is it the beginning of the opening sequence?
                    if (ch == beginArgChar)
                    {
                        //Is it the escape sequence?
                        if (pos < len && document[pos] == beginArgChar)
                        {
                            //Keep track of where it started
                            potentialArgBegin = pos - 1;

                            //Advance to argument hole parameter
                            pos++;
                            break;
                        }
                    }

                    sb.Append(ch);
                }

                //End of the doc string
                if (pos == len) break;

                int beg = pos;
                int paramLen = 0;
                bool argHoleClosed = false;

                while (pos < len)
                {
                    pos++;
                    paramLen++;
                    ch = document[pos];

                    if (ch == endArgChar)
                    {
                        pos++;

                        if (document[pos] == endArgChar)
                        {
                            argHoleClosed = true;
                        }
                        break;
                    }
                }

                if (pos == len) InvalidDocFormat();

                //Advance past the closing char of the argument hole
                pos++;

                string param = document.Substring(beg, paramLen);

                if (!argHoleClosed)
                {
                    //Wasn't a valid argument hole, put it back as it was and continue
                    NotValidStringArgument(document, sb, potentialArgBegin, paramLen);
                    continue;
                }

                try
                {
                    LocalizedString str = _stringProvider.GetString(param);
                    if (str.ResourceNotFound)
                    {
                        //Put it back and continue
                        NotValidStringArgument(document, sb, potentialArgBegin, paramLen);
                        continue;
                    }
                    sb.Append(str.Value);
                }
                //This occurs if the user didn't specify AddViewLocalization in Startup.cs
                catch (ArgumentNullException ex)
                {
                    throw new ArgumentNullException(ex.Message + ". Did you forget to call AddViewLocalization?");
                }
            }

            return sb.ToString();
        }

        private static void NotValidStringArgument(string document, StringBuilder sb, int potentialArgBegin, int paramLen)
        {
            sb.Append(document.Substring(potentialArgBegin, paramLen + 4));
        }

        private void InvalidDocFormat()
        {
            throw new InvalidOperationException("Document not correctly formatted");
        }

    }
}
