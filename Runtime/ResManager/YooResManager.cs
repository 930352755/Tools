using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// 对资源加载的管理
/// 实现单机异步同步加载资源功能
/// </summary>
public class YooResManager
{

    /// <summary>
    /// 资源路径根目录
    /// 唯一目录
    /// </summary>
    public const string YooAssetResPath = "AYooAssetRes";

    #region 普通单例
    private static YooResManager instance = null;
    public static YooResManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new YooResManager();
            }
            return instance;
        }
    }
    #endregion

    /// <summary>
    /// 是否初始化完成
    /// </summary>
    public bool ISInitial = false;

    /// <summary>
    /// 资源加载包
    /// </summary>
    public ResourcePackage package = null;

    private YooResManager()
    {
        // 初始化资源系统
        YooAssets.Initialize();
        // 创建默认的资源包
        package = YooAssets.CreatePackage("DefaultPackage");
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);
        InitializeYooAsset();
    }

#if UNITY_EDITOR

    /// <summary>
    /// 编辑器加载资源
    /// </summary>
    private void InitializeYooAsset()
    {
        EditorSimulateModeParameters initParameters = new EditorSimulateModeParameters();
        string simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, "DefaultPackage");
        initParameters.SimulateManifestFilePath = simulateManifestFilePath;
        package.InitializeAsync(initParameters).Completed += (e) =>
        {
              ISInitial = true;
              Debug.Log("<Color=#E60000>YooAsset初始化完成</Color>");
        };
    }

#else

    /// <summary>
    /// 单机离线加载资源
    /// </summary>
    private void InitializeYooAsset()
    {
        var initParameters = new OfflinePlayModeParameters();
        initParameters.DecryptionServices = new FileOffsetDecryption();
        package.InitializeAsync(initParameters).Completed += (e) =>
        {
            ISInitial = true;
            Debug.Log("<Color=#E60000>YooAsset初始化完成</Color>");
        };
    }
   
#endif

    /// <summary>
    /// 对加载路径进行修正处理
    /// </summary>
    /// <param name="path">使用时传入的加载路径</param>
    private void FixPath(ref string path)
    {
        if (!path.Contains(YooAssetResPath + "/"))
        {
            path = YooAssetResPath + "/" + path;
        }
        if (!path.Contains("Assets/"))
        {
            path = "Assets/" + path;
        }
    }

    #region 异步加载

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径根目录下的路径即可</param>
    /// <returns></returns>
    public async Task<T> LoadAssetAsync<T>(string path) where T: UnityEngine.Object
    {
        //等待初始化
        while (!ISInitial)
        {
            await Task.Yield(); 
        }
        //修正加载路径
        FixPath(ref path);

        AssetHandle async = package.LoadAssetAsync<T>(path);
        //等待加载完成
        while (!async.IsDone)
        {
            await Task.Yield();
        }
        //返回加载到的资源
        return async.GetAssetObject<T>();
    }

    /// <summary>
    /// 异步加载资源以回调方式加载
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径根目录下的路径即可</param>
    /// <param name="action">加载完成回调</param>
    public async void LoadAssetAsync<T>(string path,System.Action<T> action) where T : UnityEngine.Object
    {
        FixPath(ref path);
        T t = await LoadAssetAsync<T>(path);
        action?.Invoke(t);
    }

    /// <summary>
    /// 异步加载这个路径下某个类型所有资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public async Task<List<T>> LoadAllAssetAsync<T>(string path) where T : UnityEngine.Object
    {
        while (!ISInitial)
        {
            await Task.Yield();
        }
        FixPath(ref path);
        AllAssetsHandle async = package.LoadAllAssetsAsync<T>(path);
        while (!async.IsDone)
        {
            await Task.Yield();
        }
        List<T> t = new List<T>();
        Object[] objects = async.AllAssetObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            t.Add(objects[i] as T);
        }
        return t;
    }

    /// <summary>
    /// 以回调的方式异步加载这个路径下某个类型所有资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <param name="action">加载完成回调</param>
    public async void LoadAllAssetAsync<T>(string path, System.Action<List<T>> action) where T : UnityEngine.Object
    {
        FixPath(ref path);
        List<T> t = await LoadAllAssetAsync<T>(path);
        action?.Invoke(t);
    }

    #endregion

    #region 同步直接加载，调用前确保已经初始完成。不然返回的是空的

    /// <summary>
    /// 直接加载某个资源
    /// 调用前确保已经初始完成。不然返回的是空的
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns></returns>
    public T LoadAssetSync<T>(string path) where T : UnityEngine.Object
    {
        if (!ISInitial)
        {
            Debug.Log("加载资源的时候，发现资源系统还没有初始化呢 ！！！！");
            return null;
        }
        FixPath(ref path);
        AssetHandle async = package.LoadAssetSync<T>(path);
        return async.GetAssetObject<T>();
    }

    #endregion

    #region 场景的加载
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task LoadSceneAsync(string path)
    {
        while (!ISInitial)
        {
            await Task.Yield();
        }
        FixPath(ref path);
        YooAsset.SceneHandle async = package.LoadSceneAsync(path, UnityEngine.SceneManagement.LoadSceneMode.Single);
        while (!async.IsDone)
        {
            await Task.Yield();
        }
    }
    #endregion

}

/// <summary>
/// 资源文件偏移加载解密类
/// </summary>
public class FileOffsetDecryption : IDecryptionServices
{

    AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        managedStream = null;
        return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
    }

    AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
    {
        managedStream = null;
        return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
    }

    public static ulong GetFileOffset()
    {
        return 32;
    }
}