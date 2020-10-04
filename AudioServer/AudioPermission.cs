using System.Collections.Generic;

namespace AudioServer
{
    public class AudioPermission : Xamarin.Essentials.Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
        {
            (Android.Manifest.Permission.RecordAudio, true)
        }.ToArray();
    }
}