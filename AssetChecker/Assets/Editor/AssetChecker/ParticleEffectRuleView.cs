using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AssetChecker;
using UnityEditor;

namespace AssetChecker
{
    /// <summary>
    /// 粒子效果制作范围
    /// </summary>
    public class ParticleEffectRuleView : IEditorPanel {

        public const int MaxMatrials = 8;
        public const int MaxParticles = 160;

        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<SettingBean, bool> isFolderOut = new Dictionary<SettingBean, bool>();

        private List<ParticleEffectSettingBean> settings;

        public void Initizalize()
        {
            settings = OverviewSetting.Instance.EffectSettings;

            foreach (ParticleEffectSettingBean msb in settings)
            {
                isFolderOut[msb] = true;
            }
        }

        public void OnGUI()
        {
            if (settings == null) this.Initizalize();

            NGUIEditorTools.DrawHeader("模型规范设置");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", GUILayout.Width(80)))
                {
                    if (File.Exists(OverviewSetting.LocalParticleFilePath))
                        File.Delete(OverviewSetting.LocalParticleFilePath);
                    OverviewSetting.LocalParticleFilePath = "";
                }

                if (GUILayout.Button("Load", GUILayout.Width(80)))
                {
                    string filePath = EditorUtility.OpenFilePanel("打开", Application.dataPath, "xml");
                    OverviewSetting.LocalParticleFilePath = filePath.Replace(Application.dataPath, "Assets");
                    OverviewSetting.Instance.ReadParticelEffectSettings();
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
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("保存"))
            {
                if (string.IsNullOrEmpty(OverviewSetting.LocalParticleFilePath))
                {
                    string filePath = EditorUtility.SaveFilePanel("保存", Application.dataPath, "new file", "xml");
                    OverviewSetting.LocalParticleFilePath = filePath.Replace(Application.dataPath, "Assets");
                }
                this.saveSettingRule(OverviewSetting.LocalParticleFilePath);
                AssetDatabase.Refresh();
                Debug.Log("Save Success !");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("添加规则"))
            {
                ParticleEffectSettingBean msb = new ParticleEffectSettingBean();
                msb.Folder.Add(string.Empty);
                settings.Add(msb);
            }
            GUILayout.EndHorizontal();
        }


        private void drawSetting(ParticleEffectSettingBean modelSetting)
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
                    GUILayout.Space(30);
                    NGUIEditorTools.SetLabelWidth(120F);
                    modelSetting.MaxMatrials = EditorGUILayout.IntSlider(new GUIContent("最大材质数"), modelSetting.MaxMatrials,
                                                                        0, MaxMatrials);
                    GUILayout.FlexibleSpace();
                    modelSetting.MaxParticels = EditorGUILayout.IntSlider(new GUIContent("最大粒子数"), modelSetting.MaxParticels,
                                                                        0,MaxParticles);
                    GUILayout.EndHorizontal();
                }

            }

            GUILayout.EndVertical();
        }

        public void OnDestroy()
        {

        }

        /// <summary>
        /// 保存模型设置配置
        /// </summary>
        private void saveSettingRule(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement rootEle = xmlDoc.CreateElement("EffectConfigs");
            for (int i = 0; i < settings.Count; i++)
            {
                ParticleEffectSettingBean msb = settings[i];
                XmlElement ele = xmlDoc.CreateElement("EffectSetting");
                msb.Write(xmlDoc, ele);
                rootEle.AppendChild(ele);
            }
            xmlDoc.AppendChild(rootEle);
            xmlDoc.Save(filePath);
        }
    }

}

