using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressableTestScript : MonoBehaviour
{
    void Awake()
    {
#if UNITY_EDITOR
        if (!(AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilder is BuildScriptFastMode))
            Addressables.InternalIdTransformFunc = InternalIdTransformFunc;
#endif
    }

    private string InternalIdTransformFunc(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation location)
    {
        if (location.Data is AssetBundleRequestOptions)
        {
            string path = string.Empty;
#if UNITY_EDITOR
            path = Path.Combine(System.Environment.CurrentDirectory, location.InternalId);
#endif
            path = path.Replace("\\", "/");

            if (File.Exists(path))
                return path;

            return location.InternalId;
        }
        return location.InternalId;
    }


    private  void Start()
    {   
        Addressables.InstantiateAsync("PrefabA").Completed += handle =>
        {
            GameObject go = handle.Result;
            go.transform.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
        };
        Addressables.InstantiateAsync("PrefabB").Completed += handle =>
        {
            GameObject go = handle.Result;
            go.transform.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
        };

        //PreDownload();

        return;

    }

    [ContextMenu("ClearCache")]
    void ClearCache() 
    {   
        Addressables.CleanBundleCache();
    }

    [ContextMenu("LoadRemote")]
    void LoadRemote()
    {
        Addressables.InstantiateAsync("ZZ").Completed += handle =>
        {
            GameObject go = handle.Result;
            go.transform.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
        };
    }

    public string catalogPath = "";
    [ContextMenu("PreDownload")]
    async void PreDownload()
    {   
        //var catalogPath = "";
        AsyncOperationHandle<IResourceLocator> loadContentHandle = Addressables.LoadContentCatalogAsync(catalogPath);
        await loadContentHandle.Task;

        AsyncOperationHandle<long> downloadSizeHandle = Addressables.GetDownloadSizeAsync(loadContentHandle.Result.Keys);
        long bytes = await downloadSizeHandle.Task;

        if (bytes > 0)
        {   
            Debug.LogError(" PreDownload " + bytes / 1024 /1024 +"  "+ Time.realtimeSinceStartup );
            foreach (object key in loadContentHandle.Result.Keys)
            {
                AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key);

                while (!downloadHandle.IsDone)
                {
                    await Task.Yield();
                }

                //Asset download completed, do anything with the asset
                Debug.LogError(" PreDownload completed " + "  " + Time.realtimeSinceStartup);

                Addressables.Release(downloadHandle);
            }
        }

        Addressables.Release(downloadSizeHandle);
        Addressables.Release(loadContentHandle);
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        Addressables.InstantiateAsync("Build/PrefabA").Completed += handle =>
    //        {
    //            GameObject go = handle.Result;
    //            go.transform.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
    //        };
    //    }else if(Input.GetKeyDown(KeyCode.B))
    //    {
    //        Addressables.InstantiateAsync("Build/PrefabB").Completed += handle =>
    //        {
    //            GameObject go = handle.Result;
    //            go.transform.localPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
    //        };
    //    }
    //}
}
