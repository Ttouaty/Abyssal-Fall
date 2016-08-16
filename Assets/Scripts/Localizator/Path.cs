using UnityEngine;

namespace Localizator
{
    public static class Path
    {
        public static string WorkspaceRootPath              = Application.dataPath + "/Resources/LocalizatorWorkspace/";
        public static string DatabaseRootPath               = Application.dataPath + "/Resources/LocalizatorWorkspace/Database/";
        public static string BackupRootPath                 = Application.dataPath + "/Resources/LocalizatorWorkspace/Database/Backup/";

        public static string GeneratedFilesRootPath         = Application.dataPath + "/Resources/LocalizatorWorkspace/GeneratedFiles/";
        public static string DatabaseFilePath               = Application.dataPath + "/Resources/LocalizatorWorkspace/GeneratedFiles/Localizator.database";
        public static string DefaultLanguageFilePath        = Application.dataPath + "/Resources/LocalizatorWorkspace/GeneratedFiles/default.txt";

        public static string ScriptRootPath                 = Application.dataPath + "/Scripts/Localizator/";
        public static string EFragmentsEnumPath             = Application.dataPath + "/Scripts/Localizator/EFragmentsEnum.cs";
        public static string ELanguagesEnumPath             = Application.dataPath + "/Scripts/Localizator/ELanguagesEnum.cs";
    }
}