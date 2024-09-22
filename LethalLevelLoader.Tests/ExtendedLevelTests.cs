using BepInEx.Logging;
using FluentAssertions;
using SmiteUnit.Framework;
using SmiteUnit.Unity;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalLevelLoader.Tests
{
    // The SmiteProcessAttribute tells the test adapter which program to start
    [SmiteProcess("%GAME_EXE_PATH%", "-logfile -")]
    public class ExtendedLevelTests : MonoBehaviour
    {
        protected static ExtendedLevelTests Instance { get; private set; } = null!;
        protected static ManualLogSource Logger { get; private set; } = null!;

        [SmiteSetUp]
        public static void SetUp()
        {
            Instance = new GameObject($"{typeof(ExtendedLevelTests).Name}").AddComponent<ExtendedLevelTests>();
            GameObject.DontDestroyOnLoad(Instance.gameObject);
            Instance.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Logger = BepInEx.Logging.Logger.CreateLogSource(typeof(ExtendedLevelTests).Assembly.GetName().Name);
        }

        [SmiteTest]
        public static async Task LoadAssetBundle()
        {
            string bundleFile = "dummy.bundle";
            string fileName = "dummy.bundle";

            await Instance.StartCoroutineTask(AssetBundleLoader.Instance.LoadBundle(bundleFile, fileName));

            AssetBundleLoader.assetBundles.TryGetValue(fileName, out var assetBundle);
            assetBundle.Should().NotBeNull();
        }
    }
}
