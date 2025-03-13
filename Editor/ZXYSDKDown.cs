#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Game.Editor
{

    /// <summary>
    /// SDK配置数据
    /// </summary>
    [Serializable]
    public class SDKConfig : ScriptableObject
    {
        public List<SDKInfo> sdks = new List<SDKInfo>();
    }

    [Serializable]
    public class SDKInfo
    {
        [Header("显示名称")]
        public string displayName;

        [Header("包标识")]
        public string packageName;

        [Header("Git URL")]
        public string gitUrl;

        [Header("描述")]
        [TextArea(3, 10)]
        public string description;
    }

    /// <summary>
    /// SDK管理窗口
    /// </summary>
    public class ZXYSDKDown : EditorWindow
    {

        #region 窗口管理
        private static EditorWindow _window;
        [MenuItem("Game/ZXY SDK管理", false, 2000)]
        private static void ShowWindow()
        {
            _window = GetWindow<ZXYSDKDown>("ZXY SDK管理器");
            _window.minSize = new Vector2(600, 400);
            _window.Show();
        }
        #endregion

        #region 配置文件

        private void LoadConfig()
        {
            // 创建新配置
            _config = CreateInstance<SDKConfig>();
            // 初始化示例数据
            _config.sdks = new List<SDKInfo>
            {


                new SDKInfo
                {
                    displayName = "快速读写",
                    packageName = "com.unity.zxy.quickdata",
                    gitUrl = "https://github.com/930352755/Tools.git#SDK-QuickData",
                    description = "用于快速序列化存取的工具包"
                },


                new SDKInfo
                {
                    displayName = "Excel读取工具",
                    packageName = "com.unity.zxy.excelreadplugin",
                    gitUrl = "https://github.com/930352755/Tools.git#SDK-ExcelReadPlugin",
                    description = "用于读取Excel文件的工具包"
                },

                new SDKInfo
                {
                    displayName = "YooAsset资源加载工具",
                    packageName = "com.unity.zxy.yooassets",
                    gitUrl = "https://github.com/930352755/Tools.git#SDK-ExcelReadPlugin",
                    description = "资源管理工具，提供资源的加载，封装的固定版本YooAssets V2.1.2,添加了管理器，目前只使用其中单机加载资源功能部分。加载路径直接 Assets/AYooAssetRes 下的相对路径就行了"
                },

            };
        }

        #endregion

        #region 运行时数据
        private SDKConfig _config;
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private double _lastUpdateTime;
        private static ListRequest _listRequest;
        private static bool _isOperationInProgress;
        private DateTime _operationStartTime;
        private bool _isOperating;
        private string _currentOperation;
        private string _currentPackage;
        private double _lastRepaintTime;
        #endregion

        #region 生命周期
        private void OnEnable()
        {
            LoadConfig();
            RefreshPackageList();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawSDKList();
            DrawStatusBar();
            DrawLoadingOverlay();
        }
        #endregion

        #region 核心逻辑


        private void RefreshPackageList(Action callback = null)
        {
            if (_isOperationInProgress)
            {
                SetStatus("正在等待当前操作完成，稍后自动刷新...");
                EditorApplication.delayCall += () =>
                {
                    RefreshPackageList(callback);
                };
                return;
            }

            if (_listRequest != null && !_listRequest.IsCompleted) return;

            _listRequest = Client.List();
            EditorApplication.update += OnListProgress;

            void OnListProgress()
            {
                if (!_listRequest.IsCompleted) return;

                EditorApplication.update -= OnListProgress;
                callback?.Invoke();
                Repaint();
            }
        }

        private UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName)
        {
            // 使用安全访问模式
            var packageCollection = _listRequest?.Result;

            // 处理可能出现的空值情况
            if (packageCollection == null || !packageCollection.Any())
            {
                return null;
            }

            // 使用StringComparer.OrdinalIgnoreCase提高比较效率
            return packageCollection.FirstOrDefault(
                p => string.Equals(p.name, packageName, StringComparison.OrdinalIgnoreCase));
        }

        private void HandlePackageOperation(string packageName, string operation, Func<Request> requestFunc)
        {
            if (_isOperating)
            {
                SetStatus($"请等待当前操作完成 ({_currentPackage} {_currentOperation})", true);
                return;
            }

            _isOperating = true;
            _currentOperation = operation;
            _currentPackage = packageName;
            _operationStartTime = DateTime.Now;

            var request = requestFunc();
            EditorApplication.update += TrackProgress;

            void TrackProgress()
            {
                // 每0.5秒强制刷新界面
                if (EditorApplication.timeSinceStartup - _lastRepaintTime > 0.5)
                {
                    Repaint();
                    _lastRepaintTime = EditorApplication.timeSinceStartup;
                }

                if (!request.IsCompleted) return;

                EditorApplication.update -= TrackProgress;
                _isOperating = false;

                var duration = (DateTime.Now - _operationStartTime).TotalSeconds.ToString("F1") + "s";
                if (request.Status == StatusCode.Success)
                {
                    SetStatus($"{packageName} {operation}成功 ({duration})");
                    RefreshPackageList();
                }
                else
                {
                    SetStatus($"{packageName} {operation}失败: {GetErrorDescription(request.Error)} ({duration})", true);
                }
            }
        }

        private string GetErrorDescription(Error error)
        {
            return error?.message switch
            {
                "NotFound" => "包不存在",
                "NetworkError" => "网络连接失败",
                _ => error?.message ?? "未知错误"
            };
        }

        private void SetStatus(string message, bool isError = false)
        {
            _statusMessage = message;
            _lastUpdateTime = EditorApplication.timeSinceStartup;
            Debug.Log(isError ? $"<color=red>{message}</color>" : message);
        }

        #endregion

        #region UI绘制

        private void DrawLoadingOverlay()
        {
            if (!_isOperating) return;

            // 半透明遮罩
            var rect = new Rect(0, 0, position.width, position.height);
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.2f));

            // 进度指示器
            using (var scope = new EditorGUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();

                // 加载动画
                var spinnerRect = GUILayoutUtility.GetRect(50, 50);
                EditorGUI.ProgressBar(spinnerRect,
                    (float)(DateTime.Now - _operationStartTime).TotalSeconds % 1,
                    "加载中...");

                // 操作信息
                GUILayout.Label($"正在{_currentOperation} {_currentPackage}", EditorStyles.boldLabel);
                GUILayout.Label($"已用时 {(DateTime.Now - _operationStartTime).TotalSeconds:F1}秒");

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("刷新列表", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    RefreshPackageList(() => SetStatus("包列表已更新"));
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawSDKList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_config.sdks.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到任何SDK配置，请通过菜单创建配置资源", MessageType.Info);
            }

            foreach (var sdk in _config.sdks)
            {
                EditorGUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField(sdk.displayName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(sdk.description, EditorStyles.wordWrappedMiniLabel);

                    var packageInfo = GetPackageInfo(sdk.packageName);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(packageInfo != null ?
                            $"已安装版本: {packageInfo.version}" :
                            "未安装",
                            GUILayout.Width(150));

                        GUILayout.FlexibleSpace();

                        if (packageInfo != null)
                        {
                            if (GUILayout.Button("移除", GUILayout.Width(80)))
                            {
                                HandlePackageOperation(sdk.packageName, "移除",
                                    () => Client.Remove(sdk.packageName));
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("安装", GUILayout.Width(80)))
                            {
                                HandlePackageOperation(sdk.packageName, "安装",
                                    () => Client.Add(sdk.gitUrl));
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatusBar()
        {
            if (string.IsNullOrEmpty(_statusMessage)) return;
            if (EditorApplication.timeSinceStartup - _lastUpdateTime > 5)
            {
                _statusMessage = "";
                return;
            }

            EditorGUILayout.HelpBox(_statusMessage,
                _statusMessage.Contains("失败") ?
                MessageType.Error : MessageType.Info);
        }

        #endregion
    }
}

#endif