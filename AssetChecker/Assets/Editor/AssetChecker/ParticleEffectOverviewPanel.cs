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
        private List<EffectBean> assetList;

        private ParticleEffectRuleView ruleView = new ParticleEffectRuleView();

        private int selectAssetType;
        private string[] AssetAllTypes;

        private int sortName = 1, sortDc = 1, sortTexture = 1,sortParticles = 1, sortScore = 1;

        public void Initizalize()
        {
            if (assetList != null) return;
           
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
            assetList = new List<EffectBean>();
            int curFileIndex = 0;
            foreach (ParticleEffectSettingBean msb in fileMaps.Keys)
            {
                string[] childFiles = fileMaps[msb];
                for (int i = 0; i < childFiles.Length; i++)
                {
                    curFileIndex++;
                    EditorUtility.DisplayProgressBar("分析中", "正在分析特效数据...", curFileIndex / fileCount);
                    EffectBean asset = parseEffectAsset(childFiles[i]);
                    asset.AssetDesc = msb.AssetDesc;

                    asset.DrawCallScore = asset.DrawCallCount / (float)msb.MaxMatrials;
                    asset.ParticeScore = asset.ParticelCount/(float) msb.MaxParticels;
                    asset.Score = (asset.DrawCallScore + asset.ParticeScore) * 0.5f;

                    assetList.Add(asset);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 分析粒子特效
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private EffectBean parseEffectAsset(string filePath)
        {
            EffectBean eb = new EffectBean();
            eb.Name = Path.GetFileName(filePath);
            eb.FilePath = filePath;

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<Object>(filePath);
            GameObject gObj = GameObject.Instantiate(obj) as GameObject;

            ParticleSystem[] particleArr = gObj.GetComponentsInChildren<ParticleSystem>();

            Dictionary<string , bool> materials = new Dictionary<string, bool>();
            Dictionary<string, bool> textures = new Dictionary<string, bool>();
            int particels = 0;
            for (int i = 0; i < particleArr.Length; i++)
            {
                Renderer renderer = particleArr[i].GetComponent<Renderer>();
                Material mat = renderer.sharedMaterial;
                if (mat != null)
                {
                    materials[mat.name] = true;
                    if(mat.mainTexture != null)
                        textures[mat.mainTexture.name] = true;                    
                }
                particels += particleArr[i].maxParticles;
            }
            eb.DrawCallCount = materials.Count;
            eb.TextureCount = textures.Count;
            eb.ParticelCount = particels;
            GameObject.DestroyImmediate(gObj);
            return eb;
        }

        public void OnGUI()
        {
            int newTab = mTab;
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width * 0.25f);
            if (GUILayout.Toggle(newTab == 0, "总 览", "ButtonLeft")) newTab = 0;
            if (GUILayout.Toggle(newTab == 1, "设 置", "ButtonRight")) newTab = 1;
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
                    findAllParticleEffects();
                }
                GUILayout.Space(10);
            }
            GUILayout.Space(5);

            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            if (GUILayout.Toggle(false, "名称", "ButtonLeft", GUILayout.MaxWidth(200f)))
            {
                sortName *= -1;
                assetList.Sort((x , y)=>x.Name.CompareTo(y.Name) * sortName);
            }
            GUILayout.Toggle(false, "资源类型", "ButtonMid", GUILayout.MinWidth(100F));
            if (GUILayout.Toggle(false, "DrawCall数", "ButtonMid", GUILayout.MinWidth(100f)))
            {
                sortDc *= -1;
                assetList.Sort((x,y)=>x.DrawCallCount.CompareTo(y.DrawCallCount) * sortDc);
            }
            if (GUILayout.Toggle(false, "贴图数", "ButtonMid", GUILayout.MinWidth(80f)))
            {
                sortTexture *= -1;
                assetList.Sort((x , y)=>x.TextureCount.CompareTo(y.TextureCount) * sortTexture);
            }
            if (GUILayout.Toggle(false, "粒子总数", "ButtonMid", GUILayout.MinWidth(100f)))
            {
                sortParticles *= -1;
                assetList.Sort((x,y)=>x.ParticelCount.CompareTo(y.ParticelCount) * sortParticles);
            }
            if (GUILayout.Toggle(false, "综合评分", "ButtonRight", GUILayout.MinWidth(100F)))
            {
                sortScore *= -1;
                assetList.Sort((x, y) => x.Score.CompareTo(y.Score) * sortParticles);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            int index = 0;
            for (int i = 0; i < assetList.Count; i++)
            {
                if (selectAssetType != 0 && !assetList[i].AssetDesc.Equals(AssetAllTypes[selectAssetType])) continue;

                GUI.backgroundColor = index % 2 == 1 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                drawRow(assetList[i]);

                GUILayout.EndHorizontal();
                index++;
            }
            GUILayout.EndScrollView();
        }


        private void drawRow(EffectBean asset)
        {
            int lv = 0;
            if (asset.FilePath == GUISetting.SelectFilePath)
                GUI.color = GUISetting.SelectColor;

            if (GUILayout.Button(asset.Name, "OL TextField", GUILayout.MaxWidth(200f)))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(asset.FilePath);
                GUISetting.SelectFilePath = asset.FilePath;
            }

            GUILayout.Space(50);
            GUILayout.Label(asset.AssetDesc, GUILayout.MinWidth(100f));
            
            setGUIColor(asset.DrawCallScore);
            GUILayout.Label(asset.DrawCallCount.ToString(), GUILayout.MinWidth(100f));
            GUI.color = Color.white;

            GUILayout.Label(asset.TextureCount.ToString(), GUILayout.MinWidth(80f));
            GUI.color = Color.white;
            
            setGUIColor(asset.ParticeScore);
            GUILayout.Label(asset.ParticelCount.ToString(), GUILayout.Width(100F));

            lv = GUISetting.CalScoreLevel(asset.Score);
            GUI.color = GUISetting.ScoreColors[lv];
            GUILayout.Label(GUISetting.ScoreNames[lv], GUILayout.MinWidth(100F));
            GUI.color = Color.white;
        }

        private void setGUIColor(float score)
        {
            int lv = GUISetting.CalScoreLevel(score); ;
            GUI.color = GUISetting.ScoreColors[lv];
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

