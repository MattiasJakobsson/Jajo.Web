using System.Globalization;
using System.Threading.Tasks;

namespace SuperGlue.Localization
{
    public interface ILocalizeText
    {
        Task<string> Localize(string key, CultureInfo culture);
        Task Load();
    }
}