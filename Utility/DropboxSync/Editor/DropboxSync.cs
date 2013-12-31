using UnityEditor;
using UnityEngine;
using System.IO;

namespace Tinkerbox {
    public class DropboxSync : EditorWindow {

        public static string dropboxPath;
        public static string destination = "Assets/Binaries";

        [MenuItem ("Sync/Set Asset Folder...")]
        static void SetFolder () {
                dropboxPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
                EditorWindow.GetWindow<DropboxSync>();
        }

        [MenuItem ("Sync/Sync Binary Assets")]
        static void SyncAssets () {
                dropboxPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
                Debug.Log("Syncing Assets from: " + dropboxPath + "... please wait...");
                foreach (string dirPath in Directory.GetDirectories(dropboxPath, "*", SearchOption.AllDirectories))
                        Directory.CreateDirectory(dirPath.Replace(dropboxPath, destination));
                        
                foreach (string newPath in Directory.GetFiles(dropboxPath, "*.*", SearchOption.AllDirectories))
                        File.Copy(newPath, newPath.Replace(dropboxPath, destination), true);
                        
                AssetDatabase.Refresh();
                        
                Debug.Log("Done Syncing Assets.");
        }


        void OnGUI()
        {
                GUILayout.Label ("Set a path to a folder in dropbox that will contain your binary assets.", EditorStyles.boldLabel);
                dropboxPath = EditorGUILayout.TextField ("Text Field", dropboxPath);
                if(GUILayout.Button("Save")) {
                        EditorPrefs.SetString("dropbox_path", dropboxPath);
                }
        }
    }
}