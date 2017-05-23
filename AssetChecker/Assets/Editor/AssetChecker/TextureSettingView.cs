using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace AssetChecker
{
    /// <summary>
    /// 贴图设置界面
    /// </summary>
    public class TextureSettingView : IEditorPanel
    {
        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<TextureSettingBean, bool> isFolderOut = new Dictionary<TextureSettingBean, bool>();

        private List<TextureSettingBean> settings;
        private List<TextureSettingBean> removeSettings = new List<TextureSettingBean>();

        public void Initizalize()
        {
            settings = OverviewSetting.Instance.TextureSettings;

            foreach (TextureSettingBean msb in settings)
            {
                isFolderOut[msb] = true;
            }
        }

        public void OnGUI()
        {
            if (settings == null) this.Initizalize();

            NGUIEditorTools.DrawHeader("贴图查询设置");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("清空", GUILayout.Width(80)))
                {
                    if (File.Exists(OverviewSetting.LocalTextureFilePath))
                        File.Delete(OverviewSetting.LocalTextureFilePath);
                    OverviewSetting.LocalTextureFilePath = "";
                }

                if (GUILayout.Button("加载", GUILayout.Width(80)))
                {
                    string filePath = EditorUtility.OpenFilePanel("打开", Application.dataPath, "xml");
                    OverviewSetting.LocalTextureFilePath = filePath.Replace(Application.dataPath, "Assets");
                    OverviewSetting.Instance.ReadTextureSettings();
                    this.Initizalize();
                }
                GUILayout.Space(10);
            }

            GUILayout.Space(5);
            NGUIEditorTools.DrawSeparator();
            if (settings != null)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                for (int i = 0; i < settings.Count; i++)
                {
                    GUILayout.Space(5);
                    drawSetting(settings[i]);
                    NGUIEditorTools.DrawSeparator();
                }

                if (removeSettings.Count > 0)
                {
                    for (int i = 0; i < removeSettings.Count; i++)
                    {
                        settings.Remove(removeSettings[i]);
                    }
                    removeSettings.Clear();
                }
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("保存"))
            {
                if (string.IsNullOrEmpty(OverviewSetting.LocalTextureFilePath))
                {
                    string filePath = EditorUtility.SaveFilePanel("保存", Application.dataPath, "new file", "xml");
                    OverviewSetting.LocalTextureFilePath = filePath.Replace(Application.dataPath, "Assets");
                }
                this.saveSettingRule(OverviewSetting.LocalTextureFilePath);
                AssetDatabase.Refresh();
                Debug.Log("Save Success !");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("添加规则"))
            {
                TextureSettingBean msb = new TextureSettingBean();
                msb.Folder.Add(string.Empty);
                settings.Add(msb);
            }
            GUILayout.EndHorizontal();
        }


        private void drawSetting(TextureSettingBean modelSetting)
        {
            GUILayout.BeginVertical();
            {

                bool isFoldOut = true;
                GUILayout.BeginHorizontal();
                if (isFolderOut.ContainsKey(modelSetting))
                {
                    isFoldOut = isFolderOut[modelSetting];
                }
                isFolderOut[modelSetting] = EditorGUILayout.Foldout(isFoldOut, modelSetting.AssetDesc);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    removeSettings.Add(modelSetting);
                }
                GUILayout.EndHorizontal();

                if (isFoldOut)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("文件类型描述", GUILayout.Width(100F));
                    modelSetting.AssetDesc = GUILayout.TextField(modelSetting.AssetDesc);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(30);
                        if (GUILayout.Button("文件目录", GUILayout.Width(100F)))
                            modelSetting.Folder.Add(string.Empty);

                        GUILayout.BeginVertical();
                        for (int i = modelSetting.Folder.Count - 1; i >= 0; i--)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.TextField(modelSetting.Folder[i]);
                            if (GUILayout.Button("...", GUILayout.Width(30f)))
                            {
                                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                                modelSetting.Folder[i] = path.Replace(Application.dataPath, "Assets");
                            }
                            if (GUILayout.Button("X", GUILayout.Width(30f)))
                            {
                                modelSetting.Folder.RemoveAt(i);
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(30);
                        GUILayout.Label("MipMaps", GUILayout.Width(100F));
                        modelSetting.MipMaps = GUILayout.Toggle(modelSetting.MipMaps, "开启");
                    }
                    GUILayout.EndHorizontal();
                }

            }

            GUILayout.EndVertical();
        }

        public void OnDestroy()
        {

        }

        /// <summary>
        /// 保存设置配置
        /// </summary>
        private void saveSettingRule(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement rootEle = xmlDoc.CreateElement("TextureConfigs");
            for (int i = 0; i < settings.Count; i++)
            {
                SettingBean msb = settings[i];
                XmlElement ele = xmlDoc.CreateElement("TextureSetting");
                msb.Write(xmlDoc, ele);
                rootEle.AppendChild(ele);
            }
            xmlDoc.AppendChild(rootEle);
            xmlDoc.Save(filePath);
        }
    }
}