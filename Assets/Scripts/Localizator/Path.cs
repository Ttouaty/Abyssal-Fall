using UnityEngine;

namespace Localizator
{
	public static class Path
	{
		#if UNITY_STANDALONE 
		private static string _streamingAssetsPath          = Application.dataPath + "/StreamingAssets";
		#elif UNITY_IOS
		private static string _streamingAssetsPath          = Application.dataPath + "/Raw";
		#elif UNITY_ANDROID
		private static string _streamingAssetsPath          = path = "jar:file://" + Application.dataPath + "!/assets/";
		#endif

		public static string WorkspaceRootPath              = Application.dataPath + "/StreamingAssets/LocalizatorWorkspace/";
		public static string DatabaseRootPath               = Application.dataPath + "/StreamingAssets/LocalizatorWorkspace/Database/";
		public static string BackupRootPath                 = Application.dataPath + "/StreamingAssets/LocalizatorWorkspace/Database/Backup/";

		public static string GeneratedFilesRootPath         = Application.dataPath + "/StreamingAssets/LocalizatorWorkspace/GeneratedFiles/";
		public static string DatabaseFilePath               = Application.dataPath + "/StreamingAssets/LocalizatorWorkspace/GeneratedFiles/Localizator.database";
		public static string DefaultLanguageFilePath        = Application.dataPath + "/StreamingAssets/LocalizatorWorkspace/GeneratedFiles/default.txt";

		public static string ScriptRootPath                 = Application.dataPath + "/Scripts/Localizator/";
		public static string EFragmentsEnumPath             = Application.dataPath + "/Scripts/Localizator/EFragmentsEnum.cs";
		public static string ELanguagesEnumPath             = Application.dataPath + "/Scripts/Localizator/ELanguagesEnum.cs";
	}
}