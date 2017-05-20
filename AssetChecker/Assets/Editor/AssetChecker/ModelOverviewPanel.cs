using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetChecker;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AssetChecker
{
    /// <summary>
    /// 模型数据总览
    /// </summary>
    public class ModelOverviewPanel : IEditorPanel
    {

        private int mTab;

        private List<ModelBean> modelList;
        private Vector2 scrollPos;
        private int sortName = 1, sortTriangle = 1, sortBond = 1, sortScore = 1;
        private int selectAssetType;

        private ModelRuleView ruleView = new ModelRuleView();
        private string[] AssetAllTypes;

        public void Initizalize()
        {
            if(modelList != null)   return;
            OverviewSetting.Instance.ReadModelSettings();
            findModels();
        }
        /// <summary>
        /// 查找模型文件
        /// </summary>
        private void findModels()
        {
            scrollPos = Vector2.zero;
            List<ModelSettingBean> settings = OverviewSetting.Instance.ModelSettings;

            List<string> files = new List<string>();
            Dictionary<ModelSettingBean , string[]> fileMaps = new Dictionary<ModelSettingBean, string[]>();

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
                        string[] fileArr = Directory.GetFiles(rootFolder, "*.FBX", SearchOption.AllDirectories);
                        files.AddRange(fileArr);
                        fileCount += fileArr.Length;
                    }
                    fileMaps[settings[i]] = files.ToArray();
                    assetTypeList.Add(settings[i].AssetDesc);
                }
            }

            AssetAllTypes = assetTypeList.ToArray();
            modelList = new List<ModelBean>();
            int curFileIndex = 0;
            foreach (ModelSettingBean msb in fileMaps.Keys)
            {
                string[] childFiles = fileMaps[msb];
                for (int i = 0; i < childFiles.Length; i++)
                {
                    curFileIndex++;
                    EditorUtility.DisplayProgressBar("分析中", "正在分析模型数据...", curFileIndex / fileCount);
                    ModelBean mb = parseModel(childFiles[i]);
                    mb.AssetDesc = msb.AssetDesc;
                    
                    mb.TriangleScore = mb.TriangleCount/(float)msb.MaxTriangs;
                    mb.BondScore = mb.BondCount/ (float)msb.MaxBones;
                    mb.Score = (mb.TriangleScore + mb.BondScore)*0.5f;

                    modelList.Add(mb);
                }                    
            }

            EditorUtility.ClearProgressBar();
        }



        /// <summary>
        /// 解析模型数量
        /// </summary>
        /// <param name="filePath"></param>
        private ModelBean parseModel(string filePath)
        {
            ModelBean modelBean = new ModelBean();
            modelBean.Name = "  " + Path.GetFileNameWithoutExtension(filePath);
            modelBean.FilePath = filePath;

            UnityEngine.Object resObj = AssetDatabase.LoadAssetAtPath<Object>(filePath);
            GameObject gObj = GameObject.Instantiate(resObj) as GameObject;
            SkinnedMeshRenderer modelSkin = gObj.GetComponentInChildren<SkinnedMeshRenderer>();
            if (modelSkin != null)
                readSkinMeshRender(modelSkin, modelBean);
            else
            {
                MeshRenderer meshRenderer = gObj.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer != null) readMeshRender(meshRenderer, modelBean);
            }

            GameObject.DestroyImmediate(gObj);
            return modelBean;
        }


        private void readSkinMeshRender(SkinnedMeshRenderer modelSkin , ModelBean modelBean)
        {
            Mesh modelMesh = modelSkin.sharedMesh;
            if (modelMesh != null)
            {
                modelBean.VertexCount = modelMesh.vertexCount;
                modelBean.TriangleCount = modelMesh.triangles.Length / 3;
            }

            Transform[] boneTrans = modelSkin.bones;
            if (boneTrans != null) modelBean.BondCount = boneTrans.Length;

            Material mat = modelSkin.sharedMaterial;
            if (mat && mat.mainTexture)
            {
                Texture mainTex = mat.mainTexture;
                modelBean.TextureName = mainTex.name;
                modelBean.TextureSize = new Vector2(mainTex.width, mainTex.height);
            }
        }

        private void readMeshRender(MeshRenderer modelMeshRenderer, ModelBean modelBean)
        {
            MeshFilter mesh = modelMeshRenderer.GetComponent<MeshFilter>();
            Mesh modelMesh = mesh.sharedMesh;
            if (modelMesh != null)
            {
                modelBean.VertexCount = modelMesh.vertexCount;
                modelBean.TriangleCount = modelMesh.triangles.Length / 3;
            }
            
            Material mat = modelMeshRenderer.sharedMaterial;
            if (mat && mat.mainTexture)
            {
                Texture mainTex = mat.mainTexture;
                modelBean.TextureName = mainTex.name;
                modelBean.TextureSize = new Vector2(mainTex.width , mainTex.height);
            }
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
            if(newTab == 0)
                drawOverviewGUI();
            else if(newTab == 1)
                ruleView.OnGUI();
        }


        private void drawOverviewGUI()
        {
            NGUIEditorTools.DrawHeader("文件详情");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("筛选：");
                selectAssetType = EditorGUILayout.Popup(selectAssetType, AssetAllTypes);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("刷新" , GUILayout.Width(80)))
                {
                    findModels();
                }
                GUILayout.Space(10);
            }
            GUILayout.Space(5);

            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            if (GUILayout.Toggle(false, "模型名称", "ButtonLeft", GUILayout.MaxWidth(200f)))
            {
                sortName *= -1;
                modelList.Sort((x , y) => x.Name.CompareTo(y.Name) * sortName);
            }
            GUILayout.Toggle(false, "资源类型", "ButtonMid", GUILayout.MinWidth(100F));
            GUILayout.Toggle(false, "顶点数", "ButtonMid", GUILayout.MinWidth(80f));
            if (GUILayout.Toggle(false, "三角面数", "ButtonMid", GUILayout.MinWidth(80f)))
            {
                sortTriangle *= -1;
                modelList.Sort((x , y)=>x.TriangleCount.CompareTo(y.TriangleCount )* sortTriangle);
            }
            if (GUILayout.Toggle(false, "骨骼数", "ButtonMid", GUILayout.MinWidth(50f)))
            {
                sortBond *= -1;
                modelList.Sort((x, y)=>x.BondCount.CompareTo(y.BondCount) * sortBond);
            }
            //            GUILayout.Label("贴图名称", GUILayout.Width(150F));
            GUILayout.Toggle(false, "贴图尺寸", "ButtonMid", GUILayout.MinWidth(100F));
            if (GUILayout.Toggle(false, "综合评分", "ButtonRight", GUILayout.MinWidth(100F)))
            {
                sortScore *= -1;
                modelList.Sort((x, y)=>x.Score.CompareTo(y.Score) * sortScore);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            int index = 0;
            for (int i = 0; i < modelList.Count; i++)
            {
                if(selectAssetType != 0 && !modelList[i].AssetDesc.Equals(AssetAllTypes[selectAssetType]))  continue;

                GUI.backgroundColor = index % 2 == 1 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;

                drawRow(modelList[i]);

                GUILayout.EndHorizontal();
                index ++;
            }
            GUILayout.EndScrollView();
        }


        private void drawRow(ModelBean model)
        {
            int lv = 0;
            GUILayout.Label(model.Name , GUILayout.MaxWidth(200f));
            
            GUILayout.Space(50);
            GUILayout.Label(model.AssetDesc, GUILayout.MinWidth(100f));

            GUILayout.Label(model.VertexCount.ToString() , GUILayout.MinWidth(80f));

            setGUIColor(model.TriangleScore);
            GUILayout.Label(model.TriangleCount.ToString() , GUILayout.MinWidth(80f));

            setGUIColor(model.BondScore);
            GUILayout.Label(model.BondCount.ToString() , GUILayout.MinWidth(50f));
            GUI.color = Color.white;
            //            GUILayout.Label(model.TextureName , GUILayout.Width(150F));
            GUILayout.Label(string.Format("{0}x{1}" , model.TextureSize.x , model.TextureSize.y) , GUILayout.Width(100F));

            lv = GUISetting.CalScoreLevel(model.Score);
            GUI.color = GUISetting.ScoreColors[lv];
            GUILayout.Label(GUISetting.ScoreNames[lv], GUILayout.MinWidth(100F));
            GUI.color = Color.white;
        }


        private void setGUIColor(float score)
        {
            int lv = GUISetting.CalScoreLevel(score);
            GUI.color = GUISetting.ScoreColors[lv];
        }


        public void OnSettingGUI()
        {
           
           
            
        }

        public void OnDestroy()
        {

        }



        #region ------------------模块数量信息-------------------------

        private class ModelBean
        {
            public string Name;
            public string FilePath;
            public string AssetDesc;

            public int VertexCount;
            public int TriangleCount;
            public int BondCount;

            public string TextureName;
            public Vector2 TextureSize;

            public float VetexScore;
            public float TriangleScore;
            public float BondScore;
            public float Score;

        }

        #endregion
    }

}

