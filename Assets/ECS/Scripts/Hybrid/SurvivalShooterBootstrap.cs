using UnityEngine;
using UnityEngine.Assertions;

public sealed class SurvivalShooterBootstrap
{
    public static SurvivalShooterSettings Settings;

    public static void NewGame()
    {
        var player = Object.Instantiate(Settings.PlayerPrefab);
        var gun = Object.Instantiate(Settings.GunPrefab);
        gun.transform.parent = player.GetComponent<PlayerObject>().GunPivot.transform;
        
        // Try to find GameUi - first as root object, then search all objects
        var gameUiGo = GameObject.Find("GameUi");
        if (gameUiGo == null)
        {
            // Search in all GameObjects including children
            var allGameUis = Object.FindObjectsOfType<GameUi>();
            if (allGameUis.Length > 0)
            {
                Settings.GameUi = allGameUis[0];
                Debug.Log("[SurvivalShooterBootstrap] Found GameUi component in scene hierarchy");
            }
            else
            {
                Debug.LogError("[SurvivalShooterBootstrap] GameUi not found in scene!");
            }
        }
        else
        {
            Settings.GameUi = gameUiGo.GetComponent<GameUi>();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var settingsGo = GameObject.Find("Settings");
        if (settingsGo != null)
            Settings = settingsGo.GetComponent<SurvivalShooterSettings>();
        Assert.IsNotNull(Settings);
        
        // Find and set GameUi reference
        // Try to find GameUi - first as root object, then search all objects
        var gameUiGo = GameObject.Find("GameUi");
        if (gameUiGo == null)
        {
            // Search in all GameObjects including children
            var allGameUis = Object.FindObjectsOfType<GameUi>();
            if (allGameUis.Length > 0)
            {
                Settings.GameUi = allGameUis[0];
                Debug.Log("[SurvivalShooterBootstrap] Found GameUi component in scene hierarchy");
            }
            else
            {
                Debug.LogWarning("[SurvivalShooterBootstrap] GameUi not found in scene! UI updates will not work.");
            }
        }
        else
        {
            Settings.GameUi = gameUiGo.GetComponent<GameUi>();
            Debug.Log("[SurvivalShooterBootstrap] Found GameUi as root GameObject");
        }
    }
}
