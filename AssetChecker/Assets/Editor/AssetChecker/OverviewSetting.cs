using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;

namespace AssetChecker
{
    /// <summary>
    /// 设置管理
    /// </summary>
    public class OverviewSetting
    {

        private static OverviewSetting mInstance;
	    private OverviewSetting() { }

        public static OverviewSetting Instance
        {
            get
            {
                if(mInstance == null)   mInstance = new OverviewSetting();
                return mInstance;
            }
        }


        private List<SettingBean> mTextureSettings = new List<SettingBean>();
        private List<ModelSettingBean> mModelSettings = new List<ModelSettingBean>();
        private List<SettingBean> mEffectSettings = new List<SettingBean>();

        public List<SettingBean> TextureSettings
        {
            get { return mTextureSettings; }
            set { mTextureSettings = value; }
        }

        public List<ModelSettingBean> ModelSettings
        {
            get { return mModelSettings; }
            set { mModelSettings = value; }
        }

        public List<SettingBean> EffectSettings
        {
            get { return mEffectSettings; }
            set { mEffectSettings = value; }
        }
        /// <summary>
        /// 读取模型设置配置
        /// </summary>
        public void ReadModelSettings()
        {
            mModelSettings.Clear();
            string configPath = localFilePath;
            if (File.Exists(configPath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(configPath));
                XmlNode rootEle = xmlDoc.SelectSingleNode("ModelConfigs");

                foreach (XmlNode childNode in rootEle.ChildNodes)
                {
                    XmlElement childEle = childNode as XmlElement;
                    if (childEle == null) continue;

                    ModelSettingBean msb = new ModelSettingBean();
                    msb.Read(childEle);
                    mModelSettings.Add(msb);
                }
            }
        }


        public static string localFilePath
        {
            get { return EditorPrefs.GetString(Application.dataPath + "ModelSetting", ""); }
            set { EditorPrefs.SetString(Application.dataPath + "ModelSetting", value); }
        }
    }


    public class SettingBean
    {
        public string AssetDesc = "资源描述";
        /// <summary>
        /// 目录
        /// </summary>
        public List<string> Folder = new List<string>();
        /// <summary>
        /// 筛选的文件后缀
        /// </summary>
        public string MatchFile;

        /// <summary>
        /// 保存模型设置配置
        /// </summary>
        public virtual void Write(XmlDocument doc , XmlElement ele)
        {
            ele.SetAttribute("AssetDesc", AssetDesc);
            ele.SetAttribute("MatchFile", MatchFile);

            if (Folder.Count > 0)
            {
                for (int i = 0; i < Folder.Count; i++)
                {
                    if(string.IsNullOrEmpty(Folder[i])) continue;

                    XmlElement folderEle = doc.CreateElement("Folder");
                    folderEle.SetAttribute("Path", Folder[i]);
                    ele.AppendChild(folderEle);
                }
            }
        }


        public virtual void Read(XmlElement ele)
        {
            this.AssetDesc = ele.GetAttribute("AssetDesc");
            this.MatchFile = ele.GetAttribute("MatchFile");

            XmlNodeList childNodels = ele.ChildNodes;
            if (childNodels.Count > 0)
            {
                foreach (XmlNode childNode in childNodels)
                {
                    XmlElement childEle = childNode as XmlElement;
                    if(childEle != null)    Folder.Add(childEle.GetAttribute("Path"));
                }
            }else
                Folder.Add(string.Empty);
        }
    }

    #region ------------------------------------------------------
    public class ModelSettingBean : SettingBean
    {
        public int MaxTriangs;
        public int MaxBones;
        public int MaxTextureSize;

        /// <summary>
        /// 保存模型设置配置
        /// </summary>
        public override void Write(XmlDocument doc, XmlElement ele)
        {
            base.Write(doc , ele );

            ele.SetAttribute("MaxTriang", MaxTriangs.ToString());
            ele.SetAttribute("MaxBones", MaxBones.ToString());
            ele.SetAttribute("MaxTextrueSize", MaxTextureSize.ToString());
        }

        public override void Read(XmlElement ele)
        {
           base.Read(ele);

            MaxTriangs = Convert.ToInt32(ele.GetAttribute("MaxTriang"));
            MaxBones = Convert.ToInt32(ele.GetAttribute("MaxBones"));
            MaxTextureSize = Convert.ToInt32(ele.GetAttribute("MaxTextrueSize"));
        }
    }
    #endregion
}

