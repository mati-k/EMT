using EMT.Models;
using System.Threading.Tasks;

namespace EMT.Services
{
    public interface IConfigService
    {
        public ConfigData ConfigData { get; }
        public Task SaveConfig(ConfigData config);
    }
}
