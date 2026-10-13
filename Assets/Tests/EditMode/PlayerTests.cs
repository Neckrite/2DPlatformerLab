using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Тестовая сборка Tests (includePlatforms: Editor) — тесты EditMode
public class PlayerTests
{
    // Путь к сцене игры из Build Settings проекта
    private const string MainScenePath = "Assets/Scenes/Game.unity";

    // EDITMODE ТЕСТ: проверка правильности настройки сцены
    [Test]
    public void EditMode_SceneSetup_IsCorrect()
    {
        // Открываем сцену игры, чтобы получить доступ к её объектам
        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        Assert.IsTrue(scene.IsValid(), "Ошибка: не удалось открыть сцену Game!");

        // На сцене должен быть менеджер игры — он строит весь уровень при запуске,
        // включая игрока, монеты, платформы и камеру
        var gameManager = Object.FindAnyObjectByType<GameManager>();
        Assert.IsNotNull(gameManager, "Ошибка: на сцене нет объекта со скриптом GameManager — мир игры не будет построен!");

        // Сцена должна быть добавлена и включена в Build Settings,
        // иначе CLI-сборка из ЛР №1 соберет пустое приложение
        var buildScene = System.Linq.Enumerable
            .FirstOrDefault(EditorBuildSettings.scenes, s => s.path == MainScenePath);
        Assert.IsNotNull(buildScene,
            "Ошибка: сцена Game не добавлена в Build Settings!");
        Assert.IsTrue(buildScene.enabled,
            "Ошибка: сцена Game отключена в Build Settings!");

        // Скрипты механик должны существовать в проекте (сборка GameAssembly)
        Assert.IsNotNull(new GameObject("Probe").AddComponent<PlayerController>(),
            "Ошибка: скрипт PlayerController не компилируется в GameAssembly!");
    }
}
