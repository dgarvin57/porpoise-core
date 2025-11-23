#nullable enable

using System.ComponentModel;

namespace Porpoise.Core.Models;

[Serializable]
public class Licenses : ObjectListBase<License>
{
    public bool IsUnusedKeysAvailable()
    {
        if (Count == 0) return false;
        return this.Any(lic => lic.Status == LicenseStatusType.Avaiable);
    }
}