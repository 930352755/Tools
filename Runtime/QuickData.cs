using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 快速数据存储工具
/// 存档会放到可读写目录下
/// 也提供复制存档功能
/// Key：要求独一无二，推荐命名规则 [命名空间]_[类名]_[字段名]
/// </summary>
public static class QuickData
{

    /// <summary>
    /// 保存String类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">String数据</param>
    public static void SetString(string key, string value)
    {
        DataInfo.Instance.SetValue(key, value);
    }
    /// <summary>
    /// 获取String类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <returns>String数据</returns>
    public static string GetString(string key, string defaultValue = default)
    {
        return DataInfo.Instance.GetValue(key, defaultValue);
    }

    /// <summary>
    /// 保存Int类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">Int数据</param>
    public static void SetInt(string key, int value)
    {
        DataInfo.Instance.SetValue(key, value);
    }
    /// <summary>
    /// 获取Int类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一/param>
    /// <returns>Int数据</returns>
    public static int GetInt(string key, int defaultValue = default)
    {
        return DataInfo.Instance.GetValue(key, defaultValue);
    }

    /// <summary>
    /// 保存long类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">long数据</param>
    public static void SetLong(string key, long value)
    {
        DataInfo.Instance.SetValue(key, value);
    }
    /// <summary>
    /// 获取long类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一/param>
    /// <returns>Int数据</returns>
    public static long GetLong(string key, long defaultValue = default)
    {
        return DataInfo.Instance.GetValue(key, defaultValue);
    }

    /// <summary>
    /// 保存Bool类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">Bool数据</param>
    public static void SetBool(string key, bool value)
    {
        DataInfo.Instance.SetValue(key, value);

    }
    /// <summary>
    /// 获取Bool类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <returns>Bool数据</returns>
    public static bool GetBool(string key, bool defaultValue = default)
    {
        return DataInfo.Instance.GetValue(key, defaultValue);
    }

    /// <summary>
    /// 保存Float类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">Float数据</param>
    public static void SetFloat(string key, float value)
    {
        DataInfo.Instance.SetValue(key, value);
    }
    /// <summary>
    /// 获取Float类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <returns>Float数据</returns>
    public static float GetFloat(string key, float defaultValue = default)
    {
        return DataInfo.Instance.GetValue(key, defaultValue);
    }

    /// <summary>
    /// 保存Double类型的数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetDouble(string key, double value)
    {
        DataInfo.Instance.SetValue(key, value);
    }
    /// <summary>
    /// 获取Double类型的数据
    /// </summary>
    /// <param name="key">Key值：唯一</param>
    /// <returns>Float数据</returns>
    public static double GetDouble(string key, double defaultValue = default)
    {
        return DataInfo.Instance.GetValue(key, defaultValue);
    }

    /// <summary>
    /// 保存Enum类型的数据
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">Enum数据</param>
    public static void SetEnum<T>(string key, T value) where T : System.Enum
    {
        DataInfo.Instance.SetValue(key, value.ToString());
    }
    /// <summary>
    /// 获取Enum类型的数据
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="key">Key值：唯一</param>
    /// <returns>Enum数据</returns>
    public static T GetEnum<T>(string key, T defaultValue = default) where T : System.Enum
    {
        string valueStr = DataInfo.Instance.GetValue(key, defaultValue.ToString());
        if (System.Enum.TryParse(typeof(T), valueStr, out object result))
        {
            return (T)result;
        }
        return defaultValue;
    }

    /// <summary>
    /// 保存Object类型的数据(Object可序列化)
    /// </summary>
    /// <typeparam name="T">Object类型</typeparam>
    /// <param name="key">Key值：唯一</param>
    /// <param name="value">Object数据</param>
    public static void SetObject<T>(string key, T value)
    {
        string json = JsonConvert.SerializeObject(value);
        DataInfo.Instance.SetValue(key, json);
    }
    /// <summary>
    /// 获取Object类型的数据(Object可序列化)
    /// 拿到这列表中这个列表元素的第一个索引
    /// </summary>
    /// <typeparam name="T">Object类型</typeparam>
    /// <param name="key">Key值：唯一</param>
    /// <returns>Object数据</returns>
    public static T GetObject<T>(string key, T defaultValue = default)
    {
        try
        {
            string jsonDe = JsonConvert.SerializeObject(defaultValue);
            string jsonTa = DataInfo.Instance.GetValue(key, jsonDe);
            return JsonConvert.DeserializeObject<T>(jsonTa);
        }
        catch (JsonException ex)
        {
            Debug.LogError($"反序列化失败: {ex.Message}");
            return defaultValue;
        }
    }

    public static string GetAllInfo(bool Decrypt = false)
    {
        return DataInfo.Instance.GetAllInfo(Decrypt);
    }

    /// <summary>
    /// 存放所有数据
    /// </summary>
    [System.Serializable]
    public class DataInfo
    {

        #region 普通单例

