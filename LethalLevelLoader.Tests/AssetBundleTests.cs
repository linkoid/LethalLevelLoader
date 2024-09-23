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
    /// <summary>
    /// Ensure that Lethal Level Loader will properly load asset bundles.
    /// </summary>
    [SmiteProcess("%GAME_EXE_PATH%", "-logfile -")]
    public class AssetBundleTests : MonoBehaviour
    {
        protected static AssetBundleTests Instance { get; private set; } = null!;
        protected static ManualLogSource Logger { get; private set; } = null!;

        protected static readonly string BundlesDirectory =
            Path.Combine(
                Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location!)!.FullName,
                "TestBundles"
            );

        [SmiteSetUp]
        public static void SetUp()
        {
            Instance = new GameObject($"{typeof(AssetBundleTests).Name}").AddComponent<AssetBundleTests>();
            GameObject.DontDestroyOnLoad(Instance.gameObject);
            Instance.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Logger = BepInEx.Logging.Logger.CreateLogSource(typeof(AssetBundleTests).Assembly.GetName().Name);
        }

        [SmiteTest]
        public static async Task LoadExtendedModBundle()
        {
            // Load an asset bundle with an extended mod
            string fileName = "starlancermoons.testbundle";
            string bundleFile = Path.Combine(BundlesDirectory, fileName); 

            await Instance.StartCoroutineTask(AssetBundleLoader.Instance.LoadBundle(bundleFile, fileName));

            // Assert that the bundle was loaded
            AssetBundleLoader.assetBundles.TryGetValue(fileName, out var assetBundle);
            assetBundle.Should().NotBeNull();

            // Log the keys of all loaded extended mods
            Logger.LogDebug($"Extended Mods Dictionary Keys ({AssetBundleLoader.obtainedExtendedModsDictionary.Count}): " +
                $"{string.Join(", ", AssetBundleLoader.obtainedExtendedModsDictionary.Keys)}");

            // Assert that the extended mod was loaded
            AssetBundleLoader.obtainedExtendedModsDictionary.TryGetValue("AudioKnight", out var extendedMod);
            extendedMod.Should().NotBeNull();
        }

        [SmiteTest]
        public static async Task LoadStreamedSceneAssetBundle()
        {
            // Load an asset bundle with scenes
            string fileName = "starlancermoonsscenes.testbundle";
            string bundleFile = Path.Combine(BundlesDirectory, fileName);

            await Instance.StartCoroutineTask(AssetBundleLoader.Instance.LoadBundle(bundleFile, fileName));

            // Assert that the bundle was loaded
            AssetBundleLoader.assetBundles.TryGetValue(fileName, out var assetBundle);
            assetBundle.Should().NotBeNull();
        }

        [SmiteTest]
        public static async Task LoadExtendedLevels()
        {
            // First, load the scenes
            await LoadStreamedSceneAssetBundle();

            // Second, load the extended mod
            await LoadExtendedModBundle();

            AssetBundleLoader.obtainedExtendedModsDictionary.TryGetValue("AudioKnight", out var extendedMod);
            extendedMod.Should().NotBeNull();

            // Assert that all the extended levels were loaded
            extendedMod!.ExtendedLevels.Count.Should().Be(3);
            foreach (var level in extendedMod.ExtendedLevels)
            {
                level.SelectableLevel.Should().NotBeNull();
                level.SelectableLevel.sceneName.Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}
