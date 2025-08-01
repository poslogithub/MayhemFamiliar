using System;
using System.IO;

namespace MayhemFamiliar
{
    internal class DefaultValue
    {
        public const string MtgaLogFileName = "Player.log";
        public static readonly string synthesizerName = SpeakerConfig.SpeechAPI;
        public static string MtgaLogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "Wizards Of The Coast", "MTGA"
        );
        public static readonly string CardDatabaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw"
        );
    }
}