        private static DataInfo instance = null;
        private static readonly object lockObj = new object();
        public static DataInfo Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (instance == null) instance = new DataInfo();
                    return instance;
                }
            }
        }
        private DataInfo()
        {
            LoadData();
            Debug.Log("文件保存路径：" + FilePath);
        }

        #endregion

        #region 保存读取

        private string DataPath => GetType().Name;
        private string FilePath => Path.Combine(UnityEngine.Application.persistentDataPath, "QuickData", DataPath);
        private string DirectoryPath => Path.Combine(UnityEngine.Application.persistentDataPath, "QuickData");

        /// <summary>
        /// 加载保存数据
        /// </summary>
        private void LoadData()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            if (!File.Exists(FilePath))
            {
                StreamWriter stream = File.CreateText(FilePath);
                stream.Close();
            }
            string text = File.ReadAllText(FilePath);
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    allString = new();
                    allInt = new();
                    allLong = new();
                    allBool = new();
                    allFloat = new();
                    allDouble = new();
                    text = JsonConvert.SerializeObject(this);
                }
                else
                {
                    text = Decrypt(text);
                }
                //读取JSON写到自己里面
                JsonConvert.PopulateObject(text, this);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Game_DataBase: Direct parsing failed. Clear file :{e}");
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            StreamWriter stream = File.CreateText(FilePath);
            string jsonText = JsonConvert.SerializeObject(this);
            jsonText = Encrypt(jsonText);
            stream.Write(jsonText);
            stream.Close();
        }

        private static bool isSavingQueued = false;
        /// <summary>
        /// 一帧最多只保存一次
        /// </summary>
        private void QueueSaveData()
        {
            if (!isSavingQueued)
            {
                isSavingQueued = true;
                // Unity 主线程延迟调用
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    SaveData();
                    isSavingQueued = false;
                }, false);
            }
        }

        #endregion

        #region 加解密

        /// <summary>
        /// 加密String
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="k">密码</param>
        /// <returns></returns>
        private static string Encrypt(string content, string k = "1234567890abcdef")
        {
            byte[] keyBytes = System.Text.UTF8Encoding.UTF8.GetBytes(k);
            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged();
            rm.Key = keyBytes;
            rm.Mode = System.Security.Cryptography.CipherMode.ECB;
            rm.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            System.Security.Cryptography.ICryptoTransform ict = rm.CreateEncryptor();
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

            return System.Convert.ToBase64String(resultBytes);
        }
        /// <summary>
        /// 解密String
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="k">密码</param>
        /// <returns></returns>
        private static string Decrypt(string content, string k = "1234567890abcdef")
        {
            byte[] keyBytes = System.Text.UTF8Encoding.UTF8.GetBytes(k);
            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged();
            rm.Key = keyBytes;
            rm.Mode = System.Security.Cryptography.CipherMode.ECB;
            rm.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            System.Security.Cryptography.ICryptoTransform ict = rm.CreateDecryptor();
            byte[] contentBytes = System.Convert.FromBase64String(content);
            byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

            return System.Text.UTF8Encoding.UTF8.GetString(resultBytes);

        }

        #endregion

        #region 数据

        public Dictionary<string, string> allString = null;
        public Dictionary<string, int> allInt = null;
        public Dictionary<string, long> allLong = null;
        public Dictionary<string, bool> allBool = null;
        public Dictionary<string, float> allFloat = null;
        public Dictionary<string, double> allDouble = null;

        public void SetValue(string key, string value)
        {
            allString[key] = value;
            QueueSaveData();
        }
        public void SetValue(string key, int value)
        {
            allInt[key] = value;
            QueueSaveData();
        }
        public void SetValue(string key, long value)
        {
            allLong[key] = value;
            QueueSaveData();
        }
        public void SetValue(string key, bool value)
        {
            allBool[key] = value;
            QueueSaveData();
        }
        public void SetValue(string key, float value)
        {
            allFloat[key] = value;
            QueueSaveData();
        }
        public void SetValue(string key, double value)
        {
            allDouble[key] = value;
            QueueSaveData();
        }

        public string GetValue(string key, string defaultValue)
        {
            if (allString.ContainsKey(key))
            {
                return allString[key];
            }
            else
            {
                return defaultValue;
            }
        }
        public int GetValue(string key, int defaultValue)
        {
            if (allInt.ContainsKey(key))
            {
                return allInt[key];
            }
            else
            {
                return defaultValue;
            }
        }
        public long GetValue(string key, long defaultValue)
        {
            if (allLong.ContainsKey(key))
            {
                return allLong[key];
            }
            else
            {
                return defaultValue;
            }
        }
        public bool GetValue(string key, bool defaultValue)
        {
            if (allBool.ContainsKey(key))
            {
                return allBool[key];
            }
            else
            {
                return defaultValue;
            }
        }
        public float GetValue(string key, float defaultValue)
        {
            if (allFloat.ContainsKey(key))
            {
                return allFloat[key];
            }
            else
            {
                return defaultValue;
            }
        }
        public double GetValue(string key, double defaultValue)
        {
            if (allDouble.ContainsKey(key))
            {
                return allDouble[key];
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region 获取当前所有信息

        /// <summary>
        /// 获取当前所有信息的序列化结果
        /// </summary>
        /// <param name="Decrypt">是否解密</param>
        /// <returns></returns>
        public string GetAllInfo(bool Decrypt = false)
        {
            string jsonText = JsonConvert.SerializeObject(this);
            if (Decrypt)
            {
                return jsonText;
            }
            else
            {
                return Encrypt(jsonText);
            }
        }

        #endregion

    }

}
