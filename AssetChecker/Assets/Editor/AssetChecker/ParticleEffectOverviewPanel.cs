using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetChecker;
using UnityEditor;

namespace AssetChecker
{
    /// <summary>
    /// 粒子效果统计
    /// </summary>
    public class ParticleEffectOverviewPanel : IEditorPanel
    {

        private int mTab;
        private Vector2 scrollPos;
        private List<EffectBean> modelList;

        private ParticleEffectRuleView ruleView = new ParticleEffectRuleView();

        private int selectAssetType;
        private string[] AssetAllTypes;

        private int sortDc = 1, sortTexture = 1, sortScore = 1;

        public void Initizalize()
        {
            if (modelList != null) return;
           
            OverviewSetting.Instance.ReadParticelEffectSettings();
            findAllParticleEffects();
        }
        /// <summary>
        /// 查找模型文件
        /// </summary>
        private void findAllParticleEffects()
        {
            scrollPos = Vector2.zero;
            List<ParticleEffectSettingBean> settings = OverviewSetting.Instance.EffectSettings;

            List<string> files = new List<string>();
            Dictionary<ParticleEffectSettingBean, string[]> fileMaps = new Dictionary<ParticleEffectSettingBean, string[]>();

            List<string> assetTypeList = new List<string>();
            assetTypeList.Add("ALL");

            float fileCount = 0;
            if (settings != null)
            {

                for (int i = 0; i < settings.Count; i++)
                {
                    files.Clear();
                    for (int j = 0; j < settings[i].Folder.Count; j++)
                    {
                        string rootFolder = settings[i].Folder[j];
                        if (string.IsNullOrEmpty(rootFolder)) continue;
                        string[] fileArr = Directory.GetFiles(rootFolder, "*.prefab", SearchOption.AllDirectories);
                        files.AddRange(fileArr);
                        fileCount += fileArr.Length;
                    }
                    fileMaps[settings[i]] = files.ToArray();
                    assetTypeList.Add(settings[i].AssetDesc);
                }
            }

            AssetAllTypes = assetTypeList.ToArray();
            modelList = new List<EffectBean>();
            int curFileIndex = 0;
            foreach (ParticleEffectSettingBean msb in fileMaps.Keys)
            {
                string[] childFiles = fileMaps[msb];
                for (int i = 0; i < childFiles.Length; i++)
                {
                    curFileIndex++;
                    EditorUtility.DisplayProgressBar("分析中", "正在分析模型数据...", curFileIndex / fileCount);
//                    EffectBean mb = parseModel(childFiles[i]);
//                    mb.AssetDesc = msb.AssetDesc;
//
//                    mb.TriangleScore = mb.TriangleCount / (float)msb.MaxTriangs;
//                    mb.BondScore = mb.BondCount / (float)msb.MaxBones;
//                    mb.Score = (mb.TriangleScore + mb.BondScore) * 0.5f;
//
//                    modelList.Add(mb);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        public void OnGUI()
        {
            int newTab = mTab;
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width * 0.25f);
            if (GUILayout.Toggle(newTab == 0, "Overview", "ButtonLeft")) newTab = 0;
            if (GUILayout.Toggle(newTab == 1, "Setting", "ButtonRight")) newTab = 1;
            GUILayout.Space(Screen.width * 0.25f);
            GUILayout.EndHorizontal();

            mTab = newTab;
            if (newTab == 0)
                drawOverviewGUI();
            else if (newTab == 1)
                ruleView.OnGUI();
        }

        public void drawOverviewGUI()
        {
            NGUIEditorTools.DrawHeader("文件详情");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("筛选：");
                selectAssetType = EditorGUILayout.Popup(selectAssetType, AssetAllTypes);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("刷新", GUILayout.Width(80)))
                {
                    
                }
                GUILayout.Space(10);
            }
            GUILayout.Space(5);

            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUILayout.Toggle(false, "名称", "ButtonLeft", GUILayout.MaxWidth(200f));
            GUILayout.Toggle(false, "资源类型", "ButtonMid", GUILayout.MinWidth(100F));
            if (GUILayout.Toggle(false, "DrawCall数", "ButtonMid", GUILayout.MinWidth(100f)))
            {
                sortDc *= -1;
            }
            if (GUILayout.Toggle(false, "贴图数", "ButtonMid", GUILayout.MinWidth(80f)))
            {
                sortTexture *= -1;
            }
            if (GUILayout.Toggle(false, "粒子总数", "ButtonMid", GUILayout.MinWidth(100f)))
            {
                
            }
            if (GUILayout.Toggle(false, "规范评分", "ButtonRight", GUILayout.MinWidth(100F)))
            {
                sortScore *= -1;
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            int index = 0;
//            for (int i = 0; i < modelList.Count; i++)
//            {
//                if (selectAssetType != 0 && !modelList[i].AssetDesc.Equals(AssetAllTypes[selectAssetType])) continue;
//
//                GUI.backgroundColor = index % 2 == 1 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
//                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
//                GUI.backgroundColor = Color.white;
//
//                drawRow(modelList[i]);
//
//                GUILayout.EndHorizontal();
//                index++;
//            }
            GUILayout.EndScrollView();
        }




        public void OnDestroy()
        {
            
        }
        #region ------------------模块数量信息-------------------------

        private class EffectBean
        {
            public string Name;
            public string FilePath;
            public string AssetDesc;

            public int DrawCallCount;
            public int TextureCount;
            public int ParticelCount;
            
            public float DrawCallScore;
            public float ParticeScore;
            public float Score;

        }

        #endregion
    }

}

