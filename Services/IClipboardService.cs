using System.Threading.Tasks;

namespace EMT.Services
{
    public interface IClipboardService
    {
        public Task SetTextAsync(string text);
    }
}
