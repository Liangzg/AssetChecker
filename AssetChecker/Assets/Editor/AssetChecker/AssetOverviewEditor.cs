using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace AssetChecker
{
    /// <summary>
    /// 资源总览编辑器
    /// </summary>
    public class AssetOveriewEditor : EditorWindow
    {

        private int mTab;
        private string searchFilter = "";

        private ModelOverviewPanel modelPanel;
        private ParticleEffectOverviewPanel effectPanel;
        private TextureOverviewPanel texturePanel;

        private IEditorPanel currentPanel;

        [MenuItem("ArtTools/Asset Checker")]
        public static void ShowEditor()
        {
            AssetOveriewEditor assetOverview = GetWindow<AssetOveriewEditor>();
            assetOverview.initzalize();
            assetOverview.Show();
        }

        private void initzalize()
        {
            currentPanel = initSubPanel(0);
        }

        void OnGUI()
        {
            int newTab = mTab;
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(newTab == 0, "模型", "ButtonLeft")) newTab = 0;
            if (GUILayout.Toggle(newTab == 1, "特效", "ButtonMid")) newTab = 1;
            //            if (GUILayout.Toggle(newTab == 2, "贴图", "ButtonMid")) newTab = 2;
            //            if (GUILayout.Toggle(newTab == 3, "4", "ButtonMid")) newTab = 3;
            if (GUILayout.Toggle(newTab == 2, "贴图", "ButtonRight")) newTab = 2;

            GUILayout.EndHorizontal();
            //tab切换时初始化相应的分页
            if (mTab != newTab)
            {
                mTab = newTab;
                searchFilter = "";
                currentPanel = initSubPanel(newTab);
            }
            //搜索
            GUILayout.BeginHorizontal();
            string after = EditorGUILayout.TextField("", searchFilter, "SearchTextField", GUILayout.Width(Screen.width - 20f));

            if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
            {
                after = "";
                GUIUtility.keyboardControl = 0;
            }

            if (searchFilter != after)
            {
                NGUISettings.searchField = after;
                searchFilter = after;
            }
            GUILayout.EndHorizontal();

            if (currentPanel != null)
                currentPanel.OnGUI();
            GUILayout.Space(10);
        }

        private IEditorPanel initSubPanel(int tab)
        {
            switch (tab)
            {
                case 0:
                    if (modelPanel == null)
                    {
                        modelPanel = new ModelOverviewPanel();
                        modelPanel.Initizalize();
                    }
                    return modelPanel;
                case 1:
                    if (effectPanel == null)
                    {
                        effectPanel = new ParticleEffectOverviewPanel();
                        effectPanel.Initizalize();
                    }
                    return effectPanel;
                case 2:
                    if (texturePanel == null)
                    {
                        texturePanel = new TextureOverviewPanel();
                        texturePanel.Initizalize();
                    }
                    return texturePanel;
            }
            return null;
        }
    }
}

